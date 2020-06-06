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

namespace PCMS_Application.Areas.LRNR.Controllers
{
    public class LearnersController : Controller
    {
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        // GET: LRNR/Learners
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


            if (userID > 0)
            {
                var loggedUser = db.TblUsers.Find(userID);

                if (loggedUser.UserAccessTypeId == 5)
                {
                    var tblGuardian = db.TblGuardians.Where(a => a.UserlId == loggedUser.UserId).FirstOrDefault();
                    int guardianId = tblGuardian.GuardianId;

                    var tblLearner = db.TblLearners.Where(a => a.SchoolId == school && a.TblGuardians.Where(b => b.GuardianId == guardianId).Any());

                    return View(tblLearner.ToList());
                }
                else
                {
                    var tblLearners = db.TblLearners.Where(a => a.SchoolId == school).Include(t => t.TblSchool).Include(t => t.TblTeacher);
                    return View(tblLearners.ToList());
                }
            }
            else
            {
                var tblLearners = db.TblLearners.Where(a => a.SchoolId == school).Include(t => t.TblSchool).Include(t => t.TblTeacher);
                return View(tblLearners.ToList());
            }
        }

        // GET: LRNR/Learners/Details/5
        [AccessAttribute("Read", 3, 4, 5)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblLearner tblLearner = db.TblLearners.Find(id);
            if (tblLearner == null)
            {
                return HttpNotFound();
            }
            return View(tblLearner);
        }

        // GET: LRNR/Learners/Create
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
            ViewBag.TeacherId = new SelectList(db.TblTeachers.Where(a => a.SchoolId == school), "TeacherId", "TeacherCode");

            return View();
        }

        // POST: LRNR/Learners/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LearnerId,TeacherId,SchoolId,LearnerCode,LearnerName,LearnerAddress,LearnerEmail,LearnerContactNr,LearnerStartDate,LearnerEndDate")] TblLearner tblLearner)
        {
            if (ModelState.IsValid)
            {
                db.TblLearners.Add(tblLearner);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblLearner.SchoolId), "SchoolId", "SchoolCode", tblLearner.SchoolId);
            ViewBag.TeacherId = new SelectList(db.TblTeachers.Where(a => a.SchoolId == tblLearner.SchoolId), "TeacherId", "TeacherCode", tblLearner.TeacherId);
            return View(tblLearner);
        }

        // GET: LRNR/Learners/Edit/5
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
            TblLearner tblLearner = db.TblLearners.Find(id);
            if (tblLearner == null)
            {
                return HttpNotFound();
            }

            ViewBag.School = new SelectList(db.TblSchools.Where(a => a.SchoolId == school), "SchoolId", "SchoolCode", tblLearner.SchoolId);
            ViewBag.SchoolId = tblLearner.SchoolId;
            ViewBag.TeacherId = new SelectList(db.TblTeachers.Where(a => a.SchoolId == school), "TeacherId", "TeacherCode", tblLearner.TeacherId);

            return View(tblLearner);
        }

        // POST: LRNR/Learners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LearnerId,TeacherId,SchoolId,LearnerCode,LearnerName,LearnerAddress,LearnerEmail,LearnerContactNr,LearnerStartDate,LearnerEndDate")] TblLearner tblLearner)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblLearner).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblLearner.SchoolId), "SchoolId", "SchoolCode", tblLearner.SchoolId);
            ViewBag.TeacherId = new SelectList(db.TblTeachers.Where(a => a.SchoolId == tblLearner.SchoolId), "TeacherId", "TeacherCode", tblLearner.TeacherId);

            return View(tblLearner);
        }

        // GET: LRNR/Learners/Delete/5
        [AccessAttribute("Read", 3, 4)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblLearner tblLearner = db.TblLearners.Find(id);
            if (tblLearner == null)
            {
                return HttpNotFound();
            }
            return View(tblLearner);
        }

        // POST: LRNR/Learners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TblLearner tblLearner = db.TblLearners.Find(id);

            var guardians = db.TblGuardians.Where(a => a.LearnerId == tblLearner.LearnerId);

            if (guardians.Any())
            {
                TempData["DelFail"] = "You cannot delete a Learner that has a Guardian linked to.";
                return RedirectToAction("Index");
            }

            db.TblLearners.Remove(tblLearner);
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
