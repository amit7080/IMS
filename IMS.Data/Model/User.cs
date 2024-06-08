using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IMS.Data.Interface;
using Microsoft.AspNetCore.Identity;

namespace IMS.Data.Model
{
    public class User : IdentityUser, IAuditableEntity
    {
        [MaxLength(50)]
        [Required]
        public string FirstName { get; set; }
        [MaxLength(50)]
        [Required]
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? JoiningDate { get; set; }
        [MaxLength(100)]
        public string Address { get; set; }
        [MaxLength(255)]
        public string? ProfileImage { get; set; }
        public int? DepartmentId { get; set; }
        [ScaffoldColumn(false)]
        public DateTime? CreationDate { get; set; }
        [ScaffoldColumn(false)]
        public string? CreatedBy { get; set; }
        [ScaffoldColumn(false)]
        public DateTime? ModificationDate { get; set; }
        [ScaffoldColumn(false)]
        public string? ModifiedBy { get; set; }
        [ScaffoldColumn(false)]
        public bool IsDeleted { get; set; } = false;
        [ScaffoldColumn(false)]
        public bool IsActive { get; set; } = true;
        public string? UserToken { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        public virtual ICollection<AssignUser> Users { get; set; }
        public virtual ICollection<AssignUser> AssignedManager { get; set; }
        public virtual ICollection<AssignUser>? AssignedHr { get; set; }
    }
}
