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
    public class GradesController : Controller
    {
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        // GET: LRNR/Grades
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

            DateTime EndDate = DateTime.Parse("12/31/9999");
            ViewBag.InfinteDate = EndDate;

            if (userID > 0)
            {
                var loggedUser = db.TblUsers.Find(userID);

                if (loggedUser.UserAccessTypeId == 5)
                {
                    var tblGuardian = db.TblGuardians.Where(a => a.UserlId == loggedUser.UserId).FirstOrDefault();
                    int guardianId = tblGuardian.GuardianId;

                    var tblGrade = db.TblGrades.Where(a => a.SchoolId == school && a.TblLearner.TblGuardians.Where(b => b.GuardianId == guardianId).Any());

                    return View(tblGrade.ToList());
                }
                else
                {
                    var tblGrades = db.TblGrades.Where(a => a.SchoolId == school).Include(t => t.TblLearner).Include(t => t.TblSchool);
                    return View(tblGrades.ToList());
                }
            }
            else
            {
                var tblGrades = db.TblGrades.Where(a => a.SchoolId == school).Include(t => t.TblLearner).Include(t => t.TblSchool);
                return View(tblGrades.ToList());
            }
                
        }

        // GET: LRNR/Grades/Details/5
        [AccessAttribute("Read", 3, 4, 5)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblGrade tblGrade = db.TblGrades.Find(id);
            if (tblGrade == null)
            {
                return HttpNotFound();
            }
            return View(tblGrade);
        }

        // GET: LRNR/Grades/Create
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

            ViewBag.LearnerId = new SelectList(db.TblLearners.Where(a => a.SchoolId == school), "LearnerId", "LearnerName");
            ViewBag.SchoolId = school;
            ViewBag.School = new SelectList(db.TblSchools.Where(a => a.SchoolId == school), "SchoolId", "SchoolCode", school);

            return View();
        }

        // POST: LRNR/Grades/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GradeId,SchoolId,LearnerId,GradeName,GradeMark,GradeStartDate,GradeEndDate")] TblGrade tblGrade)
        {
            if (ModelState.IsValid)
            {
                db.TblGrades.Add(tblGrade);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LearnerId = new SelectList(db.TblLearners.Where(a => a.SchoolId == tblGrade.SchoolId), "LearnerId", "LearnerCode", tblGrade.LearnerId);
            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblGrade.SchoolId), "SchoolId", "SchoolCode", tblGrade.SchoolId);

            return View(tblGrade);
        }

        // GET: LRNR/Grades/Edit/5
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
            TblGrade tblGrade = db.TblGrades.Find(id);
            if (tblGrade == null)
            {
                return HttpNotFound();
            }

            ViewBag.LearnerId = new SelectList(db.TblLearners.Where(a => a.SchoolId == school), "LearnerId", "LearnerName", tblGrade.LearnerId);
            ViewBag.School = new SelectList(db.TblSchools.Where(a => a.SchoolId == school), "SchoolId", "SchoolName", tblGrade.SchoolId);
            ViewBag.SchoolId = tblGrade.SchoolId;

            return View(tblGrade);
        }

        // POST: LRNR/Grades/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GradeId,SchoolId,LearnerId,GradeName,GradeMark,GradeStartDate,GradeEndDate")] TblGrade tblGrade)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblGrade).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LearnerId = new SelectList(db.TblLearners.Where(a => a.SchoolId == tblGrade.SchoolId), "LearnerId", "LearnerCode", tblGrade.LearnerId);
            ViewBag.SchoolId = new SelectList(db.TblSchools.Where(a => a.SchoolId == tblGrade.SchoolId), "SchoolId", "SchoolCode", tblGrade.SchoolId);

            return View(tblGrade);
        }

        // GET: LRNR/Grades/Delete/5
        [AccessAttribute("Read", 3, 4)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TblGrade tblGrade = db.TblGrades.Find(id);
            if (tblGrade == null)
            {
                return HttpNotFound();
            }
            return View(tblGrade);
        }

        // POST: LRNR/Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TblGrade tblGrade = db.TblGrades.Find(id);
            db.TblGrades.Remove(tblGrade);
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
