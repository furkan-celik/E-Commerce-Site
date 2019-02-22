using AlbarakaECommerce.Codes;
using AlbarakaECommerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AlbarakaECommerce.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        // GET: Wishlist
        public ActionResult Index()
        {
            List<Product> prs = new List<Product>();
            foreach (var item in helperFunctions.GetActiveCustomer().WishlistCons) prs.Add(item.Product);
            return View(prs);
        }
        
        public ActionResult Add(int productId, string returnView, string returnController)
        {
            masterEntities entity = new masterEntities();
            WishlistCon con = new WishlistCon()
            {
                customerId = helperFunctions.GetActiveCustomer().id,
                productId = productId
            };
            if(helperFunctions.GetActiveCustomer().WishlistCons.SingleOrDefault(x => x.productId == productId) != null) return RedirectToAction(returnView, returnController);
            entity.WishlistCons.Add(con);
            entity.SaveChanges();
            return RedirectToAction(returnView, returnController);
        }

        public ActionResult Delete(int productId, string returnView, string returnController)
        {
            masterEntities entity = new masterEntities();
            var con = entity.WishlistCons.SingleOrDefault(x => x.productId == productId);
            if(con != null) entity.WishlistCons.Remove(con);
            entity.SaveChanges();
            return RedirectToAction(returnView, returnController);
        }
    }
}