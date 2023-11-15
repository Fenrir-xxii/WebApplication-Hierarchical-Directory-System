using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using WebApplication_Hierarchical_Directory_System.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_Hierarchical_Directory_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly DirectoryContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(DirectoryContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _webHostEnvironment = hostEnvironment;
        }
        [HttpGet("/")]
        public IActionResult Index()
        {
            var root = _db.Directories.FirstOrDefault(x => x.Parent == null);

            if (root == null)
            {
                return View(new MyDirectory());
            }
            loadDirectoryProps(root);

            return View(root);
        }
        [HttpGet("/{id}")]
        public IActionResult IndexSub(int id)
        {
            if (id == 0)
            {
                return View("Index", new MyDirectory());
            }

            var subfolder = _db.Directories.FirstOrDefault(x => x.Id == id);
            if (subfolder == null)
            {
                return View(new MyDirectory());
            }
            loadDirectoryProps(subfolder);

            return View("Index", subfolder);
        }
        [HttpPost("/getParent/{id}")]
        public IActionResult GetParent(int id)
        {
            var current = _db.Directories.FirstOrDefault(x => x.Id == id);
            if (current == null)
            {
                return View(new MyDirectory());
            }
            loadDirectoryProps(current);

            if (current.Parent == null)
            {
                return View("Index", current);
            }
            var parent = current.Parent;
            loadDirectoryProps(parent);

            return View("Index", parent);
        }
        [HttpPost("/loadDirectoryTree")]
        public IActionResult loadFromFile([FromForm] IFormFile? file)
        {
            if (file != null)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                using (var f = System.IO.File.OpenWrite(Path.Combine(_webHostEnvironment.WebRootPath, "uploads/", filename)))
                {
                    file.CopyTo(f);
                }

                var data = readFile(Path.Combine(_webHostEnvironment.WebRootPath, "uploads/", filename));
                if (data != null)
                {
                    // export existing db data to json and delete data from db
                    saveToFile();
                    _db.Database.ExecuteSqlRaw("TRUNCATE TABLE [Directories]");
                    var orderedList = data.OrderBy(x => x.Id);
                    foreach (var item in orderedList)
                    {
                        var model = new MyDirectory
                        {
                            Title = item.Title,
                            ParentId = item.ParentId
                        };
                        // inserting into db data from json file
                        _db.Add(model);
                        _db.SaveChanges(); // must be here because of foreign key
                    }
                    //_db.SaveChanges();
                    var root = _db.Directories.FirstOrDefault(x => x.Parent == null);
                    if (root == null)
                    {
                        return View("Index", new MyDirectory());
                    }
                    loadDirectoryProps(root);

                    return View("Index", root);
                }

                return View("Index", new MyDirectory());
            }

            return View("Index", new MyDirectory());
        }
        public void saveToFile()
        {
            var data = _db.Directories.AsNoTracking().ToList();
            if (data.Count > 0)
            {
                string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });

                var filename = Guid.NewGuid().ToString() + ".json";

                System.IO.File.WriteAllText(Path.Combine(_webHostEnvironment.WebRootPath, "backups/", filename), json);
            }
        }
        public List<MyDirectory> readFile(string path)
        {
            var result = new List<MyDirectory>();
            try
            {
                var tryResult = JsonConvert.DeserializeObject<List<MyDirectory>>(System.IO.File.ReadAllText(path));
                if (tryResult != null)
                {
                    result = tryResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;

        }
        public void loadDirectoryProps(MyDirectory directory)
        {
            if (directory == null)
            {
                return;
            }
            // set Directory Childrens
            var childrens = _db.Directories.Where(x => x.Parent.Id == directory.Id).ToList();
            directory.Childrens.ToList().AddRange(childrens);
            directory.Childrens = directory.Childrens.OrderBy(x => x.Id).ToList();
            // set Directory Parent
            directory.Parent = _db.Directories.FirstOrDefault(x => x.Id == directory.ParentId);
        }
    }
}
