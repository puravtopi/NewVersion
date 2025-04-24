using Microsoft.AspNetCore.Mvc;
using MS.Services;
using PainTrax.Web.Helper;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Data;
using MS.Models;
using MySql.Data.MySqlClient;
using PainTrax.Services;
using GroupDocs.Viewer.Options;
using System.Text.RegularExpressions;

namespace PainTrax.Web.Controllers
{

    public class ImportDocController : Controller
    {
        private readonly ILogger<FormsController> _logger;

        private readonly IWebHostEnvironment _environment;
        private readonly PatientIEService _ieService = new PatientIEService();
        private readonly PatientService _patientservices = new PatientService();
        private readonly ParentService _pareentservices = new ParentService();

        public ImportDocController(ILogger<FormsController> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
        }
        public IActionResult Index()
        {
            //TempData.Keep("Data");
            return View();
        }
        public void UpdateId(ref DataRow datarow, string name, string dob, string filename, string doctype = "IE", string doe = "")
        {
            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            string pid = "0";
            string bdate = "";
            string adate = "";
            try
            {
                string filenameonly = Path.GetFileNameWithoutExtension(filename);
                adate = DateTime.ParseExact((filenameonly.Substring(filenameonly.Length - 6)).ToString().Trim(), "MMddyy", null).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {

            }

            if (dob.ToString() != "")
                bdate = DateTime.ParseExact(dob.ToString().Trim(), "M/d/yyyy", null).ToString("yyyy-MM-dd");
            string[] fullname = name.ToString().Trim().Split(' ');
            DataTable dt = new DataTable();
            if (fullname.Length > 1)
            {
                if (doctype == "IE")
                {
                    dt = _pareentservices.GetData($"select * from vm_patient_ie where  fname='{fullname[0]}' and lname='{fullname[1]}' and dob='{bdate}' and doa='{adate}' and cmp_id={cmpid}");
                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_ie_id"] = dt.Rows[0]["id"]; } catch { }
                    }
                }
                if (doctype == "FU")
                {
                    string edate = "";
                    if (doe.ToString() != "")
                        edate = DateTime.ParseExact(doe.ToString().Trim(), "M/d/yyyy", null).ToString("yyyy-MM-dd");
                    //                    dt = _pareentservices.GetData($"select * from vm_patient_fu where  fname='{fullname[0]}' and lname='{fullname[1]}'  and DATE(doe)='{edate}' and cmp_id={cmpid}");

                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{edate}'  and DATE(vm_patient_fu.doa)= '{adate}' and vm_patient_ie.cmp_id = {cmpid}");
                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_fu_id"] = dt.Rows[0]["patientFU"]; } catch { }
                    }
                }

                datarow["FName"] = fullname.Length > 0 ? fullname[0].ToString().Trim() : "";
                datarow["LName"] = fullname.Length > 1 ? fullname[1].ToString().Trim() : "";
                datarow["MName"] = fullname.Length > 2 && !fullname[2].ToString().StartsWith("[") ? fullname[2].ToString().Trim() : "";
                datarow["DOA"] = adate;
                if (dob.ToString() != "") datarow["DOB"] = bdate;
                datarow["Name"] = name.ToString().Trim();
            }

        }

        [HttpPost]
        public ActionResult UploadFile(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_ie_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOB", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("History", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("GAIT", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Current Medications", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));


            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        DataRow row = ConvertDocxToHtml(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                        dataTable.Rows.Add(row);

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    // Save the workbook
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "IEs.xlsx");
            }
            //return RedirectToAction("Index");


        }

        [HttpPost]
        public ActionResult UploadFileFU(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_fu_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOE", typeof(string));
            dataTable.Columns.Add("DO1E", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("PP", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("GAIT", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Current Medications", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));


            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        DataRow row = ConvertDocxToHtmlFU(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                        dataTable.Rows.Add(row);

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    // Save the workbook
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FUs.xlsx");
            }
            //return RedirectToAction("Index");


        }




        [HttpPost]
        public ActionResult ImportData(string folderName, IFormFile file)
        {


            TempData["Message"] = TempData["Data"];
            return RedirectToAction("Index");
        }



        string GetParagraphText(Paragraph paragraph)
        {
            StringBuilder paragraphText = new StringBuilder();

            foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
            {
                foreach (var text in run.Elements<Text>())
                {
                    paragraphText.Append(text.Text);
                }
            }

            return paragraphText.ToString();
        }

        DataRow ConvertDocxToHtml(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder dob = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder gait = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                bool foundcurmedications = false;
                bool foundhistory = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("RE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("RE:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("DOB:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            dob.Append(paragraphText.Substring(("DOB:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.Contains("HISTORY OF PRESENT ILLNESS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // history.Append($"<p>{paragraphText}</p>");
                            // history.Append(paragraphText);
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = true;
                            continue;
                        }
                        if (paragraphText.Contains("CHIEF COMPLAINTS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //cc.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL/HOSPITALIZATION HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL/HOSPITALIZATION HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PHYSICAL EXAM:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GAIT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //gait.Append($"<p>{paragraphText}</p>");
                            gait.Append(paragraphText.Substring(("GAIT:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnostic Studies:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //diagnosticstudies.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append(paragraphText.Substring(("Diagnostic Studies:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnoses:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //diagnoses.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Plan:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // plan.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //curmedications.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = true;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Care:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //care.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("Care:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Goals:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //goals.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("Goals:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Precautions:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("Precautions:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Follow-up:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("Follow-up:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                            //cc.Append(paragraphText);

                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                            //pe.Append(paragraphText);

                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                            //diagnoses.Append(paragraphText);

                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                            //plan.Append(paragraphText);

                        }
                        if (foundcurmedications)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            curmedications.Append($"<p>{paragraphText}</p>");
                            //curmedications.Append(paragraphText);
                        }

                    }

                    else if (element is DocumentFormat.OpenXml.Wordprocessing.Table table)
                    {
                        html.Append("<table border='1'>");
                        foreach (var row in table.Elements<TableRow>())
                        {
                            html.Append("<tr>");
                            foreach (var cell in row.Elements<TableCell>())
                            {
                                html.Append("<td>");
                                foreach (var cellParagraph in cell.Elements<Paragraph>())
                                {
                                    foreach (var run in cellParagraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                                    {
                                        foreach (var text in run.Elements<Text>())
                                        {
                                            html.Append(text.Text);
                                        }
                                    }
                                }
                                html.Append("</td>");
                            }
                            html.Append("</tr>");
                        }
                        html.Append("</table>");
                    }
                    // Add more handling for other types of elements if needed
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();
                UpdateId(ref datarow, name.ToString(), dob.ToString(), filename);
                datarow["History"] = history.ToString().Trim();
                datarow["CC"] = cc.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["GAIT"] = gait.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();

                return datarow;

            }
        }
        DataRow ConvertDocxToHtmlFU(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder doe = new StringBuilder();
                StringBuilder doie = new StringBuilder();
                StringBuilder doa = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder gait = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                bool foundcurmedications = false;
                bool foundhistory = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("Patient Name:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("Patient Name:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Dt. of Exam:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            doe.Append(paragraphText.Substring(("Dt. of Exam:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("1st Exam Dt.:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            doie.Append(paragraphText.Substring(("1st Exam Dt.:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Dt. of Injury:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            doa.Append(paragraphText.Substring(("Dt. of Injury:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.Contains("HISTORY OF PRESENT ILLNESS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // history.Append($"<p>{paragraphText}</p>");
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = true;
                            continue;
                        }
                        if (paragraphText.Contains("Chief Complaint:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //cc.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //ros.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL/HOSPITALIZATION HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL/HOSPITALIZATION HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Physical Examination:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GAIT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //gait.Append($"<p>{paragraphText}</p>");
                            gait.Append(paragraphText.Substring(("GAIT:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnostic Studies:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //diagnosticstudies.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append(paragraphText.Substring(("Diagnostic Studies:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnoses:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //diagnoses.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Plan:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // plan.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //curmedications.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = true;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Care:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //care.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("Care:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Goals:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //goals.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("Goals:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Precautions:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("Precautions:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Follow-up:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("Follow-up:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            foundcurmedications = false;
                            foundhistory = false;
                            continue;
                        }
                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                            //cc.Append(paragraphText);

                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                            //pe.Append(paragraphText);

                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                            //diagnoses.Append(paragraphText);

                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                            //plan.Append(paragraphText);

                        }
                        if (foundcurmedications)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            curmedications.Append($"<p>{paragraphText}</p>");
                            //curmedications.Append(paragraphText);
                        }

                    }

                    else if (element is DocumentFormat.OpenXml.Wordprocessing.Table table)
                    {
                        html.Append("<table border='1'>");
                        foreach (var row in table.Elements<TableRow>())
                        {
                            html.Append("<tr>");
                            foreach (var cell in row.Elements<TableCell>())
                            {
                                html.Append("<td>");
                                foreach (var cellParagraph in cell.Elements<Paragraph>())
                                {
                                    foreach (var run in cellParagraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                                    {
                                        foreach (var text in run.Elements<Text>())
                                        {
                                            html.Append(text.Text);
                                        }
                                    }
                                }
                                html.Append("</td>");
                            }
                            html.Append("</tr>");
                        }
                        html.Append("</table>");
                    }
                    // Add more handling for other types of elements if needed
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();
                UpdateId(ref datarow, name.ToString(), "", filename, "FU", doe.ToString());
                datarow["DOA"] = DateTime.ParseExact(doa.ToString().Trim(), "M/d/yyyy", null).ToString("yyyy-MM-dd");
                datarow["DOE"] = DateTime.ParseExact(doe.ToString().Trim(), "M/d/yyyy", null).ToString("yyyy-MM-dd");
                datarow["DO1E"] = DateTime.ParseExact(doie.ToString().Trim(), "M/d/yyyy", null).ToString("yyyy-MM-dd");
                datarow["CC"] = cc.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["GAIT"] = gait.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();

                return datarow;

            }
        }

        [HttpPost]
        public ActionResult IPMCUploadFile(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_ie_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOB", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("DOS", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Reason", typeof(string));
            dataTable.Columns.Add("History", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("Activities", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Occupation", typeof(string));
            dataTable.Columns.Add("Tylenol", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));
            //DOS, Location, Reason, Occupation, Tylenol

            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            StringBuilder errfile = new StringBuilder();
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        try
                        {
                            DataRow row = IPMCConvertDocxToHtml(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                            dataTable.Rows.Add(row);
                        }
                        catch { errfile.Append(file.FileName); }

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);


                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    if (errfile.Length > 0)
                    {
                        WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData2 = new SheetData();
                        worksheetPart2.Worksheet = new Worksheet(sheetData2);

                        // Append Sheet2 to the workbook
                        Sheet sheet2 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                            SheetId = 2,
                            Name = "ErrorSheet"
                        };
                        sheets.Append(sheet2);
                        string[] lines = errfile.ToString().Split("\n");
                        foreach (var line in lines)
                        {
                            Row row = new Row();
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(line)
                            };
                            row.AppendChild(cell);
                            sheetData2.AppendChild(row);
                        }
                    }
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "IEs.xlsx");
            }
            //return RedirectToAction("Index");


        }

        [HttpPost]
        public ActionResult IPMCUploadFileFU(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_ie_id", typeof(string));
            dataTable.Columns.Add("Patient_fu_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOB", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("DOS", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Reason", typeof(string));
            dataTable.Columns.Add("History", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("Activities", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Occupation", typeof(string));
            dataTable.Columns.Add("Tylenol", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));


            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            StringBuilder errfile = new StringBuilder();
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        try
                        {
                            DataRow row = IPMCConvertDocxToHtmlFU(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                            dataTable.Rows.Add(row);
                        }
                        catch { errfile.Append(file.FileName); }

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    if (errfile.Length > 0)
                    {
                        WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData2 = new SheetData();
                        worksheetPart2.Worksheet = new Worksheet(sheetData2);

                        // Append Sheet2 to the workbook
                        Sheet sheet2 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                            SheetId = 2,
                            Name = "ErrorSheet"
                        };
                        sheets.Append(sheet2);
                        string[] lines = errfile.ToString().Split("\n");
                        foreach (var line in lines)
                        {
                            Row row = new Row();
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(line)
                            };
                            row.AppendChild(cell);
                            sheetData2.AppendChild(row);
                        }
                    }
                    // Save the workbook
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FUs.xlsx");
            }
            //return RedirectToAction("Index");


        }

        DataRow IPMCConvertDocxToHtml(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                //DOS, Location, Reason, Occupation, Tylenol
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder dob = new StringBuilder();
                StringBuilder doa = new StringBuilder();
                StringBuilder dos = new StringBuilder();
                StringBuilder location = new StringBuilder();
                StringBuilder reason = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder activities = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
                StringBuilder occupation = new StringBuilder();
                StringBuilder tylenol = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                //bool foundcurmedications = false;
                bool foundhistory = false;
                bool foundreason = false;
                bool founddiagnosticstudies = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("RE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("RE:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("DOB:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dob.ToString().Length == 0)
                                dob.Append(paragraphText.Substring(("DOB:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("DOI:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (doa.ToString().Length == 0)
                                doa.Append(paragraphText.Substring(("DOI:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("DOS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dos.ToString().Length == 0)
                                dos.Append(paragraphText.Substring(("DOS:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("LOCATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            location.Append(paragraphText.Substring(("LOCATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("REASON FOR APPOINTMENT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = true;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PRESENT COMPLAINTS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ACTIVITIES OF DAILY LIVING:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            activities.Append(paragraphText.Substring(("ACTIVITIES OF DAILY LIVING:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("OCCUPATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            occupation.Append(paragraphText.Substring(("OCCUPATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ARE YOU TAKING ANY NSAIDS OR TYLENOL:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            tylenol.Append(paragraphText.Substring(("ARE YOU TAKING ANY NSAIDS OR TYLENOL:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("PHYSICAL EXAM:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("DIAGNOSTIC STUDIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");


                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = true;
                            continue;
                        }

                        if (paragraphText.StartsWith("DIAGNOSES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            //     foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ASSESSMENT AND PLAN:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = true;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CARE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("CARE:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GOALS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("GOALS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PRECAUTIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("PRECAUTIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("FOLLOW-UP:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("FOLLOW-UP:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                        }
                        // if (foundcurmedications)
                        //  {
                        //      html.Append($"<p>{paragraphText}</p>");
                        //      curmedications.Append($"<p>{paragraphText}</p>");
                        //   }
                        if (foundreason)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            reason.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnosticstudies)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append($"<p>{paragraphText}</p>");
                        }
                    }
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string pid = "0";
                string bdate = "";
                string adate = "";
                string sdate = "";

                try
                {
                    if (doa.ToString() != "")
                        adate = DateTime.ParseExact(doa.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch (Exception ex) { }

                try
                {
                    if (dob.ToString() != "")
                        bdate = DateTime.ParseExact(dob.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch (Exception ex) { }

                try
                {
                    if (dos.ToString() != "")
                        sdate = DateTime.ParseExact(dos.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch (Exception ex) { }

                string[] fullname = name.ToString().Trim().Split(' ');
                DataTable dt = new DataTable();
                if (fullname.Length > 1)
                {
                    //                 dt = _pareentservices.GetData($"select * from vm_patient_ie where  fname='{fullname[0]}' and lname='{fullname[1]}' and dob='{bdate}' and doa='{adate}' and cmp_id={cmpid}");
                    dt = _pareentservices.GetData($"select * from vm_patient_ie where  fname='{fullname[0]}' and lname='{fullname[1]}' and   doe='{sdate}' and cmp_id={cmpid}");
                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_ie_id"] = dt.Rows[0]["id"]; } catch { }
                    }
                }

                datarow["FName"] = fullname.Length > 0 ? fullname[0].ToString().Trim() : "";
                datarow["LName"] = fullname.Length > 1 ? fullname[1].ToString().Trim() : "";
                datarow["MName"] = fullname.Length > 2 && !fullname[2].ToString().StartsWith("[") ? fullname[2].ToString().Trim() : "";
                datarow["DOA"] = adate;
                datarow["DOB"] = bdate;
                datarow["DOS"] = sdate;
                datarow["Name"] = name.ToString().Trim();
                datarow["Location"] = location.ToString().Trim();
                datarow["Reason"] = reason.ToString().Trim();
                datarow["History"] = history.ToString().Trim();
                datarow["CC"] = cc.ToString().Trim();
                datarow["Activities"] = activities.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
                datarow["Occupation"] = occupation.ToString().Trim();
                datarow["Tylenol"] = tylenol.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                //   datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();

                return datarow;

            }
        }
        DataRow IPMCConvertDocxToHtmlFU(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                //DOS, Location, Reason, Occupation, Tylenol
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder dob = new StringBuilder();
                StringBuilder doa = new StringBuilder();
                StringBuilder dos = new StringBuilder();
                StringBuilder location = new StringBuilder();
                StringBuilder reason = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder activities = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
                StringBuilder occupation = new StringBuilder();
                StringBuilder tylenol = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                //bool foundcurmedications = false;
                bool foundhistory = false;
                bool foundreason = false;
                bool founddiagnosticstudies = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("RE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("RE:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("DOB:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dob.Length == 0)
                                dob.Append(paragraphText.Substring(("DOB:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("DOI:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (doa.Length == 0)
                                doa.Append(paragraphText.Substring(("DOI:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("DOS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dos.Length == 0)
                                dos.Append(paragraphText.Substring(("DOS:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("LOCATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            location.Append(paragraphText.Substring(("LOCATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CHIEF COMPLAINT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = true;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PRESENT COMPLAINTS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ACTIVITIES OF DAILY LIVING:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            activities.Append(paragraphText.Substring(("ACTIVITIES OF DAILY LIVING:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("OCCUPATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            occupation.Append(paragraphText.Substring(("OCCUPATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ARE YOU TAKING ANY NSAIDS OR TYLENOL:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            tylenol.Append(paragraphText.Substring(("ARE YOU TAKING ANY NSAIDS OR TYLENOL:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("PHYSICAL EXAM:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("DIAGNOSTIC STUDIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");


                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = true;
                            continue;
                        }

                        if (paragraphText.StartsWith("DIAGNOSES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            //     foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ASSESSMENT AND PLAN:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = true;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CARE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("CARE:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GOALS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("GOALS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PRECAUTIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("PRECAUTIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("FOLLOW-UP:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("FOLLOW-UP:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                        }
                        // if (foundcurmedications)
                        //  {
                        //      html.Append($"<p>{paragraphText}</p>");
                        //      curmedications.Append($"<p>{paragraphText}</p>");
                        //   }
                        if (foundreason)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            reason.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnosticstudies)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append($"<p>{paragraphText}</p>");
                        }
                    }
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string pid = "0";
                string bdate = "";
                string adate = "";
                string sdate = "";
                try
                {
                    if (doa.ToString() != "")
                        adate = DateTime.ParseExact(doa.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                try
                {
                    if (dob.ToString() != "")
                        bdate = DateTime.ParseExact(dob.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                try
                {
                    if (dos.ToString() != "")
                        sdate = DateTime.ParseExact(dos.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                string[] fullname = name.ToString().Trim().Split(' ');
                DataTable dt = new DataTable();
                if (fullname.Length > 1)
                {
                    //                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{sdate}'  and DATE(vm_patient_fu.doa)= '{adate}' and vm_patient_ie.cmp_id = {cmpid}");
                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{sdate}'   and vm_patient_ie.cmp_id = {cmpid}");

                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_ie_id"] = dt.Rows[0]["patientIE_ID"]; } catch { }
                        try { datarow["Patient_fu_id"] = dt.Rows[0]["patientFU"]; } catch { }
                    }

                }
                datarow["FName"] = fullname.Length > 0 ? fullname[0].ToString().Trim() : "";
                datarow["LName"] = fullname.Length > 1 ? fullname[1].ToString().Trim() : "";
                datarow["MName"] = fullname.Length > 2 && !fullname[2].ToString().StartsWith("[") ? fullname[2].ToString().Trim() : "";
                datarow["DOA"] = adate;
                datarow["DOB"] = bdate;
                datarow["DOS"] = sdate;
                datarow["Name"] = name.ToString().Trim();
                datarow["Location"] = location.ToString().Trim();
                datarow["Reason"] = reason.ToString().Trim();
                datarow["History"] = history.ToString().Trim();
                datarow["CC"] = cc.ToString().Trim();
                datarow["Activities"] = activities.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
                datarow["Occupation"] = occupation.ToString().Trim();
                datarow["Tylenol"] = tylenol.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                //   datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();

                return datarow;

            }
        }

        [HttpPost]
        public ActionResult MNPLLCUploadFile(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_ie_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOB", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("DOS", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Reason", typeof(string));
            dataTable.Columns.Add("History", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("Activities", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Occupation", typeof(string));
            dataTable.Columns.Add("Tylenol", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));
            dataTable.Columns.Add("GAIT", typeof(string));
           
            dataTable.Columns.Add("Procedures", typeof(string));
            dataTable.Columns.Add("Goals", typeof(string));
            //DOS, Location, Reason, Occupation, Tylenol

            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            StringBuilder errfile = new StringBuilder();
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        try
                        {
                            DataRow row = MNPLLCConvertDocxToHtml(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                            dataTable.Rows.Add(row);
                        }
                        catch(Exception ex) { errfile.Append(file.FileName); }

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);


                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    if (errfile.Length > 0)
                    {
                        WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData2 = new SheetData();
                        worksheetPart2.Worksheet = new Worksheet(sheetData2);

                        // Append Sheet2 to the workbook
                        Sheet sheet2 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                            SheetId = 2,
                            Name = "ErrorSheet"
                        };
                        sheets.Append(sheet2);
                        string[] lines = errfile.ToString().Split("\n");
                        foreach (var line in lines)
                        {
                            Row row = new Row();
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(line)
                            };
                            row.AppendChild(cell);
                            sheetData2.AppendChild(row);
                        }
                    }
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "IEs.xlsx");
            }
            //return RedirectToAction("Index");


        }

        DataRow MNPLLCConvertDocxToHtml(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                //DOS, Location, Reason, Occupation, Tylenol
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder dob = new StringBuilder();
                StringBuilder doa = new StringBuilder();
                StringBuilder dos = new StringBuilder();
                StringBuilder location = new StringBuilder();
                StringBuilder reason = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder activities = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
              //  StringBuilder occupation = new StringBuilder();
              // StringBuilder tylenol = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();
                StringBuilder gait = new StringBuilder();
                StringBuilder procedures = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                //bool foundcurmedications = false;
                bool foundhistory = false;
                bool foundreason = false;
                bool founddiagnosticstudies = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("RE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("RE:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("DOB:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dob.ToString().Length == 0)
                                dob.Append(paragraphText.Substring(("DOB:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("DOI:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (doa.ToString().Length == 0)
                                doa.Append(paragraphText.Substring(("DOI:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Date:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dos.ToString().Length == 0)
                                dos.Append(paragraphText.Substring(("Date:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Location:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            location.Append(paragraphText.Substring(("Location:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("REASON FOR APPOINTMENT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = true;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CHIEF COMPLAINTS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ACTIVITIES OF DAILY LIVING:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            activities.Append(paragraphText.Substring(("ACTIVITIES OF DAILY LIVING:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL/HOSPITALIZATION HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL/HOSPITALIZATION HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                      /*  if (paragraphText.StartsWith("OCCUPATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            occupation.Append(paragraphText.Substring(("OCCUPATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }*/

                      /*  if (paragraphText.StartsWith("ARE YOU TAKING ANY NSAIDS OR TYLENOL:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            tylenol.Append(paragraphText.Substring(("ARE YOU TAKING ANY NSAIDS OR TYLENOL:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }*/


                        if (paragraphText.StartsWith("PHYSICAL EXAM:") || paragraphText.StartsWith("PHYSICAL EXAMINATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GAIT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");

                            gait.Append(paragraphText.Substring(("GAIT:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            //founddiagnosticstudies = true;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnostic Studies:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");

                            diagnosticstudies.Append(paragraphText.Substring(("Diagnostic Studies:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            //founddiagnosticstudies = true;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnoses:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            //     foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Plan:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = true;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Care:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("Care:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Procedures:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            procedures.Append(paragraphText.Substring(("Procedures:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Goals:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("Goals:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Precautions:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("Precautions:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Follow-up:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("Follow-up:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                        }
                        // if (foundcurmedications)
                        //  {
                        //      html.Append($"<p>{paragraphText}</p>");
                        //      curmedications.Append($"<p>{paragraphText}</p>");
                        //   }
                        if (foundreason)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            reason.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnosticstudies)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append($"<p>{paragraphText}</p>");
                        }
                    }
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string pid = "0";
                string bdate = "";
                string adate = "";
                string sdate = "";

                try
                {
                    if (doa.ToString() != "")
                        adate = DateTime.ParseExact(doa.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch (Exception ex) { }

                try
                {
                    if (dob.ToString() != "")
                        bdate = DateTime.ParseExact(dob.ToString().Trim(), "M/d/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch (Exception ex) { }

                try
                {
                    if (dos.ToString() != "")
                        sdate = DateTime.ParseExact(dos.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch (Exception ex) { }

                string[] fullname = name.ToString().Trim().Split(' ');
                try
                {
                    
                    string namePart = filename.Substring(0, filename.IndexOf("_")).Trim();
                    string[] nameParts = namePart.Split(',');
                    fullname= new string[] { nameParts[1].Trim(), nameParts[0].Trim() };
                }
                catch (Exception ex) { }
                DataTable dt = new DataTable();
                if (fullname.Length > 1)
                {
                    //                 dt = _pareentservices.GetData($"select * from vm_patient_ie where  fname='{fullname[0]}' and lname='{fullname[1]}' and dob='{bdate}' and doa='{adate}' and cmp_id={cmpid}");
                    dt = _pareentservices.GetData($"select * from vm_patient_ie where  fname='{fullname[0]}' and lname='{fullname[1]}' and   doe='{sdate}' and cmp_id={cmpid}");
                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_ie_id"] = dt.Rows[0]["id"]; } catch { }
                    }
                }
                Regex regex = new Regex(@"_(\d{6})_(\d{6})");
                Match match = regex.Match(filename);
                try
                {
                    if (match.Success)
                    {
                        string secondDate = match.Groups[2].Value; // Extract second MMddyy
                        string month = secondDate.Substring(0, 2);
                        string day = secondDate.Substring(2, 2);
                        string year = "20" + secondDate.Substring(4, 2); // Assuming 20xx format
                        adate = $"{year}-{month}-{day}";

                    }
                }
                catch (Exception ex) { }


                datarow["FName"] = fullname.Length > 0 ? fullname[0].ToString().Trim() : "";
                datarow["LName"] = fullname.Length > 1 ? fullname[1].ToString().Trim() : "";
                datarow["MName"] = fullname.Length > 2 && !fullname[2].ToString().StartsWith("[") ? fullname[2].ToString().Trim() : "";
                datarow["DOA"] = adate;
                datarow["DOB"] = bdate;
                datarow["DOS"] = sdate;
                datarow["Name"] = name.ToString().Trim();
                datarow["Location"] = location.ToString().Trim();
                datarow["Reason"] = reason.ToString().Trim();
                datarow["History"] = history.ToString().Trim();
                datarow["CC"] = cc.ToString().Trim();
                datarow["Activities"] = activities.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
              //  datarow["Occupation"] = occupation.ToString().Trim();
               // datarow["Tylenol"] = tylenol.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                //   datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();
                datarow["GAIT"] = gait.ToString().Trim();
                datarow["Procedures"] = procedures.ToString().Trim();
                datarow["Goals"] = goals.ToString().Trim();
                return datarow;

            }
        }
        
        [HttpPost]
        public ActionResult MNPLLCUploadFileFU(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_ie_id", typeof(string));
            dataTable.Columns.Add("Patient_fu_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOB", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("DOS", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Reason", typeof(string));
            dataTable.Columns.Add("History", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("Activities", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Occupation", typeof(string));
            dataTable.Columns.Add("Tylenol", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));
            dataTable.Columns.Add("GAIT", typeof(string));
            dataTable.Columns.Add("Case Type", typeof(string));


            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            StringBuilder errfile = new StringBuilder();
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        try
                        {
                            DataRow row = MNPLLCConvertDocxToHtmlFU(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                            dataTable.Rows.Add(row);
                        }
                        catch { errfile.Append(file.FileName); }

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    if (errfile.Length > 0)
                    {
                        WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData2 = new SheetData();
                        worksheetPart2.Worksheet = new Worksheet(sheetData2);

                        // Append Sheet2 to the workbook
                        Sheet sheet2 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                            SheetId = 2,
                            Name = "ErrorSheet"
                        };
                        sheets.Append(sheet2);
                        string[] lines = errfile.ToString().Split("\n");
                        foreach (var line in lines)
                        {
                            Row row = new Row();
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(line)
                            };
                            row.AppendChild(cell);
                            sheetData2.AppendChild(row);
                        }
                    }
                    // Save the workbook
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FUs.xlsx");
            }
            //return RedirectToAction("Index");


        }

        DataRow MNPLLCConvertDocxToHtmlFU(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                //DOS, Location, Reason, Occupation, Tylenol
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder dob = new StringBuilder();
                StringBuilder doa = new StringBuilder();
                StringBuilder dos = new StringBuilder();
                StringBuilder location = new StringBuilder();
                StringBuilder reason = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder activities = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
                StringBuilder occupation = new StringBuilder();
                StringBuilder tylenol = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();
                StringBuilder casetype= new StringBuilder();
                StringBuilder gait = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                //bool foundcurmedications = false;
                bool foundhistory = false;
                bool foundreason = false;
                bool founddiagnosticstudies = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("Patient Name:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("Patient Name:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("DOB:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dob.Length == 0)
                                dob.Append(paragraphText.Substring(("DOB:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Dt. of Injury:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (doa.Length == 0)
                                doa.Append(paragraphText.Substring(("Dt. of Injury:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Dt. of Exam:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dos.Length == 0)
                                dos.Append(paragraphText.Substring(("Dt. of Exam:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Location:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            location.Append(paragraphText.Substring(("LOCATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CHIEF COMPLAINT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Case Type:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            casetype.Append(paragraphText.Substring(("Case Type:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Chief Complaint:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ACTIVITIES OF DAILY LIVING:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            activities.Append(paragraphText.Substring(("ACTIVITIES OF DAILY LIVING:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL/HOSPITALIZATION HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL/HOSPITALIZATION HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("OCCUPATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            occupation.Append(paragraphText.Substring(("OCCUPATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ARE YOU TAKING ANY NSAIDS OR TYLENOL:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            tylenol.Append(paragraphText.Substring(("ARE YOU TAKING ANY NSAIDS OR TYLENOL:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("Physical Examination:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GAIT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            gait.Append(paragraphText.Substring(("GAIT:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnostic Studies:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append(paragraphText.Substring(("Diagnostic Studies:".Length)).Trim());

                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = true;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnoses:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            //     foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Plan:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = true;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CARE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("CARE:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GOALS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("GOALS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PRECAUTIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("PRECAUTIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Follow-up:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("Follow-up:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                        }
                        // if (foundcurmedications)
                        //  {
                        //      html.Append($"<p>{paragraphText}</p>");
                        //      curmedications.Append($"<p>{paragraphText}</p>");
                        //   }
                        if (foundreason)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            reason.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnosticstudies)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append($"<p>{paragraphText}</p>");
                        }
                    }
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string pid = "0";
                string bdate = "";
                string adate = "";
                string sdate = "";
                try
                {
                    if (doa.ToString() != "")
                        adate = DateTime.ParseExact(doa.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                try
                {
                    if (dob.ToString() != "")
                        bdate = DateTime.ParseExact(dob.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                try
                {
                    if (dos.ToString() != "")
                        sdate = DateTime.ParseExact(dos.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                string[] fullname = name.ToString().Trim().Split(' ');
                
                try
                {

                    string namePart = filename.Substring(0, filename.IndexOf("_")).Trim();
                    string[] nameParts = namePart.Split(',');
                    fullname = new string[] { nameParts[1].Trim(), nameParts[0].Trim() };
                }
                catch (Exception ex) { }
                DataTable dt = new DataTable();
                if (fullname.Length > 1)
                {
                    //                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{sdate}'  and DATE(vm_patient_fu.doa)= '{adate}' and vm_patient_ie.cmp_id = {cmpid}");
                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{sdate}'   and vm_patient_ie.cmp_id = {cmpid}");

                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_ie_id"] = dt.Rows[0]["patientIE_ID"]; } catch { }
                        try { datarow["Patient_fu_id"] = dt.Rows[0]["patientFU"]; } catch { }
                    }

                }
                datarow["FName"] = fullname.Length > 0 ? fullname[0].ToString().Trim() : "";
                datarow["LName"] = fullname.Length > 1 ? fullname[1].ToString().Trim() : "";
                datarow["MName"] = fullname.Length > 2 && !fullname[2].ToString().StartsWith("[") ? fullname[2].ToString().Trim() : "";
                datarow["DOA"] = adate;
                datarow["DOB"] = bdate;
                datarow["DOS"] = sdate;
                datarow["Name"] = name.ToString().Trim();
                datarow["Location"] = location.ToString().Trim();
                datarow["Reason"] = reason.ToString().Trim();
                datarow["History"] = history.ToString().Trim();
                datarow["CC"] = cc.ToString().Trim();
                datarow["Activities"] = activities.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
                datarow["Occupation"] = occupation.ToString().Trim();
                datarow["Tylenol"] = tylenol.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                //   datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();
                datarow["GAIT"] = gait.ToString().Trim();
                datarow["Case Type"] = casetype.ToString().Trim();

                return datarow;

            }
        }

        [HttpPost]
        public ActionResult MNPLLCUploadFileFUUpdate(List<IFormFile> files)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Patient_id", typeof(string));
            dataTable.Columns.Add("Patient_ie_id", typeof(string));
            dataTable.Columns.Add("Patient_fu_id", typeof(string));
            dataTable.Columns.Add("FName", typeof(string));
            dataTable.Columns.Add("LName", typeof(string));
            dataTable.Columns.Add("MName", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("DOB", typeof(string));
            dataTable.Columns.Add("DOA", typeof(string));
            dataTable.Columns.Add("DOS", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Reason", typeof(string));
            dataTable.Columns.Add("History", typeof(string));
            dataTable.Columns.Add("CC", typeof(string));
            dataTable.Columns.Add("Activities", typeof(string));
            dataTable.Columns.Add("ROS", typeof(string));
            dataTable.Columns.Add("Past Medical", typeof(string));
            dataTable.Columns.Add("Past Surgical", typeof(string));
            dataTable.Columns.Add("Medications", typeof(string));
            dataTable.Columns.Add("Allergies", typeof(string));
            dataTable.Columns.Add("Social History", typeof(string));
            dataTable.Columns.Add("Occupation", typeof(string));
            dataTable.Columns.Add("Tylenol", typeof(string));
            dataTable.Columns.Add("Physical Exam", typeof(string));
            dataTable.Columns.Add("Diagnostics", typeof(string));
            dataTable.Columns.Add("Diagnoses", typeof(string));
            dataTable.Columns.Add("Plan", typeof(string));
            dataTable.Columns.Add("Care", typeof(string));
            dataTable.Columns.Add("Precautions", typeof(string));
            dataTable.Columns.Add("Follow up", typeof(string));
            dataTable.Columns.Add("GAIT", typeof(string));
            dataTable.Columns.Add("Case Type", typeof(string));
            dataTable.Columns.Add("Neurological", typeof(string));
            dataTable.Columns.Add("DeepTendon", typeof(string));
            dataTable.Columns.Add("Sensory", typeof(string));
            dataTable.Columns.Add("ManualMuscle", typeof(string));



            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var downloadFolder = Path.Combine(_environment.WebRootPath);
            StringBuilder errfile = new StringBuilder();
            if (files != null)
            {
                string message = "";
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".docx")
                    {
                        string filePath = Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower());
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        try
                        {
                            DataRow row = MNPLLCConvertDocxToHtmlFUUpdate(Path.Combine(downloadFolder, "temp" + Path.GetExtension(file.FileName).ToLower()), dataTable, file.FileName);
                            dataTable.Rows.Add(row);
                        }
                        catch { errfile.Append(file.FileName); }

                        message += $"<span class='text-primary'>{file.FileName} File Processed Successfully<span><br>";
                    }
                    else
                    {
                        message += $"<span class='text-danger'>{file.FileName} File blank or not docx<span><br>";
                    }
                }
                TempData["Message"] = message.ToString();

            }
            else
            {
                TempData["Message"] = "File Not Uploaded.";
            }
            using (var stream = new MemoryStream())
            {
                // Create the Excel document in memory
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook
                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    // Add some data to the worksheet
                    // Create header row from DataTable column names
                    Row headerRow = new Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Cell cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.ColumnName)
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    // Populate the sheet with data from DataTable
                    foreach (DataRow dtRow in dataTable.Rows)
                    {
                        Row newRow = new Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dtRow[column].ToString())
                            };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }

                    if (errfile.Length > 0)
                    {
                        WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData2 = new SheetData();
                        worksheetPart2.Worksheet = new Worksheet(sheetData2);

                        // Append Sheet2 to the workbook
                        Sheet sheet2 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                            SheetId = 2,
                            Name = "ErrorSheet"
                        };
                        sheets.Append(sheet2);
                        string[] lines = errfile.ToString().Split("\n");
                        foreach (var line in lines)
                        {
                            Row row = new Row();
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(line)
                            };
                            row.AppendChild(cell);
                            sheetData2.AppendChild(row);
                        }
                    }
                    // Save the workbook
                    workbookPart.Workbook.Save();
                }

                // Return the stream as a file for download
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FUs.xlsx");
            }
            //return RedirectToAction("Index");


        }

        DataRow MNPLLCConvertDocxToHtmlFUUpdate(string docxFilePath, DataTable dataTable, string filename)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false))
            {
                //DOS, Location, Reason, Occupation, Tylenol
                Body body = wordDoc.MainDocumentPart.Document.Body;
                StringBuilder html = new StringBuilder();
                StringBuilder name = new StringBuilder();
                StringBuilder dob = new StringBuilder();
                StringBuilder doa = new StringBuilder();
                StringBuilder dos = new StringBuilder();
                StringBuilder location = new StringBuilder();
                StringBuilder reason = new StringBuilder();
                StringBuilder history = new StringBuilder();
                StringBuilder cc = new StringBuilder();
                StringBuilder activities = new StringBuilder();
                StringBuilder ros = new StringBuilder();
                StringBuilder pastmedical = new StringBuilder();
                StringBuilder pastsurgery = new StringBuilder();
                StringBuilder medications = new StringBuilder();
                StringBuilder allergies = new StringBuilder();
                StringBuilder socialhistory = new StringBuilder();
                StringBuilder occupation = new StringBuilder();
                StringBuilder tylenol = new StringBuilder();
                StringBuilder pe = new StringBuilder();
                StringBuilder diagnosticstudies = new StringBuilder();
                StringBuilder diagnoses = new StringBuilder();
                StringBuilder plan = new StringBuilder();
                StringBuilder curmedications = new StringBuilder();
                StringBuilder care = new StringBuilder();
                StringBuilder goals = new StringBuilder();
                StringBuilder precautions = new StringBuilder();
                StringBuilder followup = new StringBuilder();
                StringBuilder casetype = new StringBuilder();
                StringBuilder gait = new StringBuilder();
                StringBuilder neurological = new StringBuilder();
                StringBuilder deeptendon = new StringBuilder();
                StringBuilder sensory = new StringBuilder();
                StringBuilder manualmuscle = new StringBuilder();


                bool foundcc = false;
                bool foundpe = false;
                bool founddiagnoses = false;
                bool foundplan = false;
                //bool foundcurmedications = false;
                bool foundhistory = false;
                bool foundreason = false;
                bool founddiagnosticstudies = false;

                html.Append("<html><body>");

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string paragraphText = GetParagraphText(paragraph);
                        if (paragraphText.StartsWith("Patient Name:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            name.Append(paragraphText.Substring(("Patient Name:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("DOB:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dob.Length == 0)
                                dob.Append(paragraphText.Substring(("DOB:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Dt. of Injury:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (doa.Length == 0)
                                doa.Append(paragraphText.Substring(("Dt. of Injury:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Dt. of Exam:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            if (dos.Length == 0)
                                dos.Append(paragraphText.Substring(("Dt. of Exam:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Location:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            location.Append(paragraphText.Substring(("LOCATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            // foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CHIEF COMPLAINT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Case Type:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            casetype.Append(paragraphText.Substring(("Case Type:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = true;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Chief Complaint:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = true;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ACTIVITIES OF DAILY LIVING:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            activities.Append(paragraphText.Substring(("ACTIVITIES OF DAILY LIVING:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("REVIEW OF SYSTEMS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            ros.Append(paragraphText.Substring(("REVIEW OF SYSTEMS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST MEDICAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastmedical.Append($"<p>{paragraphText}</p>");
                            pastmedical.Append(paragraphText.Substring(("PAST MEDICAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PAST SURGICAL/HOSPITALIZATION HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //pastsurgery.Append($"<p>{paragraphText}</p>");
                            pastsurgery.Append(paragraphText.Substring(("PAST SURGICAL/HOSPITALIZATION HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("MEDICATIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //medications.Append($"<p>{paragraphText}</p>");
                            medications.Append(paragraphText.Substring(("MEDICATIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("ALLERGIES:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //allergies.Append($"<p>{paragraphText}</p>");
                            allergies.Append(paragraphText.Substring(("ALLERGIES:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("SOCIAL HISTORY:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //socialhistory.Append($"<p>{paragraphText}</p>");
                            socialhistory.Append(paragraphText.Substring(("SOCIAL HISTORY:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //  foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }


                        if (paragraphText.StartsWith("OCCUPATION:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            occupation.Append(paragraphText.Substring(("OCCUPATION:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("ARE YOU TAKING ANY NSAIDS OR TYLENOL:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            tylenol.Append(paragraphText.Substring(("ARE YOU TAKING ANY NSAIDS OR TYLENOL:".Length)).Trim());
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Neurological Examination:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            neurological.Append(paragraphText.Substring(("Neurological Examination:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Deep Tendon Reflexes:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            deeptendon.Append(paragraphText.Substring(("Deep Tendon Reflexes:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Sensory Examination:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            sensory.Append(paragraphText.Substring(("Sensory Examination:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Manual Muscle Strength Testing:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            manualmuscle.Append(paragraphText.Substring(("Manual Muscle Strength Testing:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Physical Examination:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            // pe.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = true;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GAIT:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            gait.Append(paragraphText.Substring(("GAIT:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnostic Studies:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append(paragraphText.Substring(("Diagnostic Studies:".Length)).Trim());

                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = true;
                            continue;
                        }

                        if (paragraphText.StartsWith("Diagnoses:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = true;
                            foundplan = false;
                            //     foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Plan:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = true;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (paragraphText.StartsWith("Medications:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = true;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("CARE:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            care.Append(paragraphText.Substring(("CARE:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("GOALS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            goals.Append(paragraphText.Substring(("GOALS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //    foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("PRECAUTIONS:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //precautions.Append($"<p>{paragraphText}</p>");
                            precautions.Append(paragraphText.Substring(("PRECAUTIONS:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            founddiagnosticstudies = false;
                            continue;
                        }
                        if (paragraphText.StartsWith("Follow-up:"))
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //followup.Append($"<p>{paragraphText}</p>");
                            followup.Append(paragraphText.Substring(("Follow-up:".Length)));
                            foundcc = false;
                            foundpe = false;
                            founddiagnoses = false;
                            foundplan = false;
                            //   foundcurmedications = false;
                            foundhistory = false;
                            foundreason = false;
                            founddiagnosticstudies = false;
                            continue;
                        }

                        if (foundhistory)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            //history.Append($"<p>{paragraphText}</p>");
                            history.Append(paragraphText);
                        }

                        if (foundcc)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            cc.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundpe)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            pe.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnoses)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnoses.Append($"<p>{paragraphText}</p>");
                        }
                        if (foundplan)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            plan.Append($"<p>{paragraphText}</p>");
                        }
                        // if (foundcurmedications)
                        //  {
                        //      html.Append($"<p>{paragraphText}</p>");
                        //      curmedications.Append($"<p>{paragraphText}</p>");
                        //   }
                        if (foundreason)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            reason.Append($"<p>{paragraphText}</p>");
                        }
                        if (founddiagnosticstudies)
                        {
                            html.Append($"<p>{paragraphText}</p>");
                            diagnosticstudies.Append($"<p>{paragraphText}</p>");
                        }
                    }
                }

                html.Append("</body></html>");
                DataRow datarow = dataTable.NewRow();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string pid = "0";
                string bdate = "";
                string adate = "";
                string sdate = "";
                try
                {
                    if (doa.ToString() != "")
                        adate = DateTime.ParseExact(doa.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                try
                {
                    if (dob.ToString() != "")
                        bdate = DateTime.ParseExact(dob.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                try
                {
                    if (dos.ToString() != "")
                        sdate = DateTime.ParseExact(dos.ToString().Trim(), "MM/dd/yyyy", null).ToString("yyyy-MM-dd");
                }
                catch { }

                string[] fullname = name.ToString().Trim().Split(' ');

                try
                {

                    string namePart = filename.Substring(0, filename.IndexOf("_")).Trim();
                    string[] nameParts = namePart.Split(',');
                    fullname = new string[] { nameParts[1].Trim(), nameParts[0].Trim() };
                }
                catch (Exception ex) { }
                DataTable dt = new DataTable();
                if (fullname.Length > 1)
                {
                    //                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{sdate}'  and DATE(vm_patient_fu.doa)= '{adate}' and vm_patient_ie.cmp_id = {cmpid}");
                    dt = _pareentservices.GetData($"select vm_patient_fu.*,vm_patient_ie.patient_id from vm_patient_fu inner join vm_patient_ie on vm_patient_fu.patientIE_ID = vm_patient_ie.id where vm_patient_fu.fname = '{fullname[0]}' and vm_patient_fu.lname = '{fullname[1]}'  and DATE(vm_patient_fu.doe)= '{sdate}'   and vm_patient_ie.cmp_id = {cmpid}");

                    if (dt.Rows.Count > 0)
                    {
                        datarow["Patient_id"] = dt.Rows[0]["patient_id"];
                        try { datarow["Patient_ie_id"] = dt.Rows[0]["patientIE_ID"]; } catch { }
                        try { datarow["Patient_fu_id"] = dt.Rows[0]["patientFU"]; } catch { }
                    }

                }
                datarow["FName"] = fullname.Length > 0 ? fullname[0].ToString().Trim() : "";
                datarow["LName"] = fullname.Length > 1 ? fullname[1].ToString().Trim() : "";
                datarow["MName"] = fullname.Length > 2 && !fullname[2].ToString().StartsWith("[") ? fullname[2].ToString().Trim() : "";
                datarow["DOA"] = adate;
                datarow["DOB"] = bdate;
                datarow["DOS"] = sdate;
                datarow["Name"] = name.ToString().Trim();
                datarow["Location"] = location.ToString().Trim();
                datarow["Reason"] = reason.ToString().Trim();
                datarow["History"] = history.ToString().Trim();
                datarow["CC"] = cc.ToString().Trim();
                datarow["Activities"] = activities.ToString().Trim();
                datarow["ROS"] = ros.ToString().Trim();
                datarow["Past Medical"] = pastmedical.ToString().Trim();
                datarow["Past Surgical"] = pastsurgery.ToString().Trim();
                datarow["Medications"] = medications.ToString().Trim();
                datarow["Allergies"] = allergies.ToString().Trim();
                datarow["Social History"] = socialhistory.ToString().Trim();
                datarow["Occupation"] = occupation.ToString().Trim();
                datarow["Tylenol"] = tylenol.ToString().Trim();
                datarow["Physical Exam"] = pe.ToString().Trim();
                datarow["Diagnostics"] = diagnosticstudies.ToString().Trim();
                datarow["Diagnoses"] = diagnoses.ToString().Trim();
                datarow["Plan"] = plan.ToString().Trim();
                //   datarow["Current Medications"] = curmedications.ToString().Trim();
                datarow["Care"] = care.ToString().Trim();
                datarow["Precautions"] = precautions.ToString().Trim();
                datarow["Follow up"] = followup.ToString().Trim();
                datarow["GAIT"] = gait.ToString().Trim();
                datarow["Case Type"] = casetype.ToString().Trim();
                datarow["Neurological"]=neurological.ToString().Trim();
                datarow["DeepTendon"]= deeptendon.ToString().Trim();
                datarow["Sensory"] = sensory.ToString().Trim();
                datarow["ManualMuscle"] = manualmuscle.ToString().Trim();
                return datarow;

            }
        }


    }
}