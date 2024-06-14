using Microsoft.AspNetCore.Mvc;
using OpenAI.FrontEnd.Models;
using OpenAI.FrontEnd.Services;
using System.Diagnostics;

namespace OpenAI.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOpenAIService _service;

        public HomeController(ILogger<HomeController> logger, IOpenAIService service)
        {
            _logger = logger;
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Prompt(string prompt)
        {
            return await CallAzureOpenAI(prompt);
        }

        private async Task<IActionResult> CallAzureOpenAI(string prompt, string instruction = null)
        {
            var answer = await _service.CallAzureOpenAI(prompt, instruction);

            return Json(answer);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Privacy(string prompt)
        {
            var instruction = "You are an assistant which will provide answers based on user profile. If the user is not a member of the financial are, you can't reply any financial information.";

            return await CallAzureOpenAI(prompt, instruction);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}