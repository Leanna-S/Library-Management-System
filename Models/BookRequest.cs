namespace LibraryManagementSystem.Models
{
    public class BookRequest
    {
        public Constants.BookRequestStatus Status { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TimeOfStatusUpdate { get; set; }
        public int Id { get; set; }

        //fk
        public string RequesterId { get; set; }

        public string? LibrarianId { get; set; }
        public int BookId { get; set; }

        public int? ArchiveId { get; set; }

        //nav
        public virtual ApplicationUser Requester { get; set; }

        public virtual Archive? Archive { get; set; }
        public virtual Book Book { get; set; }
        public virtual ApplicationUser? Librarian { get; set; }
    }
}