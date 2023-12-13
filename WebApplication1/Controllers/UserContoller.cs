using Contentful.Core;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserContoller(IContentfulClient client) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var users = await client.GetEntries<User>();
            return View(users);
        }

    }
}
