using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PainTrax.Web.Filter;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using System.Data;
using System.Text.RegularExpressions;



namespace PainTrax.Web.Controllers
{
    [AuthenticateUser]
    public class PatientdocumentController : Controller
    {
        private readonly ILogger<PatientdocumentController> _logger;
        private readonly IMapper _mapper;
        private readonly PatientDocumentServices _services = new PatientDocumentServices();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private IConfiguration Configuration;

        public PatientdocumentController(IMapper mapper, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment,
                                ILogger<PatientdocumentController> logger, IConfiguration configuration)
        {
            _mapper = mapper;
            _logger = logger;
            Environment = environment;
            Configuration = configuration;
        }
        public ActionResult Index(int id = 0)
        {

            HttpContext.Session.SetInt32(SessionKeys.SessionPatientId, id);
            var data = new List<tbl_patientdocument>();
            string PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();


            //var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", PatientID);

            var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments");
            bool folderExists = Directory.Exists(FolderPath);
            if (!folderExists)
                Directory.CreateDirectory(FolderPath);

            string[] dirs = Directory.GetDirectories(FolderPath, "*", SearchOption.TopDirectoryOnly);

            List<TreeViewNode> nodes = new List<TreeViewNode>();

            int i = 0;
            foreach (var item in dirs)
            {
                i++;
                string FolderName = System.IO.Path.GetFileName(item);
                //nodes.Add(new TreeViewNode { id = Convert.ToString(i), parent = "#", text = item });
                

                var FolderPathFile = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", FolderName.ToString(), PatientID);
                int j = 0;

                bool folderExistsNew = Directory.Exists(FolderPathFile);
                if (!folderExistsNew)
                    Directory.CreateDirectory(FolderPathFile);

                foreach (var item1 in Directory.GetFiles(FolderPathFile))
                {
                    j++;
                    nodes.Add(new TreeViewNode { id = j.ToString() + "-" + i.ToString() + "~" + FolderName.ToString() + "$" + System.IO.Path.GetFileName(item1), parent = i.ToString(), text = System.IO.Path.GetFileName(item1) });
                    // data.Add(new tbl_patientdocument { DocName = System.IO.Path.GetFileName(item), Path = item });
                }

                nodes.Add(new TreeViewNode { id = i.ToString(), parent = "#", text = FolderName.ToString()+"("+j+")" });

            }

            ViewBag.Json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            return View();
        }

        [HttpPost]
        public ActionResult Index(string selectedItems)
        {

            List<TreeViewNode> items = JsonConvert.DeserializeObject<List<TreeViewNode>>(selectedItems);
            //string parentid = items[0].id;
            //string SelectedFolder = string.Empty;
            //int i = 0;


            //var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments");
            //bool folderExists = Directory.Exists(FolderPath);
            //if (!folderExists)
            //    Directory.CreateDirectory(FolderPath);

            //// Dstring rootPath = @"C:\Users\Koushik\Desktop\TestFolder";
            //string[] dirs = Directory.GetDirectories(FolderPath, "*", SearchOption.TopDirectoryOnly);
            //foreach (var item in dirs)
            //{
            //    i++;
            //    string FolderName = System.IO.Path.GetFileName(item);
            //    if (i == Convert.ToInt32(parentid))
            //    {
            //        SelectedFolder = FolderName.ToString();
            //    }
            //}

            //HttpContext.Session.SetString(SessionKeys.SessionSelectedFolder, SelectedFolder);
            //string selectedFolder = HttpContext.Session.GetString(SessionKeys.SessionSelectedFolder);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Download(string filename)
        {
            string PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();
            string selectedfolder = filename.Split('~')[0];
            string filenamenew = filename.Split('~')[1];
            if (filenamenew == null)
                return Content("filename is not availble");
            var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", selectedfolder, PatientID);

            var path = Path.Combine(FolderPath, filenamenew);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        public async Task<IActionResult> DeleteDoc(string filename)
        {
            string PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();
            string selectedfolder = filename.Split('~')[0];
            string filenamenew = filename.Split('~')[1];
            if (filenamenew == null)
                return Content("filename is not availble");
            var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", selectedfolder, PatientID);
            var path = Path.Combine(FolderPath, filenamenew);

            if (path != null || path != string.Empty)
            {
                if ((System.IO.File.Exists(path)))
                {
                    System.IO.File.Delete(path);
                }

            }
            //return RedirectToAction("Index/" + PatientID + "", "Patientdocument");
            return RedirectToAction("Index", "Patientdocument", new { id = PatientID });
            // return RedirectToAction("Index");
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
        public IActionResult Delete(int id)
        {
            try
            {
                tbl_patientdocument obj = new tbl_patientdocument();
                obj.Document_ID = id;
                _services.Delete(obj);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Delete");
            }
            return RedirectToAction("Index");
        }

        public IActionResult List()
        {
            try
            {
                string PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();
                var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
                // Skiping number of Rows count
                var start = Request.Form["start"].FirstOrDefault();
                // Paging Length 10,20
                var length = Request.Form["length"].FirstOrDefault();
                // Sort Column Name
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                // Sort Column Direction ( asc ,desc)
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                // Search Value from (Search box)
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                //Paging Size (10,20,50,100)
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                string cnd = " and PatientID = " + PatientID;// + " and DocName like '%" + searchValue + "%' ";
                var Data = _services.GetAll(cnd);
                //Sorting
                //Search
                //total number of rows count 
                recordsTotal = Data.Count();
                //Paging 
                var data = Data.Skip(skip).Take(pageSize).ToList();
                //Returning Json Data
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            }
            catch (Exception ex)
            {
                SaveLog(ex, "List");
            }
            return Json("");

        }
        [HttpPost]
        public IActionResult ImportData(string selectedParent, IFormFile[] postedFile)
        {
            string PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();
            try
            {
                string SelectedFolder = selectedParent.Replace('"', ' ').Trim();
                if (postedFile != null && postedFile.Length > 0)
                {
                    foreach (var file in postedFile)
                    {
                        tbl_patientdocument obj = new tbl_patientdocument();
                        // Get the file name from the browser
                        var fileName = System.IO.Path.GetFileName(file.FileName);


                        // Get file path to be uploaded

                        var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", SelectedFolder, PatientID);


                        bool folderExists = Directory.Exists(FolderPath);
                        if (!folderExists)
                            Directory.CreateDirectory(FolderPath);

                        var filePath = Path.Combine(FolderPath, fileName);

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
                        obj.DocName = fileName.ToString();
                        obj.Path = filePath.ToString();
                        obj.PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId);
                        obj.UploadDate = System.DateTime.Now;
                        obj.CreatedBy = HttpContext.Session.GetString(SessionKeys.SessionUserName);
                        obj.CreatedDate = System.DateTime.Now;
                        _services.Insert(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "List");
            }
            return RedirectToAction("Index", "Patientdocument", new { id = PatientID });
        }

        /// <summary>
        /// Given a cell name, parses the specified cell to get the column name.
        /// </summary>
        /// <param name="cellReference">Address of the cell (ie. B2)</param>
        /// <returns>Column Name (ie. B)</returns>
        public static string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        } //end GetColumnName method

        /// <summary>
        /// Given just the column name (no row index), it will return the zero based column index.
        /// Note: This method will only handle columns with a length of up to two (ie. A to Z and AA to ZZ). 
        /// A length of three can be implemented when needed.
        /// </summary>
        /// <param name="columnName">Column Name (ie. A or AB)</param>
        /// <returns>Zero based index if the conversion was successful; otherwise null</returns>
        public static int? GetColumnIndexFromName(string columnName)
        {
            //return columnIndex;
            string name = columnName;
            int number = 0;
            int pow = 1;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                pow *= 26;
            }
            return number;
        } //end GetColumnIndexFromName method

        #region private Method
        private void SaveLog(Exception ex, string acctionname)
        {
            var msg = "";
            if (ex.InnerException == null)
            {
                _logger.LogError(ex.Message);
                msg = ex.Message;
            }
            else
            {
                _logger.LogError(ex.InnerException.Message);
                msg = ex.InnerException.Message;
            }
            var logdata = new tbl_log
            {
                CreatedDate = DateTime.Now,
                CreatedBy = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId),
                Message = msg,
            };
            new LogService().Insert(logdata);
        }
        #endregion
    }
}
