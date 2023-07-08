using CRISPR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1;

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
            List<Models.Results> results = new();

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5000);

            if (model.File == null && string.IsNullOrEmpty(model.Sequence))
            {
                ModelState.AddModelError("", "Please either paste a DNA sequence or upload a file, but not both.");
                return View(model);
            }

            if (model.File != null)
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                using (Stream stream = model.File.OpenReadStream())
                {
                    StreamContent stream_content = new StreamContent(stream);
                    content.Add(stream_content, "file", model.File.FileName);
                    HttpResponseMessage response = await client.PostAsync("http://localhost:5000/fasta", content);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResults = await response.Content.ReadAsStringAsync();
                        if (jsonResults != null)
                        {
                            results = JsonConvert.DeserializeObject<List<Models.Results>>(jsonResults);
                            if (results != null)
                            {
                                var jsonString = JsonConvert.SerializeObject(results);
                                return Json(jsonString);

                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something wrong happend sorry.");
                        return View(model);
                    }
                }
            }
            else
            {
                var query = new { name = "Query", sequence = model.Sequence };
                string json = JsonConvert.SerializeObject(query);
                StringContent string_content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/dna", string_content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResults = await response.Content.ReadAsStringAsync();
                    if (jsonResults != null)
                    {
                        results = JsonConvert.DeserializeObject<List<Models.Results>>(jsonResults);
                        if (results != null)
                        {
                            var jsonString = JsonConvert.SerializeObject(results);
                            return Json(jsonString);

                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Something wrong happend sorry.");
                    return View(model);
                }
            }
            //string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            //Directory.CreateDirectory(rootPath);
            //string filePath = Path.Combine(rootPath, model.FileName);
            //await System.IO.File.WriteAllTextAsync(filePath, model.Sequence);
            return View("Result", results);
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
