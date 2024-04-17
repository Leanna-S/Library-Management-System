using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = $"{Constants.AdminRole},{Constants.SuperAdminRole}")]
    public class RoleManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            List<RoleManagementIndexViewModel> users = [];
            foreach (var user in _context.Users)
            {
                RoleManagementIndexViewModel vm = new()
                {
                    User = user,
                    Roles = (List<string>)await _userManager.GetRolesAsync(user),
                };
                users.Add(vm);
            }
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string role, string id)
        {
            string? adminId = _userManager.GetUserId(User);
            ApplicationUser? admin = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == adminId);
            if (admin == null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return NotFound("Role does not exist");
            }
            if ((role == Constants.AdminRole || role == Constants.SuperAdminRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found");
            }
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                return NotFound("User not in role");
            }

            await _userManager.RemoveFromRoleAsync(user, role);
            RoleChange roleChange = new RoleChange()
            {
                Admin = admin,
                UserAffected = user,
                RoleAffected = role,
                TimeOfChange = DateTime.UtcNow,
                Type = Constants.RoleChangeType.Removed
            };
            _context.RoleChanges.Add(roleChange);
            _context.SaveChanges();


            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RemoveRole(string id)
        {
            ApplicationUser? user = _context.Users.FirstOrDefault(u => u.Id == id);
            string? adminId = _userManager.GetUserId(User);
            if (user == null)
            {
                return NotFound("User not found");
            }
            
            RoleManagementIndexViewModel vm = new()
            {
                User = user,
                Roles = (List<string>)await _userManager.GetRolesAsync(user),
                IsCurrentUser = (user.Id == adminId),
            };
            return View(vm);
        }

        public async Task<IActionResult> AddRole(string id)
        {
            ApplicationUser? user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesUserNotIn = _roleManager.Roles.Where(r => !userRoles.Contains(r.Name)).Select(r => r.Name.ToString()).ToList();
            RoleManagementIndexViewModel vm = new()
            {
                User = user,
                Roles = rolesUserNotIn,
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string id, string role)
        {
            string? adminId = _userManager.GetUserId(User);
            ApplicationUser? admin = _context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == adminId);
            if (admin == null)
            {
                return Forbid();
            }
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if ((role == Constants.AdminRole || role == Constants.SuperAdminRole) && !User.IsInRole(Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return NotFound("Role does not exist");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            await _userManager.AddToRoleAsync(user, role);
            RoleChange roleChange = new RoleChange()
            {
                Admin = admin,
                UserAffected = user,
                RoleAffected = role,
                TimeOfChange = DateTime.UtcNow,
                Type = Constants.RoleChangeType.Added,
            };
            _context.RoleChanges.Add(roleChange);
            _context.SaveChanges();
            
            return RedirectToAction(nameof(Index));
        }
    }
}