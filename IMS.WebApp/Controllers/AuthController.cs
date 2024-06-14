using IMS.Data.Model;
using IMS.Services.IRepositories;
using IMS.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IMS.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService; 

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid) 
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _tokenService.GenerateToken(user.Id, user.UserName, roles);

                    // Store the token in a cookie or session
                    HttpContext.Response.Cookies.Append("JWT", token, new CookieOptions { HttpOnly = true });

                    // Return success response
                    return Ok(new { success = true, message = "Login successful" });
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(new { success = false, message = "Invalid username or password" });
            }

            // Return failure response
            return BadRequest(new { success = false, message = "Invalid username or password" });
        }


        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                HttpContext.Response.Cookies.Delete("JWT");
                return View("Login");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            } 
           
        }
    }
}
