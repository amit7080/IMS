using IMS.Data.Implementation;

namespace IMS.Data.Model
{
    public class AssignUser : AuditableEntity
    {
        public int AssignUserId { get; set; }
        public string UserId { get; set; }
        public string AssignedManagerId { get; set; }
        public string? AssignedHrId { get; set; }

        public virtual User User { get; set; }
        public virtual User AssignedManager { get; set; }
        public virtual User? AssignedHr { get; set; }

    }
}
