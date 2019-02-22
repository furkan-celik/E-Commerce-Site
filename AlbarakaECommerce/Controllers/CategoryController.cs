using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AlbarakaECommerce.Models;

namespace AlbarakaECommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private masterEntities entity = new masterEntities();

        // GET: Category
        public ActionResult Index()
        {
            var categories = entity.Categories.Include(c => c.Category1);
            return View(categories.ToList());
        }

        // GET: Category/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = entity.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            ViewBag.subCategoryId = new SelectList(entity.Categories.Where(x => x.Categories1.Count == 0), "categoryId", "categoryName", "Category1.categoryName", 0);
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "categoryId,categoryName,subCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                entity.Categories.Add(category);
                entity.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.subCategoryId = new SelectList(entity.Categories, "categoryId", "categoryName", category.subCategoryId);
            return View(category);
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = entity.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.subCategoryId = new SelectList(entity.Categories.Where(x => x.subCategoryId == null), "categoryId", "categoryName", category.subCategoryId);
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "categoryId,categoryName,subCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                entity.Entry(category).State = EntityState.Modified;
                entity.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.subCategoryId = new SelectList(entity.Categories, "categoryId", "categoryName", category.subCategoryId);
            return View(category);
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = entity.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = entity.Categories.Find(id);
            entity.Categories.Remove(category);
            entity.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                entity.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
