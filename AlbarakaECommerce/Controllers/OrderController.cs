using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AlbarakaECommerce.Models;
using AlbarakaECommerce.Codes;
using System;

namespace AlbarakaECommerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        [Authorize(Roles = "Admin")]
        // GET: Order
        public ActionResult Index() //listing valid orders and passing them to view for listing them
        {
            masterEntities entity = new masterEntities();
            var orders = entity.Orders.Where(x => x.validity == true);
            return View(orders);
        }

        // GET: Order/Details/5
        public ActionResult Details(int id) //getting requested order from id and passing order to view for filling input fields
        {
            masterEntities entity = new masterEntities();
            var order = entity.Orders.SingleOrDefault(x => x.orderId == id);
            if(!helperFunctions.IsAdminUser())
                if (order.customerId != helperFunctions.GetActiveCustomer().id) return RedirectToAction("Index", "Home");
            return View(order);
        }

        [Authorize(Roles = "Admin")]
        // GET: Order/Create
        public ActionResult Create()
        {
            masterEntities entity = new masterEntities();
            var listProducts = entity.Products.Where(x => x.validity == true).ToList();
            var listCustomers = entity.Customers.Where(x => x.validity == true).ToList();
            ViewBag.listProducts = listProducts; //passing list of valid products and customers via viewbag to make a dropdown list
            ViewBag.listCustomers = listCustomers;
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                masterEntities entity = new masterEntities();
                int cId = int.Parse(collection.Get("Customer.customerId")), pId = int.Parse(collection.Get("Products"));
                var cs = entity.Customers.SingleOrDefault(x => x.id == cId);
                List<CartProduct> prList = new List<CartProduct>();
                prList.Add(new CartProduct (entity.Products.SingleOrDefault(x => x.productId == pId)));
                Order order = new Order()
                {
                    customerId = cs.id,
                    amount = int.Parse(collection.Get("amount")), 
                    shipmentAdress = collection.Get("shipmentAdress"),
                    paymentway = collection.Get("paymentway"),
                    orderState = collection.Get("orderState"),
                    shipment = collection.Get("shipment"),
                    validity = bool.Parse(collection.Get("validity"))
                };
                double totalPrice = 0;
                foreach (var item in prList)
                {
                    OrderProductCon con = new OrderProductCon()
                    {
                        Order = order,
                        Product = item.product,
                        amount = item.amount
                    };
                    totalPrice += ((Double)(item.product.productPrice * item.amount));
                    entity.OrderProductCons.Add(con);
                }
                entity.Orders.Add(order);
                entity.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                masterEntities entity = new masterEntities();
                var listProducts = entity.Products.Where(x => x.validity == true).ToList();
                var listCustomers = entity.Customers.Where(x => x.validity == true).ToList();
                ViewBag.listProducts = listProducts; //passing list of valid products and customers via viewbag to make a dropdown list
                ViewBag.listCustomers = listCustomers;
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Create",
                    controller = "Order",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        // GET: Order/Edit/5
        public ActionResult Edit(int id)
        {
            masterEntities entity = new masterEntities();
            var order = entity.Orders.SingleOrDefault(x => x.orderId == id);
            var listProducts = entity.Products.Where(x => x.validity == true).ToList();
            var listCustomers = entity.Customers.Where(x => x.validity == true).ToList();
            ViewBag.listProducts = listProducts;
            ViewBag.listCustomers = listCustomers;
            return View(order);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, FormCollection collection)
        {
            masterEntities entity = new masterEntities();
            try
            {
                // TODO: Add update logic here
                Customer cs = entity.Customers.SingleOrDefault(x => x.id == int.Parse(collection.Get("buyerId")));
                List<CartProduct> prList = new List<CartProduct>();
                prList.Add(new CartProduct (entity.Products.SingleOrDefault(x => x.productId == int.Parse(collection.Get("productId")))));
                var order = entity.Orders.SingleOrDefault(x => x.orderId == id);
                order.Customer = cs;

                foreach (var item in prList)
                {
                    bool avaliable = false;
                    foreach(var item2 in order.OrderProductCons)
                    {
                        if (item2.productId == item.product.productId && item2.amount == item.amount) avaliable = true;
                        else if(item2.productId == item.product.productId)
                        {
                            item2.amount = item.amount;
                        }
                    }
                    if (!avaliable)
                    {
                        OrderProductCon con = new OrderProductCon()
                        {
                            Order = order,
                            Product = item.product,
                            amount = item.amount
                        };
                        entity.OrderProductCons.Add(con);
                    }
                }

                order.amount = int.Parse(collection.Get("amount"));
                order.shipmentAdress = collection.Get("shipmentAdress");
                order.paymentway = collection.Get("paymentway");
                order.orderState = collection.Get("orderState");
                order.shipment = collection.Get("shipment");
                order.validity = bool.Parse(collection.Get("validity"));
                entity.SaveChanges();
                return RedirectToAction("Index", "Order/Index");
            }
            catch (Exception ex)
            {
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Edit",
                    controller = "Order",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
                return View();
            }
        }

        // GET: Order/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            masterEntities entity = new masterEntities();
            var order = entity.Orders.SingleOrDefault(x => x.orderId == id);
            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id, FormCollection collection)
        {
            masterEntities entity = new masterEntities();
            try
            {
                // TODO: Add delete logic here
                var order = entity.Orders.SingleOrDefault(x => x.orderId == id);
                order.Customer = null;
                order.OrderProductCons = null;
                entity.Orders.Remove(order);
                entity.SaveChanges();
                return RedirectToAction("Index", "Order/Index");
            }
            catch (Exception ex)
            {
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Delete",
                    controller = "Order",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
                return View();
            }
        }

        public ActionResult Bill(int id)
        {
            masterEntities entity = new masterEntities();
            var order = entity.Orders.SingleOrDefault(x => x.orderId == id);
            if (!helperFunctions.IsAdminUser())
                if (order.customerId != helperFunctions.GetActiveCustomer().id) return RedirectToAction("Index", "Home");
            return View(order);
        }
    }
}
