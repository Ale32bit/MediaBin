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
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var file = _fileStore.Files
                .FirstOrDefault(m => m.FileName == id);
            if (file == null)
            {
                return NotFound();
            }

            ViewData["FriendlySize"] = Utils.FriendlySize(file.Size);

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
            if(userFile.File == null)
            {
                ModelState.AddModelError("File", "Please select a file.");
            }

            if (ModelState.IsValid)
            {
                var extension = Path.GetExtension(userFile.File.FileName);

                string name = GenerateFileName(extension);
                if(!string.IsNullOrWhiteSpace(userFile.FileName))
                {
                    var invalids = Path.GetInvalidFileNameChars();
                    name = string.Join("_", userFile.FileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                }

                var localFile = new Models.File
                {
                    FileName = name,
                    Content = new byte[userFile.File.Length],
                };

                using var memoryStream = new MemoryStream();
                await userFile.File.CopyToAsync(memoryStream);
                localFile.Content = memoryStream.ToArray();

                await _fileStore.AddAsync(localFile);
                return Redirect("/" + name);
            }
            return View(userFile);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var file = _fileStore.Files
                .FirstOrDefault(m => m.FileName == id);
            if (file == null)
            {
                return NotFound();
            }

            return View(file);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Edit(string id, [Bind("FileName")] Models.File file)
        {
            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                ModelState.AddModelError("File", "Please select a file.");
            }

            if(ModelState.IsValid)
            {

            }

            return RedirectToAction("Details", new { id = file.FileName });
        }

        // GET: Files/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var file = _fileStore.Files.FirstOrDefault(m => m.FileName == id);
            if (file == null)
            {
                return NotFound();
            }

            return View(file);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var file = _fileStore.Files.FirstOrDefault(m => m.FileName == id);
            _fileStore.Remove(file);
            return RedirectToAction(nameof(Index));
        }

        private bool FileExists(string id)
        {
            return _fileStore.Files.Any(e => e.FileName == id);
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
