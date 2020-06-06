using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using PCMS_Application.Controllers;
using PCMS_Application.Controllers.Access;
using PCMS_Application.Models;

namespace PCMS_Application.Areas.SCHL.Controllers
{
    public class SchoolsController : Controller
    {
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        // GET: SCHL/Schools
        [AccessAttribute("Read", 3)]
        public ActionResult Index()
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            HttpCookie usesrCookie = Request.Cookies["UserInformation"];
            if (usesrCookie == null)
            {
                return RedirectToAction("LoginHomeIndex", "PCMSLogin");
            }

            int? school = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[1]);

            //Get UserID
            int? userID = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[3]);

            if (db.TblUsers.Find(userID) != null)
            {
                ViewBag.UserName = db.TblUsers.Find(userID).UserName;
            }
            else if (db.TblSystemAdmins.Find(userID) != null)
            {
                ViewBag.UserName = "PCMS";
            }
            else
            {
                ViewBag.UserName = "";
            }

            bool check = false;

            int userId = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[3]);
            if (userId == db.TblSystemAdmins.FirstOrDefault().UserId)
            {
                check = true;
            }

            ViewBag.PCMS = check;

            var School = db.TblSchools.Find(school);
            if (School != null)
            {
                ViewBag.SchoolCode = School.SchoolCode;
                ViewBag.SchoolName = School.SchoolName;

            }
            else
            {
                ViewBag.SchoolCode = null;
                ViewBag.SchoolName = null;

            }

            var tblSchools = db.TblSchools.Where(a => a.SchoolId == school).Include(t => t.TblUser);
            return View(tblSchools.ToList());
        }

        // GET: SCHL/Schools/Details/5
        [AccessAttribute("Read", 3)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblSchool tblSchool = db.TblSchools.Find(id);
            if (tblSchool == null)
            {
                return HttpNotFound();
            }
            return View(tblSchool);
        }

        // GET: SCHL/Schools/Create
        [AccessAttribute("Create", 0)]
        public ActionResult Create()
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            HttpCookie usesrCookie = Request.Cookies["UserInformation"];
            if (usesrCookie == null)
            {
                return RedirectToAction("LoginHomeIndex", "PCMSLogin");
            }

            int? school = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[1]);

            //Get UserID
            int? userID = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[3]);

            if (db.TblUsers.Find(userID) != null)
            {
                ViewBag.UserName = db.TblUsers.Find(userID).UserName;
            }
            else if (db.TblSystemAdmins.Find(userID) != null)
            {
                ViewBag.UserName = "PCMS";
            }
            else
            {
                ViewBag.UserName = "";
            }

            bool check = false;

            int userId = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[3]);
            if (userId == db.TblSystemAdmins.FirstOrDefault().UserId)
            {
                check = true;
            }

            ViewBag.PCMS = check;

            var School = db.TblSchools.Find(school);
            if (School != null)
            {
                ViewBag.SchoolCode = School.SchoolCode;
                ViewBag.SchoolName = School.SchoolName;

            }
            else
            {
                ViewBag.SchoolCode = null;
                ViewBag.SchoolName = null;

            }

            DateTime EndDate = DateTime.Parse("12/31/9999");
            ViewBag.InfinteDate = EndDate;

            return View();
        }

        // POST: SCHL/Schools/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SchoolId,SchoolCode,UserId,SchoolName,SchoolAddress,SchoolEmail,SchoolContactNr,SchoolStartDate,SchooEndDate")] TblSchool tblSchool)
        {
            TblUser tblUser = new TblUser();

            if (ModelState.IsValid)
            {
                db.TblSchools.Add(tblSchool);
                db.SaveChanges();

                tblUser.SchoolId = tblSchool.SchoolId;
                tblUser.UserName = tblSchool.SchoolName +" Admin";
                tblUser.UserAccessTypeId = 2;
                tblUser.EmailAddress = tblSchool.SchoolEmail;
                tblUser.IsUserVerified = false;
                tblUser.UserStartDate = tblSchool.SchoolStartDate;
                tblUser.UserEndDate = tblSchool.SchoolEndDate;

                db.TblUsers.Add(tblUser);
                db.SaveChanges();

                SendVerificationEmail(tblUser.EmailAddress, tblSchool.SchoolCode);

                tblSchool.UserId = tblUser.UserId;

                db.Entry(tblSchool).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.TblUsers, "UserId", "UserName", tblSchool.UserId);
            return View(tblSchool);
        }

        [NonAction]
        public void SendVerificationEmail(string emailID, string schoolCode, string emailFor = "Register")
        {
            try
            {
                var verifyUrl = "/PCMSLogin/" + emailFor;

                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                var fromEmail = new MailAddress("melshops93@gmail.com", "PCMS Alerts");

                var toMail = new MailAddress(emailID);

                var fromEmailPassword = "Shopping1.";

                var SchoolCode = schoolCode;

                string subject = "";
                string body = "";
                if (emailFor == "Register")
                {
                    subject = "PCMS Account";

                    body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                    "<tr style='background-color:#1ab394;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                    "<tr><td>We are excited to inform you of your new PCMS School account. </td></tr>" +
                    "<tr><td>Please use this School Code when you register : <strong>" + SchoolCode + "</strong></td></tr>" +
                    "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:#1ab394;color:white'><b>Register</b></a></td></tr>" +
                    "</table>";

                }
                else if (emailFor == "NewPassword")
                {
                    subject = "Reset Password";

                    body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                    "<tr style='background-color:#1ab394;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                    "<tr><td>We received your request to reset your  School account password. Please click on the button below.</td></tr>" +
                    "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:#1ab394;color:white'><b>Reset</b></a></td></tr>" +
                    "</table>";

                }
                else if (emailFor == "NewPassword")
                {
                    subject = "New Password";

                    body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                    "<tr style='background-color:#1ab394;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                    "<tr><td>Your PCMS School account is active. Please click on the button below to create your password.</td></tr>" +
                    "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:#1ab394;color:white'><b>Create Password</b></a></td></tr>" +
                    "</table>";

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
            catch (Exception e)
            {
                var msg = e.Message;
            }
        }

        // GET: SCHL/Schools/Edit/5
        [AccessAttribute("Edit", 3)]
        public ActionResult Edit(int? id)
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            HttpCookie usesrCookie = Request.Cookies["UserInformation"];
            if (usesrCookie == null)
            {
                return RedirectToAction("LoginHomeIndex", "PCMSLogin");
            }

            int? school = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[1]);

            //Get UserID
            int? userID = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[3]);

            if (db.TblUsers.Find(userID) != null)
            {
                ViewBag.UserName = db.TblUsers.Find(userID).UserName;
            }
            else if (db.TblSystemAdmins.Find(userID) != null)
            {
                ViewBag.UserName = "PCMS";
            }
            else
            {
                ViewBag.UserName = "";
            }

            bool check = false;

            int userId = Int32.Parse(pcmsLoginController.LoggedOnUserData(usesrCookie)[3]);
            if (userId == db.TblSystemAdmins.FirstOrDefault().UserId)
            {
                check = true;
            }

            ViewBag.PCMS = check;

            var School = db.TblSchools.Find(school);
            if (School != null)
            {
                ViewBag.SchoolCode = School.SchoolCode;
                ViewBag.SchoolName = School.SchoolName;

            }
            else
            {
                ViewBag.SchoolCode = null;
                ViewBag.SchoolName = null;

            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblSchool tblSchool = db.TblSchools.Find(id);
            if (tblSchool == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.TblUsers, "UserId", "UserName", tblSchool.UserId);
            return View(tblSchool);
        }

        // POST: SCHL/Schools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SchoolId,SchoolCode,UserId,SchoolName,SchoolAddress,SchoolEmail,SchoolContactNr,SchoolStartDate,SchooEndDate")] TblSchool tblSchool)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblSchool).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.TblUsers, "UserId", "UserName", tblSchool.UserId);
            return View(tblSchool);
        }

        // GET: SCHL/Schools/Delete/5
        [AccessAttribute("Delete", 0)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblSchool tblSchool = db.TblSchools.Find(id);
            if (tblSchool == null)
            {
                return HttpNotFound();
            }
            return View(tblSchool);
        }

        // POST: SCHL/Schools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TblSchool tblSchool = db.TblSchools.Find(id);

            var events = db.TblEvents.Where(a => a.SchoolId == tblSchool.SchoolId);

            var grades = db.TblGrades.Where(a => a.SchoolId == tblSchool.SchoolId);

            var guardians = db.TblGuardians.Where(a => a.SchoolId == tblSchool.SchoolId);

            var learners = db.TblLearners.Where(a => a.SchoolId == tblSchool.SchoolId);

            var teachers = db.TblTeachers.Where(a => a.SchoolId == tblSchool.SchoolId);

            var users = db.TblUsers.Where(a => a.SchoolId == tblSchool.SchoolId);

            if (events.Any() || grades.Any() || guardians.Any() || learners.Any() || teachers.Any() || users.Any())
            {
                TempData["DelFail"] = "You cannot delete a School that is linked to other tables in the system.";
                return RedirectToAction("Index");
            }



            db.TblSchools.Remove(tblSchool);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
