using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using MS.Services;
using Optivem.Framework.Core.Domain;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Security.Cryptography.Xml;
using System.Text;

namespace PainTrax.Web.Controllers
{
    public class UtilityController : Controller
    {

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private IConfiguration Configuration;
        private readonly SignatureService _services = new SignatureService();
        private readonly CompanyServices _cmpservices = new CompanyServices();
        private readonly PatientService _patientservices = new PatientService();

        public UtilityController(Microsoft.AspNetCore.Hosting.IHostingEnvironment environment,
                               IConfiguration configuration)
        {

            Environment = environment;
            Configuration = configuration;
        }

        public IActionResult UploadSign()
        {
            return View(new UploadSignVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadSign(IFormFile[] signs)
        {

            if (signs != null)
            {
                string folderPath = Path.Combine(Environment.WebRootPath, "signatures");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                foreach (var item in signs)
                {
                    try
                    {

                        var fname = Path.GetFileNameWithoutExtension(item.FileName.Split(".")[0]);

                        // Generate a unique filename 
                        string fileName = fname.Split("_")[0] + Path.GetExtension(item.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            item.CopyTo(fileStream);
                        }

                        tbl_ie_sign model = new tbl_ie_sign()
                        {

                            patient_id = Convert.ToInt32(fname.Split("_")[0]),
                            signatureData = fileName
                        };
                        _services.ManageSign(model);
                    }
                    catch (Exception)
                    {

                    }
                    //model.header_template = fileName;
                }

            }
            TempData["msg"] = "<script>alert('Upload succesfully');</script>";
            return View();
        }

        public IActionResult UploadDocuments()
        {
            var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments");
            string[] dirs = Directory.GetDirectories(FolderPath, "*", SearchOption.TopDirectoryOnly);


            List<UploadDocVM> lst = new List<UploadDocVM>();

            foreach (var item in dirs)
            {
                string FolderName = System.IO.Path.GetFileName(item);
                UploadDocVM obj = new UploadDocVM()
                {
                    DirName = FolderName
                };
                lst.Add(obj);
            }

            ViewBag.LstCmp = new SelectList(_cmpservices.GetAll(), "id", "name");

            return View(lst);
        }

        [HttpPost]

        public IActionResult UploadDocuments(UploadDocVM obj)
        {
            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            StringBuilder sb = new StringBuilder();
            try
            {
                

                if (obj.files != null && obj.files.Count > 0)
                {
                    sb.Append("<ul>");
                    foreach (var file in obj.files)
                    {
                        // Get the file name from the browser


                        var fname = Path.GetFileNameWithoutExtension(file.FileName);

                        var fulllname = fname.Split('_')[0];

                        var lastname = fulllname.Split(',')[0].TrimStart();

                        var firstname = "";

                      

                        if (fulllname.Split(',').Length>1)
                            firstname = fulllname.Split(',')[1].TrimStart();


                        var patientData = _patientservices.GetAll(" and fname='" + firstname + "' and lname='" + lastname + "' and cmp_id=" + cmpid);


                        if (patientData != null && patientData.Count == 1)
                        {
                            string PatientID = patientData[0].id.ToString();

                            var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", obj.DirName, PatientID);


                            bool folderExists = Directory.Exists(FolderPath);
                            if (!folderExists)
                                Directory.CreateDirectory(FolderPath);

                            var filePath = Path.Combine(FolderPath, file.FileName);

                            // Check If file with same name exists and delete it
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }

                            // Create a new local file and copy contents of uploaded file
                            using (var localFile = System.IO.File.OpenWrite(filePath))
                            using (var uploadedFile = file.OpenReadStream())
                            {
                                uploadedFile.CopyTo(localFile);
                            }
                            sb.AppendFormat("<li style='color:green'>{0} - Uploaded Successfully</li>", file.FileName);
                        }
                        else if (patientData != null && patientData.Count > 1)
                        {
                            sb.AppendFormat("<li style='color:red'>{0} - More than 1 entry found for this patient</li>", file.FileName);
                        }
                        else
                        {
                            sb.AppendFormat("<li style='color:red'>{0} - No Patient found</li>", file.FileName);
                        }
                    }
                    sb.Append("</ul>");

                }
            }
            catch (Exception ex)
            {

                return Json(false);
            }

            return Json(sb.ToString());
        }
    }
}
