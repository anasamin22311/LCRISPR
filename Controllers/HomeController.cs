using CRISPR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
        public IActionResult Result()
        {
            return View();
        }
        [HttpGet]
        public IActionResult UploadSequence()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadSequence(DNASequence model)
        {
            // Check if either a file or a sequence is provided, but not both
            if ((model.File != null && !string.IsNullOrEmpty(model.Sequence)) || (model.File == null && string.IsNullOrEmpty(model.Sequence)))
            {
                ModelState.AddModelError("", "Please either paste a DNA sequence or upload a file, but not both.");
                return View(model);
            }

            if (model.File != null)
            {
                using (var reader = new StreamReader(model.File.OpenReadStream()))
                {
                    model.Sequence = await reader.ReadToEndAsync();
                }
                model.FileName = model.File.FileName;
            }
            else
            {
                model.FileName = "sequence.txt";
            }

            // Save the uploaded file or text to a directory
            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            Directory.CreateDirectory(rootPath);
            string filePath = Path.Combine(rootPath, model.FileName);
            await System.IO.File.WriteAllTextAsync(filePath, model.Sequence);

            //Output
            string CrisprOut = "XR_001828562.2,PREDICTED: Cebus imitator cellular tumor antigen p53-like,...";
            var crisList = CrisprOut.Split(",");

            CrisprOutViewModel crisprOutViewModel = new CrisprOutViewModel
            {
                Name = crisList[0],
                Details = crisList[1],
                Sequence = crisList[2],
                Location = crisList[3],
                GRNA = crisList[4]
            };
            // Send the file to the external API
            //await SendFileToExternalApi(filePath);

            return View("Result", crisprOutViewModel);
        }

        //private async Task SendFileToExternalApi(string filePath)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("https://your.external.api/endpoint/");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        using (var content = new MultipartFormDataContent())
        //        {
        //            byte[] fileData = await System.IO.File.ReadAllBytesAsync(filePath);
        //            content.Add(new ByteArrayContent(fileData, 0, fileData.Length), "dnaFile", Path.GetFileName(filePath));

        //            HttpResponseMessage response = await client.PostAsync("api/YourResource", content);
        //            response.EnsureSuccessStatusCode();
        //        }
        //   }
    }
}
