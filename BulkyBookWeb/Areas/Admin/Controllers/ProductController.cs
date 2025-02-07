using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;  
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CagtegoryList = _unitOfWork.Category.GetAll().Select(
                c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(
                c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
            };
            if (id == null || id == 0)
            {
                //Create Product
                return View(productVM);

            }
            else
            {
                //update Product
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM , IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    // in update state => get old image and delete it
                    if(productVM.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }


                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Product Created Successfuly";
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Product Updated Successfuly";
                }

                _unitOfWork.Save();
                TempData["success"] = "Product Added Successfuly";
                return RedirectToAction(nameof(Index));
            }

            return View(productVM);
        }

    
   
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAllProduct()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = products });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
            if (product == null)
            {
                return Json(new { success = false , message = "Errpr While Deleting" }) ;
            }
            else
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete Successful" });
            }
        }



        #endregion
    }

}
