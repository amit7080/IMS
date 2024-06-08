using IMS.Data.Interface;
using System.ComponentModel.DataAnnotations;

namespace IMS.Data.Implementation
{
    public class AuditableEntity:IAuditableEntity
    {
        [ScaffoldColumn(false)]
        public DateTime? CreationDate { get; set; }


        [ScaffoldColumn(false)]
        public string? CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? ModificationDate { get; set; }


        [ScaffoldColumn(false)]
        public string? ModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
