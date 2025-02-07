using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Company company = new();
            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company )
        {
            if (ModelState.IsValid)
            {

                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                    TempData["success"] = "Company Created Successfuly";
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company Updated Successfuly";
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

    
   
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var company = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                return Json(new { success = false , message = "Error While Deleting" }) ;
            }
            else
            {
                
                _unitOfWork.Company.Remove(company);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete Successful" });
            }
        }



        #endregion
    }

}
