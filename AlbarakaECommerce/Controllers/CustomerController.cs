using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AlbarakaECommerce.Models;
using Microsoft.AspNet.Identity;
using AlbarakaECommerce.Codes;
using System.IO;

namespace AlbarakaECommerce.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index()
        {
            return RedirectToAction("GetAllCustomers", "Customer"); //directly formarding to getallcustomers function
        }

        public ActionResult GetAllCustomers()
        {
            masterEntities entity = new masterEntities();
            if (entity.Customers.Where(x => x.validity == true) == null) return Create();
            var list = entity.Customers.Where(x => x.validity == true).ToList(); //getting list of valid customers
            return View(list);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(FormCollection collection)
        {
            using (var context = new masterEntities())
            {
                var customer = new Customer // collecting inputs and submiting a row to database table
                {
                    customerName = collection.Get("customerName").ToString(),
                    customerSurname = collection.Get("customerSurname").ToString(),
                    customerAdress = collection.Get("customerAdress").ToString(),
                    validity = true
                };
                context.Customers.Add(customer);
                context.SaveChanges();
            }

            return View();
        }
        
        public ActionResult Delete(int? id) //finding requested id in database and than removing it
        {
            masterEntities entity = new masterEntities();
            try
            {

                if (helperFunctions.IsAdminUser())
                {
                    if(id == null) return RedirectToAction("Index");

                    var cs = entity.Customers.SingleOrDefault(x => x.id == id);

                    if(cs.AspNetUsers.Count > 0)
                    {
                        entity.AspNetUsers.Remove(cs.AspNetUsers.ToList()[0]);
                    }

                    entity.Customers.Remove(cs);
                }
                else
                {
                    if(helperFunctions.GetActiveCustomer().AspNetUsers.Count > 0)
                    {
                        entity.AspNetUsers.Remove(helperFunctions.GetActiveCustomer().AspNetUsers.ToList()[0]);
                    }

                    entity.Customers.Remove(helperFunctions.GetActiveCustomer());
                }
                entity.SaveChanges();
            }
            catch (Exception ex)
            {
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Delete",
                    controller = "Customer",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Change(int? id) //finding requested id in database and than passing it to view for filling placeholders according to old customer
        {
            //Fill forms
            masterEntities entity = new masterEntities();
            if(helperFunctions.IsAdminUser() && id != null)
            {
                var cs = entity.Customers.SingleOrDefault(x => x.id == id);
                return View(cs);
            }
            else
            {
                return View(helperFunctions.GetActiveCustomer());
            }
        }

        [HttpPost]
        public ActionResult Change(FormCollection collection) //collecting input and than changing wanted id, getting id from hidden form member
        {
            masterEntities entity = new masterEntities();
            int id = int.Parse(collection.Get("Id"));
            var cs = entity.Customers.SingleOrDefault(x => x.id == id);
            if (!helperFunctions.IsAdminUser())
                if (id != helperFunctions.GetActiveCustomer().id) return RedirectToAction("Index", "Home");
            try
            {

                cs.customerName = collection.Get("customerName").ToString();
                cs.customerSurname = collection.Get("customerSurname").ToString();
                cs.customerAdress = collection.Get("customerAdress").ToString();
                cs.PhoneNumber = collection.Get("PhoneNumber");
                cs.email = collection.Get("email");

                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file != null && file.ContentLength > 0)
                    {
                        if(cs.profilePicture == null || cs.profilePicture == "c:/users/gamew/documents/visual studio 2015/Projects/AlbarakaECommerce/AlbarakaECommerce/Images/Profiles/default.jpg")
                        {
                            cs.profilePicture = Path.Combine(Server.MapPath("~/Images/Profiles/"), "profilePicture_" + cs.id + ".jpg");
                        }
                        try { file.SaveAs(cs.profilePicture); }
                        catch (Exception ex)
                        {
                            Error err = new Error()
                            {
                                message = ex.Message,
                                view_function = "Change/FileUpload",
                                controller = "Customer",
                                timeStamp = DateTime.Now
                            };
                            if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                            entity.Errors.Add(err);
                            entity.SaveChanges();
                        }
                    }
                }

                cs.validity = true;
                entity.SaveChanges();

                return RedirectToAction("GetAllCustomers", "Customer");
            }
            catch (Exception ex)
            {
                Error err = new Error()
                {
                    message = ex.Message,
                    view_function = "Change",
                    controller = "Customer",
                    timeStamp = DateTime.Now
                };
                if (ex.InnerException != null && ex.InnerException.Message != null) err.innerExceptionMessage = ex.InnerException.Message;
                entity.Errors.Add(err);
                entity.SaveChanges();
                return View(cs);
            }
        }

        [HttpGet]
        public ActionResult Orders(int? id) //requesting orders of customer from database and than passing them to view for listing
        {
            masterEntities entity = new masterEntities();
            try
            {
                int cid;
                List<Order> orders;
                if (id == null)
                {
                    cid = int.Parse(User.Identity.GetUserId());
                    cid = (int)entity.AspNetUsers.SingleOrDefault(x => x.Id == cid).CustomerId;
                }
                else
                {
                    cid = (int)id;
                }
                orders = entity.Orders.Where(x => x.Customer.id == cid).ToList();

                return View(orders);
            }
            catch (Exception ex)
            {
                entity.Errors.Add(new Error()
                {
                    message = ex.Message,
                    innerExceptionMessage = ex.InnerException.Message,
                    view_function = "Orders",
                    controller = "Customer",
                    timeStamp = DateTime.Now
                });
                entity.SaveChanges();
                return View();
            }
        }
        
        public ActionResult Account()
        {
            masterEntities entity = new masterEntities();
            var cid = int.Parse(User.Identity.GetUserId());
            var cs = entity.AspNetUsers.SingleOrDefault(x => x.Id == cid).Customer;
            if(cs.validity == null)
            {
                return RedirectToAction("Change", "Customer", new { id = cs.id});
            }
            return View(cs);
        }
    }
}