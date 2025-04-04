using HTshop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Drawing;

namespace HTshop.Controllers
{
    [AdminFilter]
    public class AdminController : Controller
    {
        // phần sản phẩm
        //phương thức "uploadFile()"
        // phần thể loại
        //phần mã giảm giá  
        // phần đơn hàng
        // phần user 

        // GET: Admin
        private HTShopEntities db = new HTShopEntities();
        // GET: Admin
        public ActionResult Index()
        {
            DateTime homNay = DateTime.Now.Date;
            int tongDonHang = db.DONHANGs.Count(dh => DbFunctions.TruncateTime(dh.NgayDat) == homNay);
            var totalSale = (from dh in db.DONHANGs
                             join ctd in db.CTDonHangs on dh.MaDH equals ctd.MaDH
                             where DbFunctions.TruncateTime(dh.NgayDat) == homNay
                             select ctd.Gia).Sum();
            var tongDoanhThu = (from ctd in db.CTDonHangs
                                join dh in db.DONHANGs on ctd.MaDH equals dh.MaDH
                                where dh.DaThanhToan == true
                                select ctd.Gia).Sum();
            int tongDonHang2 = db.DONHANGs.Count(dh => dh.DaThanhToan == true);
            ViewBag.tongDonHang2 = tongDonHang2.ToString();
            ViewBag.tongDoanhThu = tongDoanhThu;
            if (totalSale != null)
                ViewBag.totalSale = totalSale;
            else
                ViewBag.totalSale = 0;
            ViewBag.TotalOrders = tongDonHang.ToString();
            return View();
        }
        [HttpGet]
        public ActionResult TKDoanhThuThang(int month)
        {
            var query = from o in db.DONHANGs
                        join od in db.CTDonHangs
                        on o.MaDH equals od.MaDH
                        join p in db.SANPHAMs
                        on od.MaSP equals p.MaSP
                        select new
                        {
                            CreatedDate= o.NgayDat,
                            Quantity=od.SLuong,
                            Price=od.Gia,
                            OriginalPrice=p.GiaGoc
                        };
            if (month != 0)
            {
                query = query.Where(x => x.CreatedDate.Value.Month == month);
            }

            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(x => new
            {
                Date = x.Key.Value,
                TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                TotalSell = x.Sum(y => y.Quantity * y.Price),
            }).Select(x => new
            {
                Date = x.Date,
                DoanhThu = x.TotalSell,
                LoiNhuan = (float)x.TotalSell - x.TotalBuy
            });
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult TKDoanhThuNam(int year)
        {
            var query = from o in db.DONHANGs
                        join od in db.CTDonHangs
                        on o.MaDH equals od.MaDH
                        join p in db.SANPHAMs
                        on od.MaSP equals p.MaSP
                        select new
                        {
                            CreatedDate = o.NgayDat,
                            Quantity = od.SLuong,
                            Price = od.Gia,
                            OriginalPrice = p.GiaGoc
                        };
            if (year != 0)
            {
                query = query.Where(x => x.CreatedDate.Value.Year == year);
            }

            var result = query.GroupBy(x => new { x.CreatedDate.Value.Year, x.CreatedDate.Value.Month }).Select(x => new
            {
                year = x.Key.Year,
                month = x.Key.Month,
                TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                TotalSell = x.Sum(y => y.Quantity * y.Price),
            }).Select(x => new
            {
                Nam = x.year,
                Thang = x.month,
                DoanhThu = x.TotalSell,
                LoiNhuan = (float)x.TotalSell - x.TotalBuy
            });
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DonHangIndex()
        {
            var dh = db.DONHANGs.OrderByDescending(n=>n.NgayDat).Take(5).ToList();
            return View(dh);
        }
        // phần sản phẩm
        public ActionResult SanPham(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            return View(db.SANPHAMs.ToList().OrderBy(n => n.MaSP).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //phương thức "uploadFile()"
        [HttpPost]
        public JsonResult UploadFile(HttpPostedFileBase uploadedFiles)
        {
            string returnImagePath = string.Empty;
            string fileName;
            string Extension;
            string imageName;
            string imageSavePath;

            if (uploadedFiles.ContentLength > 0)
            {
                fileName = Path.GetFileNameWithoutExtension(uploadedFiles.FileName);
                Extension = Path.GetExtension(uploadedFiles.FileName);
                imageName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss");
                imageSavePath = Server.MapPath("/QuanAo/") + imageName +
                Extension;

                uploadedFiles.SaveAs(imageSavePath);
                returnImagePath = "/QuanAo/" + imageName +
                Extension;
            }

            return Json(Convert.ToString(returnImagePath), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ThemSP()
        {
            ViewBag.MaKM = new SelectList(db.KHUYENMAIs, "MaKM", "TenKM");
            ViewBag.MaLoai = new SelectList(db.THELOAIs, "MaLoai", "TheLoai1");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemSP([Bind(Include = "MaSP,TenSP,Mota,Gia,HinhAnh,MaKM,MaLoai,NgayCapNhat,GiaGoc")] SANPHAM sp, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnh.ContentLength > 0)
                    {
                        string _FileName = Path.GetFileName(HinhAnh.FileName);
                        string _path = Path.Combine(Server.MapPath("~/QuanAo/"), _FileName);
                        HinhAnh.SaveAs(_path);
                        sp.HinhAnh = _FileName;
                    }
                    sp.NgayCapNhat = DateTime.Now;
                    db.SANPHAMs.Add(sp);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Message = "không thành công!!";
                }

            }

            ViewBag.MaKM = new SelectList(db.KHUYENMAIs, "MaKM", "TenKM", sp.KHUYENMAI.MaKM);
            ViewBag.MaLoai = new SelectList(db.THELOAIs, "MaLoai", "TheLoai1", sp.THELOAI.MaLoai);
            return View(sp);
        }
        [HttpGet]
        public ActionResult XoaSP(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sp = db.SANPHAMs.Find(id);
            if (sp == null)
            {
                return HttpNotFound();
            }
            return View(sp);
        }

        [HttpPost, ActionName("XoaSP")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SANPHAM book = db.SANPHAMs.Find(id);
            db.SANPHAMs.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM book = db.SANPHAMs.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }
        [HttpGet]
        public ActionResult SuaSP(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sp = db.SANPHAMs.Find(id);
            if (sp == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaKM = new SelectList(db.KHUYENMAIs, "MaKM", "TenKM");
            ViewBag.MaLoai = new SelectList(db.THELOAIs, "MaLoai", "TheLoai1");
            return View(sp);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SuaSP([Bind(Include = "MaSP,TenSP,Mota,Gia,HinhAnh,MaKM,MaLoai,NgayCapNhat,GiaGoc")] SANPHAM sp, HttpPostedFileBase HinhAnh, FormCollection form)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnh != null)
                    {
                        string _FileName = Path.GetFileName(HinhAnh.FileName);
                        string _path = Path.Combine(Server.MapPath("~/QuanAo"), _FileName);
                        HinhAnh.SaveAs(_path);
                        sp.HinhAnh = _FileName;
                        // get Path of old image for deleting it
                        _path = Path.Combine(Server.MapPath("~/QuanAo"), form["oldimage"]);
                        if (System.IO.File.Exists(_path))
                            System.IO.File.Delete(_path);

                    }
                    else 
                    {
                        sp.HinhAnh = form["oldimage"];
                        sp.NgayCapNhat=DateTime.Now;
                        db.Entry(sp).State = EntityState.Modified;
                        foreach (var newSize in sp.CTSizes)
                        {
                            db.Entry(newSize).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                    
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "không thành công!!" + ex.Message;
                }
                return RedirectToAction("SanPham");
            }
            ViewBag.MaKM = new SelectList(db.KHUYENMAIs, "MaKM", "TenKM", sp.KHUYENMAI.MaKM);
            ViewBag.MaLoai = new SelectList(db.THELOAIs, "MaLoai", "TheLoai1", sp.THELOAI.MaLoai);
            return View(sp);
        }

        // phần thể loại


        public ActionResult TheLoai()
        {
            var dh = db.THELOAIs.ToList();
            return View(dh);
        }
        [HttpGet]
        public ActionResult ThemTheLoai()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemTheLoai([Bind(Include = "MaLoai,TheLoai1")] THELOAI sp)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.THELOAIs.Add(sp);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Message = "không thành công!!";
                }

            }
            return View(sp);
        }
        [HttpGet]
        public ActionResult XoaTheLoai(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THELOAI sp = db.THELOAIs.Find(id);
            if (sp == null)
            {
                return HttpNotFound();
            }
            return View(sp);
        }

        [HttpPost, ActionName("XoaTheLoai")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedtl(int id)
        {
            THELOAI sp = db.THELOAIs.Find(id);
            db.THELOAIs.Remove(sp);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult SuaTheLoai(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THELOAI km = db.THELOAIs.Find(id);
            if (km == null)
            {
                return HttpNotFound();
            }
            return View(km);
        }
        [HttpPost, ActionName("SuaTheLoai")]
        [ValidateAntiForgeryToken]
        public ActionResult SuaTheLoaiPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var studentToUpdate = db.THELOAIs.Find(id);
            if (TryUpdateModel(studentToUpdate, "",
               new string[] { "MaLoai", "TheLoai1" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }



        //phần mã giảm giá

        public ActionResult KhuyenMai()
        {
            var dh = db.KHUYENMAIs.ToList();
            return View(dh);
        }
        [HttpGet]
        public ActionResult ThemKhuyenMai()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemKhuyenMai([Bind(Include = "Mota, GiaTriKM, NgayBD, NgayKT, TenKM")] KHUYENMAI km)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.KHUYENMAIs.Add(km);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Message = "không thành công!!";
                }
            }
            return View(km);
        }

        [HttpGet]
        public ActionResult XoaKhuyenMai(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHUYENMAI km = db.KHUYENMAIs.Find(id);
            if (km == null)
            {
                return HttpNotFound();
            }
            return View(km);
        }

        [HttpPost, ActionName("XoaKhuyenMai")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedKM(int id)
        {
            KHUYENMAI km = db.KHUYENMAIs.Find(id);
            db.KHUYENMAIs.Remove(km);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SuaKhuyenMai(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHUYENMAI km = db.KHUYENMAIs.Find(id);
            if (km == null)
            {
                return HttpNotFound();
            }
            return View(km);
        }

        [HttpPost, ActionName("SuaKhuyenMai")]
        [ValidateAntiForgeryToken]
        public ActionResult SuaKhuyenMaiPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var studentToUpdate = db.KHUYENMAIs.Find(id);
            if (TryUpdateModel(studentToUpdate, "",
               new string[] { "MaKM", "Mota", "GiaTriKM", "NgayBD", "NgayKT", "TenKM" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }

        // phần đơn hàng
        public ActionResult DonHang()
        {
            var dh = db.DONHANGs.ToList();
            return View(dh);
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

        [HttpGet]
        public ActionResult SuaDonHang(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DONHANG user = db.DONHANGs.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var tinhTrangList = db.TINHTRANGs.Select(x => new SelectListItem
            {
                Text = x.tinhtranggiaohang.ToString(), // Assume TinhTrang is int, otherwise, convert it to string as needed
                Value = x.id.ToString()
            }).Distinct().ToList();
            ViewBag.TinhTrang = new SelectList(tinhTrangList, "Value", "Text");
            return View(user);
        }

        [HttpPost, ActionName("SuaDonHang")]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDonHangPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var DonHangToUpdate = db.DONHANGs.Find(id);
            if (TryUpdateModel(DonHangToUpdate, "",
               new string[] { "MaDH", "NgayDat", "UserID", "NgayGiao", "DaThanhToan", "TinhTrang", "TongTien" }))
            {
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("DonHang");
                }
                catch (DataException /* dex */)
                {
                    ModelState.AddModelError("", "Không thể lưu thay đổi. Hãy thử lại.");
                }
            }
            return View(DonHangToUpdate);
        }

        // phần user 
        public ActionResult QLUser(int? page)
        {
            var dh = db.USERs.ToList();
            return View(dh);
        }

        [HttpGet]
        public ActionResult ThemUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemUser([Bind(Include = "Hoten, DiaChi, SDT, UserName, Password, Email")] USER user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.USERs.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("QLUser");
                }
                catch
                {
                    ViewBag.Message = "không thành công!!";
                }
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult XoaUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER user = db.USERs.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("XoaUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedUser(int id)
        {
            USER user = db.USERs.Find(id);
            db.USERs.Remove(user);
            db.SaveChanges();
            return RedirectToAction("QLUser");
        }

        [HttpGet]
        public ActionResult SuaUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER user = db.USERs.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("SuaUser")]
        [ValidateAntiForgeryToken]
        public ActionResult SuaUserPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userToUpdate = db.USERs.Find(id);
            if (TryUpdateModel(userToUpdate, "",
               new string[] { "UserID", "Hoten", "DiaChi", "SDT", "UserName", "Password", "Email" }))
            {
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("QLUser");
                }
                catch (DataException /* dex */)
                {
                    ModelState.AddModelError("", "Không thể lưu thay đổi. Hãy thử lại.");
                }
            }
            return View(userToUpdate);
        }
        public ActionResult binhluan()
        {
            var dh = db.RATINGs.ToList();
            return View(dh);
        }
        [HttpGet]
        public ActionResult XoaBL(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RATING user = db.RATINGs.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("XoaBL")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedBL(int id)
        {
            RATING user = db.RATINGs.Find(id);
            db.RATINGs.Remove(user);
            db.SaveChanges();
            return RedirectToAction("binhluan");
        }
    }
}
