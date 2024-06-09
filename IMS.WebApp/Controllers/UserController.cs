using IMS.Data.Constants;
using IMS.Data.Dto;
using IMS.Data.Interface;
using IMS.Data.Model;
using IMS.Data.ViewModel;
using IMS.Services.IRepositories;
using IMS.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.IO;

namespace IMS.WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentServices _departmentServices;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<AssignUser> _assignUserRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<IdentityUserRole<string>> _userRoleRepository;
        private readonly IWebHostEnvironment _env;
        public UserController(IUserRepository userRepository, UserManager<User> userManager, IRepository<AssignUser> assignUserRepository, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager, IDepartmentServices departmentServices, IRepository<IdentityUserRole<string>> userRoleRepository, IWebHostEnvironment webHostEnvironment)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _assignUserRepository = assignUserRepository;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _departmentServices = departmentServices;
            _userRoleRepository = userRoleRepository;
            _env = webHostEnvironment;
        }
        [Authorize(Roles = "Admin,Employee,Manager,HR"), Route("Employees")]
        public IActionResult Employees()
        {
            return View();
        }
        [HttpGet, Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(PaginationDtos input)
        {
            var users = await _userRepository.GetAllUser();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(input.Search))
            {
                users = users.Where(u => u.FirstName.Contains(input.Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply date filter if both start and end dates are provided
            if (!string.IsNullOrEmpty(input.StartDate) && !string.IsNullOrEmpty(input.EndDate))
            {
                var startDate = Convert.ToDateTime(input.StartDate).Date + new TimeSpan(00, 00, 00);
                var endDate = Convert.ToDateTime(input.EndDate).Date + new TimeSpan(23, 59, 59);
                users = users.Where(t => t.CreationDate >= startDate && t.CreationDate <= endDate).ToList();
            }

            // Get all roles
            var roles = await _roleManager.Roles.ToListAsync();

            // Skip and take based on pagination parameters
            users = users.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            var data = users.Select(async user =>
            {
                var userRoles = await _userRoleRepository.GetAll().Where(x => x.UserId == user.Id).ToListAsync();
                return new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.JoiningDate,
                    user.IsActive,
                    user.PhoneNumber,
                    Roles = userRoles.Select(ur => roles.Find(r => r.Id == ur.RoleId)?.Name),
                    Department = new { Name = user.DepartmentId.HasValue ? (await _departmentServices.GetDepartmentById(user.DepartmentId.Value))?.Name ?? " " : " " },
                    user.ProfileImage
                };
            }).Select(userTask => userTask.Result).ToList();

            return Json(new
            {
                recordsTotal = data.Count,
                recordsFiltered = data.Count,
                data
            });
        }


        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        /// <summary>
        /// GetAllRoles
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            return Json(await _roleManager.Roles.ToListAsync());
        }

        [HttpGet, Route("GetAllManager")]
        public async Task<IActionResult> GetAllManager()
        {
            // Get the role object for "Manager"
            var managerRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
            if (managerRole == null)
            {
                return Json(new { message = "Manager role not found" });
            }

            // Get all user IDs associated with the "Manager" role
            var managerUserIds = await _userRoleRepository.GetAll()
                .Where(x => x.RoleId == managerRole.Id)
                .Select(x => x.UserId)
                .ToListAsync();

            // Get users with IDs in managerUserIds list
            var users = await _userRepository.GetAllUser();

            var totalusers = users.Where(u => managerUserIds.Contains(u.Id))
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.FirstName + " " + u.LastName // Assuming user's full name is the username
                })
                .ToList();
            if (!totalusers.Any())
            {
                // Get the role object for "Manager"
                var adminRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    return Json(new { message = "Manager role not found" });
                }

                // Get all user IDs associated with the "Manager" role
                managerUserIds = await _userRoleRepository.GetAll()
                   .Where(x => x.RoleId == adminRole.Id)
                   .Select(x => x.UserId)
                   .ToListAsync();
                totalusers = users.Where(u => managerUserIds.Contains(u.Id))
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.FirstName + " " + u.LastName // Assuming user's full name is the username
                }).ToList();
            }
            return Json(totalusers);
        }

        [HttpGet, Route("GetAllHR")]
        public async Task<IActionResult> GetAllHR()
        {
            // Get the role object for "Manager"
            var HRRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "HR");
            if (HRRole == null)
            {
                return Json(new { message = "HR role not found" });
            }

            // Get all user IDs associated with the "Manager" role
            var HRUserIds = await _userRoleRepository.GetAll()
                .Where(x => x.RoleId == HRRole.Id)
                .Select(x => x.UserId)
                .ToListAsync();

            // Get users with IDs in managerUserIds list
            var users = await _userRepository.GetAllUser();

            var totalUsers = users.Where(u => HRUserIds.Contains(u.Id))
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.FirstName + " " + u.LastName // Assuming user's full name is the username
                })
                .ToList();
            if (!totalUsers.Any())
            {
                // Get the role object for "Manager"
                var adminRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    return Json(new { message = "Manager role not found" });
                }

                // Get all user IDs associated with the "Manager" role
                HRUserIds = await _userRoleRepository.GetAll()
                    .Where(x => x.RoleId == adminRole.Id)
                    .Select(x => x.UserId)
                    .ToListAsync();
                totalUsers = users.Where(u => HRUserIds.Contains(u.Id))
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.FirstName + " " + u.LastName // Assuming user's full name is the username
                }).ToList();
            }
            return Json(totalUsers);
        }

        [HttpGet, Route("GetCurrentUserProfileImage")]
        public async Task<IActionResult> GetCurrentUserProfileImage()
        {
            // Get the current logged-in user's identity
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // User is not logged in
            }

            // Fetch the user's data from the repository
            var user = await _userRepository.GetAllUser();
            var loggedUser = user.Find(x => x.Id == userId);

            // Return the user's profile image
            return Json(new
            {
                Name = loggedUser.FirstName,
                ProfileImage = loggedUser.ProfileImage
            });
        }

        [HttpPost, Route("CreateEmployee")]
        public async Task<bool> CreateEmployee([FromBody] RegisterViewModel user)
        {
            try
            {
                string relativeDir = Path.Combine("UserImages");
                string fullRelativeDir = Path.Combine(_env.WebRootPath, relativeDir);

                if (!Directory.Exists(fullRelativeDir))
                {
                    Directory.CreateDirectory(fullRelativeDir);
                }
                // Convert and save the profile image
                byte[] imageBytes = Convert.FromBase64String(user.ProfileImage);
                string fullRelativeFilePath = Path.Combine(fullRelativeDir, user.ImageName);
                await System.IO.File.WriteAllBytesAsync(fullRelativeFilePath, imageBytes);

                var users = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = user.Email,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.Phone,
                    Address = user.Address,
                    Gender = user.Gender,
                    JoiningDate = user.JoiningDate,
                    DOB = user.DOB,
                    DepartmentId = user.DepartmentId,
                    EmailConfirmed = false,
                    ProfileImage = user.ImageName,
                    PhoneNumberConfirmed = true,
                    CreationDate = DateTime.UtcNow,
                };

                var result = await _userManager.CreateAsync(users, user.Password);
                if (result.Succeeded)
                {
                    var roleName = (await _roleManager.Roles.ToListAsync()).Find(x => x.Id == user.Role)?.Name;

                    if (!string.IsNullOrEmpty(roleName))
                    {
                        var addToRoleResult = await _userManager.AddToRoleAsync(users, roleName);
                        if (addToRoleResult.Succeeded)
                        {
                            if (user.AssignedHrId != null || user.AssignedManagerId != null)
                            {
                                var assignUser = new AssignUser
                                {
                                    UserId = users.Id,
                                    AssignedHrId = user.AssignedHrId,
                                    AssignedManagerId = user.AssignedManagerId
                                };
                                _ = _assignUserRepository.Add(assignUser);
                                _ = _unitOfWork.commit();

                            }
                            _ = await _userManager.GenerateEmailConfirmationTokenAsync(users);
                            _ = await _userManager.GeneratePasswordResetTokenAsync(users);

                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
        [HttpPost, Route("deactivateUser")]
        public async Task<bool> DeactivateUser(string userId)
        {
            return await _userRepository.InActiveUser(userId);
        }
    }
}
