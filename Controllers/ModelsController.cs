using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISPR.Data;
using CRISPR.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Geom;

namespace CRISPR.Controllers
{
    public class ModelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModelsController> _logger;

        public ModelsController(ApplicationDbContext context, ILogger<ModelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Models
        public async Task<IActionResult> Index()
        {
            return _context.Models != null ? 
                View(await _context.Models.ToListAsync()) :
                Problem("Entity set 'ApplicationDbContext.Models'  is null.");
        }

        // GET: Models/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Models == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .FirstOrDefaultAsync(m => m.id == id);
            if (model == null)
            {
                return NotFound();
            }
            model.FileURL = GeneratePdfFile(model);

            return View(model);
        }

        // GET: Models/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Models/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Model model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                _logger.LogInformation("Model ID before saving: {ModelId}", model.id);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Model ID after saving: {ModelId}", model.id);

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Models/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Models == null)
            {
                return NotFound();
            }

            var model = await _context.Models.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        // POST: Models/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Title,SubTitle,Description,RepositoryURL,Licenses,Accuracy")] Model model)
        {
            if (id != model.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModelExists(model.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Models/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Models == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .FirstOrDefaultAsync(m => m.id == id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Models/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Models == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Models'  is null.");
            }
            var model = await _context.Models.FindAsync(id);
            if (model != null)
            {
                _context.Models.Remove(model);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModelExists(int id)
        {
          return (_context.Models?.Any(e => e.id == id)).GetValueOrDefault();
        }

        // DownloadModel Action
        [HttpGet]
        public async Task<IActionResult> DownloadModel(int? id)
        {
            if (id == null || _context.Models == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .FirstOrDefaultAsync(m => m.id == id);
            if (model == null)
            {
                return NotFound();
            }

            // Generate the PDF file
            model.FileURL = GeneratePdfFile(model);

            // Set up the file download
            var memory = new MemoryStream();
            using (var stream = new FileStream(model.FileURL, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string fileName = $"Model_{model.id}.pdf";
            return File(memory, "application/pdf", fileName);
        }
        private string GeneratePdfFile(Model model)
        {
            string folderPath = "wwwroot/pdfs/";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string fileName = $"Model_{model.id}.pdf";
            string filepath = System.IO.Path.Combine(folderPath, fileName);

            using (FileStream stream = new FileStream(filepath, FileMode.Create))
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                PageSize pageSize = PageSize.A4;
                Document document = new Document(pdfDocument, pageSize);

                // Add title
                Paragraph title = new Paragraph(model.Title)
                    .SetFontSize(24)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(title);

                // Add subtitle
                Paragraph subtitle = new Paragraph(model.SubTitle)
                    .SetFontSize(18)
                    .SetItalic()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(subtitle);

                // Add description
                Paragraph description = new Paragraph(model.Description)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.JUSTIFIED);
                document.Add(description);

                // Add repository URL
                Paragraph repositoryURL = new Paragraph("Repository URL: " + model.RepositoryURL)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(repositoryURL);

                // Add licenses
                Paragraph licenses = new Paragraph("Licenses: " + model.Licenses)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(licenses);

                // Add accuracy
                Paragraph accuracy = new Paragraph("Accuracy: " + model.Accuracy.ToString())
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(accuracy);
                // Close the document
                document.Close();
            }

            return filepath;
        }
    }
}