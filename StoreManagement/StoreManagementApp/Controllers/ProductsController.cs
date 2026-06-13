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
    public class ProductsController : Controller
    {
        // GET: Products
        // Shared database context that connects to StoreManagementDB
        private StoreManagementDBEntities db = new StoreManagementDBEntities();
        public ActionResult Index(string searchString)
        {
            // LINQ query: load products and eagerly include Category data
            // Include("Category") joins the Categories table so we see category names
            var products = db.Products
                             .Include("Category")
                             .OrderBy(p => p.ProductName)
                             .AsQueryable();

            // LINQ filter: if the user typed a search term, apply the filter
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.Description.Contains(searchString));

                // Pass search term back to the view so the input box stays filled
                ViewBag.SearchString = searchString;
            }

            return View(products.ToList());
        }

        // =============================================
        // DETAILS - View one product
        // =============================================

        /// <summary>
        /// GET: /Products/Details/5
        /// Finds and displays full details of one product,
        /// including its category name, using a LINQ query.
        /// </summary>
        /// <param name="id">The ProductID to look up</param>
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // LINQ: load the product AND its associated Category in one query
            Product product = db.Products
                                .Include("Category")
                                .Where(p => p.ProductID == id)
                                .FirstOrDefault();

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // =============================================
        // CREATE - Show blank product form
        // =============================================

        /// <summary>
        /// GET: /Products/Create
        /// Displays a blank product creation form.
        /// Uses LINQ to populate the category dropdown list
        /// with only active categories.
        /// </summary>
        public ActionResult Create()
        {
            // LINQ: get only active categories for the dropdown
            PopulateCategoryList();
            return View();
        }

        // =============================================
        // CREATE - Save new product
        // =============================================

        /// <summary>
        /// POST: /Products/Create
        /// Validates and saves a new product to the database.
        /// Automatically sets the creation date before saving.
        /// </summary>
        /// <param name="product">Product object bound from the HTML form</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductName,CategoryID,Price,StockQuantity,Description")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Set the creation timestamp before saving
                product.CreatedDate = DateTime.Now;

                // Add product to context and save to database
                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Re-populate dropdown if there was a validation error
            PopulateCategoryList(product.CategoryID);
            return View(product);
        }

        // =============================================
        // EDIT - Show pre-filled product form
        // =============================================

        /// <summary>
        /// GET: /Products/Edit/5
        /// Loads an existing product and displays it in an edit form.
        /// Pre-selects the correct category in the dropdown.
        /// </summary>
        /// <param name="id">The ProductID to edit</param>
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            // Pre-select the product's existing category in the dropdown
            PopulateCategoryList(product.CategoryID);
            return View(product);
        }

        // =============================================
        // EDIT - Save updated product
        // =============================================

        /// <summary>
        /// POST: /Products/Edit/5
        /// Validates and saves the updated product data to the database.
        /// Uses EntityState.Modified to trigger an SQL UPDATE statement.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,CategoryID,Price,StockQuantity,Description,CreatedDate")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Tell EF this entity has been changed so it runs UPDATE
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            PopulateCategoryList(product.CategoryID);
            return View(product);
        }

        // =============================================
        // DELETE - Show confirmation page
        // =============================================

        /// <summary>
        /// GET: /Products/Delete/5
        /// Shows product details and asks user to confirm deletion.
        /// Includes category name for context.
        /// </summary>
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Load product with category name for the confirmation page
            Product product = db.Products
                                .Include("Category")
                                .Where(p => p.ProductID == id)
                                .FirstOrDefault();

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // =============================================
        // DELETE - Confirm and remove product
        // =============================================

        /// <summary>
        /// POST: /Products/Delete/5
        /// Permanently removes the product record from the database.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // =============================================
        // PRIVATE HELPER - Build category dropdown
        // =============================================

        /// <summary>
        /// Helper method that uses LINQ to retrieve all active categories
        /// and stores them in ViewBag as a SelectList for dropdown menus.
        /// Called by Create and Edit actions.
        /// </summary>
        /// <param name="selectedCategoryId">Pre-selects this category in the dropdown</param>
        private void PopulateCategoryList(object selectedCategoryId = null)
        {
            // LINQ: get active categories sorted alphabetically
            var categories = db.Categories
                               .Where(c => c.IsActive == true)
                               .OrderBy(c => c.CategoryName)
                               .ToList();

            ViewBag.CategoryID = new SelectList(categories, "CategoryID", "CategoryName", selectedCategoryId);
        }

        /// <summary>
        /// Releases database resources when controller is disposed.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
        
    
