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
    public class CustomersController : Controller
    {
        // GET: Customers
        // Shared database context connecting to StoreManagementDB
        private StoreManagementDBEntities db = new StoreManagementDBEntities();
        public ActionResult Index(string searchString)
        {
            // LINQ: retrieve all customers sorted by last name, then first name
            var customers = db.Customers
                              .OrderBy(c => c.LastName)
                              .ThenBy(c => c.FirstName)
                              .AsQueryable();

            // LINQ filter: search across first name, last name and email
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.FirstName.Contains(searchString) ||
                    c.LastName.Contains(searchString) ||
                    c.Email.Contains(searchString));

                ViewBag.SearchString = searchString;
            }

            return View(customers.ToList());
        }

        // =============================================
        // DETAILS - View customer and their orders
        // =============================================

        /// <summary>
        /// GET: /Customers/Details/5
        /// Loads one customer record and eagerly includes
        /// all orders they have placed, using LINQ.
        /// </summary>
        /// <param name="id">The CustomerID to look up</param>
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // LINQ: load customer + all their orders in one query
            Customer customer = db.Customers
                                  .Include("Orders")
                                  .Where(c => c.CustomerID == id)
                                  .FirstOrDefault();

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // =============================================
        // CREATE - Show customer registration form
        // =============================================

        /// <summary>
        /// GET: /Customers/Create
        /// Displays a blank form for registering a new customer.
        /// </summary>
        public ActionResult Create()
        {
            return View();
        }

        // =============================================
        // CREATE - Save new customer
        // =============================================

        /// <summary>
        /// POST: /Customers/Create
        /// Validates the form data and saves a new customer record.
        /// Uses LINQ to check that the email is not already registered.
        /// </summary>
        /// <param name="customer">Customer object bound from the HTML form</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FirstName,LastName,Email,PhoneNumber,Address")] Customer customer)
        {
            // LINQ: check if another customer already uses this email address
            bool emailAlreadyExists = db.Customers
                                        .Any(c => c.Email == customer.Email);

            if (emailAlreadyExists)
            {
                // Add a custom error message to show on the form
                ModelState.AddModelError("Email", "This email address is already registered.");
            }

            if (ModelState.IsValid)
            {
                // Set registration date to current date and time
                customer.CreatedDate = DateTime.Now;

                db.Customers.Add(customer);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // =============================================
        // EDIT - Show pre-filled customer form
        // =============================================

        /// <summary>
        /// GET: /Customers/Edit/5
        /// Loads existing customer data and displays it in an edit form.
        /// </summary>
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // =============================================
        // EDIT - Save updated customer
        // =============================================

        /// <summary>
        /// POST: /Customers/Edit/5
        /// Saves updated customer details to the database.
        /// Checks that the new email is not taken by a different customer.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,FirstName,LastName,Email,PhoneNumber,Address,CreatedDate")] Customer customer)
        {
            // LINQ: check if another (different) customer already uses this email
            bool emailTakenByOther = db.Customers
                                       .Any(c => c.Email == customer.Email
                                              && c.CustomerID != customer.CustomerID);

            if (emailTakenByOther)
            {
                ModelState.AddModelError("Email", "This email is already used by another customer.");
            }

            if (ModelState.IsValid)
            {
                // Mark as modified so Entity Framework runs an UPDATE
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // =============================================
        // DELETE - Show confirmation page
        // =============================================

        /// <summary>
        /// GET: /Customers/Delete/5
        /// Shows customer details and requests delete confirmation.
        /// </summary>
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // =============================================
        // DELETE - Confirm and remove customer
        // =============================================

        /// <summary>
        /// POST: /Customers/Delete/5
        /// Deletes the customer from the database.
        /// Uses LINQ to check first if the customer has existing orders.
        /// If orders exist, deletion is blocked to maintain data integrity.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // LINQ: check if this customer has any orders linked to them
            bool customerHasOrders = db.Orders.Any(o => o.CustomerID == id);

            if (customerHasOrders)
            {
                // Cannot delete — would break the Orders foreign key relationship
                TempData["ErrorMessage"] = "Cannot delete this customer because they have existing orders.";
                return RedirectToAction("Index");
            }

            // Safe to delete — no orders exist for this customer
            Customer customer = db.Customers.Find(id);
            db.Customers.Remove(customer);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Releases the database context to free up resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
        
    
