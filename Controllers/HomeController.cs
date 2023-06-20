using CRISPR.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1;
using System.Diagnostics;

namespace CRISPR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult UploadSequence()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadSequence(DNASequence model, IFormFile file)
        {
            if (file != null)
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    model.Sequence = await reader.ReadToEndAsync();
                }
                model.FileName = file.FileName;
            }

            if (ModelState.IsValid)
            {
                // Save the uploaded file or text to a directory
                string filePath = Path.Combine("path/to/your/directory", model.FileName);
                await System.IO.File.WriteAllTextAsync(filePath, model.Sequence);
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}