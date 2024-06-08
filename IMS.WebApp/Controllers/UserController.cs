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



        [HttpPost, Route("createUserAsync")]
        public async Task<IActionResult> CreateUserAsync(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Role == UserRoles.Employee && (string.IsNullOrEmpty(model.AssignedHrId) || string.IsNullOrEmpty(model.AssignedManagerId)))
                    {
                        // return new BadRequestObjectResult(new { succeeded = false, msg = "Assign ManagerID or Assign HRID not be null" });
                        //return new OkObjectResult(new ResponseMessageViewModel
                        //{
                        //    IsSuccess = false,
                        //    Message = "Assign ManagerID or Assign HRID not be null"
                        //});
                    }
                    else
                    {
                        var mailResult = await _userRepository.IsEmailExist(model.Email);
                        if (!mailResult)
                        {
                            var user = new User
                            {
                                UserName = model.Email,
                                Email = model.Email,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                PhoneNumber = model.Phone,
                                Address = model.Address,
                                Gender = model.Gender,
                                JoiningDate = model.JoiningDate,
                                DOB = model.DOB,
                                DepartmentId = model.DepartmentId,
                                EmailConfirmed = false,
                                PhoneNumberConfirmed = true,
                            };
                            var result = await _userManager.CreateAsync(user, model.Password);
                            if (result.Succeeded)
                            {
                                var roles = await _userManager.AddToRoleAsync(user, model.Role);
                                if (roles.Succeeded)
                                {
                                    if (model.AssignedHrId != null || model.AssignedManagerId != null)
                                    {
                                        var assignUser = new AssignUser
                                        {
                                            UserId = user.Id,
                                            AssignedHrId = model.AssignedHrId,
                                            AssignedManagerId = model.AssignedManagerId
                                        };
                                        _ = _assignUserRepository.Add(assignUser);
                                        var assignResult = _unitOfWork.commit();
                                    }

                                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                    var ResetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);


                                    if (ResetPasswordToken != null)
                                    {
                                        user.UserToken = Uri.EscapeDataString(ResetPasswordToken);
                                        await _userManager.UpdateAsync(user);
                                    }

                                    // return new OkObjectResult(new { succeded = result, model });
                                    return new OkObjectResult(new
                                    {
                                        IsSuccess = true,
                                        Data = model,
                                        Message = "User Added"
                                    });
                                }
                                else
                                {
                                    return new OkObjectResult(new
                                    {
                                        IsSuccess = false,
                                        Message = result.Errors.Select(x => x.Description).FirstOrDefault()
                                    });
                                }

                            }
                            else
                            {
                                return new OkObjectResult(new
                                {
                                    IsSuccess = false,
                                    Message = result.Errors.Select(x => x.Description).FirstOrDefault()
                                });
                            }
                        }
                        else
                        {
                            return new OkObjectResult(new
                            {
                                IsSuccess = false,
                                Message = "Email already exist"
                            });
                            // return new BadRequestObjectResult(new { succeeded = false, msg = "Email already exist" });
                        }
                    }
                }
                return new OkObjectResult(new
                {
                    IsSuccess = false,
                    Message = "Model is not validate"
                });
            }
            catch (Exception ex)
            {

                return new OkObjectResult(new
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
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
            var loggedUser = user.Where(x => x.Id == userId).FirstOrDefault();

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
            string filePath = string.Empty;
            string relativeDir = Path.Combine("UserImages");
            string wwwRootPath = _env.WebRootPath;
            string fullRelativeDir = Path.Combine(wwwRootPath, relativeDir);

            if (!Directory.Exists(fullRelativeDir))
            {
                Directory.CreateDirectory(fullRelativeDir);
            }

            byte[] imageBytes = Convert.FromBase64String(user.ProfileImage);
            string fullRelativeFilePath = Path.Combine(relativeDir, user.ImageName);
            string fullPath = Path.Combine(wwwRootPath, fullRelativeFilePath);
            await System.IO.File.WriteAllBytesAsync(fullPath, imageBytes);

            var users = new User
            {
                Id=Guid.NewGuid().ToString(),
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
                ProfileImage= user.ImageName,
                PhoneNumberConfirmed = true,
                CreationDate=DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(users, user.Password);
            if (result.Succeeded)
            {
                var roleName=await _roleManager.Roles.ToListAsync();
                var currentRoleName= roleName.Find(x=>x.Id==user.Role);
                var roles = await _userManager.AddToRoleAsync(users, currentRoleName.Name);
                if (roles.Succeeded)
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
                        var assignResult = _unitOfWork.commit();

                    }

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(users);
                    var ResetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(users);

                }
                return true;
            }
            return false; 
        }
        [HttpPost,Route("deactivateUser")]
        public async Task<bool> DeactivateUser(string userId)
        {
           return await _userRepository.InActiveUser(userId);
        }


    }
}
