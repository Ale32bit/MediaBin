using MediaBin.Data;
using MediaBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBin.Controllers
{
    public class HomeController : Controller
    {
        private FileStore _fileStore;
        private IConfiguration _configuration;

        public HomeController(FileStore fileStore, IConfiguration configuration)
        {
            _fileStore = fileStore;
            _configuration = configuration;
        }

        // GET: Files
        public async Task<IActionResult> Index()
        {
            var files = _fileStore.Files.OrderByDescending(m => m.CreationDate)
                .Where(m => m.Type.Contains("image") || m.Type.Contains("audio") || m.Type.Contains("video")); // Media types

            return View(files);
        }

        public async Task<IActionResult> List()
        {
            var files = _fileStore.Files.OrderByDescending(m => m.CreationDate);

            return View(files);
        }

        // GET: Files/Details/5
        public async Task<IActionResult> Details(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var file = _fileStore.Files
                .FirstOrDefault(m => m.FileName == fileName);
            if (file == null)
            {
                return NotFound();
            }

            return View(file);
        }

        // GET: Files/Create
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Files/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([Bind("FileName,File")] DTO.FileUpload userFile)
        {
            if (ModelState.IsValid)
            {
                var extension = Path.GetExtension(userFile.File.FileName);

                var name = GenerateFileName(extension);

                var localFile = new Models.File
                {
                    FileName = name,
                    Content = new byte[userFile.File.Length],
                };

                using var memoryStream = new MemoryStream();
                await userFile.File.CopyToAsync(memoryStream);
                localFile.Content = memoryStream.ToArray();

                await _fileStore.AddAsync(localFile);
                return RedirectToAction(nameof(Index));
            }
            return View(userFile);
        }

        // GET: Files/Delete/5
        public async Task<IActionResult> Delete(string? fileName)
        {
            if (fileName == null)
            {
                return NotFound();
            }

            var file = _fileStore.Files.FirstOrDefault(m => m.FileName == fileName);
            if (file == null)
            {
                return NotFound();
            }

            return View(file);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string fileName)
        {
            var file = _fileStore.Files.FirstOrDefault(m => m.FileName == fileName);
            _fileStore.Remove(file);
            return RedirectToAction(nameof(Index));
        }

        private bool FileExists(string fileName)
        {
            return _fileStore.Files.Any(e => e.FileName == fileName);
        }

        private string GenerateFileName(string ext = "")
        {
            string name;
            do
            {
                var code = Utils.GenerateRandomAlphaString(_configuration.GetValue<int>("RandomNameLength"));
                name = code + ext;
            } while (_fileStore.Files.Any(m => m.FileName == name));

            return name;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
