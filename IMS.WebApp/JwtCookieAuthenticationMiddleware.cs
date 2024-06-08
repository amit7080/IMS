using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace IMS.WebApp
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class JwtCookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<JwtCookieAuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        public JwtCookieAuthenticationMiddleware(RequestDelegate next , ILogger<JwtCookieAuthenticationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var token = httpContext.Request.Cookies["JWT"];
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var jwtSettings = _configuration.GetSection("Jwt");
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = key
                    }, out SecurityToken validatedToken);

                    if (validatedToken != null)
                    {
                        httpContext.User = principal;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token validation failed.");
                }
            }

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class JwtCookieAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieAuthenticationMiddleware>();
        }
    }
}
