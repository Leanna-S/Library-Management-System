using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using System.Reflection.Emit;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public virtual DbSet<Archive> Archives { get; set; }
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<BookAuthor> BookAuthors { get; set; }
        public virtual DbSet<BookGenre> BookGenres { get; set; }
        public virtual DbSet<BookRequest> BookRequests { get; set; }
        public virtual DbSet<BookReturn> BookReturns { get; set; }
        public virtual DbSet<Genre> Genres { get; set; }
        public virtual DbSet<RoleChange> RoleChanges { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ArchiveCreating(builder);
            AuthorCreating(builder);
            BookCreating(builder);
            BookAuthorCreating(builder);
            BookGenreCreating(builder);
            BookRequestCreating(builder);
            BookReturnCreating(builder);
            GenreCreating(builder);
            RoleChangeCreating(builder);
        }

        private void ArchiveCreating(ModelBuilder builder)
        {
            builder.Entity<Archive>()
                .HasOne(ar => ar.Book)
                .WithOne(b => b.Archive)
                .HasForeignKey<Archive>(a => a.BookId)
                .IsRequired();

            builder.Entity<Archive>()
                .HasOne(ar => ar.Librarian)
                .WithMany(u => u.Archives)
                .HasForeignKey(ar => ar.LibrarianId)
                .IsRequired(true);
            builder.Entity<Archive>()
                .HasKey(ar => ar.Id);
        }

        private void AuthorCreating(ModelBuilder builder)
        {
            builder.Entity<Author>()
                .HasKey(a => a.Id);
        }

        private void BookCreating(ModelBuilder builder)
        {
            builder.Entity<Book>()
                .HasOne(b => b.BorrowingUser)
                .WithMany(u => u.CheckedOutBooks)
                .HasForeignKey(b => b.UserId)
                .IsRequired(false);
            builder.Entity<Book>()
                .HasKey(b => b.Id);
        }

        private void BookAuthorCreating(ModelBuilder builder)
        {
            builder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId)
                .IsRequired(true);
            builder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId)
                .IsRequired(true);
            builder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.AuthorId, ba.BookId });
        }

        private void BookGenreCreating(ModelBuilder builder)
        {
            builder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId)
                .IsRequired(true);
            builder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId)
                .IsRequired(true);
            builder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });
        }

        private void BookRequestCreating(ModelBuilder builder)
        {
            builder.Entity<BookRequest>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BookRequests)
                .HasForeignKey(br => br.BookId)
                .IsRequired(true);
            builder.Entity<BookRequest>()
                .HasOne(br => br.Librarian)
                .WithMany(u => u.LibrarianBookRequests)
                .HasForeignKey(br => br.LibrarianId)
                .IsRequired(false);
            builder.Entity<BookRequest>()
                .HasOne(br => br.Requester)
                .WithMany(u => u.BookRequests)
                .HasForeignKey(br => br.RequesterId)
                .IsRequired(true);
            builder.Entity<BookRequest>()
                .HasKey(br => br.Id);
        }

        private void BookReturnCreating(ModelBuilder builder)
        {
            builder.Entity<BookReturn>()
                .HasOne(bre => bre.Book)
                .WithMany(b => b.BookReturns)
                .HasForeignKey(br => br.BookId)
                .IsRequired(true);
            builder.Entity<BookReturn>()
                .HasOne(bre => bre.Librarian)
                .WithMany(u => u.BookReturns)
                .HasForeignKey(bre => bre.LibrarianId)
                .IsRequired(true);
            builder.Entity<BookReturn>()
                .HasKey(bre => bre.Id);
        }

        private void GenreCreating(ModelBuilder builder)
        {
            builder.Entity<Author>()
                .HasKey(a => a.Id);
        }

        private void RoleChangeCreating(ModelBuilder builder)
        {
            builder.Entity<RoleChange>()
                .HasOne(rc => rc.UserAffected)
                .WithMany(u => u.RoleChanges)
                .HasForeignKey(rc => rc.UserAffectedId)
                .IsRequired(true);
            builder.Entity<RoleChange>()
                .HasOne(rc => rc.Admin)
                .WithMany(u => u.AdminRoleChanges)
                .HasForeignKey(rc => rc.AdminId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<RoleChange>()
                .HasKey(rc => rc.Id);
        }
    }
}