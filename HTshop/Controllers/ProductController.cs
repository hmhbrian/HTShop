using HTshop.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
namespace HTshop.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        HTShopEntities db = new HTShopEntities();
        private List<SANPHAM> LaySP(int count)
        {
            var danhSachSP= db.SANPHAMs.OrderByDescending(a=>a.NgayCapNhat).Take(count).ToList();
            foreach (var sp in danhSachSP)
            {
                if (sp.MaKM.HasValue)
                {
                    sp.GiaSauKhuyenMai = sp.Gia * (100 - sp.KHUYENMAI.GiaTriKM) / 100;
                    double phanTramKhuyenMai = (double)sp.KHUYENMAI.GiaTriKM;
                    sp.GiaTriKhuyenMai = "-" + phanTramKhuyenMai.ToString() + "%";
                }
                else
                {
                    sp.GiaSauKhuyenMai = sp.Gia;
                    sp.GiaTriKhuyenMai = string.Empty;
                }
            }
            return danhSachSP;
        }
        public ActionResult Index( int ?page)
        {
            int pageSize = 16;
            int pageNum = (page ?? 1);
            var sp = LaySP(100);
            return View(sp.ToPagedList(pageNum,pageSize));
        }
        public ActionResult DanhMucAo()
        {
            var Menu = from tl in db.THELOAIs where tl.MaLoai>=1 && tl.MaLoai<=10 select tl;
            return PartialView(Menu);
        }
        public ActionResult DanhMucQuan()
        {
            var Menu = from tl in db.THELOAIs where tl.MaLoai >= 11 && tl.MaLoai <= 15 select tl;
            return PartialView(Menu);
        }
        public ActionResult DanhMucDoLot()
        {
            var Menu = from tl in db.THELOAIs where tl.MaLoai >= 16 && tl.MaLoai <= 17 select tl;
            return PartialView(Menu);
        }
        public ActionResult DanhMucDoBo()
        {
            var Menu = from tl in db.THELOAIs where tl.MaLoai >= 18 && tl.MaLoai <= 19 select tl;
            return PartialView(Menu);
        }
        public ActionResult DanhMucPhuKien()
        {
            var Menu = from tl in db.THELOAIs where tl.MaLoai >= 20 && tl.MaLoai <= 25 select tl;
            return PartialView(Menu);
        }
        public ActionResult ListSize(int sp)
        {
            var size=from s in db.CTSizes 
                     join si in db.SIZEs on s.MaSize equals si.MaSize
                     where s.MaSP == sp 
                     select si;
            return PartialView(size);
        }
        [HttpPost]
        public JsonResult CheckProductQuantity(int productId, string selectedSize, int quantity)
        {
            if(selectedSize!=null)
            {
                int productQuantity = (int)db.CTSizes
                .Where(c => c.MaSP == productId && c.SIZE.Size1 == selectedSize)
                .Select(c => c.Sluong)
                .FirstOrDefault();

                // Kiểm tra số lượng và trả về kết quả
                if (productQuantity - quantity >= 0)
                {
                    return Json(new { success = true });
                }
                else
                {
                    if (productQuantity > 0)
                        return Json(new { success = false, message = "Size chỉ còn " + productQuantity.ToString() });
                    return Json(new { success = false, message = "Size đã hết" });
                }
            }
            return Json(new { success = true });
            // Lấy số lượng sản phẩm theo size

        }
        public ActionResult SPtheoLoai(int id)
        {
            var danhSachSP = from s in db.SANPHAMs where s.THELOAI.MaLoai == id select s;
            foreach (var sp in danhSachSP)
            {
                if (sp.MaKM.HasValue)
                {
                    sp.GiaSauKhuyenMai = sp.Gia * (100 - sp.KHUYENMAI.GiaTriKM) / 100;
                    double phanTramKhuyenMai = (double)sp.KHUYENMAI.GiaTriKM;
                    sp.GiaTriKhuyenMai = "-" + phanTramKhuyenMai.ToString() + "%";
                }
                else
                {
                    sp.GiaSauKhuyenMai = sp.Gia;
                    sp.GiaTriKhuyenMai = string.Empty;
                }
            }
            ViewBag.id = id;
            return View(danhSachSP);
        }
        public ActionResult ChiTiet(int id)
        {
            int currentUserId;
            USER user = Session["TaikhoanUser"] as USER;
            if (user != null)
            {
                currentUserId = user.UserID;
            }
            bool hasPurchased = CheckUserPurchaseStatus(id);
            ViewBag.HasPurchased = hasPurchased;

            var item = db.SANPHAMs.Find(id);
            if (item.MaKM.HasValue)
            {
                ViewBag.GiaSauKhuyenMai = item.Gia * (100 - item.KHUYENMAI.GiaTriKM) / 100;
            }
            else
            {
                ViewBag.GiaSauKhuyenMai = item.Gia;
            }
            return View(item);
        }
        int userId;
        private bool CheckUserPurchaseStatus(int productId)
        {
            USER user = Session["TaikhoanUser"] as USER;
            if (user != null)
            {
                userId = user.UserID;
            }

            var query = db.CTDonHangs
                .Join(db.DONHANGs, ct => ct.MaDH, dh => dh.MaDH, (ct, dh) => new { ct, dh })
                .Where(joinResult => joinResult.dh.UserID == userId && joinResult.ct.MaSP == productId)
                .Select(joinResult => joinResult.ct);

            return query.Any();
        }
    }
}