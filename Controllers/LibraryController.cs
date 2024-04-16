using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private ApplicationUser? GetUser()
        {
            string? userId = _userManager.GetUserId(User);
            return _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == userId);
        }

        [Authorize(Roles = $"{Constants.BorrowerRole},{Constants.SuperAdminRole}")]
        public IActionResult CheckoutBook(int id)
        {
            ApplicationUser? user = GetUser();

            Book? book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (user == null)
            {
                return Forbid();
            }
            if (book == null)
            {
                return NotFound();
            }
            if (book.BorrowingUser != null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.BorrowerRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (book.IsArchived == true)
            {
                return Forbid("Book archived, cannot request book");
            }

            if (_context?.BookRequests.Any(br => br.RequesterId == user.Id && br.BookId == book.Id && br.Status == Constants.BookRequestStatus.Pending) ?? false)
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

        [HttpGet]
        public IActionResult Index(string? searchQuery)
        {
            string search = searchQuery?.Trim() ?? string.Empty;
            ViewBag.SearchQuery = search;
            var viewableBooks = _context.Books.Where(b => b.IsArchived == false)
                .Where(b => b.Title.Contains(search) || b.BookAuthors.Any(ba => ba.Author.Name.Contains(search)) || b.BookGenres.Any(ba => ba.Genre.Name.Contains(search)) || search == string.Empty)
                .Include(b => b.BookAuthors)
                .ThenInclude(a => a.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
                .ToList();

            string? userId = _userManager.GetUserId(User);
            var user = _context.Users.Include(u => u.BookRequests).FirstOrDefault(u => u.Id == userId);

            var currentUserBookRequests = user.BookRequests?.ToList();

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