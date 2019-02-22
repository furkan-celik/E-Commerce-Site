using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AlbarakaECommerce.Codes;
using AlbarakaECommerce.Models;
using Microsoft.AspNet.Identity;
using System.Drawing;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace AlbarakaECommerce.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            List<Product> list = ListProducts(null, null, null, null);
            return View(list);
        }

        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            Product pr = GetProduct(id);
            return View(pr);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            masterEntities entity = new masterEntities();
            ViewBag.categories = ListCategories(null, null);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetCategoryList");
                var result = client.GetAsync("").Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                ViewBag.categories = JsonConvert.DeserializeObject<List<Category>>(resultContent);
            }
            ViewBag.subCategoryId = new SelectList(entity.Categories.Where(x => x.Categories1.Count == 0), "categoryId", "categoryName", "Category1.categoryName", 0);
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(FormCollection collection)
        {
            // TODO: Add insert logic here
            Product pr = new Product()
            {
                productName = collection.Get("productName"),
                productDescription = collection.Get("productDescription"),
                remainedAmount = int.Parse(collection.Get("remainedAmount")),
                productPrice = int.Parse(collection.Get("productPrice")),
                inputDate = DateTime.Parse(collection.Get("inputDate")),
                categoryId = int.Parse(collection.Get("subCategoryId")),
                validity = bool.Parse(collection.Get("validity"))
            };

            //File upload
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    masterEntities entity = new masterEntities();
                    try
                    {
                        var fileName = "IMG_PRODUCT_" + (entity.Products.Max(x => x.productId) + 1) + Path.GetExtension(file.FileName);
                        Image img = Image.FromFile(file.FileName);
                        var pathS = Path.Combine(Server.MapPath("~/Images/Products/Small/"), fileName);
                        helperFunctions.Save((Bitmap)img, 220, 242, 100, pathS);
                        var pathM = Path.Combine(Server.MapPath("~/Images/Products/Medium/"), fileName);
                        helperFunctions.Save((Bitmap)img, 484, 441, 100, pathM);
                        var pathL = Path.Combine(Server.MapPath("~/Images/Products/Large/"), fileName);
                        helperFunctions.Save((Bitmap)img, 968, 822, 100, pathL);
                        pr.productPicture = pathM;
                    }
                    catch (Exception ex)
                    {
                        Error err = new Error()
                        {
                            message = ex.Message,
                            view_function = "Create/FileUpload",
                            controller = "Product",
                            timeStamp = DateTime.Now
                        };
                        if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                        entity.Errors.Add(err);
                        entity.SaveChanges();
                    }
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/CreateProduct");
                var json = JsonConvert.SerializeObject(pr);
                var result = client.PostAsync("", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Product/Index");
                }
            }
            return View();
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            masterEntities entity = new masterEntities();
            Product pr = GetProduct(id);
            ViewBag.subCategoryId = new SelectList(entity.Categories.Where(x => x.Categories1.Count == 0), "categoryId", "categoryName", "Category1.categoryName", pr.categoryId);
            return View(pr);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, FormCollection collection)
        {
            // TODO: Add update logic here
            Product pr;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetProduct");
                var result = client.PostAsync("", new StringContent("{ 'productId': " + id + " }", Encoding.UTF8, "application/json")).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                pr = JsonConvert.DeserializeObject<List<Product>>(resultContent)[0];
                if (pr.categoryId != null)
                {
                    result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/GetCategory", new StringContent("{ 'category': " + pr.categoryId + " }", Encoding.UTF8, "application/json")).Result;
                    resultContent = result.Content.ReadAsStringAsync().Result;
                    pr.Category = JsonConvert.DeserializeObject<Category>(resultContent);
                }
                pr.productName = collection.Get("productName");
                pr.productDescription = collection.Get("productDescription");
                pr.remainedAmount = int.Parse(collection.Get("remainedAmount"));
                pr.productPrice = int.Parse(collection.Get("productPrice"));
                pr.inputDate = DateTime.Parse(collection.Get("inputDate"));
                pr.categoryId = int.Parse(collection.Get("subCategoryId"));
                pr.validity = bool.Parse(collection.Get("validity"));
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = "IMG_PRODUCT_" + pr.productId + Path.GetExtension(file.FileName);
                        Image img = Image.FromFile(file.FileName);
                        var pathS = Path.Combine(Server.MapPath("~/Images/Products/Small/"), fileName);
                        helperFunctions.Save((Bitmap)img, 220, 242, 100, pathS);
                        var pathM = Path.Combine(Server.MapPath("~/Images/Products/Medium/"), fileName);
                        helperFunctions.Save((Bitmap)img, 484, 441, 100, pathM);
                        var pathL = Path.Combine(Server.MapPath("~/Images/Products/Large/"), fileName);
                        helperFunctions.Save((Bitmap)img, 968, 822, 100, pathL);
                    }
                }

                result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/UpdateProduct", new StringContent(JsonConvert.SerializeObject(pr), Encoding.UTF8, "application/json")).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Product/Index");
                }
            }
            return View();
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Product pr;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetProduct");
                var result = client.PostAsync("", new StringContent("{ 'productId': " + id + " }", Encoding.UTF8, "application/json")).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                pr = JsonConvert.DeserializeObject<List<Product>>(resultContent)[0];
                if (pr.categoryId != null)
                {
                    result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/GetCategory", new StringContent("{ 'category': " + pr.categoryId + " }", Encoding.UTF8, "application/json")).Result;
                    resultContent = result.Content.ReadAsStringAsync().Result;
                    pr.Category = JsonConvert.DeserializeObject<Category>(resultContent);
                }
            }
            return View(pr);
        }

        // POST: Product/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id, FormCollection collection)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/DeleteProduct");
                var result = client.PostAsync("", new StringContent(JsonConvert.SerializeObject(new Product() { productId = id }), Encoding.UTF8, "application/json")).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Product/Index");
                }
            }
            return View();
        }

        public ActionResult Buy(int id, int? count) //getting item according to id. Checking if session is null or not and than creating a new session or updating the old one
        {
            if (count == null) count = 1;
            Product pr = GetProduct(id);
            if (Session["cartList"] != null)
            {
                List<CartProduct> cartList = (List<CartProduct>)Session["cartList"];
                if (cartList.Find(x => x.product.productId == pr.productId) != null)
                {
                    cartList.Find(x => x.product.productId == pr.productId).amount += (int)count;
                }
                else
                {
                    CartProduct buff = new CartProduct(pr, (int)count);
                    cartList.Add(buff);
                }
                Session["cartList"] = cartList;
            }
            else
            {
                List<CartProduct> cartList = new List<CartProduct>();
                CartProduct buff = new CartProduct(pr, (int)count);
                cartList.Add(buff);
                Session.Add("cartList", cartList);
            }
            return RedirectToAction("Index", "Product/Index");
        }

        public ActionResult List(int? categoryId, string order, string search)
        {
            masterEntities entity = new masterEntities();
            var prList = ListProducts(null, null, null, null);
            var catHeadList = ListCategories("master", null);
            ViewBag.categories = catHeadList;

            if (categoryId != null)
            {
                prList = entity.Categories.SingleOrDefault(x => x.categoryId == categoryId).Products.ToList();
                foreach (var item in entity.Categories.SingleOrDefault(x => x.categoryId == categoryId).Categories1)
                {
                    prList.AddRange(item.Products.ToList());
                    foreach (var item2 in item.Categories1)
                    {
                        prList.AddRange(item2.Products.ToList());
                    }
                }
            }
            else if (order != null)
            {
                if (order == "cheapest")
                {
                    prList = ListProducts(null, "productPrice", "ASC", null);
                }
                else if (order == "expensive")
                {
                    prList = ListProducts(null, "productPrice", "DESC", null);
                }
                else if (order == "newest")
                {
                    prList = ListProducts(null, "inputDate", "DESC", null);
                }
                else if (order == "oldest")
                {
                    prList = ListProducts(null, "inputDate", "ASC", null);
                }
                else if (order == "popular")
                {
                    prList = entity.Products.Where(x => x.validity == true).OrderByDescending(x => x.OrderProductCons.Count).ToList();
                }
                else if (order == "nonPopular")
                {
                    prList = entity.Products.Where(x => x.validity == true).OrderBy(x => x.OrderProductCons.Count).ToList();
                }
                ViewBag.order = order;
            }
            else if (search != null)
            {
                prList = SearchProduct(search);
            }

            return View(prList);
        }

        [HttpPost]
        [Authorize]
        public ActionResult PostReview(FormCollection collection)
        {
            masterEntities entity = new masterEntities();
            var uid = int.Parse(User.Identity.GetUserId());
            var cid = entity.AspNetUsers.SingleOrDefault(x => x.Id == uid).CustomerId;
            Review review = new Review()
            {
                reviewContent = collection.Get("reviewContent"),
                reviewDate = DateTime.Now,
                customerId = cid,
                productId = int.Parse(collection.Get("productId"))
            };
            CreateReview(review);
            return RedirectToAction("Details", "Product", new { id = int.Parse(collection.Get("productId")) });
        }

        #region ServiceConnection

        #region GetProduct

        private Product GetProduct(int id)
        {
            Product pr;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetProduct");
                var result = client.PostAsync("", new StringContent("{ 'productId': " + id + " }", Encoding.UTF8, "application/json")).Result;
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    pr = JsonConvert.DeserializeObject<List<Product>>(resultContent)[0];
                    if (pr.categoryId != null)
                    {
                        result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/GetCategory", new StringContent("{ 'category': " + pr.categoryId + " }", Encoding.UTF8, "application/json")).Result;
                        resultContent = result.Content.ReadAsStringAsync().Result;
                        pr.Category = JsonConvert.DeserializeObject<Category>(resultContent);
                    }
                    return pr;
                }
            }
            return null;
        }

        #endregion

        #region ListProducts

        private class OrderReq
        {
            public string orderBy { get; set; }
            public string order { get; set; }
            public int? limit { get; set; }
            public int? categoryId { get; set; }
        }

        private List<Product> ListProducts(int? limit, string orderBy, string order, int? categoryId)
        {
            List<Product> list = new List<Product>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetPrList");
                var result = client.PostAsync("", new StringContent(JsonConvert.SerializeObject(new OrderReq() { orderBy = orderBy, order = order, limit = limit, categoryId = categoryId}), Encoding.UTF8, "application/json")).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                list = JsonConvert.DeserializeObject<List<Product>>(resultContent);
                foreach (var product in list)
                {
                    if (product.categoryId != null)
                    {
                        result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/GetCategory", new StringContent("{ 'category': " + product.categoryId + " }", Encoding.UTF8, "application/json")).Result;
                        resultContent = result.Content.ReadAsStringAsync().Result;
                        product.Category = JsonConvert.DeserializeObject<Category>(resultContent);
                    }
                }
            }
            return list;
        }

        #endregion

        #region SearchProduct

        private List<Product> SearchProduct(string search)
        {
            List<Product> list = new List<Product>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/SearchProduct");
                var result = client.PostAsync("", new StringContent("{ 'search':'" + search + "' }", Encoding.UTF8, "application/json")).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                list = JsonConvert.DeserializeObject<List<Product>>(resultContent);
                foreach (var product in list)
                {
                    if (product.categoryId != null)
                    {
                        result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/GetCategory", new StringContent("{ 'category': " + product.categoryId + " }", Encoding.UTF8, "application/json")).Result;
                        resultContent = result.Content.ReadAsStringAsync().Result;
                        product.Category = JsonConvert.DeserializeObject<Category>(resultContent);
                    }
                }
            }
            return list;
        }

        #endregion

        #region CreateReview

        private bool CreateReview(Review review)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/CreateReview");
                var result = client.PostAsync("", new StringContent(JsonConvert.SerializeObject(review), Encoding.UTF8, "application/json")).Result;
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ListCategories

        private class CatReq
        {
            public string catMode { get; set; }
            public int? subCatId { get; set; }
        }

        private List<Category> ListCategories(string catMode, int? subCatId)
        {
            List<Category> list;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetCategoryList");
                var result = client.PostAsync("", new StringContent(JsonConvert.SerializeObject(new CatReq() { catMode = catMode, subCatId = subCatId }), Encoding.UTF8, "application/json")).Result;
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    list = JsonConvert.DeserializeObject<List<Category>>(resultContent);
                    return list;
                }
            }
            return null;
        }

        #endregion

        #endregion
    }
}
