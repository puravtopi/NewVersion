﻿using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using MS.Services;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using PainTrax.Services;
using PainTrax.Web.Filter;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Data;



namespace PainTrax.Web.Controllers
{
    [AuthenticateUser]
    public class SignInSheetController : Controller
    {
        private readonly Common _commonservices = new Common();
        private readonly AppHelper _apphelper = new AppHelper();
        private readonly ParentService _parentService = new ParentService();
        private readonly PatientService _patientservices = new PatientService();
        private readonly SignInSheetService _SIservice = new SignInSheetService();
        private readonly AppStatusService _appStatusService = new AppStatusService();
        private readonly AppProviderService _appProviderService = new AppProviderService();
        private readonly AppProviderRelService _appProviderRelService = new AppProviderRelService();
        private readonly MDTImportServices _servicesMDTImport = new MDTImportServices();



        private readonly IMapper _mapper;
        private readonly AttorneysService _services = new AttorneysService();
        private readonly ILogger<AttorneysController> _logger;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public SignInSheetController(IMapper mapper, ILogger<AttorneysController> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _mapper = mapper;
            _logger = logger;
            Environment = environment;
        }

        public IActionResult Index()
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
            return View();
        }

        public IActionResult SISheetReport()
        {

            var objPro = new MDTImportReportVM();
            objPro.lstMDTImportReport = new List<MDTImportReportVM>();
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);


            //HttpContext.Session.SetString(SessionKeys.Sessiondldoe, SIDate);
            //if (!string.IsNullOrEmpty(Location_ID))
            //{ HttpContext.Session.SetString(SessionKeys.Sessiondldlloc, Location_ID); }



            ViewBag.SelectedDate = HttpContext.Session.GetString(SessionKeys.Sessiondldoe);
            ViewBag.SelectedLocation = HttpContext.Session.GetString(SessionKeys.Sessiondldlloc);
            return View(objPro);
        }
        // [HttpPost]
        public IActionResult SISheetReport1(DateTime? fdate, DateTime? tdate)
        {
            //fdate = Convert.ToDateTime(HttpContext.Session.GetString(SessionKeys.Sessiondldoe));
            //tdate = Convert.ToDateTime(HttpContext.Session.GetString(SessionKeys.Sessiondldoe));


            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            // Skiping number of Rows count
            var start = Request.Form["start"].FirstOrDefault();
            // Paging Length 10,20
            var length = Request.Form["length"].FirstOrDefault();
            // Sort Column Name
            var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
            // Sort Column Direction ( asc ,desc)
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            // Search Value from (Search box)
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            //Paging Size (10,20,50,100)
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            string cnd = "and cmp_id=" + cmpid + " and attorney like '%" + searchValue + "%' ";

            //var Data = _services.GetAll(cnd);
            //var Data = _services.GetAll(cnd);
            var Data = _SIservice.GetPatientsSIDNL(HttpContext.Session.GetString(SessionKeys.Sessiondldoe), HttpContext.Session.GetString(SessionKeys.Sessiondldlloc), cmpid);
            //Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                var property = typeof(SI_Report).GetProperties()[Convert.ToInt32(sortColumn)];
                if (sortColumnDirection.ToUpper() == "ASC")
                {
                    Data = Data.OrderBy(x => property.GetValue(x, null)).ToList();
                }
                else
                {
                    Data = Data.OrderByDescending(x => property.GetValue(x, null)).ToList();
                }
            }

            //Search
            //total number of rows count 
            recordsTotal = Data.Count();
            //Paging 
            var data = Data.Skip(skip).Take(pageSize).ToList();
            //Returning Json Data
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public IActionResult ExportToSISheetReport()
        {
            try
            {
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                // var data = _SIservice.GetPatientsSIDNL();
                var data = _SIservice.GetPatientsSIDNL(HttpContext.Session.GetString(SessionKeys.Sessiondldoe), HttpContext.Session.GetString(SessionKeys.Sessiondldlloc), cmpid);

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    //new DataColumn("ieid", typeof(string)),
                    //new DataColumn("fuid", typeof(string)),
                    new DataColumn("fname", typeof(string)),
                    new DataColumn("lname", typeof(string)),
                    new DataColumn("visitiefu", typeof(string)),
                    new DataColumn("followupdate", typeof(string)),
                    new DataColumn("doa", typeof(string)),
                    new DataColumn("doe", typeof(string)),
                    new DataColumn("casetype", typeof(string)),
                    new DataColumn("location", typeof(string)),
                    new DataColumn("requested", typeof(string)),
                    new DataColumn("scheduled", typeof(string)),
                    new DataColumn("alert", typeof(string)),
                    new DataColumn("inhouse", typeof(string))

                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var cnt in data)
                {
                    dt.Rows.Add(cnt.fname, cnt.lname, cnt.visitiefu, cnt.followupdate, cnt.doa, cnt.doe, cnt.casetype, cnt.location, cnt.requested, cnt.scheduled, cnt.alert, cnt.inhouse);
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
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SI Sheet Report.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }


        [HttpPost]
        public IActionResult GetSIDate(string SIDate, string Location_ID)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            HttpContext.Session.SetString(SessionKeys.Sessiondldoe, SIDate);
            if (!string.IsNullOrEmpty(Location_ID))
            { HttpContext.Session.SetString(SessionKeys.Sessiondldlloc, Location_ID); }


            DataTable dt = _SIservice.GetPatientsByDOE(SIDate, Location_ID, cmpid.ToString());


            ViewBag.SelectedDate12 = "test selected date";

            string jsonString = string.Empty;

            if (dt != null && dt.Rows.Count > 0)
            {
                jsonString = JsonConvert.SerializeObject(dt, Formatting.Indented);
            }

            return Json(jsonString);

            // var data = _SIservice.GetPatientsByDOE(SIDate, Location_ID, cmpid.ToString());
            // var objPtDOE = new PatientsByDOE();
            // objPtDOE.lstPatientsByDOE = data;
            //// TempData["ProSXquery"] = query;

            // return View(objPtDOE);



        }

        public ActionResult GetPatientsSIDNL(string SIDate, string Location_ID)
        {

            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            // Skiping number of Rows count
            var start = Request.Form["start"].FirstOrDefault();
            // Paging Length 10,20
            var length = Request.Form["length"].FirstOrDefault();
            // Sort Column Name
            var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
            // Sort Column Direction ( asc ,desc)
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            // Search Value from (Search box)
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            //Paging Size (10,20,50,100)
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            string cnd = "and cmp_id=" + cmpid + " and attorney like '%" + searchValue + "%' ";

            // var Data = _SIservice.GetPatientsSIDNL(); // _SIservice.GetPatientsSIDNL(SIDate, Location_ID, cmpid.ToString());
            // var data = _SIservice.GetPatientsSIDNL();
            var Data = _SIservice.GetPatientsSIDNL(HttpContext.Session.GetString(SessionKeys.Sessiondldoe), HttpContext.Session.GetString(SessionKeys.Sessiondldlloc), cmpid);



            // ViewBag.TotalClaimNo = _dashboardservices.GetTotalClaimNo(cmpid.Value);
            //Search
            //total number of rows count 
            recordsTotal = Data.Count();
            //Paging 
            var data = Data.Skip(skip).Take(pageSize).ToList();
            //Returning Json Data
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });



        }


        [HttpPost]
        public IActionResult GetSIDateDetails(string IEID, string FUID, string type)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            DataTable dt = _SIservice.GetPatientsByDOEDetails(IEID, FUID, type, cmpid.ToString());


            string jsonString = string.Empty;

            if (dt != null && dt.Rows.Count > 0)
            {
                jsonString = JsonConvert.SerializeObject(dt, Formatting.Indented);
            }

            return Json(jsonString);

        }

    }
}
