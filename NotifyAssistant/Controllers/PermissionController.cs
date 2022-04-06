using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyAssistant.Models.Entity;
using NotifyAssistant.Models.Service;

namespace NotifyAssistant.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly IAssistantService _assistant;
        private readonly AssistantDbContext _db;

        public PermissionController(
            IAssistantService assistant,
            AssistantDbContext db)
        {
            _assistant = assistant;
            _db = db;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetPermission()
        {
            var userId = _assistant.GetUserId();
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return BadRequest();
            }

            user.Role = "Admin";
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult CancelPermission()
        {
            var userId = _assistant.GetUserId();
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return BadRequest();
            }

            user.Role = "User";
            _db.SaveChanges();

            return Ok();
        }
    }
}
