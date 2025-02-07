using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;

        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(null,includeProperties:"Category,CoverType");
            return View(productList);
        }
        public IActionResult Details(int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            } else
            {
                ShoppingCart Cart = new()
                {
                    Product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productId, includeProperties: "Category,CoverType"),
                    Count = 1,
                    ProductId = (int)productId
                };
                return View(Cart);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            if (cart == null)
            {
                return NotFound();
            }
            else
            {
                // find user
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    c => c.ApplicationUserId == claim.Value && c.ProductId == cart.ProductId);
                    

                if (cartFromDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(cart);
                } else
                {
                    _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, cart.Count);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index ));
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}