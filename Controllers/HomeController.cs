using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using migration_accelerator.Models;

namespace migration_accelerator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger,IOptions<AppSettings> options,IConfiguration config)
        {
            _logger = logger;
            _appSettings = options.Value;
             _config = config;
        }

        public IActionResult Index()
        {
          
        var model = new AppSettings
        {
            ConnectionString = _config.GetConnectionString("DefaultConnection"),
            OnBaseURL = _appSettings.OnBaseURL,
            GC_OnBaseURL = _appSettings.GC_OnBaseURL,
            ShowXMLs = _appSettings.ShowXMLs,
            ShowSimulator = _appSettings.ShowSimulator,
            LogDocumentsInDB = _appSettings.LogDocumentsInDB
        };

        return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
