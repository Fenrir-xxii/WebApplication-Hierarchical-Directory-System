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
            loadDirectory(root);


            return View(root);
        }
        [HttpGet("/{id}")]
        public IActionResult IndexSub(int id)
        {
            if (id == 0)
            {
                return View("Index", new MyDirectory());
            }
            //var subfolder = GetById(id);
            var subfolder = _db.Directories.FirstOrDefault(x => x.Id == id);
            loadDirectory(subfolder);
            return View("Index", subfolder);
        }
        [HttpPost("/getParent/{id}")]
        public IActionResult GetParent(int id)
        {
            var current = _db.Directories.FirstOrDefault(x => x.Id == id);
            loadDirectory(current);
            if (current.Parent == null)
            {
                return View("Index", current);
            }
            var parent = current.Parent;
            loadDirectory(parent);
            return View("Index", parent);
        }

        public void loadDirectory(MyDirectory directory)
        {
            // set Directory Childrens
            var childrens = _db.Directories.Where(x => x.Parent.Id == directory.Id).OrderBy(x => x.Id).ToList();
            directory.Childrens.ToList().AddRange(childrens);
            directory.Childrens = directory.Childrens.OrderBy(x => x.Id).ToList();
            // set Parent
            directory.Parent = _db.Directories.FirstOrDefault(x => x.Id == directory.ParentId);
        }
        [HttpGet("/saveToFile")]
        public void saveToFile()
        {
            var data = _db.Directories.ToList();
            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            var filename = Guid.NewGuid().ToString() + ".json";

            System.IO.File.WriteAllText(Path.Combine(_webHostEnvironment.WebRootPath, "backups/", filename), json);
        }
        [HttpGet("/saveToFile")]
        public List<MyDirectory> readFile(string path)
        {
            return JsonConvert.DeserializeObject<List<MyDirectory>>(System.IO.File.ReadAllText(path));
        }
        [HttpGet("/loadFile")]
        public IActionResult loadFile()
        {
            // TO DO
            // 1-st export existing db to json and delete
            // 2nd inserting into db data from json file
            
            var data = readFile(@"data.json");
            var root = data.FirstOrDefault();

            return View("Index", root);
        }
        [HttpPost("/loadDirectoryTree")]
        public IActionResult loadFromFile([FromForm] IFormFile? file)
        {
            if(file != null)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                using (var f = System.IO.File.OpenWrite(Path.Combine(_webHostEnvironment.WebRootPath, "uploads/", filename)))
                {
                    file.CopyTo(f);
                }
                // CHANGE

                var data = readFile(Path.Combine(_webHostEnvironment.WebRootPath, "uploads/", filename));
                if(data != null)
                {
                    // TO DO
                    // 1-st export existing db to json and delete
                    // 2nd inserting into db data from json file
                    saveToFile();
                    _db.Database.ExecuteSqlRaw("TRUNCATE TABLE [Directories]");
                    foreach (var item in data)
                    {
                        var model = new MyDirectory
                        {
                            Title = item.Title,
                            ParentId = item.ParentId
                        };
                        _db.Add(model);
                        _db.SaveChanges();
                    }
                    //_db.SaveChanges();
                    var root = _db.Directories.FirstOrDefault(x => x.Parent == null);
                    loadDirectory(root);


                    return View("Index", root);
                }
                

                //var root = data.FirstOrDefault();
                //return View("Index", root);
                return View("Index", new MyDirectory());
            }
            // error loading/reading file
            return View("Index", new MyDirectory());
        }
    }
}
