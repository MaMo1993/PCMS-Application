using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PCMS_Application.Controllers;
using PCMS_Application.Controllers.Access;
using PCMS_Application.Models;

namespace PCMS_Application.Areas.SCHL.Controllers
{
    public class EventsController : Controller
    {
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        // GET: SCHL/Events
        [AccessAttribute("Read", 3, 4, 5)]
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


            var tblEvents = db.TblEvents.Where(a => a.SchoolId == school).Include(t => t.TblSchool);
            return View(tblEvents.ToList());
        }

        // GET: SCHL/Events/Details/5
        [AccessAttribute("Read", 3, 4, 5)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblEvent tblEvent = db.TblEvents.Find(id);
            if (tblEvent == null)
            {
                return HttpNotFound();
            }
            return View(tblEvent);
        }

        // GET: SCHL/Events/Create
        [AccessAttribute("Read", 3, 4)]
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

        // POST: SCHL/Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventId,SchoolId,Event,EventDate,EventTime,EventPlace")] TblEvent tblEvent)
        {
            if (ModelState.IsValid)
            {
                db.TblEvents.Add(tblEvent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblEvent.SchoolId), "SchoolId", "SchoolCode", tblEvent.SchoolId);
            return View(tblEvent);
        }

        // GET: SCHL/Events/Edit/5
        [AccessAttribute("Read", 3, 4)]
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
            TblEvent tblEvent = db.TblEvents.Find(id);
            if (tblEvent == null)
            {
                return HttpNotFound();
            }

            ViewBag.School = new SelectList(db.TblSchools.Where(a => a.SchoolId == school), "SchoolId", "SchoolCode", tblEvent.SchoolId);
            ViewBag.SchoolId = tblEvent.SchoolId;

            return View(tblEvent);
        }

        // POST: SCHL/Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventId,SchoolId,Event,EventDate,EventTime,EventPlace")] TblEvent tblEvent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblEvent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblEvent.SchoolId), "SchoolId", "SchoolCode", tblEvent.SchoolId);
            return View(tblEvent);
        }

        // GET: SCHL/Events/Delete/5
        [AccessAttribute("Read", 3, 4)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblEvent tblEvent = db.TblEvents.Find(id);
            if (tblEvent == null)
            {
                return HttpNotFound();
            }
            return View(tblEvent);
        }

        // POST: SCHL/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TblEvent tblEvent = db.TblEvents.Find(id);
            db.TblEvents.Remove(tblEvent);
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
