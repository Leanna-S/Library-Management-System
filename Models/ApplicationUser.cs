using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Book> CheckedOutBooks { get; set; }
        public virtual ICollection<BookRequest> LibrarianBookRequests { get; set; }
        public virtual ICollection<BookRequest> BookRequests { get; set; }

        public virtual ICollection<Archive> Archives { get; set; }
        public virtual ICollection<RoleChange> RoleChanges { get; set; }

        public virtual ICollection<RoleChange> AdminRoleChanges { get; set; }
        public virtual ICollection<BookReturn> BookReturns { get; set; }
        public virtual ICollection<BookReturn> LibrarianBookReturns { get; set; }
    }
}