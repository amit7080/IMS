using IMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services.IRepositories
{
    public interface IDepartmentServices
    {
        Task<List<Department>> GetAllDepartments();
        Task<Department> CreateDepartments(Department department);
        Task<bool> DeleteDepartment(int id);
        Task<Department> GetDepartmentById(int id);
        Task<Department> UpdateDepartments(Department department);
    }
}
