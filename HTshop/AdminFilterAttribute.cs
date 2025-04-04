using System.Web.Mvc;

namespace HTshop
{
    public class AdminFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra xem session admin có tồn tại không
            if (filterContext.HttpContext.Session["Taikhoanadmin"] == null)
            {
                // Nếu session là null, chuyển hướng về trang chủ
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Nếu session tồn tại, tiếp tục thực hiện action
            base.OnActionExecuting(filterContext);
        }
    }
}
