using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Data;
using static PainTrax.Web.Helper.EnumHelper;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Bibliography;
using System.Reflection;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit;
using Org.BouncyCastle.Asn1.Ocsp;
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;
using Color = DocumentFormat.OpenXml.Spreadsheet.Color;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PainTrax.Web.Controllers
{
    [SessionCheckFilter]
    public class ReportController : Controller
    {


        private readonly ILogger<ReportController> _logger;
        private readonly POCServices _services = new POCServices();
        private readonly ProSXServices _servicesProSX = new ProSXServices();
        private readonly IVFRServices _servicesIVFR = new IVFRServices();
        private readonly ProSXDetailsServices _servicesProSXDetails = new ProSXDetailsServices();
        private readonly DailyCountServices _servicesDailyCount = new DailyCountServices();
        private readonly PtsIEServices _servicesPtsIE = new PtsIEServices();
        private readonly MDTImportServices _servicesMDTImport = new MDTImportServices();
        private readonly Common _commonservices = new Common();
        private readonly SurgeryCentreService _surgeryCentreService = new SurgeryCentreService();

        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult POCReport()
        {

            var objPOC = new POCReportVM();
            objPOC.lstPOCReport = new List<POCReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            objPOC._executed = false;
            objPOC._requested = false;
            objPOC._scheduled = false;

            

            return View(objPOC);
        }

        [HttpPost]
        public IActionResult POCReport(DateTime? fdate, DateTime? tdate, int locationid = 0, int mcodeid = 0, bool _executed = false, bool _requested = false, bool _scheduled = false)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string query = " where pm.cmp_id=" + cmpid.ToString();
            if (locationid > 0)
            {
                query += " and lc.id =" + locationid;
            }

            if (mcodeid==1)
            {
                query += " and pp.inhouseprocbit=1 ";
            }
            else if (mcodeid == 2)
            {
                query += " and pp.Other =1 ";
            }
            else if (mcodeid == 3)
            {
                query += " and pp.INhouseProcbit<>1 and  ISNULL(pp.Other,0) <> 1  ";
            }

            string _query = "";

            if (_requested)
            {
                if (fdate != null && tdate != null)
                {
                    _query = " (tp.Requested BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";
                }
            }
            if (_executed)
            {
                if (fdate != null && tdate != null)
                {
                    if (!string.IsNullOrEmpty(_query))
                        _query = _query + " Or (tp.Executed BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";
                    else
                        _query = " (tp.Executed BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";

                }
            }
            if (_scheduled)
            {
                ViewBag.ShowTransfer = true;
                if (fdate != null && tdate != null)
                {
                    if (!string.IsNullOrEmpty(_query))
                        _query = _query + " Or (tp.Scheduled BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";
                    else
                        _query = " (tp.Scheduled BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";

                }
            }

            if (!string.IsNullOrEmpty(_query))
            {
                query = query + " and (" + _query + ")";
            }


            var data = _services.GetPOCReport(query);
            var objPOC = new POCReportVM();
            objPOC.lstPOCReport = data;
            TempData["query"] = query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);



            string cnd = " and cmp_id=" + cmpid;
            var sdata = _surgeryCentreService.GetAll(cnd);
            var list = new List<SelectListItem>();



            foreach (var item in sdata)
            {
                list.Add(new SelectListItem
                {
                    Text = item.Surgerycenter_name.ToString(),
                    Value = item.Id.ToString()
                });
            }
            ViewBag.surgoryList = list;

            return View(objPOC);

        }

        [HttpPost]
        public IActionResult TransferToExecute(string ids, string dates)
        {
            ids = ids.TrimStart(',');
            dates = dates.TrimStart(',');

            if (!string.IsNullOrEmpty(ids))
            {
                var arrayId = ids.Split(',');
                var arrayDates = dates.Split(",");

                for (int i = 0; i < arrayId.Length; i++)
                {
                    if (!string.IsNullOrEmpty(arrayDates[i]))
                        _services.TransferToExecute(arrayId[i], arrayDates[i]);
                }
            }
            return Json(1);
        }

        [HttpPost]
        public IActionResult TransferToResheduled(string ids,string sDate="")
        {
            ids = ids.TrimStart(',');

            if (!string.IsNullOrEmpty(ids))
            {
                    var arrayId = ids.Split(',');

                    for (int i = 0; i < arrayId.Length; i++)
                    {
                    _services.TransferToReschedules(arrayId[i],sDate);
                }
            }
            return Json(1);
        }
        [HttpPost]
        public IActionResult UpdatePOCReport(string ids, string side = "", string level = "")
        {
            ids = ids.TrimStart(',');
            side = side.TrimStart(',');
            level = level.TrimStart(',');

            if (!string.IsNullOrEmpty(ids))
            {
                if (!string.IsNullOrEmpty(side) && !string.IsNullOrEmpty(level))
                {
                    var arrayId = ids.Split(',');
                    var arraySide = side.Split(',');
                    var arrayLevel = level.Split(',');

                    for (int i = 0; i < arrayId.Length; i++)
                    {                        
                        _services.UpdatePOCReportSideandLevel(arrayId[i], arraySide[i], arrayLevel[i]);                        
                    }
                }
                
            }
            return Json(1);
        }

        public IActionResult ExportToExcel()
        {
            try
            {
                string query = TempData["query"].ToString();
                var data = _services.GetPOCReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Sex", typeof(string)),
                    new DataColumn("Name", typeof(string)),
                    new DataColumn("Provider", typeof(string)),
                    new DataColumn("Case", typeof(string)),
                    new DataColumn("DOE", typeof(string)),
                    new DataColumn("DOB", typeof(string)),
                    new DataColumn("DOA", typeof(string)),
                    new DataColumn("MCODE", typeof(string)),
                    new DataColumn("Side", typeof(string)),
                    new DataColumn("Level", typeof(string)),
                    new DataColumn("Phone", typeof(string)),
                    new DataColumn("Location", typeof(string)),                 
                    new DataColumn("Ins Co", typeof(string)),
                    new DataColumn("Claim Number", typeof(string)),
                    new DataColumn("Policy No", typeof(string)),
                    new DataColumn("MC", typeof(string)),
                    new DataColumn("Allergies", typeof(string)),
                    new DataColumn("Requested", typeof(string)),
                    new DataColumn("Scheduled", typeof(string)),
                    new DataColumn("Executed", typeof(string)),
                    new DataColumn("Note", typeof(string)),                   
                                    
                   // new DataColumn("PhoneNo", typeof(string)),                                   
                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var user in data)
                {
                    var sex = string.IsNullOrEmpty(user.gender) ? "" : (user.gender.ToLower() == "male" ? "M" : "F");
                    dt.Rows.Add(sex,user.name, user.providerName, user.casetype, user.doe == null ? "" : user.doe.Value.ToShortDateString(), user.dob == null ? "" : user.dob.Value.ToShortDateString(), user.doa == null ? "" : user.doa.Value.ToShortDateString(), user.mcode, user.sides, user.level, user.phone, user.location,user.cmpname, user.primary_claim_no, user.primary_policy_no, user.mc, user.allergies, user.requested == null ? "" : user.requested.Value.ToShortDateString(), user.scheduled == null ? "" : user.scheduled.Value.ToShortDateString(), user.executed == null ? "" : user.executed.Value.ToShortDateString(), user.note);
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var (defaultStyleIndex, headerStyleIndex) = CreateStyles(workbookPart.AddNewPart<WorkbookStylesPart>());

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName), StyleIndex = headerStyleIndex };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                    // ✅ Enable AutoFilter for the header row
                    string lastColumn = GetExcelColumnName(dt.Columns.Count);
                    var autoFilter = new AutoFilter() { Reference = $"A1:{lastColumn}1" };
                    worksheetPart.Worksheet.Append(autoFilter);                    
                    
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "POCReport.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }        
        private static (uint defaultStyleIndex, uint headerStyleIndex) CreateStyles(WorkbookStylesPart stylesPart)
        {
            Stylesheet stylesheet = new Stylesheet();

            // ✅ Fonts
            DocumentFormat.OpenXml.Spreadsheet.Fonts fonts = new DocumentFormat.OpenXml.Spreadsheet.Fonts(
                new Font(new Color { Rgb = "FF000000" }), // 0 - black font (default)
                new Font(new Color { Rgb = "FFFFFFFF" })  // 1 - white font (for header)
            );
            stylesheet.Fonts = fonts;

            // ✅ Fills
            Fills fills = new Fills();
            fills.Append(new Fill(new PatternFill() { PatternType = PatternValues.None })); // 0 - none
            fills.Append(new Fill(new PatternFill() { PatternType = PatternValues.Gray125 })); // 1 - gray125 (required)
            fills.Append(new Fill( // 2 - blue fill
                new PatternFill(
                    new ForegroundColor { Rgb = "4d4dff" } // blue
                )
                { PatternType = PatternValues.Solid }
            ));
            stylesheet.Fills = fills;

            // ✅ Borders
            Borders borders = new Borders(new DocumentFormat.OpenXml.Spreadsheet.Border()); // default border
            stylesheet.Borders = borders;

            // ✅ CellFormats
            CellFormats cellFormats = new CellFormats();

            // 0 - Default (black text, no fill)
            cellFormats.Append(new CellFormat { FontId = 0, FillId = 0, BorderId = 0 });

            // 1 - Header (white text + blue background)
            cellFormats.Append(new CellFormat
            {
                FontId = 1,
                FillId = 2,
                BorderId = 0,
                ApplyFont = true,
                ApplyFill = true
            });

            stylesheet.CellFormats = cellFormats;
            stylesPart.Stylesheet = stylesheet;
            stylesPart.Stylesheet.Save();

            return (0, 1); // (defaultStyleIndex, headerStyleIndex)
        }
        // Helper: Convert column index to Excel column letter (A, B, ..., Z, AA, AB, etc.)
        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        } 
        [HttpGet]
        public IActionResult ProSXReport()
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            var objPro = new ProSXReportVM();
            objPro.lstProSXReport = new List<ProSXReportVM>();
          
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            // ViewBag.dateList= _servicesProSX.GetProSXReportDate(cmpid.Value);
            var dates = _servicesProSX.GetProSXReportDate(cmpid.Value.ToString()); // return List<DateTime>

            var dateList = new List<SelectListItem>
{
    new SelectListItem { Value = "", Text = "--Select Date--", Selected = true }
};

            dateList.AddRange(dates.Select(d => new SelectListItem
            {
                Value = d.ToString("yyyy-MM-dd"), // ✅ machine-readable
                Text = d.ToString("MM/dd/yyyy")  // ✅ user-friendly
            }));

            ViewBag.dateList = dateList;

            return View(objPro);
        }

        [HttpPost]
        public IActionResult ProSXReport(DateTime? fdate, DateTime? tdate, int locationid = 0, int mcodeid = 0, bool _executed = false, bool _requested = false, bool _scheduled = false)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string query = " where pm.cmp_id=" + cmpid.ToString();

            string _query = "";

            if (fdate != null)
            {
                _query = " (tp.Scheduled = '" + fdate.Value.ToString("yyyy/MM/dd") + "' )";
            }
            if (tdate != null)
            {
                _query = " (tp.Scheduled = '" + tdate.Value.ToString("yyyy/MM/dd") + "' )";
            }

            if (fdate != null && tdate != null)
            {
                _query = " (tp.Scheduled = '" + fdate.Value.ToString("yyyy/MM/dd") + "' )";


            }

            if (!string.IsNullOrEmpty(_query))
            {
                query = query + " and (" + _query + ")";
            }


            var data = _servicesProSX.GetProSXReport(query);
            var objPOC = new ProSXReportVM();
            objPOC.lstProSXReport = data;
            TempData["ProSXquery"] = query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            var dates = _servicesProSX.GetProSXReportDate(cmpid.Value.ToString()); // return List<DateTime>

            var dateList = new List<SelectListItem>
{
    new SelectListItem { Value = "", Text = "--Select Date--", Selected = true }
};

            dateList.AddRange(dates.Select(d => new SelectListItem
            {
                Value = d.ToString("yyyy-MM-dd"), // ✅ machine-readable
                Text = d.ToString("MM/dd/yyyy")  // ✅ user-friendly
            }));

            ViewBag.dateList = dateList;
            return View(objPOC);

        }

        public IActionResult ExportToExcelProSX()
        {
            try
            {
                string query = TempData["ProSXquery"].ToString();
                var data = _servicesProSX.GetProSXReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Name", typeof(string)),
                    new DataColumn("MC", typeof(string)),
                    new DataColumn("Case", typeof(string)),
                    new DataColumn("Location", typeof(string)),
                    new DataColumn("MCODE", typeof(string)),
                    new DataColumn("Vaccinated", typeof(string)),
                    new DataColumn("Scheduled", typeof(string)),
                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var proSX in data)
                {
                    dt.Rows.Add(proSX.name, proSX.mc, proSX.casetype, proSX.location, proSX.mcode, proSX.vaccinated, proSX.scheduled);
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName) };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProSX Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }


        [HttpGet]
        public IActionResult IVFRReport()
        {

            var objPro = new IVFRReportVM();
            objPro.lstIVFRReport = new List<IVFRReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            //objPro._executed = false;
            //objPro._requested = false;
            objPro._scheduled = false;

            return View(objPro);
        }

        [HttpPost]
        public IActionResult IVFRReport(DateTime? fdate, DateTime? tdate, int locationid = 0, int mcodeid = 0, bool _executed = false, bool _requested = false, bool _scheduled = false)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string query = " where pm.cmp_id=" + cmpid.ToString();

            string _query = "";

            //if (fdate != null)
            //{
            //    _query = " (tp.Scheduled >= '" + fdate.Value.ToString("yyyy/MM/dd") + "' )";
            //}
            if (locationid > 0)
            {
                query += " and lc.id =" + locationid;
            }

            if (_scheduled)
            {
                if (fdate != null && tdate != null)
                {
                    if (!string.IsNullOrEmpty(_query))
                        _query = _query + " Or (tp.Scheduled BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";
                    else
                        _query = " (tp.Scheduled BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";

                }
            }

            if (!string.IsNullOrEmpty(_query))
            {
                query = query + " and (" + _query + ")";
            }


            var data = _servicesIVFR.GetIVFRReport(query);
            var objPOC = new IVFRReportVM();
            objPOC.lstIVFRReport = data;
            TempData["IVFRquery"] = query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            return View(objPOC);

        }

        public IActionResult ExportToExcelIVFR()
        {
            try
            {
                string query = TempData["IVFRquery"].ToString();
                var data = _servicesIVFR.GetIVFRReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Name", typeof(string)),
                     new DataColumn("Sex", typeof(string)),
                    new DataColumn("MCODE", typeof(string)),
                    new DataColumn("Case", typeof(string)),
                    new DataColumn("DOB", typeof(string)),
                    new DataColumn("DOA", typeof(string)),
                    new DataColumn("SSN", typeof(string)),
                    new DataColumn("Phone", typeof(string)),
                    new DataColumn("Location", typeof(string)),
                    new DataColumn("Address", typeof(string)),
                    new DataColumn("Ins Co", typeof(string)),
                    new DataColumn("Claim Number", typeof(string)),
                    new DataColumn("Policy No", typeof(string)),
                    new DataColumn("Scheduled", typeof(string)),

                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var IVFR in data)
                {
                    dt.Rows.Add(IVFR.name, IVFR.gender, IVFR.mcode, IVFR.casetype, IVFR.dob == null ? "" : IVFR.dob.Value.ToShortDateString(), IVFR.doa == null ? "" : IVFR.doa.Value.ToShortDateString(), IVFR.ssn, IVFR.phone, IVFR.location, IVFR.Address, IVFR.InsCo, IVFR.primary_claim_no, IVFR.primary_policy_no, IVFR.scheduled == null ? "" : IVFR.scheduled.Value.ToShortDateString());
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName) };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "IVFR Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }


        [HttpGet]
        public IActionResult DailyCountReport()
        {

            var objPro = new DailyCountReportVM();
            objPro.lstDailyCountReport = new List<DailyCountReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            //objPro._executed = false;
            //objPro._requested = false;
            //objPro._scheduled = false;

            return View(objPro);
        }
        [HttpPost]
        public IActionResult DailyCountReport(DateTime? fdate, DateTime? tdate)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string query = " where pm.cmp_id=" + cmpid.ToString();

            string _query = "";

            //if (fdate != null)
            //{
            //    _query = " (tp.Scheduled >= '" + fdate.Value.ToString("yyyy/MM/dd") + "' )";
            //}



            if (fdate != null && tdate != null)
            {


                _query = "'" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "'";

            }




            var data = _servicesDailyCount.GetDailyCountReport(_query);
            var objPOC = new DailyCountReportVM();
            objPOC.lstDailyCountReport = data;
            TempData["DailyCountquery"] = query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            return View(objPOC);

        }

        public IActionResult ExportToExcelDailyCount()
        {
            try
            {
                string query = TempData["DailyCountquery"].ToString();
                var data = _servicesDailyCount.GetDailyCountReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("DOE", typeof(string)),
                    new DataColumn("Location", typeof(string)),
                    new DataColumn("NoOfIE", typeof(Int64)),
                    new DataColumn("NoOfFU", typeof(Int64)),


                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var cnt in data)
                {
                    dt.Rows.Add(cnt.doe, cnt.location, cnt.NoOFIE, cnt.NoOFFU);
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName) };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Daily Count Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }


        [HttpGet]
        public IActionResult MDTImportReport()
        {

            var objPro = new MDTImportReportVM();
            objPro.lstMDTImportReport = new List<MDTImportReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            //objPro._executed = false;
            //objPro._requested = false;
            //objPro._scheduled = false;

            return View(objPro);
        }
        [HttpPost]
        public IActionResult MDTImportReport(DateTime? fdate, DateTime? tdate)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string query = " pm.cmp_id=" + cmpid.ToString();

            string _query = "";

            if (fdate != null && tdate != null)
            {


                _query = "'" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "' and " + query;

            }

            var data = _servicesMDTImport.GeMDTImportReport(_query);
            var objPOC = new MDTImportReportVM();
            objPOC.lstMDTImportReport = data;
            TempData["MDTImportquery"] = _query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            return View(objPOC);

        }

        public IActionResult ExportToMDTImport()
        {
            try
            {
                string query = TempData["MDTImportquery"].ToString();
                var data = _servicesMDTImport.GeMDTImportReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("DOE", typeof(string)),
                    new DataColumn("PatientIE_ID", typeof(string)),
                    new DataColumn("lname", typeof(string)),
                    new DataColumn("fname", typeof(string)),

                    new DataColumn("mname", typeof(string)),
                    new DataColumn("gender", typeof(string)),
                    new DataColumn("dob", typeof(string)),
                    new DataColumn("address", typeof(string)),
                    new DataColumn("city", typeof(string)),
                    new DataColumn("state", typeof(string)),
                    new DataColumn("zip", typeof(string)),
                    new DataColumn("home_ph", typeof(string)),

                    new DataColumn("mobile", typeof(string)),
                    new DataColumn("location", typeof(string))

                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var cnt in data)
                {
                    dt.Rows.Add(cnt.doe, cnt.PatientIE_ID, cnt.lname, cnt.fname, cnt.mname, cnt.gender, cnt.dob, cnt.address, cnt.city, cnt.state, cnt.zip, cnt.home_ph, cnt.mobile, cnt.location);
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName) };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MDT Import Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }




        [HttpGet]
        public IActionResult PtsIEReport()
        {

            var objPro = new PtsIEReportVM();
            objPro.lstPtsIEReport = new List<PtsIEReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            //objPro._executed = false;
            //objPro._requested = false;
            //objPro._scheduled = false;

            return View(objPro);
        }
        [HttpPost]
        public IActionResult PtsIEReport(DateTime? fdate, DateTime? tdate)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string _query = "";


            if (fdate != null && tdate != null)
            {


                _query = "'" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "' and ie.cmp_id = " + cmpid.ToString();

            }


            var data = _servicesPtsIE.GetPtsIEReport(_query);
            var objPOC = new PtsIEReportVM();
            objPOC.lstPtsIEReport = data;
            TempData["lstPtsIEReport"] = _query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            return View(objPOC);

        }

        public IActionResult ExportToExcelPtsIE()
        {
            try
            {
                string query = TempData["lstPtsIEReport"].ToString();
                var data = _servicesPtsIE.GetPtsIEReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {

                    new DataColumn("PName", typeof(string)),
                    new DataColumn("mobile", typeof(string)),
                    new DataColumn("location", typeof(string)),
                    new DataColumn("CaseType", typeof(string)),
                    new DataColumn("doe", typeof(string)),
                    new DataColumn("doa", typeof(string)),
                    new DataColumn("Ins", typeof(string)),
                    new DataColumn("primary_policy_no", typeof(string)),
                    new DataColumn("Attorney", typeof(string)),
                    new DataColumn("LastVisit", typeof(string))


                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var cnt in data)
                {
                    dt.Rows.Add(cnt.PName, cnt.mobile, cnt.location, cnt.CaseType, cnt.doe, cnt.doa, cnt.Ins, cnt.primary_policy_no, cnt.Attorney, cnt.LastVisit);
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName) };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pts IE Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }




        [HttpGet]
        public IActionResult ProSXDetailsReport()
        {

            var objPro = new ProSXDetailsReportVM();
            objPro.lstProSXDetailsReport = new List<ProSXDetailsReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            //objPro._executed = false;
            //objPro._requested = false;
            //objPro._scheduled = false;

            return View(objPro);
        }

        [HttpPost]
        public IActionResult ProSXDetailsReport(DateTime? fdate, DateTime? tdate, int locationid = 0)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            string query = " where ie.cmp_id=" + cmpid.ToString();

            string _query = "";


            if (locationid > 0)
            {
                query += " and lc.id =" + locationid;
            }


            if (fdate != null && tdate != null)
            {
                _query = _query + " (pd.Requested BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "') Or (pd.Scheduled BETWEEN '" + fdate.Value.ToString("yyyy/MM/dd") + "' and '" + tdate.Value.ToString("yyyy/MM/dd") + "')";

            }


            if (!string.IsNullOrEmpty(_query))
            {
                query = query + " and (" + _query + ")";
            }


            var data = _servicesProSXDetails.GetPtsIEReport(query);
            var objPOC = new ProSXDetailsReportVM();
            objPOC.lstProSXDetailsReport = data;
            TempData["ProSXDetailsReportRquery"] = query;

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            return View(objPOC);

        }

        public IActionResult ExportToExcelProSXDetails()
        {
            try
            {
                string query = TempData["ProSXDetailsReportRquery"].ToString();
                var data = _servicesProSXDetails.GetPtsIEReport(query);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                     new DataColumn("name", typeof(string)),
                     new DataColumn("sex", typeof(string)),
                     new DataColumn("MC", typeof(string)),
                     new DataColumn("CaseType", typeof(string)),
                     new DataColumn("location", typeof(string)),
                     new DataColumn("Vaccinated", typeof(string)),
                     new DataColumn("MCODE", typeof(string)),
                     new DataColumn("BodyPart", typeof(string)),
                     new DataColumn("Ins_ver_status", typeof(string)),
                     new DataColumn("MC_Status", typeof(string)),
                     new DataColumn("Case_Status", typeof(string)),
                     new DataColumn("InsVerStatus", typeof(string)),
                     new DataColumn("Vac_Status", typeof(string)),
                     new DataColumn("Scheduled", typeof(string)),
                     new DataColumn("Executed", typeof(string)),
                     new DataColumn("Requested", typeof(string))
                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var t in data)
                {
                    dt.Rows.Add(t.name, t.sex, t.mc, t.casetype, t.location, t.vaccinated, t.mcode, t.bodypart, t.ins_ver_status, t.mc_status, t.case_status, t.insverstatus, t.vac_status, t.scheduled,t.executed,t.requested);//, IVFR.scheduled == null ? "" : IVFR.scheduled.Value.ToShortDateString());
                }

                // Create a new Excel file
                var memoryStream = new MemoryStream();
                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Users" };
                    sheets.Append(sheet);

                    var headerRow = new Row();
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(column.ColumnName) };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = new Row();
                        foreach (var value in row.ItemArray)
                        {
                            var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(value.ToString()) };
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProSXDetails Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }



    }
}
