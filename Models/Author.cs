namespace LibraryManagementSystem.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //nav
        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
    }
}