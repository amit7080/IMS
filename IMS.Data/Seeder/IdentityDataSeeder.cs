using IMS.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class IdentityDataSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<IdentityDataSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "Manager", NormalizedName = "MANAGER" },
                new IdentityRole { Name = "HR", NormalizedName = "HR" },
                new IdentityRole { Name = "Employee", NormalizedName = "EMPLOYEE" }
            };

            foreach (var role in roles)
            {
                await CreateRoleAsync(role);
            }

            var adminUser = new User
            {
                UserName = "admin@gmail.com",
                NormalizedUserName = "ADMIN@GMAIL.COM",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Admin",
                Gender = "0",
                Address = "Address",
                CreationDate = DateTime.UtcNow
            };

            var adminUserPassword = "Admin@123";
            await CreateUserAsync(adminUser, adminUserPassword);
            var adminInRole = await _userManager.IsInRoleAsync(adminUser, "Admin");
            if (!adminInRole)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    private async Task CreateRoleAsync(IdentityRole role)
    {
        if (!await _roleManager.RoleExistsAsync(role.Name))
        {
            await _roleManager.CreateAsync(role);
        }
    }

    private async Task CreateUserAsync(User user, string password)
    {
        if (await _userManager.FindByEmailAsync(user.Email) == null)
        {
            await _userManager.CreateAsync(user, password);
        }
    }
}
