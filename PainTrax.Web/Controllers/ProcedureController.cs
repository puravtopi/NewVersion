﻿using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PainTrax.Web.Filter;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;


namespace PainTrax.Web.Controllers
{
    [AuthenticateUser]
    public class ProcedureController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ProcedureController> _logger;
        private readonly ProcedureService _services = new ProcedureService();
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public ProcedureController(IMapper mapper, ILogger<ProcedureController> logger,Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _mapper = mapper;
            _logger = logger;
            Environment = environment;
        }

        public IActionResult Index(string searchtxt = "")
        {
            var data = new List<tbl_procedures>();
            try
            {               
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId).ToString();
                string cnd = "and cmp_id= " + cmpid;
                if (!string.IsNullOrEmpty(searchtxt))
                {
                    cnd = " and title like '%" + searchtxt + "%' ";
                    var result = _services.GetAll();
                    data = result;
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Index");
            }
            return View();
        }

        public IActionResult Create()
        {            
            tbl_procedures obj = new tbl_procedures();
            try
            {               
                obj.inhouseprocbit = false;
                obj.sides = false;
                obj.haslevel = false;
                obj.cf = false;
                obj.pn = false;
                obj.inout = false;
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Create");
            }
            return View(obj);
        }


        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create(tbl_procedures model, IFormFile upload_template)
        {
            try
            {
                model.position = model.position == null ? "" : model.position;
                model.cmp_id = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                if (upload_template != null)
                {                   
                    string folderPath = Path.Combine(Environment.WebRootPath, "Uploads/InjectionReports",model.cmp_id.ToString());
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // string fileName = Path.GetFileName(upload_template.FileName);                    
                    //string filePath = Path.Combine(folderPath, fileName);
                    string originalFileName = Path.GetFileName(upload_template.FileName);
                    string extension = Path.GetExtension(originalFileName);
                    string fileNameasmcode = model.mcode;
                    string fileName = fileNameasmcode + extension;
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        upload_template.CopyTo(fileStream);
                    }
                    model.upload_template = fileName;
                }
                _services.Insert(model);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Create");
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var data = new tbl_procedures();
            try
            {
                tbl_procedures obj = new tbl_procedures();
                obj.id = id;
                data = _services.GetOne(obj);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Edit");
            }
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(tbl_procedures model, IFormFile upload_template)
        {
            try
            {
                model.position = model.position == null ? "" : model.position;
                if (upload_template != null && upload_template.Length > 0)
                {
                    string folderPath = Path.Combine(Environment.WebRootPath, "Uploads/InjectionReports", model.cmp_id.ToString());
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string originalFileName = Path.GetFileName(upload_template.FileName);
                    string extension = Path.GetExtension(originalFileName);
                    string fileNameasmcode = model.mcode;
                    string fileName = fileNameasmcode + extension;
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        upload_template.CopyTo(fileStream);
                    }
                    model.upload_template = fileName;
                }
                _services.Update(model);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Edit");
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            try
            {
                tbl_procedures obj = new tbl_procedures();
                obj.id = id;
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
                string cnd = " and cmp_id=" + cmpid + " and (mcode like '%" + searchValue + "%' or bodypart like '%" + searchValue + "%' or heading like '%" + searchValue + "%' or ccdesc like '%" + searchValue + "%' Or ";
                cnd += " pedesc like '%" + searchValue + "%' or adesc like '%" + searchValue + "%' or pdesc like '%" + searchValue + "%' or cf like '%" + searchValue + "%' )";

                var Data = _services.GetAll(cnd);
                //Sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    var property = typeof(tbl_procedures).GetProperties()[Convert.ToInt32(sortColumn)];
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
            catch (Exception ex)
            {
                SaveLog(ex, "List");
            }
            return Json("");
        }

        public IActionResult ImportData(IFormFile postedFile)
        {
            try
            {
                if (postedFile != null && postedFile.Length > 0)
                {
                    DataTable dt = this.Read2007Xlsx(postedFile);
                    int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                    int? userid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[i]["Position"].ToString()))
                        {
                            tbl_procedures obj = new tbl_procedures()
                            {
                                cmp_id = cmpid,
                                position = dt.Rows[i]["Position"].ToString(),
                                display_order = string.IsNullOrEmpty(dt.Rows[i]["Display Order"].ToString()) ? 0 : Convert.ToInt16(dt.Rows[i]["Display Order"].ToString()),
                                hasmuscle = dt.Rows[i]["Muscle"].ToString(),
                                hassubprocedure = dt.Rows[i]["Sub Procedure"].ToString(),
                                pedesc = dt.Rows[i]["R_PEDesc"].ToString(),
                                pdesc = dt.Rows[i]["R_PDesc"].ToString(),
                                s_ccdesc = dt.Rows[i]["S_CCDesc"].ToString(),
                                s_adesc = dt.Rows[i]["S_ADesc"].ToString(),
                                e_heading = dt.Rows[i]["E-Heading"].ToString(),
                                e_pedesc = dt.Rows[i]["E_PEDesc"].ToString(),
                                e_pdesc = dt.Rows[i]["E_PDesc"].ToString(),
                                sidesdefault = dt.Rows[i]["Default Value for Sides"].ToString(),
                                inhouseprocbit =false,
                                sides = false,
                                haslevel = true,
                                cf = false,
                                pn = false,
                                inout = false,                           
                                bodypart = dt.Rows[i]["Body Part"].ToString(),
                                heading = dt.Rows[i]["R - Heading"].ToString(),
                                hasmedication = dt.Rows[i]["Medication"].ToString(),
                                ccdesc = dt.Rows[i]["R_CCDesc"].ToString(),
                                adesc = dt.Rows[i]["R_ADesc"].ToString(),
                                s_heading = dt.Rows[i]["S - Heading"].ToString(),
                                s_pedesc = dt.Rows[i]["S_PEDesc"].ToString(),
                                e_ccdesc = dt.Rows[i]["E_CCDesc"].ToString(),
                                e_adesc = dt.Rows[i]["E_ADesc"].ToString(),
                                levelsdefault = dt.Rows[i]["E_ADesc"].ToString(),
                                mcode = dt.Rows[i]["MCODE"].ToString(),
                                mcode_desc = dt.Rows[i]["Desc"].ToString(),                              
                            };

                            _services.Insert(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "ImportData");
            }
            return RedirectToAction("Index");
        }

        public DataTable Read2007Xlsx(IFormFile postedFile)
        {
            DataTable dt = new DataTable();
            try
            {
                if (postedFile != null && postedFile.Length > 0)
                {
                    // Read the uploaded Excel file using Open XML
                    using (Stream stream = postedFile.OpenReadStream())
                    {

                        using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(stream, false))
                        {
                            WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                            IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                            string relationshipId = sheets.First().Id.Value;
                            WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                            Worksheet workSheet = worksheetPart.Worksheet;
                            SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                            IEnumerable<Row> rows = sheetData.Descendants<Row>();
                            foreach (Cell cell in rows.ElementAt(0))
                            {
                                dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                            }
                            foreach (Row row in rows) //this will also include your header row...
                            {
                                DataRow tempRow = dt.NewRow();
                                int columnIndex = 0;
                                foreach (Cell cell in row.Descendants<Cell>())
                                {
                                    // Gets the column index of the cell with data
                                    int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
                                    cellColumnIndex--; //zero based index
                                    if (columnIndex < cellColumnIndex)
                                    {
                                        do
                                        {
                                            tempRow[columnIndex] = ""; //Insert blank data here;
                                            columnIndex++;
                                        }
                                        while (columnIndex < cellColumnIndex);
                                    }//end if block
                                    tempRow[columnIndex] = GetCellValue(spreadSheetDocument, cell);
                                    columnIndex++;
                                }//end inner foreach loop
                                dt.Rows.Add(tempRow);
                            }//end outer foreach loop
                        }//end using block
                        dt.Rows.RemoveAt(0); //...so i'm taking it out here.
                    }
                }
            }//end try
            catch (Exception ex)
            {
                SaveLog(ex, "Read2007Xlsx");
            }

            return dt;
        }//end Read2007Xlsx method        
        public static string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        } //end GetColumnName method
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

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return "";
            }
            string value = cell.CellValue.InnerXml;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }//end GetCellValue method

        [HttpGet]
        public ActionResult DownloadDocument()
        {
            var path = Path.Combine(Environment.WebRootPath + "/Uploads/Sample", "Procedure.xlsx");
            var fs = new FileStream(path, FileMode.Open);

            // Return the file. A byte array can also be used instead of a stream
            return File(fs, "application/octet-stream", "Procedure.xlsx");
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                var data = _services.GetAll(); // Retrieve all attorneys from the database

                // Create a new DataTable
                DataTable dt = new DataTable();
                // Add columns to the DataTable
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Position", typeof(string)),
                    new DataColumn("DisplayOrder", typeof(int)),
                    new DataColumn("Muscle", typeof(string)),
                    new DataColumn("Sub Procedure", typeof(string)),
                    new DataColumn("R_PEDesc", typeof(string)),
                    new DataColumn("R_PDesc", typeof(string)),
                    new DataColumn("S_CCDesc", typeof(string)),
                    new DataColumn("S_ADesc", typeof(string)),
                    new DataColumn("E-Heading", typeof(string)),
                    new DataColumn("E_PEDesc", typeof(string)),
                    new DataColumn("E_PDesc", typeof(string)),
                    new DataColumn("SidesDefault", typeof(string)),
                    new DataColumn("InHouseProcbit", typeof(bool)),
                    new DataColumn("sides", typeof(bool)),
                    new DataColumn("haslevel", typeof(bool)),
                    new DataColumn("cf", typeof(bool)),
                    new DataColumn("pn", typeof(bool)),
                    new DataColumn("InOut", typeof(bool)),
                    new DataColumn("BodyPart", typeof(string)),
                    new DataColumn("Heading", typeof(string)),
                    new DataColumn("HasMedication", typeof(string)),
                    new DataColumn("CCDesc", typeof(string)),
                    new DataColumn("ADesc", typeof(string)),
                    new DataColumn("S_PEDesc", typeof(string)),
                    new DataColumn("S_Heading", typeof(string)),
                    new DataColumn("E_CCDesc", typeof(string)),
                    new DataColumn("E_ADesc", typeof(string)),
                    new DataColumn("LevelsDefault", typeof(string)),
                    new DataColumn("MCode", typeof(string)),
                    new DataColumn("MCode Desc", typeof(string))
                });

                // Populate the DataTable with data from the list of attorneys
                foreach (var procedure in data)
                {
                    dt.Rows.Add(procedure.position, procedure.display_order, procedure.hasmuscle, procedure.hassubprocedure, procedure.pedesc, procedure.pdesc, procedure.s_ccdesc, procedure.s_adesc,
                        procedure.e_heading, procedure.e_pedesc, procedure.e_pdesc, procedure.sidesdefault, procedure.inhouseprocbit, procedure.sides, procedure.haslevel, procedure.cf, procedure.pn,
                        procedure.inout, procedure.bodypart, procedure.heading, procedure.hasmedication, procedure.ccdesc, procedure.adesc, procedure.s_pedesc, procedure.s_heading, procedure.e_ccdesc,
                        procedure.e_adesc, procedure.levelsdefault, procedure.mcode, procedure.mcode_desc);
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
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Procedure" };
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
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Procedure.xlsx");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Content("Error: " + ex.Message);
            }
        }


        #region Private Method
        private void SaveLog(Exception ex, string actionname)
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
                Message = msg
            };
            new LogService().Insert(logdata);            
        }
        #endregion
    }
}
