using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AlbarakaECommerce.Models;

namespace AlbarakaECommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private masterEntities entity = new masterEntities();

        // GET: Coupon
        public ActionResult Index()
        {
            return View(entity.Coupons.ToList());
        }

        // GET: Coupon/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coupon coupon = entity.Coupons.Find(id);
            if (coupon == null)
            {
                return HttpNotFound();
            }
            return View(coupon);
        }

        // GET: Coupon/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Coupon/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,couponName,couponCode,discount,endDate")] Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                entity.Coupons.Add(coupon);
                entity.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(coupon);
        }

        // GET: Coupon/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coupon coupon = entity.Coupons.Find(id);
            if (coupon == null)
            {
                return HttpNotFound();
            }
            return View(coupon);
        }

        // POST: Coupon/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,couponName,couponCode,discount,endDate")] Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                entity.Entry(coupon).State = EntityState.Modified;
                entity.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // GET: Coupon/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coupon coupon = entity.Coupons.Find(id);
            if (coupon == null)
            {
                return HttpNotFound();
            }
            return View(coupon);
        }

        // POST: Coupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Coupon coupon = entity.Coupons.Find(id);
            entity.Coupons.Remove(coupon);
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
