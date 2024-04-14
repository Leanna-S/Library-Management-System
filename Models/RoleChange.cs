namespace LibraryManagementSystem.Models
{
    public class RoleChange
    {
        public int Id { get; set; }
        public Constants.RoleChangeType Type { get; set; }
        public string RoleAffected { get; set; }
        public DateTime TimeOfChange { get; set; }

        //fk
        public string UserAffectedId { get; set; }

        public string AdminId { get; set; }

        //nav
        public virtual ApplicationUser UserAffected { get; set; }

        public virtual ApplicationUser Admin { get; set; }
    }
}