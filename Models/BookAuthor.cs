namespace LibraryManagementSystem.Models
{
    public class BookAuthor
    {
        //fk
        public int BookId { get; set; }

        public int AuthorId { get; set; }

        //nav
        public virtual Book Book { get; set; }

        public virtual Author Author { get; set; }
    }
}