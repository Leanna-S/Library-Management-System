using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = $"{Constants.SuperAdminRole}")]
    public class TrackingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleChanges()
        {
            var roleChanges = _context.RoleChanges
                .Include(rc => rc.UserAffected)
                .Include(rc => rc.Admin)
                .ToList();
            return View(roleChanges);
        }

        public IActionResult BookReturns()
        {
            var bookReturns = _context.BookReturns
                .Include(br => br.Book)
                .Include(br => br.BorrowingUser)
                .Include(br => br.Librarian)
                .ToList();
            return View(bookReturns);
        }

        public IActionResult Archives()
        {
            var archives = _context.Archives
                .Include(a => a.BookRequestsAffected)
                .Include(a => a.Librarian)
                .Include(a => a.Book)
                .ToList();
            return View(archives);
        }

        public IActionResult BookRequests()
        {
            var bookRequests = _context.BookRequests
                .Include(br => br.Librarian)
                .Include(br => br.Book)
                .Include(br => br.Requester)
                .ToList();
            return View(bookRequests);
        }
    }
}