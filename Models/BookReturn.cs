namespace LibraryManagementSystem.Models
{
    public class BookReturn
    {
        public DateTime TimeOfReturn { get; set; }
        public int Id { get; set; }

        //fk
        public int BookId { get; set; }

        public string LibrarianId { get; set; }

        //nav
        public virtual Book Book { get; set; }

        public virtual ApplicationUser Librarian { get; set; }
    }
}