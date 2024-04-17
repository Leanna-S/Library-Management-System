using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = $"{Constants.LibrarianRole},{Constants.SuperAdminRole}")]
    public class BookManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //shows all books, has options like return and archive, add button in corner, (un-archive hidden, visible to only SuperAdmins)
        public IActionResult Index()
        {
            var viewableBooks = _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(a => a.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
                .Include(b => b.BorrowingUser)
                .Include(b => b.Archive)
                .ToList();
            return View(viewableBooks);
        }

        public IActionResult ReturnBook(int id)
        {
            var book = _context.Books.Where(b => b.Id == id).FirstOrDefault();
            string? librarianId = _userManager.GetUserId(User);
            ApplicationUser? librarian = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == librarianId);
            if (book == null)
            {
                return NotFound();
            }
            if (librarian == null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.LibrarianRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (book.BorrowingUser == null)
            {
                return Forbid("Cannot return a book that is not borrowed");
            }

            BookReturn bookReturn = new BookReturn()
            {
                Book = book,
                Librarian = librarian,
                BorrowingUser = book.BorrowingUser,
                TimeOfReturn = DateTime.UtcNow,
            };
            _context.BookReturns.Add(bookReturn);
            book.BorrowingUser = null;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateBook(string title, string summary, string contents, int[] genres, int[] authors)
        {
            string? librarianId = _userManager.GetUserId(User);
            ApplicationUser? librarian = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == librarianId);
            if (librarian == null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.LibrarianRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            Book book = new Book()
            {
                Title = title,
                Summary = summary,
                Contents = contents,
            };
            _context.Books.Add(book);
            foreach (int genreId in genres)
            {
                Genre? genre = _context.Genres.Where(g => g.Id == genreId).FirstOrDefault();
                if (genre == null)
                {
                    continue;
                }
                BookGenre bookGenre = new BookGenre()
                {
                    Book = book,
                    Genre = genre,
                };
                _context.BookGenres.Add(bookGenre);
            }
            foreach (int authorId in authors)
            {
                Author? author = _context.Authors.Where(a => a.Id == authorId).FirstOrDefault();
                if (author == null)
                {
                    continue;
                }
                BookAuthor bookAuthor = new BookAuthor()
                {
                    Book = book,
                    Author = author,
                };
                _context.BookAuthors.Add(bookAuthor);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddBook()
        {
            AddBookViewModel viewModel = new AddBookViewModel()
            {
                Authors = _context.Authors.ToList(),
                Genres = _context.Genres.ToList(),
            };
            return View(viewModel);
        }

        public IActionResult ArchiveBook(int id)
        {
            var book = _context.Books
                .Include(b => b.BookRequests)
                .Include(b => b.BorrowingUser)
                .Where(b => b.Id == id)
                .FirstOrDefault();
            string? librarianId = _userManager.GetUserId(User);
            ApplicationUser? librarian = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == librarianId);
            if (book == null)
            {
                return NotFound();
            }
            if (librarian == null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.LibrarianRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (book.BorrowingUser != null)
            {
                return Forbid("Cannot archive book that is borrowed");
            }
            if (book.Archive != null)
            {
                return Forbid("Book already archived");
            }

            Archive archive = new Archive()
            {
                TimeOfArchive = DateTime.UtcNow,
                Librarian = librarian,
                BookRequestsAffected = book.BookRequests,
                Book = book,
            };

            _context.Archives.Add(archive);
            _context.SaveChanges();
            foreach (BookRequest br in book.BookRequests)
            {
                br.Status = Constants.BookRequestStatus.Denied;
                br.TimeOfStatusUpdate = DateTime.UtcNow;
                br.Librarian = librarian;
                br.Reason = "Book archived";
                br.Archive = archive;
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{Constants.SuperAdminRole}")]
        public IActionResult UnarchiveBook(int id)
        {
            var book = _context.Books
                .Include(b => b.Archive)
                .Where(b => b.Id == id)
                .FirstOrDefault();
            string? superAdminId = _userManager.GetUserId(User);
            ApplicationUser? superAdmin = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == superAdminId);
            if (book == null)
            {
                return NotFound();
            }
            if (superAdmin == null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (book.BorrowingUser != null)
            {
                return BadRequest("Cannot archive book that is borrowed");
            }
            if (book.Archive == null)
            {
                return BadRequest("Book not archived");
            }

            _context.Archives.Remove(book.Archive);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}