namespace LibraryManagementSystem.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //nav
        public virtual ICollection<BookGenre> BookGenres { get; set; }
    }
}