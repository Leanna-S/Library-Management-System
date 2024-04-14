namespace LibraryManagementSystem.Models
{
    public class Book
    {
        // self properties
        public int Id { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public bool IsArchived { get; set; }

        public string Contents { get; set; }

        //fk properties
        public string? UserId { get; set; }

        public int? ArchiveId { get; set; }

        //nav properties

        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
        public virtual ICollection<BookGenre> BookGenres { get; set; }
        public virtual ApplicationUser? BorrowingUser { get; set; }

        public virtual ICollection<BookReturn> BookReturns { get; set; }

        //public virtual Archive? Archive { get; set; }
        public virtual ICollection<BookRequest> BookRequests { get; set; }
    }
}