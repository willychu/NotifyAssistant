using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyAssistant.Models.Entity;
using NotifyAssistant.Models.LineNotify;
using NotifyAssistant.Models.Service;

namespace NotifyAssistant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AssistantDbContext _db;
        private readonly ILineNotifyService _lineNotify;

        public AdminController(
            AssistantDbContext db,
            ILineNotifyService lineNotify)
        {
            _db = db;
            _lineNotify = lineNotify;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> NotifyAll(NotifyContentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Error");
            }
            
            var tokenList = await _db.Users
                .Where(user => user.LineNotifyAccessToken != null)
                .Select(user => user.LineNotifyAccessToken)
                .ToListAsync();
            
            if (tokenList.Any())
            {
                foreach (var token in tokenList)
                {
                    await Task.Run(async () =>
                    {
                        // 呼叫 Status Endpoint
                        await _lineNotify.NotifyAsync(token, model.NotifyContent, model.NotifySilent);
                    });
                }
            }
            
            return RedirectToAction("NotifyCompleted");
        }
        
        [HttpGet]
        public IActionResult NotifyCompleted()
        {
            return View();
        }
    }
}
