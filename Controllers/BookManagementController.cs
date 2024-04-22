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

        [HttpGet]
        public IActionResult Index(string searchQuery)
        {
            // filter books using the search and include all needed items
            ViewBag.SearchQuery = searchQuery;
            var viewableBooks = _context.Books
                .Where(b => b.Title.Contains(searchQuery) || b.BookAuthors.Any(ba => ba.Author.Name.Contains(searchQuery)) || b.BookGenres.Any(ba => ba.Genre.Name.Contains(searchQuery)) || searchQuery == null)
                .Include(b => b.BorrowingUser)
                .Include(b => b.BookAuthors)
                .ThenInclude(a => a.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
                .Include(b => b.Archive)
                .ToList();
            return View(viewableBooks);
        }

        public IActionResult ReturnBook(int bookId)
        {
            // double checks user is in appropriate role
            if (!UserHelper.UserInRole(User, Constants.LibrarianRole, Constants.SuperAdminRole))
            {
                return Forbid();
            }

            // gets books and current user
            var book = _context.Books.Where(b => b.Id == bookId).FirstOrDefault();
            ApplicationUser? librarian = UserHelper.GetUser(User, _userManager, _context);
            if (librarian == null)
            {
                return Forbid();
            }
            if (book == null)
            {
                return NotFound();
            }
            if (book.BorrowingUser == null)
            {
                return Forbid("Cannot return a book that is not borrowed");
            }

            // creates book return
            BookReturn bookReturn = new BookReturn()
            {
                Book = book,
                Librarian = librarian,
                BorrowingUser = book.BorrowingUser,
                TimeOfReturn = DateTime.UtcNow,
            };
            _context.BookReturns.Add(bookReturn);
            // return book - no borrowing user
            book.BorrowingUser = null;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateBook(string title, string summary, string contents, int[] genres, int[] authors)
        {
            // verifies user in valid role
            if (!UserHelper.UserInRole(User, Constants.LibrarianRole, Constants.SuperAdminRole))
            {
                return Forbid();
            }
            //creates new book
            Book book = new Book()
            {
                Title = title,
                Summary = summary,
                Contents = contents,
            };
            _context.Books.Add(book);

            // create book genre entries to link book to genres
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
            // create book author entries to link book to authors
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
                Authors = [.. _context.Authors],
                Genres = [.. _context.Genres],
            };
            return View(viewModel);
        }

        public IActionResult ArchiveBook(int bookId)
        {
            //verifies user in appropriate role
            if (!UserHelper.UserInRole(User, Constants.LibrarianRole, Constants.SuperAdminRole))
            {
                return Forbid();
            }
            //gets current user
            ApplicationUser? librarian = UserHelper.GetUser(User, _userManager, _context);
            if (librarian == null)
            {
                return Forbid();
            }

            //get book to be archived
            var book = _context.Books
                .Include(b => b.BookRequests)
                .Include(b => b.BorrowingUser)
                .Where(b => b.Id == bookId)
                .FirstOrDefault();

            if (book == null)
            {
                return NotFound();
            }

            if (book.BorrowingUser != null)
            {
                return Forbid("Cannot archive book that is borrowed");
            }
            if (book.Archive != null)
            {
                return Forbid("Book already archived");
            }
            //get book requests that archive would affect
            var bookRequests = book.BookRequests.Where(br => br.Status == Constants.BookRequestStatus.Pending).ToList();
            //create archive
            Archive archive = new Archive()
            {
                TimeOfArchive = DateTime.UtcNow,
                Librarian = librarian,
                BookRequestsAffected = bookRequests,
                Book = book,
            };

            _context.Archives.Add(archive);

            //update all book requests to denied if status is pending
            foreach (BookRequest br in bookRequests)
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
        public IActionResult UnarchiveBook(int bookId)
        {
            // verifies user in appropriate role
            if (!UserHelper.UserInRole(User, Constants.SuperAdminRole))
            {
                return Forbid();
            }

            //get book to unarchive
            var book = _context.Books
                .Include(b => b.Archive)
                .Where(b => b.Id == bookId)
                .FirstOrDefault();

            if (book == null)
            {
                return NotFound();
            }

            if (book.BorrowingUser != null)
            {
                return BadRequest("Cannot archive book that is borrowed");
            }
            if (book.Archive == null)
            {
                return BadRequest("Book not archived");
            }

            //remove the archive
            _context.Archives.Remove(book.Archive);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult History(int bookId)
        {
            var book = _context.Books
                .Include(b => b.BookReturns)
                .ThenInclude(br => br.BorrowingUser)
                .Include(b => b.BookRequests)
                .ThenInclude(br => br.Requester)
                .FirstOrDefault(b => b.Id == bookId);
            return View(book);
        }
    }
}