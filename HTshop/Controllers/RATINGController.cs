using HTshop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HTshop.Controllers
{
    public class RATINGController : Controller
    {
        HTShopEntities db = new HTShopEntities();
        // GET: Rating
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoadReview(int ProductID)
        {
            var item = db.RATINGs.Where(x => x.masp == ProductID).OrderByDescending(x => x.id).ToList();
            ViewBag.countReview = item.Count();
            return PartialView(item);
        }
        public ActionResult Review(int ProductID)
        {

            USER user = Session["TaikhoanUser"] as USER;
            ViewBag.productID = ProductID;

            var item = new RATING();
            if (user != null)
            {
                ViewBag.id = user.UserID;
                ViewBag.TenD = user.Hoten;
            }

            return PartialView(item);
        }
        [HttpPost]
        public ActionResult PostReview(RATING rt)
        {
            if (ModelState.IsValid)
            {
                rt.crdate = DateTime.Now;
                db.RATINGs.Add(rt);
                db.SaveChanges();
                return RedirectToAction("ChiTiet", "Product", new { id = rt.masp });
            }
            return RedirectToAction("ChiTiet", "Product", new { id = rt.masp });
        }
    }
}