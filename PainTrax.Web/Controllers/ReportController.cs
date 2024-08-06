using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Data;

namespace PainTrax.Web.Controllers
{
    public class ReportController : Controller
    {


        private readonly ILogger<ReportController> _logger;
        private readonly POCServices _services = new POCServices();
        private readonly Common _commonservices = new Common();

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

            if (mcodeid.Equals("1"))
            {
                query += " and pp.inhouseprocbit=1 ";
            }
            else if (mcodeid.Equals("2"))
            {
                query += " and pp.Other =1 ";
            }
            else if (mcodeid.Equals("3"))
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
        public IActionResult TransferToResheduled(string ids)
        {
            ids = ids.TrimStart(',');

            if (!string.IsNullOrEmpty(ids))
            {
                var arrayId = ids.Split(',');

                for (int i = 0; i < arrayId.Length; i++)
                {
                    _services.TransferToReschedules(arrayId[i]);
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
                    new DataColumn("Name", typeof(string)),
                    new DataColumn("Gender", typeof(string)),
                    new DataColumn("Case", typeof(string)),
                    new DataColumn("DOB", typeof(string)),
                    new DataColumn("DOA", typeof(string)),
                    new DataColumn("MCODE", typeof(string)),
                    new DataColumn("Phone", typeof(string)),
                    new DataColumn("Location", typeof(string)),
                    new DataColumn("Ins Co", typeof(string)),
                    new DataColumn("Claim Number", typeof(string)),
                    new DataColumn("Policy No", typeof(string)),
                    new DataColumn("MC", typeof(string)),
                    new DataColumn("Requested", typeof(string)),
                    new DataColumn("Scheduled", typeof(string)),
                    new DataColumn("Executed", typeof(string)),                   
                                    
                   // new DataColumn("PhoneNo", typeof(string)),                                   
                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var user in data)
                {
                    dt.Rows.Add(user.name,user.gender, user.casetype, user.dob == null ? "" : user.dob.Value.ToShortDateString(), user.doa == null ? "" : user.doa.Value.ToShortDateString(), user.mcode, user.phone, user.location, user.cmpname, user.primary_claim_no, user.primary_policy_no, user.mc,user.requested==null?"":user.requested.Value.ToShortDateString(), user.scheduled==null?"":user.scheduled.Value.ToShortDateString(), user.executed == null ? "" : user.executed.Value.ToShortDateString());
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
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "POCReport.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }
    }
}
