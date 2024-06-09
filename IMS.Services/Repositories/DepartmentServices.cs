using IMS.Data.Context;
using IMS.Data.Implementation;
using IMS.Data.Interface;
using IMS.Data.Model;
using IMS.Services.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace IMS.Services.Repositories
{
    public class DepartmentServices : IDepartmentServices
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly IRepository<Department> _departmentReposoitory;
        public DepartmentServices(IUnitOfWork unitofWork)
        {
            _unitofWork = unitofWork;
            _departmentReposoitory = _unitofWork.GetRepository<Department>();
        }
        public Task<List<Department>> GetAllDepartments()
        {
            return _departmentReposoitory.GetAll().ToListAsync();
        }
        public async Task<Department> CreateDepartments(Department department)
        {
            _ = _departmentReposoitory.Add(department);
            _=_unitofWork.commit();
            return department;
        }
        public async Task<Department> UpdateDepartments(Department department)
        {
            var departments = await _departmentReposoitory.GetByIdAsync(department.DepartmentId);
            departments.Name = department.Name;
            _ = _departmentReposoitory.Update(departments);
            _unitofWork.commit();
            return department;
        }
        public async Task<bool> DeleteDepartment(int id)
        {
            _departmentReposoitory.Delete(await _departmentReposoitory.GetByIdAsync(id));
            _unitofWork.commit();
            return true;
        }
        public async Task<Department> GetDepartmentById(int id)
        {
            return await _departmentReposoitory.GetByIdAsync(id);
        }
    }
}
