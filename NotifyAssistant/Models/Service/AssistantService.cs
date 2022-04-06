using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using NotifyAssistant.Models.Entity;

namespace NotifyAssistant.Models.Service
{
    public class AssistantService : IAssistantService
    {
        private readonly IHttpContextAccessor _accessor;

        public AssistantService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }

        public int? GetUserId()
        {
            var nameClaim = _accessor.HttpContext?.User.Identity?.Name;
            if (nameClaim == null)
            {
                return null;
            }

            return int.Parse(nameClaim);
        }

        public async Task<bool> SignInAsync(User user)
        {
            if (_accessor.HttpContext == null)
            {
                return false;
            }
            
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Nickname", user.Nickname),
                new Claim("AvatarUrl", user.AvatarUrl)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            
            await _accessor.HttpContext.SignInAsync(claimsPrincipal);

            return true;
        }


        public async Task<bool> SignOutAsync()
        {
            if (_accessor.HttpContext == null)
            {
                return false;
            }
            
            await _accessor.HttpContext.SignOutAsync();

            return true;
        }
    }
}
