using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LibraryManagementSystem.Controllers
{
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LibraryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = $"{Constants.BorrowerRole},{Constants.SuperAdminRole}")]
        public IActionResult CancelBookRequest(int id)
        {
            string? userId = _userManager.GetUserId(User);
            ApplicationUser? user = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == userId);

            Book? book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (user == null)
            {
                return Forbid();
            }
            if (book == null)
            {
                return NotFound();
            }
            if (book.BorrowingUser == user)
            {
                return Forbid("Can't cancel a book that is already checked out to you");
            }
            if (!User.IsInRole(Constants.BorrowerRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (book.Archive != null)
            {
                return Forbid("Book archived, cannot change status");
            }
            var requests = _context.BookRequests.Where(br => br.RequesterId == user.Id && br.BookId == book.Id && br.Status == Constants.BookRequestStatus.Pending);

            if (!requests.Any())
            {
                return NotFound("No request found");
            }
            foreach (var request in requests)
            {
                request.Status = Constants.BookRequestStatus.Cancelled;
                request.TimeOfStatusUpdate = DateTime.UtcNow;
                request.Reason = "User cancelled request";
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult BookContent(int id)
        {
            string? userId = _userManager.GetUserId(User);
            ApplicationUser? user = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == userId);
            Book? book = _context.Books.Include(b => b.BorrowingUser).FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            if (user == null)
            {
                return NotFound();
            }
            if (user != book.BorrowingUser && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            return View(book);
        }

        [Authorize(Roles = $"{Constants.BorrowerRole},{Constants.SuperAdminRole}")]
        public IActionResult RequestBook(int id)
        {
            string? userId = _userManager.GetUserId(User);
            ApplicationUser? user = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == userId);

            Book? book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (user == null)
            {
                return Forbid();
            }
            if (book == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(Constants.BorrowerRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (book.BorrowingUser == user)
            {
                return BadRequest("Can't request book that is already checked out to you");
            }
            if (book.Archive != null)
            {
                return BadRequest("Book archived, cannot request book");
            }

            if (_context.BookRequests.Any(br => br.RequesterId == user.Id && br.BookId == book.Id && br.Status == Constants.BookRequestStatus.Pending))
            {
                return Conflict("Request already made for book");
            }

            BookRequest request = new()
            {
                Status = Constants.BookRequestStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                Requester = user,
                Book = book,
            };
            _context.BookRequests.Add(request);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{Constants.BorrowerRole},{Constants.SuperAdminRole}")]
        public IActionResult UserBookRequests()
        {
            string? userId = _userManager.GetUserId(User);
            var bookRequests = _context.BookRequests
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .Where(br => br.Requester.Id == userId).ToList();

            return View(bookRequests);
        }

        [HttpGet]
        public IActionResult Index(string? searchQuery)
        {
            string search = searchQuery?.Trim() ?? string.Empty;
            ViewBag.SearchQuery = search;
            var viewableBooks = _context.Books.Where(b => b.Archive == null)
                .Where(b => b.Title.Contains(search) || b.BookAuthors.Any(ba => ba.Author.Name.Contains(search)) || b.BookGenres.Any(ba => ba.Genre.Name.Contains(search)) || search == string.Empty)
                .Include(b => b.BorrowingUser)
                .Include(b => b.BookAuthors)
                .ThenInclude(a => a.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
                .Include(b => b.Archive)
                .ToList();

            string? userId = _userManager.GetUserId(User);
            var user = _context.Users.Include(u => u.BookRequests).FirstOrDefault(u => u.Id == userId);

            var currentUserBookRequests = user?.BookRequests?.ToList();

            LibraryIndexViewModel viewModel = new LibraryIndexViewModel()
            {
                Books = viewableBooks,
                User = user,
                CurrentUserBookRequests = currentUserBookRequests ?? new List<BookRequest>(),
            };

            return View(viewModel);
        }
    }
}