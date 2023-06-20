using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CRISPR.Data;
using CRISPR.Models;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;

namespace CRISPR.Controllers
{
    public class DataSetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataSetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DataSets
        public async Task<IActionResult> Index()
        {
              return _context.DataSets != null ? 
                          View(await _context.DataSets.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.DataSets'  is null.");
        }

        // GET: DataSets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DataSets == null)
            {
                return NotFound();
            }

            var dataSet = await _context.DataSets
                .FirstOrDefaultAsync(m => m.id == id);
            if (dataSet == null)
            {
                return NotFound();
            }

            return View(dataSet);
        }

        // GET: DataSets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DataSets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,SubTitle,Description,RepositoryURL,Licenses,FileType,FileSize,FileURL,Accuracy")] DataSet dataSet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dataSet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dataSet);
        }

        // GET: DataSets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DataSets == null)
            {
                return NotFound();
            }

            var dataSet = await _context.DataSets.FindAsync(id);
            if (dataSet == null)
            {
                return NotFound();
            }
            return View(dataSet);
        }

        // POST: DataSets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Title,SubTitle,Description,RepositoryURL,Licenses,FileType,FileSize,FileURL,Accuracy")] DataSet dataSet)
        {
            if (id != dataSet.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dataSet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DataSetExists(dataSet.id))
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
            return View(dataSet);
        }

        // GET: DataSets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DataSets == null)
            {
                return NotFound();
            }

            var dataSet = await _context.DataSets
                .FirstOrDefaultAsync(m => m.id == id);
            if (dataSet == null)
            {
                return NotFound();
            }

            return View(dataSet);
        }

        // POST: DataSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DataSets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DataSets'  is null.");
            }
            var dataSet = await _context.DataSets.FindAsync(id);
            if (dataSet != null)
            {
                _context.DataSets.Remove(dataSet);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DataSetExists(int id)
        {
          return (_context.DataSets?.Any(e => e.id == id)).GetValueOrDefault();
        }

        // DownloadDataSet Action
        // DownloadDataSet Action
        [HttpGet]
        public async Task<IActionResult> DownloadDataSet(int? id)
        {
            if (id == null || _context.DataSets == null)
            {
                return NotFound();
            }

            var dataSet = await _context.DataSets
                .FirstOrDefaultAsync(m => m.id == id);
            if (dataSet == null)
            {
                return NotFound();
            }

            // Generate the PDF file
            dataSet.FileURL = GeneratePdfFile(dataSet);

            // Set up the file download
            var memory = new MemoryStream();
            using (var stream = new FileStream(dataSet.FileURL, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string fileName = $"DataSet_{dataSet.id}.pdf";
            return File(memory, "application/pdf", fileName);
        }

        private string GeneratePdfFile(DataSet dataSet)
        {
            string folderPath = "wwwroot/pdfs/";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string fileName = $"DataSet_{dataSet.id}.pdf";
            string filepath = System.IO.Path.Combine(folderPath, fileName);

            using (FileStream stream = new FileStream(filepath, FileMode.Create))
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                PageSize pageSize = PageSize.A4;
                Document document = new Document(pdfDocument, pageSize);

                // Add title
                Paragraph title = new Paragraph(dataSet.Title)
                    .SetFontSize(24)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(title);

                // Add subtitle
                Paragraph subtitle = new Paragraph(dataSet.SubTitle)
                    .SetFontSize(18)
                    .SetItalic()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(subtitle);

                // Add description
                Paragraph description = new Paragraph(dataSet.Description)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.JUSTIFIED);
                document.Add(description);

                // Add repository URL
                Paragraph repositoryURL = new Paragraph("Repository URL: " + dataSet.RepositoryURL)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(repositoryURL);

                // Add licenses
                Paragraph licenses = new Paragraph("Licenses: " + dataSet.Licenses)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(licenses);

                // Add accuracy
                Paragraph accuracy = new Paragraph("Accuracy: " + dataSet.Accuracy.ToString())
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
