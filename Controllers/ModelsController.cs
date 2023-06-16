using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISPR.Data;
using CRISPR.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;

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
        private string GeneratePdfFile(Model model)
        {
            string folderPath = "wwwroot/pdfs/";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"Model_{model.id}.pdf";
            string filePath = System.IO.Path.Combine(folderPath, fileName);

            using (var writer = new PdfWriter(filePath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf, PageSize.A4);
                    var pageSize = PageSize.A4;

                    // Set up fonts, colors, and background
                    var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var subTitleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var textFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    var titleColor = new DeviceRgb(30, 144, 255); // Dodger Blue
                    var subtitleColor = new DeviceRgb(70, 130, 180); // Steel Blue
                    var backgroundColor = new DeviceRgb(240, 248, 255); // Alice Blue

                    // Add background color
                    var background = new Rectangle(0, 0, pageSize.GetWidth(), pageSize.GetHeight());
                    var pdfPage = pdf.AddNewPage();
                    var pdfCanvas = new PdfCanvas(pdfPage);
                    pdfCanvas.SetFillColor(backgroundColor).Rectangle(background).Fill();
                    var backgroundCanvas = new Canvas(pdfPage, pdfPage.GetMediaBox());
                    // Add title
                    var title = new Paragraph(model.Title)
                        .SetFont(titleFont)
                        .SetFontSize(24)
                        .SetFontColor(titleColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginTop(50)
                        .SetMarginBottom(10);
                    document.Add(title);

                    // Add subtitle
                    var subTitle = new Paragraph(model.SubTitle)
                        .SetFont(subTitleFont)
                        .SetFontSize(18)
                        .SetFontColor(subtitleColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20);
                    document.Add(subTitle);

                    // Add description
                    var description = new Paragraph(model.Description)
                        .SetFont(textFont)
                        .SetFontSize(12)
                        .SetTextAlignment(TextAlignment.JUSTIFIED)
                        .SetMarginBottom(20);
                    document.Add(description);

                    // Add repository URL
                    var repositoryUrl = new Paragraph($"Repository URL: {model.RepositoryURL}")
                        .SetFont(textFont)
                        .SetFontSize(12)
                        .SetMarginBottom(10);
                    document.Add(repositoryUrl);

                    // Add licenses
                    var licenses = new Paragraph($"Licenses: {model.Licenses}")
                        .SetFont(textFont)
                        .SetFontSize(12)
                        .SetMarginBottom(10);
                    document.Add(licenses);

                    // Add accuracy
                    var accuracy = new Paragraph($"Accuracy: {model.Accuracy}")
                        .SetFont(textFont)
                        .SetFontSize(12)
                        .SetMarginBottom(10);
                    document.Add(accuracy);

                    // Add footer
                    var footer = new Paragraph("Generated by CRISPR")
                        .SetFont(textFont)
                        .SetFontSize(10)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetMarginTop(30)
                        .SetMarginRight(30)
                        .SetFixedPosition(pageSize.GetWidth() - 150, 30, 120);
                    document.Add(footer);

                    document.Close();
                }
            }

            return filePath.Replace("wwwroot", "");
        }

    }
}
