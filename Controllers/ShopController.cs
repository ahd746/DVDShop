using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DVDShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace DVDShop.Controllers
{
    public class ShopController : Controller
    {
        private readonly DVDShopContext _context;

        private readonly IConfiguration _configuration;

        public ShopController(DVDShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            var categories = _context.Category.OrderBy(c => c.Name).ToList();
            return View(categories);
        }
        public IActionResult Browse(string category)
        {
            ViewBag.Category = category;
            var products = _context.Product.Where(p => p.Category.Name == category).OrderBy(p => p.Name).ToList();
            return View(products);
        }

        public IActionResult ProductDetails(string product)
        {
            var selectedProduct = _context.Product.SingleOrDefault(p => p.Name == product);
            return View(selectedProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int Quantity, int ProductId)
        {
            var product = _context.Product.SingleOrDefault(p => p.ProductId == ProductId);
            var price = product.Price;
            var cartUsername = GetCartUserName();

            var cartItem = _context.Cart.SingleOrDefault(c => c.ProductId == ProductId && c.Username == cartUsername);
            if (cartItem == null)
            {
                var cart = new Cart
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    Username = cartUsername
                };
                _context.Cart.Add(cart);
            }
            else
            {
                cartItem.Quantity += Quantity;
                _context.Update(cartItem);
            }
            _context.SaveChanges();
            return RedirectToAction("Cart");

        }
        private String GetCartUserName()
        {
            if (HttpContext.Session.GetString("CartUsername") == null)
            {
                var cartUsername = "";
                if (User.Identity.IsAuthenticated)
                {
                    cartUsername = User.Identity.Name;
                }
                else
                {
                    cartUsername = Guid.NewGuid().ToString();
                }
                HttpContext.Session.SetString("CartUsername", cartUsername);
            }
            return HttpContext.Session.GetString("CartUsername");
        }

        public IActionResult Cart()
        {
            var cartUsername = GetCartUserName();
            var cartItems = _context.Cart.Include(c => c.Product).Where(c => c.Username == cartUsername).ToList();
            return View(cartItems);
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = _context.Cart.SingleOrDefault(c => c.CartId == id);
            _context.Cart.Remove(cartItem);
            _context.SaveChanges();
            return RedirectToAction("Cart");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Models.Order order)
        {
            order.OrderDate = DateTime.Now;
            order.UserId = User.Identity.Name;

            var cartItems = _context.Cart.Where(c => c.Username == User.Identity.Name);
            decimal cartTotal = (from c in cartItems
                                 select c.Quantity * c.Price).Sum();
            order.Total = cartTotal;
            HttpContext.Session.SetString("cartTotal", cartTotal.ToString());
            HttpContext.Session.SetObject("Order", order);
            return RedirectToAction("Payment");
        }

        public IActionResult Payment()
        {
            var order = HttpContext.Session.GetObject<Models.Order>("Order");
            ViewBag.Total = order.Total;
            ViewBag.CentsTotal = order.Total * 100;
            ViewBag.PublishableKey = _configuration.GetSection("Stripe")["PublishableKey"];
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Payment(String stripeEmail, string stripeToken)
        {
            StripeConfiguration.ApiKey = _configuration.GetSection("Stripe")["SecretKey"];
            var cartUsername = HttpContext.Session.GetString("CartUsername");
            var cartItems = _context.Cart.Where(c => c.Username == cartUsername);
            var order = HttpContext.Session.GetObject<Models.Order>("Order");

            var customerService = new CustomerService();
            var chargeService = new ChargeService();
            var customer = customerService.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            _context.Order.Add(order);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetail.Add(orderDetail);
            }
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                _context.Cart.Remove(item);
            }

            _context.SaveChanges();

            return RedirectToAction("Details", "Orders", new { id= order.OrderId});
        }

    }
}

