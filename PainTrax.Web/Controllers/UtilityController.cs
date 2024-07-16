using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Optivem.Framework.Core.Domain;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Security.Cryptography.Xml;

namespace PainTrax.Web.Controllers
{
    public class UtilityController : Controller
    {

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private IConfiguration Configuration;
        private readonly SignatureService _services = new SignatureService();

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
    }
}
