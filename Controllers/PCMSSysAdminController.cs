using System.Web.Mvc;
using System.Security.Cryptography;
using System.Web.UI;
using System.Text;
using System.Web.Security;
using System.Data.Entity.Core.EntityClient;
using System.Threading;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using PCMS_Application.Models;
using PCMS_Application.Controllers;
using System;
using System.Linq;
using System.Web;

namespace PCMS_Application.Controllers
{
    public class PCMSSysAdminController : Controller
    {
        // GET: PCMSSysAdmin
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();
        private static string EmailG;
        private static int UserIdG;


        // GET: SysAdmin
        public ActionResult Login()
        {

            //Uri currentUrl = Request.UrlReferrer;
            //if (currentUrl == null)
            //{
            //    return HttpNotFound();
            //}

            return View("Login");
        }

        #region Delete after first create
        //public void Create()
        //{
        //    var sysAdmin = db.TblSystemAdmins;

        //    if (sysAdmin.Any())
        //    {
        //        db.TblSystemAdmins.RemoveRange(sysAdmin);
        //        db.SaveChanges();
        //    }

        //    TblSystemAdmin tblSysAdmin = new TblSystemAdmin();
        //    Random numb = new Random();

        //    tblSysAdmin.UserId = ((numb.Next(100000000, 999999999)) * -1);
        //    tblSysAdmin.Username = Hash("PCMS");
        //    tblSysAdmin.PasswordHash = Hash("PCMS@Admin1");
        //    tblSysAdmin.EmailAddress = "melshops93@gmail.com";

        //    db.TblSystemAdmins.Add(tblSysAdmin);
        //    db.SaveChanges();
        //}
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "SchoolId")] TblSchool tblSchool, FormCollection form)
        {

            //Uri currentUrl = Request.UrlReferrer;
            //if (currentUrl == null)
            //{
            //    return HttpNotFound();
            //}

            //Create(); //Only Called when we need to create the first PCMS User
            try
            {
                string username = Hash(form["EmailAddress"]);
                string password = form["PasswordHash"];
                var user = db.TblSystemAdmins.Where(t => t.Username == username);

                if (user.Any())
                {
                    if (user.FirstOrDefault().PasswordHash == Hash(password))
                    {
                        SetGlobals(user.FirstOrDefault().UserId, user.FirstOrDefault().EmailAddress);

                        try
                        {
                            string userData = "";
                            int? schoolId = tblSchool.SchoolId;
                            if (schoolId == null)
                            {
                                schoolId = 0;
                            }
                            userData = EmailG + ";" + schoolId + ";" + 0 + ";" + UserIdG + ";";


                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, EmailG, DateTime.Now,
                            DateTime.Now.AddDays(2),
                            true,
                            userData,
                            FormsAuthentication.FormsCookiePath);

                            // Encrypt the ticket.
                            string encTicket = FormsAuthentication.Encrypt(ticket);

                            // Create the cookie.
                            HttpCookie userCookie = new HttpCookie("UserInformation", encTicket);
                            userCookie.Expires = DateTime.Now.AddDays(2);

                            Response.Cookies.Add(userCookie);
                            //return RedirectToAction("NavigationHomeIndex", "PCMSNavigation");
                            return RedirectToAction("SelectSchool");
                        }
                        catch (Exception e)
                        {
                            string error = e.Message;
                        }

                        //return RedirectToAction("NavigationHomeIndex", "PCMSNavigation");
                        return RedirectToAction("SelectSchool");
                    }
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

            return RedirectToAction("Login");
        }

        public void SetGlobals(int uId, string mail)
        {
            UserIdG = uId;
            EmailG = mail;
        }

        //GET
        public ActionResult SelectSchool()
        {
            List<SchoolList> schoolList = new List<SchoolList>();

            var allSchool = db.TblSchools;


            foreach (var school in allSchool)
            {
                string listText = school.SchoolCode;
                listText += " - " + school.SchoolName;

                schoolList.Add(new SchoolList
                {
                    SchoolId = school.SchoolId,
                    School = listText,
                });
            }

            ViewBag.SchoolId = new SelectList(schoolList, "SchoolId", "School");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectSchool([Bind(Include = "SchoolId")] TblSchool tblSchool)
        {
            try
            {
                string userData = "";
                int schoolId = tblSchool.SchoolId;

                userData = EmailG + ";" + schoolId + ";" + 0 + ";" + UserIdG + ";";


                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, EmailG, DateTime.Now,
                DateTime.Now.AddDays(2),
                true,
                userData,
                FormsAuthentication.FormsCookiePath);

                // Encrypt the ticket.
                string encTicket = FormsAuthentication.Encrypt(ticket);

                // Create the cookie.
                HttpCookie userCookie = new HttpCookie("UserInformation", encTicket);
                userCookie.Expires = DateTime.Now.AddDays(2);

                Response.Cookies.Add(userCookie);
                return RedirectToAction("NavigationHomeIndex", "PCMSNavigation");
            }
            catch (Exception e)
            {
                string error = e.Message;
            }


            return RedirectToAction("LoginHomeIndex");

        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(FormCollection form)
        {
            var sysAdmin = db.TblSystemAdmins.FirstOrDefault();

            if (sysAdmin != null)
            {
                //Send Email for reset password
                string restCode = Guid.NewGuid().ToString();
                SendVerificationEmail(sysAdmin.EmailAddress, restCode, "ResetPassword");

                //Confirm password does not have issues
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
            }
            TempData["ResetCheck"] = "Reset password link has been sent to your email address";
            return View();
        }

        [NonAction]
        public void SendVerificationEmail(string emailID, string activationCode, string emailFor = "ResetPassword")
        {
            var verifyUrl = "/PCMSSysAdmin/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("melshops93@gmail.com", "PCMS Notifications");

            var toMail = new MailAddress(emailID);

            var fromEmailPassword = "Shopping1.";

            string subject = "";
            string body = "";
            if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";

                body = "Hi,<br/><br/>We got request for reset your account password. Please click on the below link to reset password" +
                    "<br/><br/><a href=" + link + "> Reset password link</a>";
            }

            var smpt = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toMail)
            {

                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smpt.Send(message);

        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(FormCollection form)
        {
            if (form["PasswordHash"] != form["ConfirmPassword"])
            {
                TempData["message2"] = "Passwords do not match";
                return View();

            }
            else if (form["PasswordHash"].Length < 8)
            {
                TempData["message2"] = "Password is too short. Must be minimum of 8 characters";
                return View();
            }
            else
            {
                string emailAddress = form["EmailAddress"];

                var sysAdmin = db.TblSystemAdmins.Where(a => a.EmailAddress == emailAddress).FirstOrDefault();

                if (sysAdmin != null)
                {
                    sysAdmin.PasswordHash = Hash(form["PasswordHash"]);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    await db.SaveChangesAsync();

                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["message1"] = "Invalid Email Address";
                }
                return View();
            }


        }


        public static string Hash(string value)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value))
                );

        }

        public ActionResult NotAuthorized()
        {
            TempData["message"] = "You are not authorized to access this section";
            return View();
        }

        public ActionResult GoToLogout()
        {
            HttpCookie userCookie = Request.Cookies["UserInformation"];
            if (userCookie == null)
            {
                return RedirectToAction("LoginHomeIndex", "../Login");
            }

            userCookie.Expires = DateTime.Now.AddDays(-12d);

            Response.Cookies.Add(userCookie);
            return RedirectToAction("Index", "Home");
        }

        public class SchoolList
        {
            public int? SchoolId { get; set; }
            public string School { get; set; }
            public string Combined { get; set; }
        }
    }
}