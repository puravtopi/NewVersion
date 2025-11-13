using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Humanizer;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Mvc;
using MS.Models;
using MS.Services;
using Newtonsoft.Json;
using PainTrax.Services;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PainTrax.Web.Controllers
{
    public class ImportPdfController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly PatientIEService _ieService = new PatientIEService();
        private readonly PatientService _patientservices = new PatientService();
        private readonly ParentService _pareentservices = new ParentService();
        private readonly Common _commonservices = new Common();
        private readonly InscosService _inscosservices = new InscosService();
        private readonly AttorneysService _attorneyservices = new AttorneysService();
        private readonly AadjusterService _aadjusterService = new AadjusterService();
        private readonly EmpService _empService = new EmpService();
        private readonly UserService _userService = new UserService();

        public ImportPdfController( IWebHostEnvironment environment)
        {
            _environment = environment;
            
        }
        public IActionResult Index()
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            var providers = _userService.GetProviders(cmpid.Value);
            ViewBag.providerList = providers;
            return View();
        }
        [HttpPost]
        public IActionResult Index(List<IFormFile> files, string provider, string locationid)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            var providers = _userService.GetProviders(cmpid.Value);
            ViewBag.providerList = providers;
            string message = "";
            if ( locationid==null  || locationid=="0" )
            {
                message += $"<span class='text-danger'>Location not selected <span><br>";
                TempData["Message"] = message;
                return View();
            }

            var importFolder = Path.Combine(_environment.WebRootPath);
            if (files != null)
            {
                
                string data = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".pdf")
                    {
                        string filePath = Path.Combine(importFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        string[] lines = GetPdfData(filePath);
                        var pdfdata = ExtractKeyValuePairs(lines);
                        TempData["pdfdata"] = JsonConvert.SerializeObject(pdfdata);
                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                        var result=SaveDetails(pdfdata,provider,locationid);
                        if(result==1)
                            message += $"<span class='text-primary'>{file.FileName} File Import Successfully<span><br>";
                        else if(result==2)
                            message += $"<span class='text-danger'>{file.FileName} File data already imported <span><br>";
                        else
                             message += $"<span class='text-danger'>{file.FileName} File not imported <span><br>";
                        data += string.Join("<br> ", lines);

                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();
                TempData["data"] = data.ToString();
                TempData["alert"] = "alert alert-success";

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            return View();

        }

        private int calculateAge(DateTime bday)
        {
            DateTime today = DateTime.Today;

            int age = today.Year - bday.Year;

            if (today.Month < bday.Month ||
       ((today.Month == bday.Month) && (today.Day < bday.Day)))
            {
                age--;  //birthday in current year not yet reached, we are 1 year younger ;)
                        //+ no birthday for 29.2. guys ... sorry, just wrong date for birth
            }

            return age;
        }


        public int SaveDetails(List<KeyValuePair<string,string>> data,string provider,string locationid)
        {
            try
            {
                int patientId = 0, priminsId = 0, secinsId = 0, attornyId = 0, adjusterId = 0, empId = 0;

                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                int? userid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId);
                int age =calculateAge(DateTime.ParseExact(data.FirstOrDefault(x => x.Key == "D.O.B").Value, "MM/dd/yyyy", null));
                var nameValue = data.FirstOrDefault(x => x.Key == "Name").Value;

                tbl_patient objPatient = new tbl_patient()
                {
                    account_no = data.FirstOrDefault(x => x.Key == "MRN").Value,
                    address = data.FirstOrDefault(x => x.Key == "Address").Value,
                    dob = DateTime.ParseExact(data.FirstOrDefault(x => x.Key == "D.O.B").Value, "MM/dd/yyyy", null),
                    email = data.FirstOrDefault(x => x.Key == "Email").Value  ,
                    fname = nameValue.Split(' ')[0],
                    gender = data.FirstOrDefault(x => x.Key == "Sex").Value=="Male"? "1" : data.FirstOrDefault(x => x.Key == "Sex").Value == "Female" ? "2" : "3",
                    home_ph = data.FirstOrDefault(x => x.Key == "HomePhone").Value,
                    lname = nameValue.Split(' ').Length > 1 ? nameValue.Split(' ')[^1] : "",
                    mobile = data.FirstOrDefault(x => x.Key == "MobilePhone").Value,
                    ssn = data.FirstOrDefault(x => x.Key == "SSN").Value,
                    cmp_id = cmpid,
                    createdby = userid,
                    
                    age = age
                };
                List<tbl_patient> ptdata = new List<tbl_patient>();
                var dobStr = objPatient.dob.HasValue    ? objPatient.dob.Value.ToString("yyyy-MM-dd")    : "";
                var query = " and account_no='" + objPatient.account_no + "' and fname='" + objPatient.fname + "' and lname='" + objPatient.lname + "' and dob='" + dobStr + "' and cmp_id=" + cmpid + "";
                ptdata = _patientservices.GetAll(query);
                if (ptdata.Count > 0)
                {
                    return 2;
                }
                else
                {
                    patientId = _patientservices.Insert(objPatient);
                }
                    HttpContext.Session.SetInt32(SessionKeys.SessionPatientId, patientId);
                string pid = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();
                ViewBag.patientId = patientId;
                query = "";
                List<tbl_inscos> insdata = new List<tbl_inscos>();
                tbl_inscos objInscos = new tbl_inscos();
                var addressList = data
                    .Where(x => x.Key.Equals("Address", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Value)
                    .ToList();
                string carrier = data.FirstOrDefault(x => x.Key == "Carrier").Value;
                string address = addressList.Count > 1 ? addressList[1] : null;

                if (!string.IsNullOrEmpty(carrier))
                {
                    query = " and cmpname='" + carrier + "' and cmp_id=" + cmpid + "";
                    insdata = _inscosservices.GetAll(query);

                    objInscos = new tbl_inscos()
                    {
                        address1 = address,
                        cmpname = carrier,
                        telephone = data.FirstOrDefault(x => x.Key == "PhoneNumber").Value,
                        cmp_id = cmpid,
                        createdby = userid
                    };

                    if (insdata.Count > 0)
                    {
                        objInscos.id = insdata[0].id.Value;
                        _inscosservices.Update(objInscos);
                        priminsId = insdata[0].id.Value;
                    }
                    else
                    {
                        priminsId = _inscosservices.Insert(objInscos);
                    }
                }

                //save IE details 

                tbl_patient_ie objIE = new tbl_patient_ie()
                {
                    adjuster_id = adjusterId,
                    attorney_id = attornyId,
                    created_by = userid,
                  
                    doe = DateTime.Now,
                    emp_id = empId,
                    is_active = true,
                    location_id = Convert.ToInt32( locationid),                    
                    patient_id = patientId,
                    primary_ins_cmp_id = priminsId,
                    primary_policy_no = data.FirstOrDefault(x => x.Key == "PolicyNo").Value,


                };
                int ie = 0;
                 ie = _ieService.Insert(objIE);

                HttpContext.Session.SetInt32(SessionKeys.SessionIEId, ie);
                return 1;
//                return Json(new { status = 1, patintid = patientId, ieid = ie });

            }
            catch (Exception ex)
            {
                return 0;
  //              return Json(new { status = 0 });
            }

                
        }


        public static List<KeyValuePair<string, string>> ExtractKeyValuePairs(string []lines)
        {
            var pairs = new List<KeyValuePair<string, string>>();

            // This regex finds *all* key:value pairs per line
            string pattern = @"([A-Za-z0-9\/\.\(\)\- ]+?):\s*([^:]*)(?=\s+[A-Za-z0-9\/\.\(\)\- ]+?:|$)";
            foreach (string pdfText in lines)
            {
                string text = pdfText
                .Replace("Home Phone", "HomePhone")
                .Replace("Work Phone", "WorkPhone")
                .Replace("Mobile Phone", "MobilePhone")
                .Replace("Preferred Contact Method", "PreferredContactMethod")
                .Replace("PMS ID", "PMSID")
                .Replace("Marital Status", "MaritalStatus")
                .Replace("Emergency Contact", "EmergencyContact")
                .Replace("Emergency Contact Phone", "EmergencyContactPhone")
                .Replace("Primary Care Provider", "PrimaryCareProvider")
                .Replace("Referring Providers", "ReferringProviders")
                .Replace("Policy #", "PolicyNo")
                .Replace("Group ID/Name", "GroupID/Name")
                .Replace("Phone Number", "PhoneNumber")
                .Replace("Fax Number", "FaxNumber");
                
                foreach (var rawLine in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    foreach (Match match in Regex.Matches(line, pattern))
                    {
                        string key = match.Groups[1].Value.Trim();
                        string value = match.Groups[2].Value.Trim();

                        pairs.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
            return pairs;
        
        }
        protected string[] GetPdfData(string filename)
        {
            StringBuilder text = new StringBuilder();

            if (System.IO.File.Exists(filename))
            {
                using (PdfReader reader = new PdfReader(filename))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                    {
                        var strategy = new SimpleTextExtractionStrategy();
                          string currentText = PdfTextExtractor.GetTextFromPage(
                               pdfDoc.GetPage(page),
                              strategy
                           );
                        //var finder = new TextMarginFinder();
                        //string currentText=PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), finder);
                        currentText = System.Text.RegularExpressions.Regex.Replace(currentText, @" {4,}", "\n");
                        text.Append(currentText);
                    }
                }
            }

            string cont = text.ToString();
            string[] lines = cont.Split(new[] { '\n' });
            return lines;
        }

     
    }
}
