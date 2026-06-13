using StoreManagementApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace StoreManagementApp.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Orders
        // Shared database context for all actions in this controller
        private StoreManagementDBEntities db = new StoreManagementDBEntities();
        public ActionResult Index(string statusFilter)
        {
            // LINQ: load all orders and include customer details, newest first
            var orders = db.Orders
                           .Include("Customer")
                           .OrderByDescending(o => o.OrderDate)
                           .AsQueryable();

            // LINQ filter: show only orders matching the selected status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.Status == statusFilter);
                ViewBag.StatusFilter = statusFilter;
            }

            // LINQ: build a distinct list of statuses for the filter dropdown
            ViewBag.StatusList = db.Orders
                                   .Select(o => o.Status)
                                   .Distinct()
                                   .OrderBy(s => s)
                                   .ToList();

            return View(orders.ToList());
        }

        // =============================================
        // DETAILS - Full order view with all items
        // =============================================

        /// <summary>
        /// GET: /Orders/Details/5
        /// Loads a complete order record including:
        /// - The customer who placed it
        /// - All order line items
        /// - The product details for each line item
        /// Uses LINQ with nested Include() for deep eager loading.
        /// </summary>
        /// <param name="id">The OrderID to look up</param>
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // LINQ: load the order and eagerly include Customer, OrderItems,
            // and the Product linked to each OrderItem (3-level join)
            Order order = db.Orders
                            .Include("Customer")
                            .Include("OrderItems.Product")
                            .Where(o => o.OrderID == id)
                            .FirstOrDefault();

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // =============================================
        // CREATE - Show new order form
        // =============================================

        /// <summary>
        /// GET: /Orders/Create
        /// Displays a blank order creation form.
        /// Uses LINQ to populate the customer dropdown list.
        /// </summary>
        public ActionResult Create()
        {
            // LINQ: load customers sorted by surname for the dropdown
            PopulateCustomerDropdown();
            PopulateStatusList();
            return View();
        }

        // =============================================
        // CREATE - Save new order
        // =============================================

        /// <summary>
        /// POST: /Orders/Create
        /// Validates and saves a new order header to the database.
        /// TotalAmount starts at 0 and increases as items are added.
        /// </summary>
        /// <param name="order">Order object bound from the HTML form</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerID,Status,ShippingAddress")] Order order)
        {
            if (ModelState.IsValid)
            {
                // Set the order date to the current date and time
                order.OrderDate = DateTime.Now;

                // Total starts at zero; increases when order items are added
                order.TotalAmount = 0;

                db.Orders.Add(order);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            PopulateCustomerDropdown(order.CustomerID);
            PopulateStatusList(order.Status);
            return View(order);
        }

        // =============================================
        // EDIT - Show pre-filled order form
        // =============================================

        /// <summary>
        /// GET: /Orders/Edit/5
        /// Loads an existing order and displays it in the edit form.
        /// Allows updating the order status and shipping address.
        /// </summary>
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Order order = db.Orders.Find(id);

            if (order == null)
                return HttpNotFound();

            PopulateCustomerDropdown(order.CustomerID);
            PopulateStatusList(order.Status);
            return View(order);
        }

        // =============================================
        // EDIT - Save updated order
        // =============================================

        /// <summary>
        /// POST: /Orders/Edit/5
        /// Saves changes to an existing order record in the database.
        /// Marks the entity as Modified so EF generates an UPDATE statement.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,CustomerID,OrderDate,TotalAmount,Status,ShippingAddress")] Order order)
        {
            if (ModelState.IsValid)
            {
                // Mark entity as Modified to trigger a SQL UPDATE
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            PopulateCustomerDropdown(order.CustomerID);
            PopulateStatusList(order.Status);
            return View(order);
        }

        // =============================================
        // DELETE - Show confirmation page
        // =============================================

        /// <summary>
        /// GET: /Orders/Delete/5
        /// Shows the order details and requests confirmation before deleting.
        /// Includes the customer name for clear identification.
        /// </summary>
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // LINQ: include customer name on the confirmation page
            Order order = db.Orders
                            .Include("Customer")
                            .Where(o => o.OrderID == id)
                            .FirstOrDefault();

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // =============================================
        // DELETE - Confirm and remove order
        // =============================================

        /// <summary>
        /// POST: /Orders/Delete/5
        /// Permanently removes the order from the database.
        /// Because OrderItems has ON DELETE CASCADE, all linked
        /// order items are automatically deleted by SQL Server as well.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Find the order by primary key
            Order order = db.Orders.Find(id);

            // Remove order — SQL Server CASCADE will also delete its OrderItems
            db.Orders.Remove(order);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // =============================================
        // PRIVATE HELPERS - Populate dropdowns
        // =============================================

        /// <summary>
        /// Helper method: uses LINQ to build a SelectList of all customers
        /// (showing "FirstName LastName") and stores it in ViewBag.CustomerID.
        /// </summary>
        /// <param name="selectedCustomerId">Customer to pre-select in the dropdown</param>
        private void PopulateCustomerDropdown(object selectedCustomerId = null)
        {
            // LINQ: get customers and build a full name for the dropdown
            var customers = db.Customers
                               .OrderBy(c => c.LastName)
                               .ThenBy(c => c.FirstName)
                               .ToList()
                               .Select(c => new
                               {
                                   c.CustomerID,
                                   FullName = c.FirstName + " " + c.LastName
                               });

            ViewBag.CustomerID = new SelectList(customers, "CustomerID", "FullName", selectedCustomerId);
        }

        /// <summary>
        /// Helper method: provides a fixed list of valid order statuses
        /// for the Status dropdown on the Create and Edit forms.
        /// </summary>
        /// <param name="selectedStatus">Status to pre-select in the dropdown</param>
        private void PopulateStatusList(string selectedStatus = "Pending")
        {
            // Fixed list of valid order statuses for the dropdown
            var statuses = new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" };
            ViewBag.StatusList = new SelectList(statuses, selectedStatus);
        }

        /// <summary>
        /// Releases the database connection when the controller is disposed.
        /// Prevents resource leaks and unclosed SQL connections.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
    
