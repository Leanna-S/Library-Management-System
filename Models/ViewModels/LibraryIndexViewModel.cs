namespace LibraryManagementSystem.Models.ViewModels
{
    public class LibraryIndexViewModel
    {
        public List<Book> Books { get; set; }
        public ApplicationUser? User { get; set; }
    }
}