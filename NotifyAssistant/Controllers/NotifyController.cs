using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyAssistant.Models.Entity;
using NotifyAssistant.Models.LineNotify;
using NotifyAssistant.Models.Service;
using System.Net;

namespace NotifyAssistant.Controllers
{
    [Authorize(Roles = "User, Admin")]
    public class NotifyController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAssistantService _assistant;
        private readonly AssistantDbContext _db;
        private readonly ILineNotifyService _lineNotify;

        public NotifyController(
            IHttpContextAccessor httpContextAccessor,
            IAssistantService assistant,
            AssistantDbContext db,
            ILineNotifyService lineNotify)
        {
            _httpContextAccessor = httpContextAccessor;
            _assistant = assistant;
            _db = db;
            _lineNotify = lineNotify;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isSubscribe = false;
            
            var userId = _assistant.GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.LineNotifyAccessToken != null)
            {
                // 呼叫 Status Endpoint
                var statusMessage = await _lineNotify.GetStatusAsync(user.LineNotifyAccessToken);
                if (statusMessage.StatusCode == HttpStatusCode.OK)
                {
                    isSubscribe = true;
                }
            }
            
            ViewBag.IsSubscribe = isSubscribe;

            return View();
        }
        
        [HttpGet]
        public IActionResult Subscribe()
        {
            var state = Guid.NewGuid().ToString();
            _httpContextAccessor.HttpContext.Session.SetString("LineNotifyState", state);
            
            var redirectUrl = _lineNotify.GetAuthorizeEndpoint(state);

            return Ok(new { redirectUrl });
        }
        
        [HttpGet]
        public async Task<IActionResult> SubscribeCallback([FromQuery] NotifyCallbackRequestViewModel reqModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            
            var state = _httpContextAccessor.HttpContext.Session.GetString("LineNotifyState");
            if (state == null || !state.Equals(reqModel.State))
            {
                TempData["ErrorMessage"] = "Invalid state";
                return RedirectToAction("Index", "Error");
            }
            
            // 呼叫 Token Endpoint
            var tokenMessage = await _lineNotify.GetTokenAsync(reqModel.Code);
            if (tokenMessage.StatusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Error occurred on token endpoint";
                return RedirectToAction("Index", "Error");
            }
            var tokenData = await _lineNotify.ParseJsonResponseAsync<NotifyTokenUrlResponse>(tokenMessage);
            
            var userId = _assistant.GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.LineNotifyAccessToken = tokenData.AccessToken;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unsubscribe()
        {
            var userId = _assistant.GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("請重新登入！");
            }
            
            if (user.LineNotifyAccessToken != null)
            {
                // 呼叫 Status Endpoint
                var statusMessage = await _lineNotify.GetStatusAsync(user.LineNotifyAccessToken);
                if (statusMessage.StatusCode == HttpStatusCode.OK)
                {
                    // 呼叫 Revoke Endpoint (Access Token 必須要存在才會回傳 200)
                    var revokeMessage = await _lineNotify.RevokeTokenAsync(user.LineNotifyAccessToken);
                    if (revokeMessage.StatusCode != HttpStatusCode.OK)
                    {
                        return BadRequest("Error occurred on revoke endpoint");
                    }
                }

                user.LineNotifyAccessToken = null;
                await _db.SaveChangesAsync();
            }
            
            return Ok();
        }
    }
}
