using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using HTshop.Models;
using HTshop.Others;
using PayPal.Api;

namespace HTshop.Controllers
{
    public class GioHangController : Controller
    {
        HTShopEntities db=new HTShopEntities();
        // GET: GioHang
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }
        public ActionResult ThemGioHang(int ms, string selectedSize,int quantity,string url)
        {
            try
            {
                List<GioHang> lstGioHang = LayGioHang();
                GioHang sp = lstGioHang.Find(n => n.iMaSP == ms);

                if (sp == null)
                {
                    sp = new GioHang(ms, selectedSize, quantity);
                    lstGioHang.Add(sp);
                }
                else
                {
                    sp.iSoLuong =sp.iSoLuong+quantity;
                }

                return Redirect(url);
            }
            catch (Exception ex)
            {
                // Xử lý exception tại đây (ví dụ: ghi log, hiển thị thông báo lỗi)
                return RedirectToAction("Error", "Home");
            }
        }
        //Tính tổng SL
        private int TongSoLuong()
        {
            int iTongSL = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSL = lstGioHang.Sum(n => n.iSoLuong);
            }
            return iTongSL;
        }
        //Tính Tổng  
        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => n.ThanhTien);
            }
            return dTongTien;
        }
        //Action trả về View GioHang
        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            ViewBag.CheckCart = lstGioHang;
            return View(lstGioHang);
        }
        public ActionResult GioHangPartial()
        {
            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.CountProduct=lstGioHang.Count();
            return PartialView();
        }
        public ActionResult Partial_Item_Cart()
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang != null)
            {
                return PartialView(lstGioHang);
            }
            return PartialView();
        }
        public ActionResult XoaSPKhoiGioHang(int imasp)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iMaSP == imasp);
            if (sp != null)
            {
                lstGioHang.RemoveAll(n => n.iMaSP == imasp);
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult CapNhatGioHang(int imasp, FormCollection f)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iMaSP == imasp);
            if (sp != null)
            {
                sp.iSoLuong = int.Parse(f["SoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult XoaGioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear();
            return RedirectToAction("Index", "Product");
        }
        public ActionResult Partial_Item_Payment()
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang != null)
            {
                return PartialView(lstGioHang);
            }
            return PartialView();
        }
        public ActionResult Partial_CheckOut()
        {
            return PartialView();
        }
        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaikhoanUser"] == null)
                return RedirectToAction("Login", "Home");
            if (Session["GioHang"] == null)
                return RedirectToAction("Index", "Home");
            List<GioHang> lstGiohang = LayGioHang();
            ViewBag.TongSl = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(lstGiohang);
        }
        public int Size(string size)
        {
            var item = db.SIZEs.FirstOrDefault(si => si.Size1 == size);
            if (item != null)
            {
                int sizeId = item.MaSize;
                return sizeId;
            }
            return -1;
        }
        int MaPT;
        [HttpPost]
        public ActionResult DatHang(FormCollection form, OrderViewModel model)
        {
            DONHANG dh = new DONHANG();
            USER kh = (USER)Session["TaikhoanUser"];
            List<GioHang> lstgiohang = LayGioHang();
            dh.UserID = kh.UserID;
            dh.NgayDat = DateTime.Now;
            dh.NgayGiao = DateTime.Today.AddDays(5);
            dh.DaThanhToan = false;
            dh.TinhTrang = 1;
            dh.TongTien = TongTien();
            db.DONHANGs.Add(dh);
            db.SaveChanges();
            foreach (var item in lstgiohang)
            {
                CTDonHang ctdh = new CTDonHang();
                ctdh.MaDH = dh.MaDH;
                ctdh.MaSP = item.iMaSP;
                ctdh.SLuong = item.iSoLuong;
                ctdh.Gia = (decimal)item.dGia;
                if (item.Size != null)
                    ctdh.Size = item.Size;
                db.CTDonHangs.Add(ctdh);
            }
            db.SaveChanges();
            TempData["MaPThuc"] = model.TypePayment;
            // Session["GioHang"] = null;
            if (model.TypePayment == 1)
                return RedirectToAction("PaymentWithVnpay", "GioHang");
            else if (model.TypePayment == 2)
                return RedirectToAction("PaymentWithPaypal", "GioHang");
            return RedirectToAction("PaymentWithCOD", "GioHang");
        }
        public ActionResult XacNhanDonhang()
        {
            return View();
        }
        void CapNhatSL(int MaDH)
        {
            List<CTDonHang> dsCTDH = db.CTDonHangs.Where(c => c.MaDH == MaDH).ToList();

            // Lặp qua từng chi tiết đơn hàng và cập nhật số lượng
            foreach (CTDonHang CTDHnew in dsCTDH)
            {
                // Lấy thông tin chi tiết size của sản phẩm theo size và maSP
                CTSize ctSize = db.CTSizes.FirstOrDefault(c => c.MaSP == CTDHnew.MaSP && c.SIZE.Size1 == CTDHnew.Size);

                // Kiểm tra nếu chi tiết size tồn tại và số lượng mua không vượt quá số lượng trong kho
                if (ctSize != null && ctSize.Sluong >= CTDHnew.SLuong)
                {
                    ctSize.Sluong -= CTDHnew.SLuong;
                }
            }

            // Lưu các thay đổi vào CSDL
            db.SaveChanges();
        }
        public ActionResult PaymentWithCOD()
        {
            USER kh = (USER)Session["TaikhoanUser"];
            List<GioHang> lstgiohang = LayGioHang();
            var DHnew = db.DONHANGs.OrderByDescending(hd => hd.NgayDat).FirstOrDefault();
            THANHTOAN tt = new THANHTOAN();
            Random rd = new Random();
            tt.MaTT = rd.Next(0, 10000).ToString();
            tt.MaDH = DHnew.MaDH;
            tt.NgayTra = DateTime.Now;
            tt.TienTra = Convert.ToDouble(TongTien().ToString());
            if (TempData.ContainsKey("MaPThuc"))
            {
                if (TempData["MaPThuc"] is int mapt)
                {
                    if (mapt == 1)
                        tt.MaPT = 1;
                    else if (mapt == 2)
                        tt.MaPT = 2;
                    if (mapt == 3)
                        tt.MaPT = 3;

                }
            }
            db.THANHTOANs.Add(tt);
            db.SaveChanges();
            CapNhatSL(DHnew.MaDH);
            double tongtien1 = 0;
            // Gửi email xác nhận đơn hàng
            string emailBody = "Xin chào " + kh.Hoten + ",\n\n Cảm ơn bạn đã đặt hàng tại HTshop.Thông tin về đơn hàng của bạn:\n\n";
            foreach (var item in lstgiohang)
            {
                emailBody += "Tên Sản Phẩm: " + item.sTenSP + "Hình ảnh Sản Phẩm" +
                    item.sAnh + "\nSố lượng: " + item.iSoLuong.ToString()
                    + "\nĐơn giá: " + item.ThanhTien.ToString() + "\n\n";
                tongtien1 = Convert.ToDouble(TongTien().ToString());
            }
            emailBody += "Tổng tiền: " + tongtien1.ToString() + "\n\n Chúng tôi sẽ  liên hệ với bạn sớm nhất có thể để xác nhận đơn hàng. Cảm ơn bạn đã mua hàng tại HTShop của chúng tôi.\n\nTrân trọng,\nHTShop";
            GuiEmail(kh.Email, "Thông tin đơn hàng từ HTShop", emailBody);
            ViewBag.Message = "Đặt hàng thành công hóa đơn " + DHnew.MaDH;
            Session["GioHang"] = null;
            return View();
        }
        private void GuiEmail(string toEmail, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.From = new MailAddress("alba18302010@gmail.com");
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = false;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com"; // Thay đổi thành SMTP server của bạn
            smtp.Port = 587; // Thay đổi thành cổng SMTP của bạn
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("alba18302010@gmail.com", "tmyr ujul xvhe twzv");
            smtp.EnableSsl = true;

            smtp.Send(mail);
        }
        public ActionResult PaymentWithVnpay()
        {
            OrderViewModel model= new OrderViewModel();
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", (TongTien()*100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY                                                               
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult PaymentVnpayConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                USER kh = (USER)Session["TaikhoanUser"];
                List<GioHang> lstgiohang = LayGioHang();
                if (checkSignature)
                {
                    var DHnew = db.DONHANGs.OrderByDescending(hd => hd.NgayDat).FirstOrDefault();
                    int ttoan=0;
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        DHnew.DaThanhToan = true;
                        db.SaveChanges();
                        THANHTOAN tt = new THANHTOAN();
                        Random rd = new Random();
                        tt.MaTT = rd.Next(0, 10000).ToString();
                        tt.MaDH = DHnew.MaDH;
                        tt.NgayTra = DateTime.Now;
                        tt.TienTra=Convert.ToDouble(TongTien().ToString());
                        if (TempData.ContainsKey("MaPThuc"))
                        {
                            // Chuyển đổi giá trị từ object sang int
                            if (TempData["MaPThuc"] is int mapt)
                            {
                                if (mapt == 1)
                                {
                                    tt.MaPT = 1;
                                    ttoan = 1;
                                }    
                                else if (mapt == 2)
                                {
                                    tt.MaPT = 2;
                                    ttoan = 2;
                                }
                                if (mapt == 3)
                                {
                                    tt.MaPT = 3;
                                    ttoan = 3;
                                }
                            }
                        }   
                        db.THANHTOANs.Add(tt);
                        db.SaveChanges();
                        CapNhatSL(DHnew.MaDH);
                        double tongtien1 = 0;
                        // Gửi email xác nhận đơn hàng
                        string emailBody = "Xin chào " + kh.Hoten + ",\n\n Cảm ơn bạn đã đặt hàng tại HTshop.Thông tin về đơn hàng của bạn:\n\n";
                        foreach (var item in lstgiohang)
                        {
                            emailBody += "Tên Sản Phẩm: " + item.sTenSP + "Hình ảnh Sản Phẩm" +
                                item.sAnh + "\nSố lượng: " + item.iSoLuong.ToString()
                                + "\nĐơn giá: " + item.ThanhTien.ToString() + "\n\n";
                            tongtien1 = Convert.ToDouble(TongTien().ToString());
                        }
                        emailBody += "Tổng tiền: " + tongtien1.ToString() + "\n\n Chúng tôi sẽ  liên hệ với bạn sớm nhất có thể để xác nhận đơn hàng. Cảm ơn bạn đã mua hàng tại HTShop của chúng tôi.\n\nTrân trọng,\nHTShop";
                        GuiEmail(kh.Email, "Thông tin đơn hàng từ HTShop", emailBody);
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + DHnew.MaDH + " | Mã giao dịch: " + tt.MaTT;
                        Session["GioHang"] = null;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + DHnew.MaDH + " | Mã giao dịch: " + ttoan + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
        }
        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            USER kh = (USER)Session["TaikhoanUser"];
            List<GioHang> lstgiohang = LayGioHang();
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/GioHang/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                    else
                    {
                        var DHnew = db.DONHANGs.OrderByDescending(hd => hd.NgayDat).FirstOrDefault();
                        DHnew.DaThanhToan = true;
                        db.SaveChanges();
                        THANHTOAN tt = new THANHTOAN();
                        Random rd = new Random();
                        tt.MaTT = rd.Next(0, 10000).ToString();
                        tt.MaDH = DHnew.MaDH;
                        tt.NgayTra = DateTime.Now;
                        tt.TienTra = Convert.ToDouble(TongTien().ToString());
                        if (TempData.ContainsKey("MaPThuc"))
                        {
                            // Chuyển đổi giá trị từ object sang int
                            if (TempData["MaPThuc"] is int mapt)
                            {
                                if (mapt == 1)
                                    tt.MaPT = 1;
                                else if (mapt == 2)
                                    tt.MaPT = 2;
                                if (mapt == 3)
                                    tt.MaPT = 3;
                            }
                        }
                        TempData["MaTT"] = tt.MaTT;
                        db.THANHTOANs.Add(tt);
                        db.SaveChanges();
                        CapNhatSL(DHnew.MaDH);
                        double tongtien1 = 0;
                        // Gửi email xác nhận đơn hàng
                        string emailBody = "Xin chào " + kh.Hoten + ",\n\n Cảm ơn bạn đã đặt hàng tại HTshop.Thông tin về đơn hàng của bạn:\n\n";
                        foreach (var item in lstgiohang)
                        {
                            emailBody += "Tên Sản Phẩm: " + item.sTenSP + "Hình ảnh Sản Phẩm" +
                                item.sAnh + "\nSố lượng: " + item.iSoLuong.ToString()
                                + "\nĐơn giá: " + item.ThanhTien.ToString() + "\n\n";
                            tongtien1 = Convert.ToDouble(TongTien().ToString());
                        }
                        emailBody += "Tổng tiền: " + tongtien1.ToString() + "\n\n Chúng tôi sẽ  liên hệ với bạn sớm nhất có thể để xác nhận đơn hàng. Cảm ơn bạn đã mua hàng tại HTShop của chúng tôi.\n\nTrân trọng,\nHTShop";
                        GuiEmail(kh.Email, "Thông tin đơn hàng từ HTShop", emailBody);
                        Session["GioHang"] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            return View("SuccessView");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private double TongTien_USD()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => Math.Round(n.dGia / 24380, 2) * n.iSoLuong);
            }
            return dTongTien;
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            List<GioHang> lstgiohang = LayGioHang();
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            foreach (var item in lstgiohang)
            {
                itemList.items.Add(new Item()
                {
                    name = item.sTenSP + " Size "+item.Size,
                    currency = "USD",
                    price = Math.Round(item.dGia / 24380, 2).ToString(),
                    quantity = item.iSoLuong.ToString(),
                    sku = item.iMaSP.ToString(),
                });
            }
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = TongTien_USD().ToString(),
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = TongTien_USD().ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }
    }
}