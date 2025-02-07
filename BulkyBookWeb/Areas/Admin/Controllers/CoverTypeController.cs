using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> coverTypes = _unitOfWork.CoverType.GetAll();
            return View(coverTypes);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Create(CoverType cover)
        {
            if (cover != null)
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.CoverType.Add(cover);
                    _unitOfWork.Save();
                    TempData["success"] = "Cover Updated Successfuly";

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return View(cover);
                }
            }
            else
            {
                return NotFound();
            }
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
                var cover = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
                return View(cover);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType cover)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(cover);
                _unitOfWork.Save();
                TempData["success"] = "Cover Updated Successfuly";

                return RedirectToAction(nameof(Index));
            }
            return View(cover);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                var cover = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
                return View(cover);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var cover = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            if (cover == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.CoverType.Remove(cover);
                _unitOfWork.Save();
                TempData["success"] = "Cover Deleted Successfuly";

                return RedirectToAction("Index");
            }
        }

    }
}
