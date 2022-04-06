using Microsoft.AspNetCore.Mvc;
using NotifyAssistant.Models.Entity;

namespace NotifyAssistant.Controllers
{
    public class HomeController : Controller
    {
        private readonly AssistantDbContext _db;

        public HomeController(AssistantDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var count = _db.Users.Count();
            ViewBag.Count = count;
            return View();
        }
    }
}