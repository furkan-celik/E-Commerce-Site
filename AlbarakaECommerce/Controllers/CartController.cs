using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AlbarakaECommerce.Models;
using AlbarakaECommerce.Codes;
using Microsoft.AspNet.Identity;

namespace AlbarakaECommerce.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index(string couponCode)
        {
            List<CartProduct> cartList = (List<CartProduct>)Session["cartList"]; //getting cartList session and converting it to list
            if (cartList == null) return RedirectToAction("Index", "Product/Index"); //if session is null than return to Product/Index. In future not item in cart can shown
            masterEntities entity = new masterEntities();
            if(couponCode != null)
            {
                var coupon = entity.Coupons.SingleOrDefault(x => x.couponCode == couponCode);
                if (coupon != null && coupon.endDate > DateTime.Now) ViewBag.discount = coupon.discount;
                ViewBag.couponCode = couponCode;
            }
            return View(cartList);
        }

        public ActionResult Remove(int id)
        {
            List<CartProduct> cartList = (List<CartProduct>)Session["cartList"];
            for (int i = 0; i < cartList.Count; i++)
            {
                if (cartList[i].product.productId == id) //finding currect list member
                {
                    cartList.Remove(cartList[i]); //removing member of list and than saving it back to session
                    Session["cartList"] = cartList;
                    return RedirectToAction("Index", "Cart/Index"); //returning to cart index, if cart became empty than index automaticaly forward request to products
                }
            }
            return RedirectToAction("Index", "Cart/Index");
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            masterEntities entity = new masterEntities();
            CartProduct prSe;
            List<CartProduct> prList = (List<CartProduct>)Session["cartList"];
            for (int i = 0; i < prList.Count; i++)
            {
                var productId = prList[i].product.productId;
                Product prDb = entity.Products.SingleOrDefault(x => x.productId == productId); //getting item from database for checking if requested amount is abaliable
                if (prList[i].product.productId == prDb.productId) //finding right member of session list and than checking wanted amount of the product and avaliable amount
                {
                    prSe = prList[i];
                    if (collection.Get("amount" + productId) != null && int.Parse(collection.Get("amount" + productId)) <= prDb.remainedAmount)
                    {
                        //Amount'u ayri olarak save ederek check atmem lazım
                        prSe.amount = int.Parse(collection.Get("amount" + productId));
                        prList[i] = prSe;
                        if (prSe.amount == 0) return Remove(productId);
                    }
                }
            }
            return RedirectToAction("Index", "Cart/Index");
        }

        [Authorize]
        public ActionResult Buy(string method, int adressId, string couponCode)
        {
            masterEntities entity = new masterEntities();
            try
            {
                List<CartProduct> prList = (List<CartProduct>)Session["cartList"];
                string userAdress = "bagcilar"/*entity.Customers.SingleOrDefault(x => x.customerId == 1).customerAdress*/;
                var uid = int.Parse(User.Identity.GetUserId());
                var cid = entity.AspNetUsers.SingleOrDefault(x => x.Id == uid).CustomerId;
                var adress = entity.Adresses.SingleOrDefault(x => x.adressId == adressId);
                var coupon = entity.Coupons.SingleOrDefault(x => x.couponCode == couponCode && x.endDate > DateTime.Now);
                Order order = new Order()
                {
                    customerId = (int)cid,
                    amount = 1,
                    shipmentAdress = userAdress,
                    paymentway = method,
                    orderState = "preparing",
                    validity = true,
                    purchaseDate = DateTime.Now,
                    adress = adress.adressId
                };
                if (coupon != null) order.CouponId = coupon.id;
                entity.Orders.Add(order);
                entity.SaveChanges();
                double totalPrice = 0;
                foreach (var item in prList)
                {
                    OrderProductCon con = new OrderProductCon()
                    {
                        orderId = order.orderId,
                        productId = item.product.productId,
                        amount = item.amount
                    };
                    totalPrice += ((double)item.product.productPrice * item.amount);
                    entity.OrderProductCons.Add(con);
                    entity.SaveChanges();
                }
                if(coupon != null)
                    totalPrice = totalPrice * coupon.discount / 100.0;
                order.totalPrice = (int)totalPrice;
                entity.SaveChanges();
                Session["cartList"] = null;
                return RedirectToAction("Index", "Home/Index");
            }
            catch (Exception ex)
            {
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Index",
                    controller = "Cart",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
                return RedirectToAction("Index", "Cart/Index");
            }
        }

        [Authorize]
        public ActionResult Checkout(string couponCode)
        {
            ViewBag.couponCode = couponCode;
            if(helperFunctions.GetActiveCustomer().validity != true)
            {
                return RedirectToAction("Change", "Customer");
            }
            return View();
        }

        public ActionResult Adress(string method, string couponCode)
        {
            ViewBag.method = method;
            ViewBag.couponCode = couponCode;
            if (helperFunctions.GetActiveCustomer().validity != true)
            {
                return RedirectToAction("Change", "Customer");
            }
            return View(helperFunctions.GetActiveCustomer().Adresses.ToList());
        }

        [HttpPost]
        public ActionResult Adress(FormCollection collection)
        {
            string method = collection.Get("method");
            string couponCode = collection.Get("couponCode");
            int adressId = int.Parse(collection.Get("item.adressId"));
            if(adressId != 0)
            {
                return Buy(method, adressId, couponCode);
            }
            else
            {
                masterEntities entity = new masterEntities();
                try
                {
                    Adress adress = new Adress()
                    {
                        customerId = helperFunctions.GetActiveCustomer().id,
                        responsibleName = collection.Get("responsibleName"),
                        adressLine = collection.Get("adressLine"),
                        district = collection.Get("district"),
                        province = collection.Get("province"),
                        country = collection.Get("country"),
                        phoneNumber = Int64.Parse(collection.Get("phoneNumber")),
                        emailAdress = collection.Get("emailAdress")
                    };
                    entity.Adresses.Add(adress);
                    entity.SaveChanges();

                    return Buy(method, adressId, couponCode);
                }
                catch (Exception ex)
                {
                    Error err = new Error()
                    {
                        message = ex.Message,
                        view_function = "Delete",
                        controller = "Adress",
                        timeStamp = DateTime.Now
                    };
                    if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                    entity.Errors.Add(err);
                    entity.SaveChanges();
                    return RedirectToAction("Index", "Cart");
                }
            }
        }
    }
}