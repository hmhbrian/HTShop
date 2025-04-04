using HTshop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace HTshop.Controllers
{
    public class MailController : Controller
    {
        // GET: Mail
        public ActionResult SendMail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendMail(Mail model)
        {
            //Cấu hình thông tin gmail (khai báo thư viện System.Net)
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                //(Khai báo thư viện System.Net)
                Credentials = new NetworkCredential("alba18302010@gmail.com", "tmyr ujul xvhe twzv"),
                // your email(abc@gmail.com) and your password(****)
                EnableSsl = true
            };
            //Tao email
            MailMessage message = new MailMessage();
            message.From = new MailAddress(model.From); message.ReplyToList.Add(model.From);
            message.To.Add(new MailAddress(model.To));
            message.Subject = model.Subject;
            message.Body = model.Notes;

            var f = Request.Files["attachment"];
            var path = Path.Combine(Server.MapPath("~/UploadFile"), f.FileName); if (!System.IO.File.Exists(path))
            {
                f.SaveAs(path);
            }
            //(Khai báo thư viện System.Net.Mime)
            Attachment data = new Attachment(Server.MapPath("~/UploadFile/" + f.FileName), MediaTypeNames.Application.Octet);
            message.Attachments.Add(data);
            //Gửi email
            mail.Send(message);
            return View("SendMail");
        }
    }
}