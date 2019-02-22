using System;
using System.Collections.Generic;
using AlbarakaECommerce.Models;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace AlbarakaECommerce.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        // GET: Home
        public ActionResult Index()
        {
            masterEntities entity = new masterEntities();
            //var prs = entity.Products.OrderByDescending(x => x.inputDate).Take(6);
            List<Product> prs = new List<Product>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://furkanfunctionapps.azurewebsites.net/api/GetPrList");
                var result = client.PostAsync("",new StringContent("{'orderBy':'inputDate', 'order':'DESC', 'limit': 6 }", Encoding.UTF8, "application/json")).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                prs = JsonConvert.DeserializeObject<List<Product>>(resultContent);
                HttpRequestMessage req = new HttpRequestMessage();
                foreach (var product in prs)
                {
                    if (product.categoryId != null)
                    {
                        result = client.PostAsync("http://furkanfunctionapps.azurewebsites.net/api/GetCategory", new StringContent("{ 'category': " + product.categoryId + " }", Encoding.UTF8, "application/json")).Result;
                        resultContent = result.Content.ReadAsStringAsync().Result;
                        product.Category = JsonConvert.DeserializeObject<Category>(resultContent);
                    }
                }
                result = client.PostAsync("", new StringContent("{'orderBy':'productPrice', 'order':'ASC', 'limit': 8 }", Encoding.UTF8, "application/json")).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
                ViewBag.cheapestPrs = JsonConvert.DeserializeObject<List<Product>>(resultContent);
            }
            ViewBag.popularPrs = entity.Products.OrderByDescending(x => x.OrderProductCons.Count).Take(8).ToList();
            return View(prs);
        }
    }
}