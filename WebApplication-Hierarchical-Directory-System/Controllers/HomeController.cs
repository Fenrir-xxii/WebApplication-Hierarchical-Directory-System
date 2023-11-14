using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Xml.Linq;
using WebApplication_Hierarchical_Directory_System.Models;

namespace WebApplication_Hierarchical_Directory_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly DirectoryContext _db;
        public HomeController(DirectoryContext db)
        {
            _db = db;
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

            //write string to file
            System.IO.File.WriteAllText(@"data.json", json);
        }
    }
}
