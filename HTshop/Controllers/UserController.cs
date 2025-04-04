using HTshop.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace HTshop.Controllers
{
    [UserFilter]
    public class UserController : Controller
    {
        HTShopEntities db = new HTShopEntities();
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index","Home");
        }
        
        public ActionResult info(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER book = db.USERs.Find(id); 
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }
        [HttpGet]
        public ActionResult edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER us = db.USERs.Find(id);
            if (us == null)
            {
                return HttpNotFound();
            }
            return View(us);
        }
        [HttpPost, ActionName("edit")]
        [ValidateAntiForgeryToken]
        public ActionResult editPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var studentToUpdate = db.USERs.Find(id);
            if (TryUpdateModel(studentToUpdate, "",
               new string[] { "UserID", "Hoten","DiaChi","SDT","UserName","Password","Email" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("info", new {id = studentToUpdate.UserID});
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }
        public ActionResult TinhTrangDH(int? userid)
        {
            var tt = from dh in db.DONHANGs where dh.UserID == userid select dh;
            return View(tt);
        }
        public ActionResult DetailsDonHang(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy danh sách chi tiết đơn hàng dựa trên MaDH
            List<CTDonHang> danhSachChiTietDonHang = db.CTDonHangs
                .Where(ct => ct.MaDH == id)
                .OrderByDescending(ct => ct.MaDH)
                .ToList();

            if (danhSachChiTietDonHang == null || !danhSachChiTietDonHang.Any())
            {
                return HttpNotFound();
            }

            return View(danhSachChiTietDonHang);
        }
    }
}