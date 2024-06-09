using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using IMS.Data.Context;
using IMS.Data.Implementation;
using IMS.Data.Interface;
using IMS.Data.Model;
using IMS.Data.Seeder;
using IMS.Services.IRepositories;
using IMS.Services.Repositories;
using IMS.WebApp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,          //Validate the server generate the token
            ValidateAudience = true,        //Validate the recipient
            ValidateLifetime = true,        //Token expired or not
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<SetupIdentityDataSeeder>();
builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.User.AllowedUserNameCharacters = string.Empty;
    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<IMSDbContext>()
            .AddDefaultTokenProviders();
builder.Services.AddDbContext<IMSDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("IMS")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IDepartmentServices, DepartmentServices>();
builder.Services.AddTransient<IdentityDataSeeder, IdentityDataSeeder>();
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
}
);

builder.Services.AddControllersWithViews();
var app = builder.Build();
app.UseMiddleware<JwtCookieAuthenticationMiddleware>(); // Add this line to use the middleware

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=login}/{id?}");


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseNotyf();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CustomExceptionMiddleware>();

await app.RunAsync();
