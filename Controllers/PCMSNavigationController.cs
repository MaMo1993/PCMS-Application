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
using PCMS_Application.Controllers.Access;

namespace PCMS_Application.Controllers
{
    public class PCMSNavigationController : Controller
    {
        // GET: PCMSNavigation
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        public ActionResult NavigationHomeIndex()
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

            if (userID > 0)
            {
                var children = db.TblLearners.Where(a => a.SchoolId == school).Count();
                var parents = db.TblGuardians.Where(a => a.SchoolId == school).Count();
                var teachers = db.TblTeachers.Where(a => a.SchoolId == school).Count();
                var schhols = db.TblSchools.Where(a => a.SchoolId == school).Count();

                ViewBag.Children = children;
                ViewBag.Parents = parents;
                ViewBag.Teachers = teachers;
                ViewBag.Schools = schhols;
            }
            else
            {
                
                var children = db.TblLearners.Count();
                var parents = db.TblGuardians.Count();
                var teachers = db.TblTeachers.Count();
                var schools = db.TblSchools.Count();

                ViewBag.Children = children;
                ViewBag.Parents = parents;
                ViewBag.Teachers = teachers;
                ViewBag.Schools = schools;
            }
                

            return View();
        }

        [AccessAttribute("Read", 3)]
        public ActionResult SchoolNav()
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

            return View();
        }

        [AccessAttribute("Read", 3, 4)]
        public ActionResult TeacherNav()
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
            return View();
        }

        [AccessAttribute("Read", 3, 4, 5)]
        public ActionResult LearnerNav()
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

            return View();
        }

        [AccessAttribute("Read", 3, 4, 5)]
        public ActionResult GuardianNav()
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
            return View();
        }

        [AccessAttribute("Read", 0)]
        public ActionResult PCMSAdmin()
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

            return View();
        }

    }
}