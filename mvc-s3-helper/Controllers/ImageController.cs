using MVC_S3_Helper.Helpers;
using MVC_S3_Helper.Models;
using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVC_S3_Helper.Controllers
{
    public class ImageController : Controller
    {
        private ImageUploadDBContext db = new ImageUploadDBContext();

        // GET: Image
        public ActionResult Index()
        {
            var lst = db.Images.ToList();
            System.Diagnostics.Debug.Print(lst.Count.ToString());
            return View(lst);
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(string additionInfo)
        {
            if(Request.Files.Count > 0)
            {
                // S3 credentials
                var s3BktName = ConfigurationManager.AppSettings["S3BucketName"];
                var s3AccessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
                var s3SecretAccessKey = ConfigurationManager.AppSettings["AWSSecretAccessKey"];
                var s3Region = ConfigurationManager.AppSettings["AWSRegion"];
                var s3FolderName = "Test";
                var s3ObjectKey = s3FolderName + "/";

                var awsS3Helper = new AwsS3Helper();
                var s3Connected = awsS3Helper.ConnectS3(s3AccessKey, s3SecretAccessKey, s3Region) && awsS3Helper.CreateFolder(s3BktName, s3FolderName);

                //  Get all files from Request object  
                HttpFileCollectionBase files = Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];

                    // Saving S3
                    if (s3Connected)
                    {
                        s3ObjectKey += Path.GetFileName(file.FileName);
                        awsS3Helper.UploadS3File(s3ObjectKey, s3BktName, file.ContentType, file.InputStream);
                    }

                    string filePath = GetPartialFilePath(file);

                    //Adding database Entry
                    var image = new Image
                    {
                        ImageTitle = Path.GetFileNameWithoutExtension(file.FileName),
                        ImagePath = filePath,
                        ImagePathS3 = s3ObjectKey,
                        OriginalFileName = Path.GetFileName(file.FileName)
                    };
                    db.Images.Add(image);

                    filePath = Server.MapPath(filePath);
                    file.SaveAs(filePath);
                }

                db.SaveChanges();
                return Json(new { Message = "Images uploaded successfully." });
            }

            return Json(new { Message = "Please select atleast image first." });
        }

        public ActionResult Details(int id)
        {
            var image = db.Images.Find(id);
            if (image == null)
            {
                return new HttpNotFoundResult();
            }

            //Retrieving Images from S3
            var s3BktName = ConfigurationManager.AppSettings["S3BucketName"];
            var s3AccessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
            var s3SecretAccessKey = ConfigurationManager.AppSettings["AWSSecretAccessKey"];
            var s3Region = ConfigurationManager.AppSettings["AWSRegion"];

            var awsS3Helper = new AwsS3Helper();
            if (awsS3Helper.ConnectS3(s3AccessKey, s3SecretAccessKey, s3Region, s3BktName))
            {
                image.ImagePathS3 = awsS3Helper.GetS3FileUrl(image.ImagePathS3);
            }

            return View(image);
        }

        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var image = db.Images.Find(id);
            if (image == null)
            {
                return new HttpNotFoundResult();
            }

            return View(image);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Image model, HttpPostedFileBase newImage)
        {
            if (ModelState.IsValid)
            {
                var image = db.Images.Find(model.ID);
                if (image == null)
                {
                    return new HttpNotFoundResult();
                }

                // S3 credentials
                var s3BktName = ConfigurationManager.AppSettings["S3BucketName"];
                var s3AccessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
                var s3SecretAccessKey = ConfigurationManager.AppSettings["AWSSecretAccessKey"];
                var s3Region = ConfigurationManager.AppSettings["AWSRegion"];
                var s3FolderName = "Test";
                var s3ObjectKey = s3FolderName + "/" + Path.GetFileName(newImage.FileName);

                var awsS3Helper = new AwsS3Helper();
                if (awsS3Helper.ConnectS3(s3AccessKey, s3SecretAccessKey, s3Region))
                {
                    // Delete the existing image first
                    awsS3Helper.DeleteS3Object(image.ImagePathS3, s3BktName, null);

                    awsS3Helper.UploadS3File(s3ObjectKey, s3BktName, newImage.ContentType, newImage.InputStream);
                }

                string filePath = GetPartialFilePath(newImage);
                var oldImagePath = image.ImagePath;

                // Saving in DB
                image.ImageTitle = Path.GetFileNameWithoutExtension(newImage.FileName);
                image.ImagePath = filePath;
                image.ImagePathS3 = s3ObjectKey;
                image.OriginalFileName = Path.GetFileName(newImage.FileName);

                db.Entry(image).State = EntityState.Modified;
                db.SaveChanges();

                // Delete existing file
                string sFName = HttpContext.Server.MapPath(oldImagePath);
                System.IO.File.Delete(sFName);

                // Saving in physical location
                filePath = Server.MapPath(filePath);
                newImage.SaveAs(filePath);

                return RedirectToAction("Index");
            }

            return View();
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image img = db.Images.Find(id);
            if (img == null)
            {
                return HttpNotFound();
            }
            return View(img);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Image img = db.Images.Find(id);
            if (img == null)
            {
                return HttpNotFound();
            }
            // remove the file from disk...
            try
            {
                string sFName = HttpContext.Server.MapPath(img.ImagePath);
                System.IO.File.Delete(sFName);
            }
            catch (Exception exc)
            {
                throw new HttpException(500, exc.Message);
            }

            // Remove file from S3
            // S3 Related works
            var s3BktName = ConfigurationManager.AppSettings["S3BucketName"];
            var s3AccessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
            var s3SecretAccessKey = ConfigurationManager.AppSettings["AWSSecretAccessKey"];
            var s3Region = ConfigurationManager.AppSettings["AWSRegion"];

            var awsS3Helper = new AwsS3Helper();
            if (awsS3Helper.ConnectS3(s3AccessKey, s3SecretAccessKey, s3Region))
            {
                awsS3Helper.DeleteS3Object(img.ImagePathS3, s3BktName, null);
            }

            // Saving database entry
            db.Images.Remove(img);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [NonAction]
        private string GetPartialFilePath(HttpPostedFileBase file)
        {
            string partialPath = "~/Images/";

            // Checking for Internet Explorer  
            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
            {
                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                partialPath += testfiles[testfiles.Length - 1];
            }
            else
            {
                partialPath += file.FileName;
            }

            return partialPath;
        }
    }
}