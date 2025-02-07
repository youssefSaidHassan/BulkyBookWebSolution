using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order Can't Exactly Match Name.");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Category Added Successfuly";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                var category = _unitOfWork.Category.GetFirstOrDefault((System.Linq.Expressions.Expression<Func<Category, bool>>)(c => c.Id == id));
                return View(category);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order Can't Exactly Match Name.");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "Category Updated Successfuly";

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {

                var category = _unitOfWork.Category.GetFirstOrDefault((System.Linq.Expressions.Expression<Func<Category, bool>>)(c => c.Id == id));
                return View(category);
            }
        }
        [HttpPost ]
        [ValidateAntiForgeryToken]

        public IActionResult DeletePost(int? id)
        {
            var category = _unitOfWork.Category.GetFirstOrDefault((c => c.Id == id));
            if (category == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.Category.Remove(category);
                _unitOfWork.Save();
                TempData["success"] = "Category Deleted Successfuly";

                return RedirectToAction("Index");
            }
        }
    }
}
