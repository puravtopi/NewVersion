﻿using AutoMapper;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using MS.Services;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;

namespace PainTrax.Web.Controllers
{
    public class FormsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<FormsController> _logger;

        private readonly IWebHostEnvironment _environment;
        private readonly PatientIEService _ieService = new PatientIEService();
        private readonly PatientService _patientservices = new PatientService();

        public FormsController(ILogger<FormsController> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
        }
        public IActionResult Index(string searchtxt = "")
        {
            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

            string cnd = " and cmp_id=" + cmpid;

            if (!string.IsNullOrEmpty(searchtxt))
                cnd = " and title like '%" + searchtxt + "%' ";

            var result = _patientservices.GetAll(cnd);
            var data = result;

            var downloadFolder = Path.Combine(_environment.WebRootPath, "Downloads/" + cmpid);
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
                Directory.CreateDirectory(downloadFolder + "/Forms");
                Directory.CreateDirectory(downloadFolder + "/BS");
                Directory.CreateDirectory(downloadFolder + "/Letters");
                Directory.CreateDirectory(downloadFolder + "/Others");
            }
            else
            {
                if (!Directory.Exists(downloadFolder + "/Forms")) Directory.CreateDirectory(downloadFolder + "/Forms");
                if (!Directory.Exists(downloadFolder + "/BS")) Directory.CreateDirectory(downloadFolder + "/BS");
                if (!Directory.Exists(downloadFolder + "/Letters")) Directory.CreateDirectory(downloadFolder + "/Letters");
                if (!Directory.Exists(downloadFolder + "/Others")) Directory.CreateDirectory(downloadFolder + "/Others");

            }

            string[] formsPaths = Directory.GetFiles(downloadFolder + "/Forms");
            string[] bsPaths = Directory.GetFiles(downloadFolder + "/BS");
            string[] lettersPaths = Directory.GetFiles(downloadFolder + "/Letters");
            string[] othersPaths = Directory.GetFiles(downloadFolder + "/Others");

            List<string> pdffiles = new List<string>();
            List<string> bsfiles = new List<string>();
            List<string> lettersfiles = new List<string>();
            List<string> othersfiles = new List<string>();
            for (int i = 0; i < formsPaths.Length; i++)
            {
                if (formsPaths[i].ToLower().EndsWith("pdf"))
                    pdffiles.Add(formsPaths[i].Substring(formsPaths[i].LastIndexOf("\\") + 1));
            }
            for (int i = 0; i < bsPaths.Length; i++)
            {
                if (bsPaths[i].ToLower().EndsWith("pdf"))
                    bsfiles.Add(bsPaths[i].Substring(bsPaths[i].LastIndexOf("\\") + 1));
            }
            for (int i = 0; i < lettersPaths.Length; i++)
            {
                if (lettersPaths[i].ToLower().EndsWith("pdf"))
                    lettersfiles.Add(lettersPaths[i].Substring(lettersPaths[i].LastIndexOf("\\") + 1));
            }
            for (int i = 0; i < othersPaths.Length; i++)
            {
                if (othersPaths[i].ToLower().EndsWith("pdf"))
                    othersfiles.Add(othersPaths[i].Substring(othersPaths[i].LastIndexOf("\\") + 1));
            }
            ViewBag.pdffiles = pdffiles;
            ViewBag.bsfiles = bsfiles;
            ViewBag.lettersfiles = lettersfiles;
            ViewBag.othersfiles = othersfiles;


            return View(data);

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
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                // Sort Column Direction ( asc ,desc)
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                // Search Value from (Search box)
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                //Paging Size (10,20,50,100)
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                string cnd = " and patient_id in (select id from tbl_patient where cmp_id=" + cmpid + ")";

                if (!string.IsNullOrEmpty(searchValue))
                    cnd = " and (fname like '%" + searchValue + "%' or lname  like '%" + searchValue + "%' or location  like '%" + searchValue + "%' or DATE_FORMAT(dob,\"%m/%d/%Y\") = '" + searchValue + "' or DATE_FORMAT(doe,\"%m/%d/%Y\") = '" + searchValue + "') ";

                var Data = _ieService.GetAll(cnd);

                //Sorting

                //Search


                //total number of rows count 
                recordsTotal = Data.Count();
                //Paging 
                var data = Data.Skip(skip).Take(pageSize).ToList();
                //Returning Json Data
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            }
            catch (Exception)
            {
                throw;
            }

        }
        public IActionResult GeneratePdf(string pdffile, string id, string txt_date = "", string txt_surgery = "", string txt_docName = "", string txt_MCode_Proc = "", string txtProcedureCode = "")
        {
            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
            Dictionary<string, string> controls = new Dictionary<string, string>();

            try
            {
                if (txt_date != null && txt_date.Trim() != "") controls.Add("txt_date", txt_date);
                if (txt_surgery != null && txt_surgery.Trim() != "") controls.Add("txt_surgery", txt_surgery);
                if (txt_docName != null && txt_docName.Trim() != "") controls.Add("txt_docName", txt_docName);
                if (txt_MCode_Proc != null && txt_MCode_Proc.Trim() != "") controls.Add("txt_MCode_Proc", txt_MCode_Proc);
                if (txtProcedureCode != null && txtProcedureCode.Trim() != "") controls.Add("txtProcedureCode", txtProcedureCode);

            }
            catch (Exception ex)
            {
                SaveLog(ex, "Read Textboxes");
            }

            PdfHelper _pdfhelper = new PdfHelper();
            string outputfilename = "";
            var uploadsFolder = "";
            var filePath = "";

            try
            {
                uploadsFolder = Path.Combine(_environment.WebRootPath, "Downloads/" + cmpid);
                filePath = Path.Combine(uploadsFolder, pdffile);

            }
            catch (Exception ex)
            {
                SaveLog(ex, "set Paths");
            }

            byte[] pdfBytes = null;
            try
            {
                pdfBytes = _pdfhelper.Stamping(filePath, "Id", id, controls, cmpid);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Pdf Stamping");
            }

            //string htmlContent = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "demo.html"));

            return File(pdfBytes, "application/pdf");
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
