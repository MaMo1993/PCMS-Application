using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using PCMS_Application.Controllers;
using PCMS_Application.Controllers.Access;
using PCMS_Application.Models;

namespace PCMS_Application.Areas.TCHR.Controllers
{
    public class TeachersController : Controller
    {
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        // GET: TCHR/Teachers
        [AccessAttribute("Read", 3, 4)]
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


            var tblTeachers = db.TblTeachers.Where(a => a.SchoolId == school).Include(t => t.TblSchool).Include(t => t.TblUser);
            return View(tblTeachers.ToList());
        }

        // GET: TCHR/Teachers/Details/5
        [AccessAttribute("Read", 3, 4)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblTeacher tblTeacher = db.TblTeachers.Find(id);
            if (tblTeacher == null)
            {
                return HttpNotFound();
            }
            return View(tblTeacher);
        }

        // GET: TCHR/Teachers/Create
        [AccessAttribute("Read", 3)]
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

            ViewBag.SchoolId = school;
            ViewBag.School = new SelectList(db.TblSchools.Where(a => a.SchoolId == school), "SchoolId", "SchoolCode", school);

            return View();
        }

        // POST: TCHR/Teachers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TeacherId,SchoolId,TeacherCode,UserId,TeacherName,TeacherAddress,TeacherEmail,TeacherContactNr,TeacherStartDate,TeacherEndDate")] TblTeacher tblTeacher)
        {
            TblUser tblUser = new TblUser();

            if (ModelState.IsValid)
            {
                db.TblTeachers.Add(tblTeacher);
                db.SaveChanges();

                tblUser.SchoolId = tblTeacher.SchoolId;
                tblUser.UserName = tblTeacher.TeacherName;
                tblUser.UserAccessTypeId = 4;
                tblUser.EmailAddress = tblTeacher.TeacherEmail;
                tblUser.IsUserVerified = false;
                tblUser.UserStartDate = tblTeacher.TeacherStartDate;
                tblUser.UserEndDate = tblTeacher.TeacherEndDate;

                db.TblUsers.Add(tblUser);
                db.SaveChanges();

                var Schl = db.TblSchools.Find(tblTeacher.SchoolId);

                var code = Schl != null ? Schl.SchoolCode : "";

                SendVerificationEmail(tblUser.EmailAddress, code);

                tblTeacher.UserId = tblUser.UserId;

                db.Entry(tblTeacher).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblTeacher.SchoolId), "SchoolId", "SchoolCode", tblTeacher.SchoolId);

            return View(tblTeacher);
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
                    "<tr style='background-color:steelblue;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                    "<tr><td>We are excited to inform you of your new PCMS  Teacher account. </td></tr>" +
                    "<tr><td>Please use this School Code when you register : <strong>" + SchoolCode + "</strong></td></tr>" +
                    "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:steelblue;color:white'><b>Register</b></a></td></tr>" +
                    "</table>";

                }
                else if (emailFor == "NewPassword")
                {
                    subject = "Reset Password";

                    body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                    "<tr style='background-color:steelblue;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                    "<tr><td>We received your request to reset your  Teacher account password. Please click on the button below.</td></tr>" +
                    "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:steelblue;color:white'><b>Reset</b></a></td></tr>" +
                    "</table>";

                }
                else if (emailFor == "NewPassword")
                {
                    subject = "New Password";

                    body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                    "<tr style='background-color:steelblue;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                    "<tr><td>Your PCMS Teacher account is active. Please click on the button below to create your password.</td></tr>" +
                    "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:steelblue;color:white'><b>Create Password</b></a></td></tr>" +
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

        // GET: TCHR/Teachers/Edit/5
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
            TblTeacher tblTeacher = db.TblTeachers.Find(id);
            if (tblTeacher == null)
            {
                return HttpNotFound();
            }

            ViewBag.School = new SelectList(db.TblSchools.Where(a => a.SchoolId == school), "SchoolId", "SchoolCode", tblTeacher.SchoolId);
            ViewBag.SchoolId = tblTeacher.SchoolId;

            return View(tblTeacher);
        }
        [AccessAttribute("Read", 3, 4)]

        // POST: TCHR/Teachers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TeacherId,SchoolId,TeacherCode,UserId,TeacherName,TeacherAddress,TeacherEmail,TeacherContactNr,TeacherStartDate,TeacherEndDate")] TblTeacher tblTeacher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblTeacher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblTeacher.SchoolId), "SchoolId", "SchoolCode", tblTeacher.SchoolId);

            return View(tblTeacher);
        }

        // GET: TCHR/Teachers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblTeacher tblTeacher = db.TblTeachers.Find(id);
            if (tblTeacher == null)
            {
                return HttpNotFound();
            }
            return View(tblTeacher);
        }
        [AccessAttribute("Read", 3)]

        // POST: TCHR/Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TblTeacher tblTeacher = db.TblTeachers.Find(id);

            var learners = db.TblLearners.Where(a => a.TeacherId == tblTeacher.TeacherId);

            if (learners.Any())
            {
                TempData["DelFail"] = "You cannot delete a Teacher that has Learners linked to.";
                return RedirectToAction("Index");
            }

            db.TblTeachers.Remove(tblTeacher);
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
