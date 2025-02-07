using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoopingCartViewModel ShoopingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoopingCartVM = new ShoopingCartViewModel()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };


            foreach (var cart in ShoopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count,
                    cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                ShoopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }


            return View(ShoopingCartVM);
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            ShoopingCartVM = new ShoopingCartViewModel()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoopingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value);

            ShoopingCartVM.OrderHeader.Name = ShoopingCartVM.OrderHeader.ApplicationUser.Name;
            ShoopingCartVM.OrderHeader.PhoneNumber = ShoopingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoopingCartVM.OrderHeader.City = ShoopingCartVM.OrderHeader.ApplicationUser.City;
            ShoopingCartVM.OrderHeader.StreetAddress = ShoopingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoopingCartVM.OrderHeader.State = ShoopingCartVM.OrderHeader.ApplicationUser.State;
            ShoopingCartVM.OrderHeader.PostalCode = ShoopingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count,
                    cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                ShoopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }


            return View(ShoopingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoopingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");

           
            ShoopingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoopingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(a => a.Id == claim.Value);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoopingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoopingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            } 
            else
            {
                ShoopingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoopingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            foreach (var cart in ShoopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count,
                    cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                ShoopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }

             
            _unitOfWork.OrderHeader.Add(ShoopingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoopingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoopingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {

                    //Stripe setting 
                    var domain = "https://localhost:44337/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    LineItems = new List<SessionLineItemOptions>(), // items in shopping cart
                    Mode = "payment",
                    SuccessUrl = domain+$"customer/cart/OrderConfirmation?id={ShoopingCartVM.OrderHeader.Id}",
                    CancelUrl = domain+$"customer/cart/index",
                };
                foreach (var item in ShoopingCartVM.ListCart)
                {

                    var sessionLineItem =  new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount =(long)(item.Price *100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title,
                            },
                        },
                        Quantity = item.Count,

                    };
                    options.LineItems.Add(sessionLineItem);
                }   
                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStripePayment(ShoopingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation" , "Cart" , new { id = ShoopingCartVM.OrderHeader.Id});
            }
                

            // end stripe

            
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                _unitOfWork.OrderHeader.UpdateStripePayment(ShoopingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            } 
            // remove shooping cart 
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                c=>c.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }
            }
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);

            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

    }
}
