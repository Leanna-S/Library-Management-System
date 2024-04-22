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

        public BorrowRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            //get all book requests
            var bookRequests = _context.BookRequests
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .Include(b => b.Archive)
                .ToList();

            //get all the possible book statuses
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
            //get all book requests that pass search filters
            var bookRequests = _context.BookRequests
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .Include(br => br.Archive)
                .Where(br => status == null || br.Status == status)
                .Where(br => searchQuery == null || br.Requester.UserName!.Contains(searchQuery) || br.Book.Title.Contains(searchQuery))
                .Where(br => startDate == null || br.CreatedDate >= startDate)
                .Where(br => endDate == null || br.CreatedDate <= endDate)
                .ToList();

            var statuses = Enum.GetValues<Constants.BookRequestStatus>();

            BorrowRequestIndexViewModel vm = new()
            {
                BookRequests = bookRequests,
                Statuses = statuses
            };

            //set default form data to the current search
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
        public IActionResult ChangeStatus(Constants.BookRequestStatus status, int bookRequestId, string? reason)
        {
            //check user in valid role
            if (!UserHelper.UserInRole(User, Constants.SuperAdminRole, Constants.LibrarianRole))
            {
                return Forbid();
            }

            //gets current user
            ApplicationUser? librarian = UserHelper.GetUser(User, _userManager, _context);
            if (librarian == null)
            {
                return Forbid();
            }

            //get the current book request
            var bookRequest = _context.BookRequests
                .Include(br => br.Requester)
                .Include(br => br.Book)
                .FirstOrDefault(br => br.Id == bookRequestId);

            if (bookRequest == null)
            {
                return NotFound();
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

            //update book request values
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

        [HttpPost]
        public IActionResult UpdateRequestStatus(int bookRequestId)
        {
            var bookRequest = _context.BookRequests.FirstOrDefault(br => br.Id == bookRequestId);
            if (bookRequest == null)
            {
                return NotFound("No request found");
            }
            return View(bookRequest);
        }
    }
}