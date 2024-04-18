namespace LibraryManagementSystem.Models.ViewModels
{
    public class BorrowRequestIndexViewModel
    {
        public List<BookRequest> BookRequests { get; set; }
        public Constants.BookRequestStatus[] Statuses { get; set; }
    }
}