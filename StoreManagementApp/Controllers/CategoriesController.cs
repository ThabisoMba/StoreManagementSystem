using StoreManagementApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Lifetime;
using System.Web;
using System.Web.Mvc;

namespace StoreManagementApp.Controllers
{
    public class CategoriesController : Controller
    {
        // GET: Categories
        // This connects the application to StoreManagementDB.
        private StoreManagementDBEntities db = new StoreManagementDBEntities();
        public ActionResult Index()
        {
            // LINQ query: filter only active categories and sort A-Z
            var categories = db.Categories
                               .Where(c => c.IsActive == true)
                               .OrderBy(c => c.CategoryName)
                               .ToList();
            return View(categories);
        }
        // Finds and displays a single category by its ID.
        public ActionResult Details(int? id)
        {
            // Validate that an ID was provided in the URL
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // LINQ query: find the category with the matching ID
            Category category = db.Categories
                                  .Where(c => c.CategoryID == id)
                                  .FirstOrDefault();

            // Return 404 if category does not exist
            if (category == null)
                return HttpNotFound();

            return View(category);
        }
        // Displays an empty form for creating a new category.
        public ActionResult Create()
        {
            return View();
        }
        // POST: /Categories/Create
        // Receives the submitted form data, validates it,
        // and saves the new category to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryName,Description,IsActive")] Category category)
        {
            // Check all data annotations on the model are satisfied
            if (ModelState.IsValid)
            {
                // Set the creation date to right now
                category.CreatedDate = DateTime.Now;

                // Add the new category to the database context
                db.Categories.Add(category);

                // Execute the INSERT statement in SQL Server
                db.SaveChanges();

                // Redirect user back to the full categories list
                return RedirectToAction("Index");
            }

            // If validation failed, return the form with error messages shown
            return View(category);
        }
        // GET: /Categories/Edit/5
        // Loads an existing category from the database
        // and pre-fills the edit form with its current values.
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Use Entity Framework Find() to locate by primary key
            Category category = db.Categories.Find(id);

            if (category == null)
                return HttpNotFound();

            return View(category);
        }
        // POST: /Categories/Edit/5
        // Receives the updated form data, validates it,
        // and saves the changes to the database using LINQ.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName,Description,CreatedDate,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                // Mark entity as Modified so EF generates an UPDATE SQL statement
                db.Entry(category).State = EntityState.Modified;

                // Execute the UPDATE statement in SQL Server
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(category);
        }
        // DELETE - Show confirmation page
        // GET: /Categories/Delete/5
        // Displays a confirmation page showing the category
        // details before the user confirms deletion.
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Category category = db.Categories.Find(id);

            if (category == null)
                return HttpNotFound();

            return View(category);
        }
        // DELETE - Confirm and remove from database
        // POST: /Categories/Delete/5
        // Permanently removes the category from the database
        // after the user clicks the Confirm Delete button.
        // ActionName("Delete") maps this to the Delete route.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Retrieve the category from the database
            Category category = db.Categories.Find(id);

            // Remove it from the database context
            db.Categories.Remove(category);

            // Execute the DELETE statement in SQL Server
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        // DISPOSE - Release database resources
        // Releases the database context and all unmanaged resources
        // when the controller is no longer needed. This prevents
        // memory leaks and unclosed database connections.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);

        }
    }
}