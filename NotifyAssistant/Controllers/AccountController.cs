using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyAssistant.Models.Entity;
using NotifyAssistant.Models.LineLogin;
using NotifyAssistant.Models.Service;
using System.Net;
using System.Text;

namespace NotifyAssistant.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IAssistantService _assistant;
        private readonly AssistantDbContext _db;
        private readonly ILineLoginService _lineLogin;

        public AccountController(
            IHttpContextAccessor accessor,
            IAssistantService assistant,
            AssistantDbContext db,
            ILineLoginService lineLogin)
        {
            _accessor = accessor;
            _assistant = assistant;
            _db = db;
            _lineLogin = lineLogin;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult LineLogin()
        {
            // 設定 state
            var state = Guid.NewGuid().ToString();
            _accessor.HttpContext.Session.SetString("LineLoginState", state);
            
            var redirectUrl = _lineLogin.GetAuthorizeEndpoint(state);

            return Ok(new { redirectUrl });
        }
        
        [HttpGet]
        public async Task<IActionResult> LineCallback([FromQuery] LineCallbackRequestViewModel reqModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Login");
            }
            
            // 檢查 state
            var state = _accessor.HttpContext.Session.GetString("LineLoginState");
            if (state == null || !state.Equals(reqModel.State))
            {
                TempData["ErrorMessage"] = "Invalid state";
                return RedirectToAction("Index", "Error");
            }
            
            // 呼叫 Token Endpoint
            var tokenMessage = await _lineLogin.GetTokenAsync(reqModel.Code);
            if (tokenMessage.StatusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Error occurred on token endpoint";
                return RedirectToAction("Index", "Error");
            }
            var tokenData = await _lineLogin.ParseJsonResponseAsync<LineTokenUrlResponse>(tokenMessage);
            if (tokenData!.IdToken == null)
            {
                TempData["ErrorMessage"] = "Scope \"openid\" is not provided";
                return RedirectToAction("Index", "Error");
            }
            
            // 解析 ID Token
            var idTokenData = _lineLogin.DecodeIdToken(tokenData.IdToken);
            
            // 取得 LINE User Profile
            var profileMessage = await _lineLogin.GetProfileAsync(tokenData.AccessToken);
            if (profileMessage.StatusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Error occurred on profile endpoint";
                return RedirectToAction("Index", "Error");
            }
            var profileData = await _lineLogin.ParseJsonResponseAsync<LineUserProfile>(profileMessage);
            if (profileData!.UserId == null)
            {
                TempData["ErrorMessage"] = "Invalid access_token";
                return RedirectToAction("Index", "Error");
            }
            
            // 異動資料庫
            var user = await _db.Users.FirstOrDefaultAsync(user => user.LineUserId == profileData.UserId);
            if (user == null)
            {
                // 新增使用者資料
                user = new User
                {
                    Nickname = profileData.DisplayName,
                    AvatarUrl = profileData.PictureUrl,
                    Role = "User",
                    LineUserId = profileData.UserId,
                    LineLoginAccessToken = tokenData.AccessToken,
                    LineLoginIdToken = tokenData.IdToken,
                    LineLoginRefreshToken = tokenData.RefreshToken
                };
                
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                // 更新使用者資料
                user.Nickname = profileData.DisplayName;
                user.AvatarUrl = profileData.PictureUrl;
                user.LineLoginAccessToken = tokenData.AccessToken;
                user.LineLoginIdToken = tokenData.IdToken;
                user.LineLoginRefreshToken = tokenData.RefreshToken;
                
                await _db.SaveChangesAsync();
            }

            await _assistant.SignInAsync(user);
            
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (_assistant.IsAuthenticated())
            {
                var userId = _assistant.GetUserId();
                var user = await _db.Users.FindAsync(userId);
                if (user != null)
                {
                    if (user.LineLoginAccessToken != null)
                    {
                        // 撤銷 LINE Login Access Token (即使 Access Token 已撤銷也會回傳 200)
                        var revokeMessage = await _lineLogin.RevokeTokenAsync(user.LineLoginAccessToken);
                        if (revokeMessage.StatusCode != HttpStatusCode.OK)
                        {
                            TempData["ErrorMessage"] = "Error occurred on revoke endpoint";
                            return RedirectToAction("Index", "Error");
                        }

                        user.LineLoginAccessToken = null;
                        user.LineLoginIdToken = null;
                        user.LineLoginRefreshToken = null;
                        await _db.SaveChangesAsync();
                    }
                }
            }
            
            await _assistant.SignOutAsync();
            
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
