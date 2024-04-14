using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LibraryManagementSystem.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string? seedPw)
        {
            ArgumentNullException.ThrowIfNull(seedPw);
            string superAdminId = await SeedUser(serviceProvider, "a@a.com", seedPw);
            await SeedRole(serviceProvider, superAdminId, Constants.SuperAdminRole);
            await CreateRole(serviceProvider, Constants.AdminRole);
            await CreateRole(serviceProvider, Constants.LibrarianRole);
            await CreateRole(serviceProvider, Constants.BorrowerRole);
            SeedAuthors(serviceProvider, 15);
            SeedBooks(serviceProvider, 20);
            SeedBookAuthors(serviceProvider, 25);
            SeedGenres(serviceProvider, 6);
            SeedBookGenres(serviceProvider, 25);
        }

        private static void SeedBookGenres(IServiceProvider serviceProvider, int amount)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.BookGenres.Count() > 1)
            {
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                var bookGenre = new BookGenre()
                {
                    BookId = context.Books.ElementAt(i % context.Books.Count()).Id,
                    GenreId = context.Genres.ElementAt(i % context.Genres.Count()).Id,
                };
                context.BookGenres.Add(bookGenre);
            }
            context.SaveChanges();
        }

        private static void SeedGenres(IServiceProvider serviceProvider, int amount)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Genres.Count() > 1)
            {
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                var genre = new Genre()
                {
                    Name = $"Genre {i}",
                };
                context.Genres.Add(genre);
            }
            context.SaveChanges();
        }

        private static void SeedAuthors(IServiceProvider serviceProvider, int amount)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Authors.Count() > 1)
            {
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                var author = new Author()
                {
                    Name = $"Author {i}",
                };
                context.Authors.Add(author);
            }
            context.SaveChanges();
        }

        public static void SeedBooks(IServiceProvider serviceProvider, int amount)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Books.Count() > 1)
            {
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                var book = new Book()
                {
                    Title = $"Book {i}",
                    Summary = $"Book {i} summary",
                    IsArchived = false,
                    Contents = $"Book {i} contents"
                };
                context.Books.Add(book);
            }
            context.SaveChanges();
        }

        public static void SeedBookAuthors(IServiceProvider serviceProvider, int amount)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.BookAuthors.Count() > 1)
            {
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                var bookAuthor = new BookAuthor()
                {
                    BookId = context.Books.ElementAt(i % context.Books.Count()).Id,
                    AuthorId = context.Authors.ElementAt(i % context.Authors.Count()).Id,
                };
                context.BookAuthors.Add(bookAuthor);
            }
            context.SaveChanges();
        }

        public static async Task<string> SeedUser(IServiceProvider serviceProvider, string userName, string seedPw)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>()!;
            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    UserName = userName,
                    EmailConfirmed = true,
                };
                var result = await userManager.CreateAsync(user, seedPw);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to creat seed user");
                }
            }
            return user.Id;
        }

        public static async Task<IdentityResult> CreateRole(IServiceProvider serviceProvider, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>()!;

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            return result;
        }

        public static async Task<IdentityResult> SeedRole(IServiceProvider serviceProvider, string userId, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>()!;

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>()!;
            var user = await userManager.FindByIdAsync(userId) ?? throw new Exception("Seed user not found");
            IdentityResult result = await userManager.AddToRoleAsync(user, role);
            return result;
        }
    }
}