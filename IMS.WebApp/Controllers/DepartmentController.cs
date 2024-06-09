using IMS.Data.Dto;
using IMS.Data.Interface;
using IMS.Data.Model;
using IMS.Services.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace IMS.WebApp.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ILogger<DepartmentController> _logger;
        private readonly IDepartmentServices _departmentRepository;
        private readonly IUnitOfWork _unitofWork;

        public DepartmentController(ILogger<DepartmentController> logger, IDepartmentServices departmentRepository, IUnitOfWork unitofWork)
        {
            _logger = logger;
            _departmentRepository = departmentRepository;
            _unitofWork = unitofWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet, Route("Department")]
        public async Task<IActionResult> Department()
        {
            return View(await _departmentRepository.GetAllDepartments());
        }

        [HttpPost, Route("GetDepartmentById")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            return Json(await _departmentRepository.GetDepartmentById(id));
        }
        [HttpGet,Route("FetchDepartments")]
        public async Task<IActionResult> FetchDepartments(PaginationDtos input)
        {
            List<Department> departments = await _departmentRepository.GetAllDepartments();
            var count = departments.Count;
            if (input != null && !string.IsNullOrEmpty(input.Search) && departments.Any())
            {
                departments = departments.Where(t =>
                    !string.IsNullOrEmpty(input.Search) && t.Name.Contains(input.Search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            if (!string.IsNullOrEmpty(input?.StartDate) && !string.IsNullOrEmpty(input.EndDate))
            {
                var startDate = Convert.ToDateTime(input.StartDate).Date + new TimeSpan(00, 00, 00);
                var endDate = Convert.ToDateTime(input.EndDate).Date + new TimeSpan(23, 59, 59);
                departments = departments.Where(t =>
                    t.CreationDate >= startDate && t.CreationDate <= endDate
                ).ToList();
            }


            return Json(new
            {
                recordsTotal = count,
                recordsFiltered = count,
                data = departments.Skip(input.SkipCount).Take(input.MaxResultCount).ToList()
            });
        }
        [HttpGet, Route("GetAllDepartments")]
        public async Task<List<Department>> GetAllDepartments()
        {
            return await _departmentRepository.GetAllDepartments();
        }
        // POST: Department/Create
        [HttpPost, Route("createDepartment")]
        [ValidateAntiForgeryToken]
        public async Task CreateDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                _ = await _departmentRepository.CreateDepartments(department);
            }
        }
        // POST: Department/Create
        [ValidateAntiForgeryToken]
        [HttpPost, Route("EditDepartment")]
        public async Task EditDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                _ = await _departmentRepository.UpdateDepartments(department);
            }
        }
        // POST: Department/delete
        [HttpPost, Route("deleteDepartment")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            if (ModelState.IsValid)
            {
               _= await _departmentRepository.DeleteDepartment(id);
                _unitofWork.commit();
                return RedirectToAction("Department", "Department");
            }
            return RedirectToAction("Department", "Home");
        }
    }
}
