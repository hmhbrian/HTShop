using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HTshop.Models
{
    public class GioHang
    {
        public int iMaSP { get; set; }
        public string sTenSP { get; set; }
        public string sAnh { get; set; }
        public double dGia { get; set; }
        public string Size { get; set; }
        public int iSoLuong { get; set; }
        public double ThanhTien => iSoLuong * dGia;

        public GioHang(int maSP, string selectedSize, int quantity)
        {
            using (var db = new HTShopEntities())
            {
                var sp = db.SANPHAMs.SingleOrDefault(n => n.MaSP == maSP);

                if (sp == null)
                {
                    // Xử lý trường hợp không tìm thấy sản phẩm
                    throw new Exception("Sản phẩm không tồn tại.");
                }

                iMaSP = maSP;
                sTenSP = sp.TenSP;
                sAnh = sp.HinhAnh;
                if (sp.MaKM == null)
                    dGia = (double)sp.Gia;
                else
                    dGia = Convert.ToDouble(sp.Gia * (100 - sp.KHUYENMAI.GiaTriKM) / 100);
                iSoLuong = quantity;
                Size = selectedSize;
            }
        }
    }

}