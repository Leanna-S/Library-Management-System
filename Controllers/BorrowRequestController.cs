using Humanizer;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = $"{Constants.LibrarianRole},{Constants.SuperAdminRole}")]
    public class BorrowRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Librarians would like to be able to sort and/or filter the list of requests by status, date, user, or book.

        public BorrowRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            string? userId = _userManager.GetUserId(User);
            var bookRequests = _context.BookRequests
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .Include(b => b.Archive)
                .ToList();
            var statuses = Enum.GetValues<Constants.BookRequestStatus>();
            BorrowRequestIndexViewModel vm = new()
            {
                BookRequests = bookRequests,
                Statuses = statuses
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Index(Constants.BookRequestStatus? status, string? searchQuery, DateTime? startDate, DateTime? endDate)
        {
            string? userId = _userManager.GetUserId(User);
            var bookRequests = _context.BookRequests
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .Include(br => br.Archive)
                .Where(br => status == null || br.Status == status)
                .Where(br => searchQuery == null
                || br.Requester.UserName!.Contains(searchQuery)
                || br.Book.Title.Contains(searchQuery))
                .Where(br => startDate == null || br.CreatedDate >= startDate)
                .Where(br => endDate == null || br.CreatedDate <= endDate)
                .ToList();
            var statuses = Enum.GetValues<Constants.BookRequestStatus>();
            BorrowRequestIndexViewModel vm = new()
            {
                BookRequests = bookRequests,
                Statuses = statuses
            };
            ViewBag.Status = status;
            ViewBag.SearchQuery = searchQuery;
            if (startDate != null)
            {
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            }
            if (endDate != null)
            {
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult ChangeStatus(Constants.BookRequestStatus status, int id, string? reason)
        {
            var bookRequest = _context.BookRequests
                .Include(br => br.Requester)
                .Include(br => br.Book)
                .FirstOrDefault(br => br.Id == id);
            string? librarianId = _userManager.GetUserId(User);
            ApplicationUser? librarian = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == librarianId);

            if (librarian == null)
            {
                return Forbid();
            }
            if (bookRequest == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(Constants.LibrarianRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (bookRequest.Book.Archive != null)
            {
                return BadRequest("Book archived, cannot change status");
            }

            if (status != Constants.BookRequestStatus.Approved && status != Constants.BookRequestStatus.Denied)
            {
                return BadRequest("Cannot change to status type");
            }
            if (bookRequest.Status != Constants.BookRequestStatus.Pending)
            {
                return BadRequest("Cannot change status, status not pending");
            }
            if (bookRequest.Book.UserId != null)
            {
                return BadRequest("Book checked out to a user");
            }
            bookRequest.TimeOfStatusUpdate = DateTime.UtcNow;
            bookRequest.Status = status;
            bookRequest.Librarian = librarian;
            bookRequest.Reason = reason;
            if (status == Constants.BookRequestStatus.Approved)
            {
                bookRequest.Book.BorrowingUser = bookRequest.Requester;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UpdateRequestStatus(int id)
        {
            var bookRequest = _context.BookRequests.FirstOrDefault(br => br.Id == id);
            if (bookRequest == null)
            {
                return NotFound("No request found");
            }
            return View(bookRequest);
        }
    }
}