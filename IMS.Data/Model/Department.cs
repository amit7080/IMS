using IMS.Data.Implementation;

namespace IMS.Data.Model
{
    public class Department : AuditableEntity
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
    }
}
