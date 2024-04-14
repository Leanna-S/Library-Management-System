namespace LibraryManagementSystem.Models
{
    public static class Constants
    {
        public const string SuperAdminRole = "SuperAdmin";
        public const string AdminRole = "Admin";
        public const string LibrarianRole = "Librarian";
        public const string BorrowerRole = "Borrower";

        public enum BookRequestStatus
        {
            Approved,
            Denied,
            Pending,
        }

        public enum RoleChangeType
        {
            Added,
            Removed,
        }
    }
}