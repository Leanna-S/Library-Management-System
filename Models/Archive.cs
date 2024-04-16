namespace LibraryManagementSystem.Models
{
    public class Archive
    {
        public DateTime TimeOfArchive { get; set; }
        public int Id { get; set; }

        //fk
        public int BookId { get; set; }

        public string LibrarianId { get; set; }

        //nav
        public virtual Book Book { get; set; }

        public virtual ICollection<BookRequest> BookRequestsAffected { get; set; }

        public virtual ApplicationUser Librarian { get; set; }
    }
}