using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PCMS_Application.Models;
using System.Security.Cryptography;
using System.Web.UI;
using System.Text;
using System.Web.Security;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Threading;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace PCMS_Application.Controllers
{
    public class PCMSLoginController : Controller
    {
        // GET: PCMSLogin
        private PCMS_DbEntities db = new PCMS_DbEntities();
        private static PCMSLoginController pcmsLoginController = new PCMSLoginController();

        internal object LoggedOnUserData(object usesrCookie)
        {
            throw new NotImplementedException();
        }

        // GET: ERPLogin
        //Please don't add Encryption here
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }


        //Please don't add Encryption here
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(FormCollection form)
        {
            //From the register form
            string schoolCode = (form["SchoolCode"].ToUpper()).Trim();
            string email = form["EmailAddress"];


            if (form["PasswordHash"].Length < 6)
            {
                TempData["message2"] = "Password is too short. Must be minimum of 6 characters";
                return View();
            }

            if (form["PasswordHash"] != form["ConfirmPassword"])
            {
                TempData["message2"] = "Passwords do not match";
                return View();
            }

            var school = db.TblSchools.Where(a => a.SchoolCode.ToString() == schoolCode).FirstOrDefault();

            if (school == null)
            {
                TempData["message1"] = "School doesn't Exists";
                return View();
            }


            //Find the User Account using the Legal Entity and the Email
            var userAccs = db.TblUsers.Where(a => a.EmailAddress == email && a.SchoolId == school.SchoolId && a.UserAccessTypeId != null);

            //Check if a user account was found.
            if (userAccs.FirstOrDefault() == null)
            {
                TempData["message1"] = "User doesn't Exists";
                return View();
            }

            var terEmailCheck = userAccs.Where(a => a.EmailAddress == email && a.SchoolId == school.SchoolId && a.UserAccessTypeId == null || a.UserEndDate < DateTime.Now).FirstOrDefault();
            if (terEmailCheck != null)
            {
                TempData["message1"] = "User account has been terminated or is no longer active.";
                return View();
            }

            var userAcc = db.TblUsers.Where(a => a.EmailAddress == email && a.SchoolId == school.SchoolId && a.UserAccessTypeId != null).FirstOrDefault();
            if (userAcc == null)
            {
                TempData["message1"] = "User Entity doesn't Exists";
                return View();
            }

            //check if user'a account is verified
            if (userAcc.IsUserVerified == true)
            {
                TempData["message2"] = "Email Already Registered";
                return View(userAcc);
            }

            userAcc.UserPasswordHash = Hash(form["PasswordHash"]);

            //if it is not, verify the user account
            userAcc.IsUserVerified = true;

            db.Configuration.ValidateOnSaveEnabled = false;
            await db.SaveChangesAsync();

            if (userAcc.UserStartDate > DateTime.Now)
            {
                DateTime futureDate = DateTime.Parse(userAcc.UserStartDate.ToString());
                TempData["success"] = "Registration successful. Please note that the account is inactive and will only be active from " + futureDate.ToShortDateString();
            }
            else
            {
                TempData["success"] = "Registration successful. You may now login.";
            }
            return RedirectToAction("LoginHomeIndex");

        }

        public ActionResult LoginHomeIndex(int? intended, string urlRedirect)
        {
            ViewBag.Intended = intended;
            ViewBag.UrlRedirect = urlRedirect;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(FormCollection form)
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            string email = form["EmailAddress"];
            string location = form["UrlRedirect"];

            int? intendedAssocId = null;
            if (form["Intended"] != null && form["Intended"] != "")
            {
                intendedAssocId = int.Parse(form["Intended"]);
            }
            try
            {
                //Find the LE
                string SchoolCode = (form["SchoolCode"].ToUpper()).Trim();

                var school = db.TblSchools.Where(a => a.SchoolCode.ToString() == SchoolCode).FirstOrDefault();
                if (school != null)
                {
                    //Find the User
                    var user = db.TblUsers.Where(t => t.EmailAddress == email && t.SchoolId == school.SchoolId).FirstOrDefault();

                    if (user != null)
                    {

                        //Checking User Date
                        if (user.UserStartDate > DateTime.Now.Date)
                        {
                            DateTime futureDate = (DateTime)user.UserStartDate;
                            TempData["message1"] = "Login Failed. This user will only be active from " + futureDate.ToShortDateString();
                        }
                        else
                        {
                            TempData["message1"] = "Login Failed. The user is inactive.";
                        }

                        //Checking User fields
                        if (user.IsUserVerified == false)
                        {
                            TempData["message1"] = "Email Not Verified. Please Verify your email";
                            return RedirectToAction("LoginHomeIndex", new { intended = intendedAssocId, urlRedirect = location });
                        }
                        else if (user.UserPasswordHash != Hash(form["PasswordHash"]))
                        {
                            TempData["message1"] = "Login Failed. Incorrect password";
                            return RedirectToAction("LoginHomeIndex", new { intended = intendedAssocId, urlRedirect = location });
                        }

                        //Successful Login
                        var tmp = db.TblSchools.Where(m => m.SchoolId == school.SchoolId && m.UserId == user.UserId).FirstOrDefault();

                        string userData = "";

                        if (tmp != null)
                        {
                            userData = email + ";" + user.SchoolId + ";" + tmp.SchoolId + ";" + user.UserId + ";";
                        }
                        else
                        {
                            userData = email + ";" + user.SchoolId + ";" + 0 + ";" + user.UserId + ";";
                        }

                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, email, DateTime.Now,
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

                        if (location != null && location != "")
                        {
                            if (intendedAssocId != null)
                            {
                                if (tmp.SchoolId == intendedAssocId)
                                {
                                    return Redirect(location);
                                }
                                else
                                {
                                    return RedirectToAction("NavigationHomeIndex", "PCMSNavigation");
                                }
                            }
                            else
                            {
                                return RedirectToAction("NavigationHomeIndex", "PCMSNavigation");
                            }
                        }
                        else
                        {
                            return RedirectToAction("NavigationHomeIndex", "PCMSNavigation");
                        }
                    }
                    else
                    {
                        TempData["message"] = "Login Failed. Check your email address and School Code";
                    }
                }
                else
                {
                    TempData["message"] = "Login Failed. Check your School Code";
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

            return RedirectToAction("LoginHomeIndex", new { intended = intendedAssocId, urlRedirect = location });
        }

        public static string Hash(string value)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value))
                );

        }

        //Register

        [NonAction]
        public void SendEmail(string emailID, string activationCode, string emailFor)
        {
            var verifyUrl = "/PCMSLogin/" + "ResetPassword" + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("melshops93@gmail.com", "PCMS Notifications");

            var toMail = new MailAddress(emailID);

            var fromEmailPassword = "Shopping1.";

            string subject = "";
            string body = "";
            if (emailFor == "NewAccount")
            {
                subject = "PCMS Account Active!";

                body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                        "<tr style='background-color:steelblue;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                        "<tr><td><h3>Your PCMS Account is active</h3></td></tr>" +
                         "<tr><td>Click below to create your account's new password</td></tr>" +
                        "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:steelblue;color:white'><b>Create Password</b></a></td></tr>" +
                        "</table>";

            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Password Reset";

                body = "<table style='border-color:white;text-align:center;background-size:auto;background-color:whitesmoke' width='40%'>" +
                        "<tr style='background-color:#1ab394;color:white'><td height='100'><font size='4'><b>Thank you for choosing PCMS</b></font></td></tr>" +
                        "<tr><td>Hello, We got a request to reset your account's password</td></tr>" +
                         "<tr><td>If this request is from you, click the button below.</td></tr>" +
                        "<tr><td style='padding:15px;'><a class='button' href='" + link + "' style='text-decoration: none;line-height:35px;text-align:center;height:35px;width:30%;display:inline-block;border-radius:12px;text-size-adjust:auto;background-color:#1ab394;color:white'><b>Reset Password</b></a></td></tr>" +
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

        //GET: Forgot password 
        //Please don't add Encryption here
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            return View();
        }

        //POST: Forgot Password
        //Please don't add Encryption here
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(FormCollection form, TblUser user)
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            string message = "";

            string email = form["EmailAddress"];

            string SCCode = form["SCode"];

            var account = db.TblUsers.Where(a => a.EmailAddress == email && a.TblSchool.SchoolCode.ToString() == SCCode && a.UserAccessTypeId != null).FirstOrDefault();

            if (account != null)
            {
                //Send Email for reset password
                string restCode = Guid.NewGuid().ToString();
                SendEmail(account.EmailAddress, restCode, "ResetPassword");
                user.ResetPassword = restCode;

                //Confirm password does not have issues
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();

                TempData["message"] = "Reset password link has been sent to your email address.";
            }
            else
            {
                TempData["message1"] = "Account not found";
            }

            ViewBag.Message = message;
            return View();
        }


        //Log out
        //Please don't add Encryption here
        public ActionResult Logout()
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            HttpCookie userCookie = Request.Cookies["UserInformation"];
            if (userCookie == null)
            {
                return RedirectToAction("LoginHomeIndex", "Login");
            }

            //Get  UserID
            int? userID = Int32.Parse(pcmsLoginController.LoggedOnUserData(userCookie)[3]);

            //Get LegalEntityId 
            int? entity = Int32.Parse(pcmsLoginController.LoggedOnUserData(userCookie)[1]);

            userCookie.Expires = DateTime.Now.AddDays(-12d);

            Response.Cookies.Add(userCookie);

            return RedirectToAction("Index", "Home");
        }

        //Reset Password 
        //Please don't add Encryption here
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(FormCollection form, TblUser user)
        {
            Uri currentUrl = Request.UrlReferrer;
            if (currentUrl == null)
            {
                return HttpNotFound();
            }

            if (form["PasswordHash"] != form["ConfirmPassword"])
            {
                TempData["message2"] = "Passwords do not match";
                ViewBag.SchoolId = new SelectList(db.TblSchools, "SchoolId", "SchoolCode", user.SchoolId);
                return View();

            }
            else if (form["PasswordHash"].Length < 6)
            {
                TempData["message2"] = "Password is too short. Must be minimum of 6 characters";
                ViewBag.SchoolId = new SelectList(db.TblSchools, "SchoolId", "SchoolCode", user.SchoolId);
                return View();
            }
            else
            {
                string emailAddress = form["EmailAddress"];

                string schoolCode = form["SCode"];

                var school = db.TblSchools.Where(a => a.SchoolCode.ToString() == schoolCode).FirstOrDefault();

                if (school != null)
                {
                    var theUser = db.TblUsers.Where(a => a.EmailAddress == emailAddress && a.SchoolId == school.SchoolId && a.UserAccessTypeId != null).FirstOrDefault();
                    if (theUser != null)
                    {
                        theUser.UserPasswordHash = Hash(form["PasswordHash"]);
                        db.Configuration.ValidateOnSaveEnabled = false;
                        await db.SaveChangesAsync();

                        TempData["success"] = "New password updated successfully";

                        return RedirectToAction("LoginHomeIndex");

                    }
                    else
                    {
                        TempData["message1"] = "Invalid Email Address/School Code";
                    }
                }
                else
                {
                    TempData["message1"] = "Invalid Email Address/Legal Entity Code";
                }
                ViewBag.SchoolId = new SelectList(db.TblSchools, "SchoolId", "SchoolCode", user.SchoolId);
                return View();
            }
        }


        //Create a random passwordsalt
        private string CreateSalt()
        {
            int size = 10;

            var randomSaltCrypto = new RNGCryptoServiceProvider();

            var salt = new byte[size];
            randomSaltCrypto.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        //Generate the passwordhash receives two parameters and returns a hashed password
        private string GeneratePassWordHash(string password, string salt)
        {
            byte[] passwrdAndSalt = Encoding.ASCII.GetBytes(password + salt);

            SHA256Managed sha256string = new SHA256Managed();
            byte[] hashedPassword = sha256string.ComputeHash(passwrdAndSalt);

            return Convert.ToBase64String(hashedPassword);
        }


        // Decrypt the key and put the information in an array and return the array
        // [0] = email, [1] = LegalentityId, [2] = AssociateId, [3] = UserID
        public string[] LoggedOnUserData(HttpCookie userCookie)
        {
            string[] UserCookieInformation = new string[4];

            string encryptedData = userCookie.Value;

            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(encryptedData);
            string userInformation = ticket.UserData;

            //
            string tmpUserInformation = "";
            for (int i = 0, k = 0; i < userInformation.Length; i++)
            {
                if (userInformation[i] != ';')
                {
                    tmpUserInformation += userInformation[i];
                }
                else
                {
                    UserCookieInformation[k] = tmpUserInformation;
                    tmpUserInformation = "";
                    k++;
                }
            }

            string[] t = UserCookieInformation;
            return UserCookieInformation;
        }


    }
}