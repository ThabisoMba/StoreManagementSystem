using StoreManagementApp.Models;
using System.Linq;
using System.Web.Mvc;

namespace StoreManagementApp.Controllers
{
    public class HomeController : Controller
    {
        private StoreManagementDBEntities db = new StoreManagementDBEntities();

        // =============================================
        // DASHBOARD PAGE
        // =============================================
        public ActionResult Index()
        {
            // TOTAL COUNTS
            ViewBag.TotalCustomers = db.Customers.Count();

            ViewBag.TotalProducts = db.Products.Count();

            ViewBag.TotalOrders = db.Orders.Count();

            ViewBag.TotalCategories = db.Categories.Count();

            // TOTAL REVENUE
            ViewBag.TotalRevenue =
                db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // CATEGORIES
            ViewBag.Categories = db.Categories
                                   .OrderBy(c => c.CategoryName)
                                   .ToList();

            // PRODUCTS
            ViewBag.Products = db.Products
                                 .Include("Category")
                                 .OrderBy(p => p.ProductName)
                                 .ToList();

            // CUSTOMERS
            ViewBag.Customers = db.Customers
                                  .OrderBy(c => c.LastName)
                                  .ToList();

            // ORDERS
            ViewBag.Orders = db.Orders
                               .Include("Customer")
                               .OrderByDescending(o => o.OrderDate)
                               .ToList();

            return View();
        }

        // ABOUT PAGE
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        // CONTACT PAGE
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}