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

        [HttpPost]
        [Authorize(Roles = $"{Constants.BorrowerRole},{Constants.SuperAdminRole}")]
        public IActionResult CancelBookRequest(int bookId)
        {
            //get current user
            ApplicationUser? user = UserHelper.GetUser(User, _userManager, _context);
            if (user == null)
            {
                return Forbid();
            }
            if (!UserHelper.UserInRole(User, Constants.BorrowerRole, Constants.SuperAdminRole))
            {
                return Forbid();
            }

            //get book
            Book? book = _context.Books.FirstOrDefault(b => b.Id == bookId);

            if (book == null)
            {
                return NotFound();
            }
            if (book.BorrowingUser == user)
            {
                return BadRequest("Can't cancel a book that is already checked out to you");
            }

            if (book.Archive != null)
            {
                return BadRequest("Book archived, cannot change status");
            }

            // get book request on book from user that is pending
            var request = _context.BookRequests.FirstOrDefault(br => br.RequesterId == user.Id && br.BookId == book.Id && br.Status == Constants.BookRequestStatus.Pending);

            if (request == null)
            {
                return NotFound("No requests found");
            }

            // change status of request
            request.Status = Constants.BookRequestStatus.Cancelled;
            request.TimeOfStatusUpdate = DateTime.UtcNow;
            request.Reason = "User cancelled request";

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult BookContent(int bookId)
        {
            //get current user
            ApplicationUser? user = UserHelper.GetUser(User, _userManager, _context);
            if (user == null)
            {
                return Forbid();
            }

            //get book
            Book? book = _context.Books.Include(b => b.BorrowingUser).FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                return NotFound();
            }
            //makes sure user is borrowing user or super admin
            if (user != book.BorrowingUser && !UserHelper.UserInRole(User, Constants.SuperAdminRole))
            {
                return Forbid();
            }
            return View(book);
        }

        [HttpPost]
        [Authorize(Roles = $"{Constants.BorrowerRole},{Constants.SuperAdminRole}")]
        public IActionResult RequestBook(int bookId)
        {
            // gets current user
            ApplicationUser? user = UserHelper.GetUser(User, _userManager, _context);
            if (user == null)
            {
                return Forbid();
            }
            //makes sure user in right role
            if (!UserHelper.UserInRole(User, Constants.BorrowerRole, Constants.SuperAdminRole))
            {
                return Forbid();
            }

            //gets book
            Book? book = _context.Books.FirstOrDefault(b => b.Id == bookId);

            if (book == null)
            {
                return NotFound();
            }
            if (book.BorrowingUser == user)
            {
                return BadRequest("Can't request book that is already checked out to you");
            }
            if (book.Archive != null)
            {
                return BadRequest("Book archived, cannot request book");
            }

            //checks if user has any other pending request for current book
            if (_context.BookRequests.Any(br => br.RequesterId == user.Id && br.BookId == book.Id && br.Status == Constants.BookRequestStatus.Pending))
            {
                return Conflict("Request already made for book");
            }

            // create request
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
            //get requests from current user
            string? userId = UserHelper.GetUserId(User, _userManager);

            var bookRequests = _context.BookRequests
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .Where(br => br.Requester.Id == userId).ToList();

            return View(bookRequests);
        }

        [HttpGet]
        public IActionResult Index(string? searchQuery)
        {
            //get books that pass search
            var viewableBooks = _context.Books.Where(b => b.Archive == null)
                .Where(b => searchQuery == null || b.Title.Contains(searchQuery) || b.BookAuthors.Any(ba => ba.Author.Name.Contains(searchQuery)) || b.BookGenres.Any(ba => ba.Genre.Name.Contains(searchQuery)))
                .Include(b => b.BorrowingUser)
                .Include(b => b.BookAuthors)
                .ThenInclude(a => a.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
                .Include(b => b.Archive)
                .Include(b => b.BookRequests)
                .ThenInclude(br => br.Requester)
                .ToList();

            //get default value for search
            ViewBag.SearchQuery = searchQuery;

            //get user
            ApplicationUser? user = UserHelper.GetUser(User, _userManager, _context);

            LibraryIndexViewModel viewModel = new LibraryIndexViewModel()
            {
                Books = viewableBooks,
                User = user
            };

            return View(viewModel);
        }
    }
}