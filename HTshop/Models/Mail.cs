using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HTshop.Models
{
    public class Mail
    {
        [DisplayName("Người Gửi:")]
        public string From { get; set; }

        [DisplayName("Người Nhận:")]
        public string To { get; set; }

        [DisplayName("Tiêu Đề:")]
        public string Subject { get; set; }

        [DisplayName("Nội Dung:")]
        public string Notes { get; set; }

        [DisplayName("File đính kèm :")]
        public string Attachment { get; set; }
    }
}