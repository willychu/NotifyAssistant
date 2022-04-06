using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyAssistant.Models.Entity;
using NotifyAssistant.Models.Service;

namespace NotifyAssistant.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAssistantService _assistant;
        private readonly AssistantDbContext _db;

        public ProfileController(
            IAssistantService assistant,
            AssistantDbContext db)
        {
            _assistant = assistant;
            _db = db;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var userId = _assistant.GetUserId();
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            return View(user);
        }
    }
}
