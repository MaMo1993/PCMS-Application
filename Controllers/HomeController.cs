using PCMS_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace PCMS_Application.Controllers
{
    public class HomeController : Controller
    {
        private PCMS_DbEntities db = new PCMS_DbEntities();

        public ActionResult Index()
        {
            var children = db.TblLearners.Count();
            var parents = db.TblGuardians.Count();
            var teachers = db.TblTeachers.Count();
            var schools = db.TblSchools.Count();

            ViewBag.Children = children;
            ViewBag.Parents = parents;
            ViewBag.Teachers = teachers;
            ViewBag.Schhols = schools;


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMsg(FormCollection form)
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            if (form["email"] != null && form["email"] != "" && form["name"] != null && form["name"] != "" && form["message"] != null && form["message"] != "")
            {
                var senderEmail = new MailAddress("melshops93@gmail.com", "PCMS Alerts");
                var receiverEmail = new MailAddress("melshops93@gmail.com");
                var senderEmailPW = "Shopping1.";

                string subject = form["subject"] != "" ? form["subject"] : "No Subject";

                string body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='50%'>" +
                    "<tr style='background-color:steelblue;color:white'><td height='100'><font size='4'><b>Message from App</b></font></td></tr>" +
                    "<tr><td><h3>Hello PCMS User</h3></td></tr>" +
                    "<tr><td>" + form["message"] + "</td></tr><tr><td>From: " + form["name"] + "(" + form["email"] + ")</td></tr></table>";

                var smpt = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, senderEmailPW)
                };

                var message = new MailMessage(senderEmail, receiverEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                smpt.Send(message);

                TempData["Sent"] = "Your message has been sent. Thank you!";
            }

            return RedirectToAction("Index","Home");
        }       

    }
}