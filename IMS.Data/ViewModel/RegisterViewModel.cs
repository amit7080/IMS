using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Data.ViewModel
{
    public class RegisterViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        [Required]
        public string Role { get; set; }
        public DateTime DOB { get; set; }
        public DateTime JoiningDate { get; set; }
        public int? DepartmentId { get; set; }
        public string? AssignedManagerId { get; set; }
        public string? AssignedHrId { get; set; }
        public string? ProfileImage { get; set; }
        public string ImageName { get; set; }
    }
}
