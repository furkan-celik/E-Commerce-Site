using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AlbarakaECommerce.Models;
using AlbarakaECommerce.Codes;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AlbarakaECommerce.Controllers
{
    [Authorize]
    public class AdressController : Controller
    {
        private masterEntities entity = new masterEntities();

        // GET: Adress
        public ActionResult Index()
        {
            var adresses = helperFunctions.GetActiveCustomer().Adresses.ToList();
            return View(adresses);
        }

        // GET: Adress/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Adress adress = entity.Adresses.Find(id);
            if (adress == null)
            {
                return HttpNotFound();
            }
            return View(adress);
        }

        // GET: Adress/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Adress/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/CreateAdress");
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("customerId", helperFunctions.GetActiveCustomer().id.ToString());
                dict.Add("responsibleName", collection.Get("responsibleName").ToString());
                dict.Add("adressLine", collection.Get("adressLine").ToString());
                dict.Add("district", collection.Get("district").ToString());
                dict.Add("province", collection.Get("province").ToString());
                dict.Add("country", collection.Get("country").ToString());
                dict.Add("phoneNumber", collection.Get("phoneNumber").ToString());
                dict.Add("emailAdress", collection.Get("emailAdress").ToString());
                string json = JsonConvert.SerializeObject(dict);
                var result = client.PostAsync(client.BaseAddress,
                        new StringContent(json.ToString(), Encoding.UTF8, "application/json"))
                    .Result;
            }

            return RedirectToAction("Index");
        }

        // GET: Adress/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Adress adress = entity.Adresses.Find(id);
            if (adress.customerId != helperFunctions.GetActiveCustomer().id)
            {
                return HttpNotFound();
            }
            return View(adress);
        }

        // POST: Adress/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collection)
        {
            var aid = int.Parse(collection.Get("adressId"));
            var adress = entity.Adresses.SingleOrDefault(x => x.adressId == aid);
            try
            {
                adress.responsibleName = collection.Get("responsibleName");
                adress.adressLine = collection.Get("adressLine");
                adress.district = collection.Get("district");
                adress.province = collection.Get("province");
                adress.country = collection.Get("country");
                adress.phoneNumber = Int64.Parse(collection.Get("phoneNumber"));
                adress.emailAdress = collection.Get("emailAdress");
                entity.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Edit",
                    controller = "Adress",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
                return View(adress);
            }
        }

        // GET: Adress/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Adress adress = entity.Adresses.Find(id);
            if (adress == null)
            {
                return HttpNotFound();
            }
            return View(adress);
        }

        // POST: Adress/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Adress adress = entity.Adresses.Find(id);
            entity.Adresses.Remove(adress);
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
