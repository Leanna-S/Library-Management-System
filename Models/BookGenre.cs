namespace LibraryManagementSystem.Models
{
    public class BookGenre
    {
        //fk
        public int BookId { get; set; }

        public int GenreId { get; set; }

        //nav
        public virtual Book Book { get; set; }

        public virtual Genre Genre { get; set; }
    }
}