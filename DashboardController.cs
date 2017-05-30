using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using JobBoardMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using static JobBoardMVC.Controllers.ManageController;
using System.Data.Entity.Infrastructure;

namespace JobBoardMVC
{
    public class DashboardController : Controller
    {
        private JobBoardMvcContext db = new JobBoardMvcContext();
        private UserManager _userManager;

        // GET: Dashboard
        [Authorize]
        public ActionResult Index()
        {
            var userID = Guid.Parse(User.Identity.GetUserId());
            var savedJobs = db.SavedJobs.Where(s => s.UserID == userID);

            // grab a count of the number of jobs currently inside the SavedJobs table.
            int count = savedJobs.Count();

            // store it in viewbag for the View to display
            ViewBag.Counts = count;

            return View(savedJobs.ToList());
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: File(Resume)
        //public ActionResult RetrieveResume(Guid id)
        //{
        //    var fileToRetrieve = db.Resumes.Find(id);
        //    return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        //}

        // GET: Photo
        public ActionResult RetrieveProfilePhoto(Guid id)
        {
            var photoToRetrieve = db.Photos.Find(id);
            return File(photoToRetrieve.Content, photoToRetrieve.ContentType);
        }

        [Authorize]
        public ActionResult UserProfile()
        {
            //Get user id to display User's info on the user profile page
            var id = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(s => s.Resumes).SingleOrDefault(s => s.Id == id); ;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile([Bind(Include = "FirstName, LastName, Location, Languages, Experience")] User _userProfile)
        {
            if (ModelState.IsValid)
            {
                var userName = User.Identity.GetUserName();//get user's username for database reference
                //get row in database that corresponds to the user so we can edit data in specific columns
                var user = (from u in db.Users
                            where u.UserName == userName
                            select u).First();
                //check incoming data to make sure there is something there and that it doesnt match what is already in the database before updating
                if ((_userProfile.FirstName != user.FirstName) && (_userProfile.FirstName.Trim().Length != 0))
                    user.FirstName = _userProfile.FirstName;
                if ((_userProfile.LastName != user.LastName) && (_userProfile.LastName.Trim().Length != 0))
                    user.LastName = _userProfile.LastName;
                if ((_userProfile.Location != user.Location) && (_userProfile.Location.Trim().Length != 0))
                    user.Location = _userProfile.Location;
                if ((_userProfile.Languages != user.Languages) && (_userProfile.Languages.Trim().Length != 0))
                    user.Languages = _userProfile.Languages;
                if ((_userProfile.Experience != user.Experience) && (_userProfile.Experience.Trim().Length != 0))
                    user.Experience = _userProfile.Experience;

                await db.SaveChangesAsync();
            }

            return View(_userProfile);
        }

        public ActionResult UploadPhoto()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadPhoto(HttpPostedFileBase upload)
        {
            var id = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User _user = db.Users.Find(id);
            if (_user == null)
            {
                return HttpNotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (_user.Photos.Any(f => f.FileType == FileType.Photo))
                        {
                            db.Photos.Remove(_user.Photos.First(f => f.FileType == FileType.Photo));
                        }
                        var newPhoto = new ProfilePhoto
                        {
                            FileId = Guid.NewGuid(),
                            FileName = System.IO.Path.GetFileName(upload.FileName),
                            FileType = FileType.Photo,
                            ContentType = upload.ContentType,
                            UserId = _user.Id

                        };
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            newPhoto.Content = reader.ReadBytes(upload.ContentLength);
                        }
                        _user.Photos = new List<ProfilePhoto> { newPhoto };
                        db.Photos.Add(newPhoto);
                        await db.SaveChangesAsync();
                    }
                }

                return RedirectToAction("UserProfile", new { Message = ManageMessageId.UploadPhotoSuccess });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult UploadResume()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadResume(HttpPostedFileBase upload)
        {
            var id = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User _user = db.Users.Find(id);
            if (_user == null)
            {
                return HttpNotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        var resume = new ResumeModel
                        {
                            FileId = Guid.NewGuid(),
                            FileName = System.IO.Path.GetFileName(upload.FileName),
                            FileType = FileType.Resume,
                            ContentType = upload.ContentType,
                            UserId = _user.Id

                        };
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            resume.Content = reader.ReadBytes(upload.ContentLength);
                        }
                        _user.Resumes = new List<ResumeModel> { resume };
                        db.Resumes.Add(resume);
                        await db.SaveChangesAsync();
                    }
                }

                return RedirectToAction("UserProfile", new { Message = ManageMessageId.UploadResumeSuccess });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        // GET: Dashboard/DeleteResume/5
        [Authorize]
        public ActionResult DeleteResume(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ResumeModel resumeToDelete = db.Resumes.Find(id);
            if (resumeToDelete == null)
            {
                return HttpNotFound();
            }
            return View(resumeToDelete);
        }

        // POST: Dashboard/DeleteResume/5
        [Authorize]
        [HttpPost, ActionName("DeleteResume")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteResumeConfirmed(Guid fileId)
        {
            try
            {
                //Guid FileId = Guid.Parse(fileId);
                var fileToDelete = db.Resumes.Find(fileId);
                db.Resumes.Remove(fileToDelete);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return RedirectToAction("UserProfile");
        }
       

        [Authorize]
        public ActionResult DisplayCompanies()
        {
            var userID = Guid.Parse(User.Identity.GetUserId());
            var userSavedCompanies = db.SavedCompanies.Where(s => s.UserID == userID);
            // grab a count of the number of jobs currently inside the SavedJobs table.
            int count = userSavedCompanies.Count();
            // store it in viewbag for the View to display
            ViewBag.Counts = count;

            return View(userSavedCompanies.ToList());
        }

        // Save - this action takes a name value passed to it from a save button on an individual company
        //        info page, then adds the associated UserId, Company name, city, and state information
        //        to the SavedCompanies table. It is called via ajax from the page, so it doesn't return a
        //        view. It's possible, therefore, that the syntax of the action method should be changed
        //        at some point.

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Save(string name)
        {
            if (name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Company company = await db.Companies.FindAsync(name);
            if (company == null)
            {
                return HttpNotFound();
            }


            var userID = Guid.Parse(User.Identity.GetUserId());
            var userSavedCompanies = db.SavedCompanies.Where(s => s.UserID == userID);

            if (ModelState.IsValid)
            {
                var savedCompany = new SavedCompany();
                savedCompany.UserID = userID;
                savedCompany.CompanyCompanyName = company.CompanyName;
                savedCompany.City = company.City;
                savedCompany.State = company.State;

                // Check to see if Company has been previosuly saved by the user.
                // If not true - new SavedCompany() record is added to db.SavedCompanies datatable.
                if (!userSavedCompanies.Any(s => s.CompanyCompanyName == company.CompanyName))
                {
                    db.SavedCompanies.Add(savedCompany);
                    await db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Dashboard/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedJob savedJob = db.SavedJobs.Find(id);
            if (savedJob == null)
            {
                return HttpNotFound();
            }
            return View(savedJob);
        }

        // POST: Dashboard/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SavedJob savedJob = db.SavedJobs.Find(id);
            db.SavedJobs.Remove(savedJob);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Dashboard/DeleteCompany/5
        // Delete action for SavedCompany table
        // Accessed from DisplayCompanies view

        [Authorize]
        public ActionResult DeleteCompany(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedCompany savedCompany = db.SavedCompanies.Find(id);
            if (savedCompany == null)
            {
                return HttpNotFound();
            }
            return View(savedCompany);
        }

        // POST: Dashboard/Delete/5
        // Delete action for SavedCompany table
        // Accessed from DeleteCompany view

        [Authorize]
        [HttpPost, ActionName("Delete Company")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCompanyConfirmed(int id)
        {
            SavedCompany savedCompany = db.SavedCompanies.Find(id);
            db.SavedCompanies.Remove(savedCompany);
            db.SaveChanges();
            return RedirectToAction("DisplayCompanies");
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
