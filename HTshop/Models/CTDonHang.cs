﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HTshop.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public partial class CTDonHang
    {
        [DisplayName("Mã đơn hàng")]
        public int MaDH { get; set; }
        [DisplayName("Tên sản phẩm")]
        public int MaSP { get; set; }
        [DisplayName("Số Lượng")]
        public Nullable<int> SLuong { get; set; }
        [DisplayName("Giá")]
        public Nullable<decimal> Gia { get; set; }
        [DisplayName("Size")]
        public string Size { get; set; }
    
        public virtual DONHANG DONHANG { get; set; }
        public virtual SANPHAM SANPHAM { get; set; }
    }
}
