using HTshop.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HTshop.Controllers
{
    public class HomeController : Controller
    {
        HTShopEntities db=new HTShopEntities();
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["username"];
            var matkhau = collection["password"];
            if (string.IsNullOrEmpty(tendn))
            {
                ViewBag.Loi1 = "Phải nhập tên  đăng nhập";

            }
            if (string.IsNullOrEmpty(matkhau))
            {
                ViewBag.Loi2 = "Phải nhập mật khẩu";

            }
            else
            {
                ADMIN ad = db.ADMINs.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);
                USER us = db.USERs.SingleOrDefault(n => n.UserName == tendn && n.Password == matkhau);
                if (ad != null)
                {
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else if (us != null)
                {
                    Session["TaikhoanUser"] = us;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Dangky()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Dangky([Bind(Include = "UserID,Hoten,DiaChi,SDT,UserName,Password,Email")] USER us, FormCollection f)
        {
            var tendn = f["username"];
            var ht = f["Name"];
            var sdt = f["phone"];
            var email = f["Email"];
            var pw = f["Password"];
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên người dùng hoặc email đã tồn tại trong cơ sở dữ liệu chưa
                if (db.USERs.Any(u => u.UserName == us.UserName || u.Email == us.Email))
                {
                    // Nếu đã tồn tại, thông báo lỗi và đưa ra view
                    ViewData["err3"] = "Tên người dùng hoặc Email đã tồn tại. Vui lòng chọn thông tin khác.";
                    return View(us);
                }
                try
                {
                    us.Hoten = ht;
                    us.SDT = sdt;
                    us.Email = email;
                    us.UserName = tendn;
                    us.Password = pw;
                    db.USERs.Add(us);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                catch
                {
                    ViewBag.Message = "không thành công!!";
                }
            }
            return View(us);
        }
        public List<SANPHAM> Laysanphammoi(int count)
        {
            var danhSachSP = db.SANPHAMs.OrderByDescending(a => a.NgayCapNhat).Take(count).ToList();
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
        public ActionResult Index()
        {
            var sp = Laysanphammoi(5);
            return View(sp);
        }
        public ActionResult Find(string searchString)
        {
            ViewBag.Keyword=searchString; 
            var danhSachSP = db.SANPHAMs.Include(b => b.THELOAI).Include(b => b.KHUYENMAI);
            if(!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                danhSachSP=danhSachSP.Where(b=>b.TenSP.ToLower().Contains(searchString));
            }
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
            return View(danhSachSP.ToList());
        }
        public List<SANPHAM> Laysanphamrandom(int count)
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 100); // Lấy số ngẫu nhiên từ 1 đến 100
            ViewBag.Number = randomNumber;
            return db.SANPHAMs.OrderByDescending(a => a.MaSP == randomNumber).Take(count).ToList();
        }
        public ActionResult spIndex()
        {
            var danhSachSP = Laysanphamrandom(6);
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
            return View(danhSachSP);

        }
        public ActionResult LienHe()
        {
            return View();
        }
        public ActionResult GioiThieu()
        {
            return View();
        }
        public ActionResult Footer()
        {
            return View();
        }
    }
}