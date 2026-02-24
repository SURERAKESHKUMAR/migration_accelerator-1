using Microsoft.AspNetCore.Mvc;
using migration_accelerator.Models;

namespace migration_accelerator.Controllers
{
    public class RepositoryController : Controller
    {
        [HttpGet]
        public IActionResult Create()
        {
            var model = new RepositoryViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(RepositoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Here you can save to database or call an API
                TempData["Success"] = $"Repository '{model.RepoName}' created with branches: {string.Join(", ", model.SelectedBranches)}";
                return RedirectToAction("Create");
            }

            return View(model);
        }
    }
}
