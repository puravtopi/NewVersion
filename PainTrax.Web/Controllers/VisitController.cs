using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using HtmlToOpenXml;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MS.Models;
using MS.Services;
using Newtonsoft.Json;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Data;
using System.Text;
using static PainTrax.Web.Helper.EnumHelper;
using System.Text.RegularExpressions;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using iText.Signatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Linq;

namespace PainTrax.Web.Controllers
{
    [SessionCheckFilter]
    public class VisitController : Controller
    {
        private readonly Common _commonservices = new Common();
        private readonly PatientService _patientservices = new PatientService();
        private readonly PatientFUService _patientFUservices = new PatientFUService();
        private readonly InscosService _inscosservices = new InscosService();
        private readonly AttorneysService _attorneyservices = new AttorneysService();
        private readonly AadjusterService _aadjusterService = new AadjusterService();
        private readonly EmpService _empService = new EmpService();
        private readonly PatientIEService _ieService = new PatientIEService();
        private readonly UserService _userService = new UserService();
        private readonly MacrosMasterService _macroService = new MacrosMasterService();
        private readonly TreatmentMasterService _treatmentService = new TreatmentMasterService();
        private readonly DiagcodesService _diagcodesService = new DiagcodesService();

        private readonly POCServices _pocService = new POCServices();
        private readonly LocationsService _locService = new LocationsService();
        private readonly CaseTypeService _caseTypeService = new CaseTypeService();
        private readonly WebsiteMacrosMasterService _websiteMacrosService = new WebsiteMacrosMasterService();
        private readonly PrintSettingServices _printService = new PrintSettingServices();
        private readonly DefaultDataServices _defaultService = new DefaultDataServices();
        private readonly DefaultValueSettingServices _defaultSettingService = new DefaultValueSettingServices();
        private readonly ForwardServices _forwardServices = new ForwardServices();
        private EnumHelper enumHelper = new EnumHelper();
        private readonly FUPage3Service _fuPage3services = new FUPage3Service();
        private readonly FUOtherService _fuOtherservices = new FUOtherService();
        private readonly ILogger<VisitController> _logger;
        private readonly SettingsService _settingservices = new SettingsService();
        private readonly ProcedureService _procedureservices = new ProcedureService();
        private readonly ReferringPhysicianService _physicianService = new ReferringPhysicianService();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private IMapper _mapper;
        string SessionDiffDoc = "true";


        public VisitController(ILogger<VisitController> logger, IMapper mapper, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _logger = logger;
            _mapper = mapper; Environment = environment;
        }

        #region IE method
        public IActionResult Index(string searchtxt = "", string f = "")
        {
            ViewBag.pageSize = HttpContext.Session.GetInt32(SessionKeys.SessionPageSize).Value;
            ViewBag.filterCase = f;
            return View();
        }

        public IActionResult List(string f = "")
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
                string cnd = " and cmp_id = " + cmpid;

                if (!string.IsNullOrEmpty(searchValue))
                {
                    cnd = cnd + " and (fname like '%" + searchValue + "%' or lname  like '%" + searchValue + "%' or CONCAT(fname,' ',lname)  LIKE '%" + searchValue + "%' or CONCAT(lname,' ',fname)  LIKE '%" + searchValue + "%' or " +
                        "location  like '%" + searchValue + "%' or DATE_FORMAT(dob,\"%m/%d/%Y\") = '" + searchValue + "' or DATE_FORMAT(doe,\"%m/%d/%Y\") = '" + searchValue + "'  or " +
                        "compensation like '%" + searchValue + "%' or DATE_FORMAT(doa,\"%m/%d/%Y\") = '" + searchValue + "') ";
                }

                if (!string.IsNullOrEmpty(f))
                {
                    if (f == "A")
                    {
                        cnd = cnd + " AND  attorney_id=0 and patient_id IN(SELECT id FROM tbl_patient WHERE cmp_id = " + cmpid + ")";
                    }
                    else if (f == "I")
                    {
                        cnd = cnd + " AND primary_ins_cmp_id=0 AND patient_id IN (SELECT id FROM tbl_patient WHERE cmp_id=" + cmpid + ")";
                    }
                    else if (f == "C")
                    {
                        cnd = cnd + " AND  (primary_claim_no IS NULL OR primary_claim_no='') AND patient_id IN (SELECT id FROM tbl_patient WHERE cmp_id=" + cmpid + ")";
                    }
                }

                var Data = _ieService.GetAll(cnd);
                //tbl_users user = new tbl_users();
                //for (int i = 0; i < Data.Count; i++)
                //{
                //    user.Id = Data[i].provider_id;
                //    var providerData = _userService.GetOne(user);
                //    if (providerData != null)
                //    {
                //        Data[i].providerName = providerData.fullname;
                //    }


                //}

                //Sorting
                if (sortColumn != "0")
                {
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {

                        var _sortColumn = Convert.ToInt32(sortColumn);

                        if (_sortColumn > 0)
                            _sortColumn = _sortColumn - 1;

                        var property = typeof(vm_patient_ie).GetProperties()[_sortColumn];
                        if (sortColumnDirection.ToUpper() == "ASC")
                        {
                            Data = Data.OrderBy(x => property.GetValue(x, null)).ToList();
                        }
                        else
                        {
                            Data = Data.OrderByDescending(x => property.GetValue(x, null)).ToList();
                        }
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

        public IActionResult Create(int id = 0)
        {
            VisitVM obj = new VisitVM();
            int patientId = 0;

            try
            {
                obj.vaccinated = false;
                obj.mc = false;

                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

                ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
                ViewBag.csList = _commonservices.GetCaseType(cmpid.Value);
                ViewBag.asList = _commonservices.GetAccidenttype(cmpid.Value);
                ViewBag.stateList = _commonservices.GetState(cmpid.Value);
                ViewBag.physicianList = _commonservices.GetPhysician(cmpid.Value);

                var _defaultdata = _defaultService.GetOneByCompany(cmpid.Value);

                var macroList = _websiteMacrosService.GetAutoComplete(cmpid.Value);
                ViewBag.allmacroList = JsonConvert.SerializeObject(macroList);

                obj.listWebsiteMacros = macroList;

                macroList = _websiteMacrosService.GetAutoComplete(cmpid.Value, "CC");
                ViewBag.ccmacroList = JsonConvert.SerializeObject(macroList);

                macroList = _websiteMacrosService.GetAutoComplete(cmpid.Value, "PE");
                ViewBag.pemacroList = JsonConvert.SerializeObject(macroList);

                var providers = _userService.GetProviders(cmpid.Value);
                ViewBag.providerList = providers;

                if (id > 0)
                {
                    var ieData = _ieService.GetOnebyPatientId(id);

                    var dataPOC = this.getPOC(id);

                    if (ieData != null)
                    {
                        obj.id = ieData.id;
                        obj.casetype = ieData.casetype;
                        obj.locationid = ieData.location_id;
                        obj.location = ieData.location;
                        obj.doa = ieData.doa;
                        obj.dos = ieData.doe;
                        obj.dov = ieData.doe;
                        obj.prime_claim_no = ieData.primary_claim_no;
                        obj.prime_policy_no = ieData.primary_policy_no;
                        obj.prime_WCB_group = ieData.primary_wcb_group;
                        obj.providerid = ieData.provider_id;

                        obj.sec_claim_no = ieData.secondary_claim_no;
                        obj.sec_policy_no = ieData.secondary_policy_no;
                        obj.sec_WCB_group = ieData.secondary_wcb_group;
                        obj.alert_note = ieData.alert_note;

                        obj.physicianid = ieData.physicianid;
                        obj.compensation = ieData.compensation;
                        obj.accidentType = ieData.accidentType;
                        obj.state = ieData.state;
                        patientId = ieData.patientId.Value;
                        HttpContext.Session.SetInt32(SessionKeys.SessionPatientId, patientId);

                    }


                    if (ieData.primary_ins_cmp_id != null)
                    {
                        var primaryCmp = _inscosservices.GetOne(ieData.primary_ins_cmp_id.Value);

                        if (primaryCmp != null)
                        {
                            obj.prime_cmpname = primaryCmp.cmpname;
                            obj.prime_address = primaryCmp.address1;
                            obj.prime_phone = primaryCmp.telephone;
                        }
                    }

                    if (ieData.secondary_ins_cmp_id != null)
                    {
                        var secondaryCmp = _inscosservices.GetOne(ieData.secondary_ins_cmp_id.Value);

                        if (secondaryCmp != null)
                        {
                            obj.sec_cmpname = secondaryCmp.cmpname;
                            obj.sec_address = secondaryCmp.address1;
                            obj.sec_phone = secondaryCmp.telephone;
                        }
                    }

                    if (ieData.emp_id != null)
                    {
                        var empData = _empService.GetOne(ieData.emp_id.Value);

                        if (empData != null)
                        {
                            obj.emp_address = empData.address;
                            obj.emp_fax_no = empData.fax;
                            obj.emp_name = empData.name;
                            obj.emp_phone = empData.phone;
                        }
                    }

                    if (ieData.adjuster_id != null)
                    {
                        var adjData = _aadjusterService.GetOne(ieData.adjuster_id.Value);

                        if (adjData != null)
                        {
                            obj.adj_email = adjData.emailaddress;
                            obj.adj_fax_no = adjData.fax;
                            obj.adj_name = adjData.adjuster;
                            obj.adj_phone = adjData.telephone;
                        }
                    }

                    if (ieData.attorney_id != null)
                    {
                        var attrData = _attorneyservices.GetOne(ieData.attorney_id.Value);

                        if (attrData != null)
                        {
                            obj.attory_email = attrData.EmailId;
                            obj.attory_fax_no = "";
                            obj.attory_name = attrData.Attorney;
                            obj.attory_phone = attrData.ContactNo;
                        }
                    }

                    var page1Data = _ieService.GetOnePage1(id);

                    if (page1Data != null)
                    {
                        obj.Page1 = page1Data;

                        //if (!string.IsNullOrEmpty(dataPOC.strCCDesc))
                        //    obj.Page1.cc = obj.Page1.cc + "<p>" + dataPOC.strCCDesc + "</p>";

                        //if (!string.IsNullOrEmpty(dataPOC.strPEDesc))
                        //    obj.Page1.pe = obj.Page1.pe + "<p>" + dataPOC.strPEDesc + "</p>";

                        //if (!string.IsNullOrEmpty(dataPOC.strADesc))
                        //    obj.Page1.assessment = obj.Page1.assessment + "<p>" + dataPOC.strADesc + "</p>";

                        ViewBag.Assessment = page1Data.assessment;

                        string daignoLink = "";
                        if (!string.IsNullOrEmpty(page1Data.bodypart))
                        {
                            for (int i = 0; i < page1Data.bodypart.Split(',').Length; i++)
                            {
                                var linkbody = page1Data.bodypart.Split(',')[i].Replace(" ", "_");
                                daignoLink += "<a href='javascript:void(0)' onclick=openDaignoModel('" + linkbody + "')>" + page1Data.bodypart.Split(',')[i] + "</a><br/>";
                            }

                        }
                        ViewBag.DaignoLink = daignoLink;
                    }
                    else
                        obj.Page1 = new tbl_ie_page1();

                    if (dataPOC != null && string.IsNullOrEmpty(obj.Page1.plan))
                        obj.Page1.plan = dataPOC.strPoc;


                    var page2Data = _ieService.GetOnePage2(id);

                    if (page2Data != null)
                    {
                        obj.Page2 = page2Data;
                    }
                    else
                        obj.Page2 = new tbl_ie_page2();

                    var NEData = _ieService.GetOneNE(id);

                    if (NEData != null)
                    {
                        obj.NE = NEData;
                    }
                    else
                    {
                        obj.NE = new tbl_ie_ne();
                    }

                    obj.NE.neurological_exam = string.IsNullOrEmpty(obj.NE.neurological_exam) ? _defaultdata.ne : obj.NE.neurological_exam;

                    var page3Data = _ieService.GetOnePage3(id);

                    if (page3Data != null)
                    {
                        obj.Page3 = page3Data;
                    }
                    else
                        obj.Page3 = new tbl_ie_page3();




                    obj.Page3.care = string.IsNullOrEmpty(obj.Page3.care) ? _defaultdata.care : obj.Page3.care;
                    obj.Page3.goal = string.IsNullOrEmpty(obj.Page3.goal) ? _defaultdata.goal : obj.Page3.goal;


                    var pageComment = _ieService.GetOneComment(id);

                    if (pageComment != null)
                    {
                        obj.Comment = pageComment;
                    }
                    else
                        obj.Comment = new tbl_ie_comment();

                    var pageOther = _ieService.GetOneOtherPage(id);

                    if (pageOther != null)
                    {
                        obj.Other = pageOther;


                        var tretmentdesc = string.IsNullOrEmpty(pageOther.treatment_delimit_desc) ? null : pageOther.treatment_delimit_desc.Split('^');


                        var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);

                        List<tbl_treatment_master> lst = new List<tbl_treatment_master>();
                        int i = 0;
                        try
                        {
                            foreach (var item in _data)
                            {
                                if (!string.IsNullOrEmpty(pageOther.treatment_delimit))
                                {
                                    if (pageOther.treatment_delimit.Contains(item.id.ToString()))
                                        item.pre_select = true;
                                    else
                                        item.pre_select = false;
                                }

                                //if (tretmentdesc != null)
                                //    item.treatment_details = tretmentdesc[i];


                                lst.Add(item);
                                i++;
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        obj.Other.listTreatmentMaster = lst;
                    }
                    else
                    {
                        obj.Other = new tbl_ie_other();
                        var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);
                        obj.Other.followup_duration = "2 weeks.";
                        obj.Other.listTreatmentMaster = _data;
                    }



                    var patientData = _patientservices.GetOne(ieData.patient_id.Value);

                    if (patientData != null)
                    {
                        obj.fname = patientData.fname;
                        obj.lname = patientData.lname;
                        obj.mname = patientData.mname;
                        obj.gender = patientData.gender;
                        obj.dob = patientData.dob;
                        obj.email = patientData.email;
                        obj.handeness = patientData.handeness;
                        obj.ssn = patientData.ssn;
                        obj.address = patientData.address;
                        obj.city = patientData.city;
                        obj.state = patientData.state;
                        obj.zip = patientData.zip;
                        obj.account_no = patientData.account_no;
                        obj.home_ph = patientData.home_ph;
                        obj.mobile = patientData.mobile;
                        obj.mc = patientData.mc;
                        obj.vaccinated = patientData.vaccinated;
                        obj.patientid = patientData.id;
                        obj.age = patientData.age;
                        patientId = obj.patientid.Value;
                        //  obj.physicianid = patientData.physicianid;
                    }
                }
                else
                {
                    obj.patientid = 0;
                    obj.id = 0;
                    obj.Page1 = new tbl_ie_page1();
                    obj.Page2 = new tbl_ie_page2();
                    obj.Page3 = new tbl_ie_page3();
                    obj.NE = new tbl_ie_ne();
                    obj.Comment = new tbl_ie_comment();
                    obj.Other = new tbl_ie_other();
                    obj.Other.followup_duration = "2-4 weeks.";
                    obj.dos = System.DateTime.Now;
                    obj.locationid = HttpContext.Session.GetInt32(SessionKeys.SessionLocationId);
                    obj.Page3.gait = "Guarded";

                    var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);


                    if (_defaultdata != null)
                    {
                        obj.NE.neurological_exam = _defaultdata.ne;
                        obj.Page3.goal = _defaultdata.goal;
                        obj.Page3.care = _defaultdata.care;
                    }

                    obj.Other.listTreatmentMaster = _data;

                    var defaultPage1 = _defaultSettingService.GetOnePage1(cmpid.Value);

                    if (defaultPage1 != null)
                    {


                        obj.Page1.allergies = defaultPage1.allergies;
                        obj.Page1.dd = defaultPage1.dd;
                        obj.Page1.daignosis_desc = defaultPage1.daignosis_desc;
                        obj.Page1.assessment = defaultPage1.daignosis_desc;
                        obj.Page1.cc = defaultPage1.cc;
                        obj.Page1.pe = defaultPage1.pe;
                        obj.Page1.family_history = defaultPage1.family_history;
                        obj.Page1.history = defaultPage1.history;
                        obj.Page1.medication = defaultPage1.medication;
                        obj.Page1.note = defaultPage1.note;

                        obj.Page1.occupation = defaultPage1.occupation;
                        obj.Page1.plan = defaultPage1.plan;
                        obj.Page1.pmh = defaultPage1.pmh;
                        obj.Page1.psh = defaultPage1.psh;
                        obj.Page1.rom = defaultPage1.rom;
                        obj.Page1.social_history = defaultPage1.social_history;
                        obj.Page1.vital = defaultPage1.vital;
                        obj.Page1.work_status = defaultPage1.work_status;
                        ViewBag.Assessment = defaultPage1.assessment;
                    }

                    var defaultPage2 = _defaultSettingService.GetOnePage2(cmpid.Value);

                    if (defaultPage2 != null)
                    {
                        obj.Page2.aod = defaultPage2.aod;
                        obj.Page2.other = defaultPage2.other;
                        obj.Page2.ros = defaultPage2.ros;

                    }
                    var defaultNE = _defaultSettingService.GetOneNE(cmpid.Value);

                    if (defaultNE != null)
                    {
                        obj.NE.neurological_exam = defaultNE.neurological_exam;
                        obj.NE.sensory = defaultNE.sensory;
                        obj.NE.manual_muscle_strength_testing = defaultNE.manual_muscle_strength_testing;
                        obj.NE.other_content = defaultNE.other_content;
                    }

                }

                ViewBag.showPrint = id > 0 ? true : false;

                //for documents
                {

                    HttpContext.Session.SetInt32(SessionKeys.SessionPatientId, patientId);

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
                        var FolderPathFile = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", FolderName.ToString(), patientId.ToString());
                        int j = 0;

                        bool folderExistsNew = Directory.Exists(FolderPathFile);
                        if (!folderExistsNew)
                            Directory.CreateDirectory(FolderPathFile);

                        foreach (var item1 in Directory.GetFiles(FolderPathFile))
                        {
                            j++;
                            nodes.Add(new TreeViewNode { id = j.ToString() + "-" + i.ToString() + "~" + FolderName.ToString() + "$" + System.IO.Path.GetFileName(item1), parent = i.ToString(), text = System.IO.Path.GetFileName(item1) });
                        }

                        nodes.Add(new TreeViewNode { id = i.ToString(), parent = "#", text = FolderName.ToString() + "(" + j + ")" });
                    }

                    obj.doc_json = JsonConvert.SerializeObject(nodes, Formatting.Indented);

                }
                if (obj.Page3 != null)
                {
                    obj.Page3.diagcervialbulge_study = (obj.Page3.diagcervialbulge_study == null ) ? "1" : obj.Page3.diagcervialbulge_study;
                    obj.Page3.diagthoracicbulge_study = (obj.Page3.diagthoracicbulge_study == null ) ? "1" : obj.Page3.diagthoracicbulge_study;
                    obj.Page3.diaglumberbulge_study = (obj.Page3.diaglumberbulge_study == null) ? "1" : obj.Page3.diaglumberbulge_study;
                    obj.Page3.diagleftshoulder_study = (obj.Page3.diagleftshoulder_study == null ) ? "1" : obj.Page3.diagleftshoulder_study;
                    obj.Page3.diagrightshoulder_study = (obj.Page3.diagrightshoulder_study == null ) ? "1" : obj.Page3.diagrightshoulder_study;
                    obj.Page3.diagleftknee_study = (obj.Page3.diagleftknee_study == null ) ? "1" : obj.Page3.diagleftknee_study;
                    obj.Page3.diagrightknee_study = (obj.Page3.diagrightknee_study == null ) ? "1" : obj.Page3.diagrightknee_study;
                    obj.Page3.diaglumberbulge_study = (obj.Page3.diaglumberbulge_study == null ) ? "1" : obj.Page3.diaglumberbulge_study;

                    obj.Page3.other1_study = (obj.Page3.other1_study == null ) ? "1" : obj.Page3.other1_study;
                    obj.Page3.other2_study = (obj.Page3.other2_study == null ) ? "1" : obj.Page3.other2_study;
                    obj.Page3.other3_study = (obj.Page3.other3_study == null ) ? "1" : obj.Page3.other3_study;
                    obj.Page3.other4_study = (obj.Page3.other4_study == null ) ? "1" : obj.Page3.other4_study;
                    obj.Page3.other5_study = (obj.Page3.other5_study == null ) ? "1" : obj.Page3.other5_study;
                    obj.Page3.other6_study = (obj.Page3.other6_study == null) ? "1" : obj.Page3.other6_study;
                    obj.Page3.other7_study = (obj.Page3.other7_study == null ) ? "1" : obj.Page3.other7_study;
                }

            }
            catch (Exception ex)
            {
                SaveLog(ex, "Create");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult SaveDetails(VisitVM model)
        {
            try
            {
                var data = model;
                int patientId = 0, priminsId = 0, secinsId = 0, attornyId = 0, adjusterId = 0, empId = 0;

                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                int? userid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId);
                int age = calculateAge(model.dob.Value);

                tbl_patient objPatient = new tbl_patient()
                {
                    account_no = model.account_no,
                    address = model.address,
                    city = model.city,
                    dob = model.dob,
                    email = model.email,
                    fname = model.fname,
                    gender = model.gender,
                    home_ph = model.home_ph,
                    lname = model.lname,
                    mc = model.mc,
                    mname = model.mname,
                    mobile = model.mobile,
                    ssn = model.ssn,
                    state = model.state,
                    physicianid = model.physicianid,
                    vaccinated = model.vaccinated,
                    zip = model.zip,
                    cmp_id = cmpid,
                    createdby = userid,
                    age = age
                };

                if (model.id.Value > 0)
                {
                    objPatient.id = model.patientid;
                    _patientservices.Update(objPatient);
                    patientId = model.patientid.Value;
                }
                else
                {
                    patientId = _patientservices.Insert(objPatient);
                }

                HttpContext.Session.SetInt32(SessionKeys.SessionPatientId, patientId);
                var query = "";
                List<tbl_inscos> insdata = new List<tbl_inscos>();
                tbl_inscos objInscos = new tbl_inscos();

                if (!string.IsNullOrEmpty(model.prime_cmpname))
                {
                    query = " and cmpname='" + model.prime_cmpname + "' and cmp_id=" + cmpid + "";

                    insdata = _inscosservices.GetAll(query);

                    //save primary insurance

                    objInscos = new tbl_inscos()
                    {
                        address1 = model.prime_address,
                        cmpname = model.prime_cmpname,
                        telephone = model.prime_phone,
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

                if (!string.IsNullOrEmpty(model.sec_cmpname))
                {

                    query = " and cmpname='" + model.sec_cmpname + "' and cmp_id=" + cmpid + "";

                    insdata = _inscosservices.GetAll(query);

                    //save secondary insurance

                    objInscos = new tbl_inscos()
                    {
                        address1 = model.sec_address,
                        cmpname = model.sec_cmpname,
                        telephone = model.sec_phone,
                        cmp_id = cmpid,
                        createdby = userid

                    };

                    if (insdata.Count > 0)
                    {
                        objInscos.id = insdata[0].id.Value;
                        _inscosservices.Update(objInscos);
                        secinsId = insdata[0].id.Value;
                    }
                    else
                    {
                        secinsId = _inscosservices.Insert(objInscos);
                    }
                }

                if (!string.IsNullOrEmpty(model.attory_name))
                {

                    query = " and Attorney='" + model.attory_name + "' and cmp_id=" + cmpid + "";

                    var attrydata = _attorneyservices.GetAll(query);


                    //save attorney

                    tbl_attorneys objAttorneys = new tbl_attorneys()
                    {
                        Attorney = model.attory_name,
                        EmailId = model.attory_email,
                        ContactNo = model.attory_phone,
                        cmp_id = cmpid,
                        CreatedBy = userid

                    };

                    if (attrydata.Count > 0)
                    {
                        objAttorneys.Id = attrydata[0].Id.Value;
                        _attorneyservices.Update(objAttorneys);
                        attornyId = attrydata[0].Id.Value;
                    }
                    else
                    {
                        attornyId = _attorneyservices.Insert(objAttorneys);
                    }
                }

                if (!string.IsNullOrEmpty(model.adj_name))
                {
                    query = " and adjuster='" + model.adj_name + "' and cmp_id=" + cmpid + "";

                    var adjdata = _aadjusterService.GetAll(query);

                    //save adjuster

                    tbl_adjuster objAdjuster = new tbl_adjuster()
                    {
                        adjuster = model.adj_name,
                        emailaddress = model.adj_email,
                        telephone = model.adj_phone,
                        fax = model.adj_fax_no,
                        cmp_id = cmpid,
                        created_by = userid
                    };

                    if (adjdata.Count > 0)
                    {
                        objAdjuster.id = adjdata[0].id.Value;
                        _aadjusterService.Update(objAdjuster);
                        adjusterId = adjdata[0].id.Value;
                    }
                    else
                    {
                        adjusterId = _aadjusterService.Insert(objAdjuster);
                    }
                }

                if (!string.IsNullOrEmpty(model.emp_name))
                {
                    query = " and name='" + model.emp_name + "' and patient_id=" + model.patientid + "";

                    var empdata = _empService.GetAll(query);

                    //save employee

                    tbl_emp objEmp = new tbl_emp()
                    {
                        address = model.emp_address,
                        name = model.emp_name,
                        phone = model.emp_phone,
                        fax = model.emp_fax_no,
                        patient_id = patientId
                    };

                    if (empdata.Count > 0)
                    {
                        objEmp.id = empdata[0].id.Value;
                        _empService.Update(objEmp);
                        empId = empdata[0].id.Value;
                    }
                    else
                    {
                        empId = _empService.Insert(objEmp);
                    }
                }

                //save IE details 

                tbl_patient_ie objIE = new tbl_patient_ie()
                {
                    adjuster_id = adjusterId,
                    attorney_id = attornyId,
                    created_by = userid,
                    doa = model.doa,
                    doe = model.dos,
                    emp_id = empId,
                    is_active = true,
                    location_id = model.locationid,
                    provider_id = model.providerid,
                    patient_id = patientId,
                    primary_claim_no = model.prime_claim_no,
                    primary_ins_cmp_id = priminsId,
                    primary_policy_no = model.prime_policy_no,
                    primary_wcb_group = model.prime_WCB_group,
                    secondary_claim_no = model.sec_claim_no,
                    secondary_ins_cmp_id = secinsId,
                    secondary_policy_no = model.sec_policy_no,
                    secondary_wcb_group = model.sec_WCB_group,
                    compensation = model.compensation,
                    accident_type = model.accidentType,
                    state = model.state,
                    physicianid = model.physicianid,
                    alert_note = model.alert_note,
                    referring_physician = model.referring_physician

                };
                int ie = 0;
                if (model.id.Value > 0)
                {
                    objIE.id = model.id.Value;
                    _ieService.Update(objIE);
                    ie = model.id.Value;
                }
                else
                    ie = _ieService.Insert(objIE);

                HttpContext.Session.SetInt32(SessionKeys.SessionIEId, ie);

                return Json(new { status = 1, patintid = patientId, ieid = ie });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0 });
            }


        }

        [HttpPost]
        public IActionResult CheckPatientName(VisitVM model)
        {
            try
            {

                if (model != null)
                {
                    int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                    string cnd = " and fname='" + model.fname + "' and lname='" + model.lname + "' and DATE(DOB)='" + model.dob.Value.ToString("yyyy-MM-dd") + "' and cmp_id=" + cmpid;
                    var data = _patientservices.GetAll(cnd);

                    if (data.Count > 0)
                        return Json(new { result = 1, data = data });
                    else
                        return Json(new { result = 0 });
                }
                return Json(new { result = 0 });
            }
            catch (Exception ex)
            {
                SaveLog(ex, "CheckPatientName");
                return Json(new { result = -1, message = "An error occurred while checking patient name." });
            }
        }

        [HttpPost]
        public IActionResult CheckPatientNameOnly(VisitVM model)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                string cnd = "";

                if (!string.IsNullOrEmpty(model.mname))
                    cnd = " and fname='" + model.fname + "' and lname='" + model.lname + "' and mname='" + model.mname + "'  and cmp_id=" + cmpid;
                else
                    cnd = " and fname='" + model.fname + "' and lname='" + model.lname + "' and mname is null  and cmp_id=" + cmpid;
                var data = _patientservices.GetAll(cnd);

                if (data.Count > 0)
                    return Json(new { result = 1, data = data });
                else
                    return Json(new { result = 0 });
            }
            catch (Exception ex)
            {
                SaveLog(ex, "CheckPatientNameOnly");
                return Json(new { result = -1, message = "An error occurred " });
            }
        }

        [HttpPost]
        public IActionResult GetMacroDetails(string bodyName)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                string cnd = " and bodypart='" + bodyName + "' and cmp_id=" + cmpid;

                var data = _macroService.GetAll(cnd);

                if (data.Count > 0)
                    return Json(new { result = 1, data = data });
                else
                    return Json(new { result = 0 });
            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetMacroDetails");
                return Json(new { result = -1, message = "An error occurred" });
            }

        }

        [HttpPost]
        public IActionResult SavePage1(tbl_ie_page1 model)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;
                var data = 0;
                if (model.id > 0)
                {
                    data = model.id.Value;
                    _ieService.UpdatePage1(model);
                }
                else
                    data = _ieService.InsertPage1(model);

                return Json(data);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SavePage1");
                return Json("");
            }
        }

        [HttpPost]
        public IActionResult SavePage2(tbl_ie_page2 model)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;
                var data = 0;
                if (model.id > 0)
                {
                    data = model.id.Value;
                    _ieService.UpdatePage2(model);
                }
                else
                    data = _ieService.InsertPage2(model);

                return Json(data);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SaveNE");
                return Json("");
            }
        }

        [HttpPost]
        public IActionResult SaveNE(tbl_ie_ne model)
        {
            try
            {

                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;
                var data = 0;
                if (model.id > 0)
                {
                    data = model.id.Value;
                    _ieService.UpdatePageNE(model);
                }
                else
                    data = _ieService.InsertNE(model);

                return Json(data);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SaveNE");
                return Json("");
            }
        }

        [HttpPost]
        public IActionResult SavePage3(tbl_ie_page3 model, int ieid, int patientid, int? id, string discharge_medications)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;

                model.ie_id = ieid;
                model.patient_id = patientid;

                if (model.diagcervialbulge_date != null)
                    model.diagcervialbulge = true;
                else
                    model.diagcervialbulge = false;

                if (model.diagleftknee_date != null)
                    model.diagleftknee = true;
                else
                    model.diagleftknee = false;

                if (model.diaglumberbulge_date != null)
                    model.diaglumberbulge = true;
                else
                    model.diaglumberbulge = false;

                if (model.diagrightknee_date != null)
                    model.diagrightknee = true;
                else
                    model.diagrightknee = false;

                if (model.diagthoracicbulge_date != null)
                    model.diagthoracicbulge = true;
                else
                    model.diagthoracicbulge = false;

                if (model.diagrightshoulder_date != null)
                    model.diagrightshoulder = true;
                else
                    model.diagrightshoulder = false;

                if (model.diagleftshoulder_date != null)
                    model.diagleftshoulder = true;
                else
                    model.diagleftshoulder = false;

                if (model.other1_date != null)
                    model.other1 = true;
                else
                    model.other1 = false;

                if (model.other2_date != null)
                    model.other2 = true;
                else
                    model.other2 = false;

                if (model.other3_date != null)
                    model.other3 = true;
                else
                    model.other3 = false;

                if (model.other4_date != null)
                    model.other4 = true;
                else
                    model.other4 = false;

                if (model.other5_date != null)
                    model.other5 = true;
                else
                    model.other5 = false;

                if (model.other6_date != null)
                    model.other6 = true;
                else
                    model.other6 = false;

                if (model.other7_date != null)
                    model.other7 = true;
                else
                    model.other7 = false;
                int data = 0;
                if (id > 0)
                {
                    model.id = id;
                    _ieService.UpdatePage3(model);
                    data = id.Value;
                }
                else
                    data = _ieService.InsertPage3(model);

                if (data > 0)
                    return Json(1);
                else
                    return Json(0);
            }
            catch (Exception ex)
            {
                return Json(0);
            }
        }

        [HttpPost]
        public IActionResult SaveOther(tbl_ie_other model)
        {

            var data = 0;
            if (model.id > 0)
            {
                data = model.id.Value;
                _ieService.UpdateOtherPage(model);
            }
            else
                data = _ieService.InsertOtherPage(model);

            return Json(data);
        }

        [HttpPost]
        public IActionResult SaveComment(tbl_ie_comment model)
        {

            var data = 0;
            if (model.id > 0)
            {
                data = model.id.Value;
                _ieService.UpdateComment(model);
            }
            else
                data = _ieService.InsertComment(model);

            return Json(data);
        }

        [HttpPost]
        public IActionResult GetComment(int ieId)
        {

            var data = new tbl_ie_comment();
            try
            {
                data = _ieService.GetOneComment(ieId);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetComment");
            }

            return Json(data);
        }


        [HttpPost]
        public IActionResult SaveSign([FromBody] tbl_ie_sign model)
        {
            if (string.IsNullOrEmpty(model.signatureData))
                return BadRequest("Invalid signature data.");
            try
            {
                //var base64Data = model.signatureData.Contains(",")
                //                ? model.signatureData.Split(',')[1]
                //                : model.signatureData;
                var base64Data = ExtractBase64Data(model.signatureData);

                if (base64Data == null || !IsBase64String(base64Data))
                {
                    return BadRequest("Invalid Base-64 string.");
                }


                var imageData = Convert.FromBase64String(base64Data);
                var signaturesDir = Path.Combine(Environment.WebRootPath, "signatures");
                Debug.WriteLine($"Received tbl_ie_sign: id={model.id}, ie_id={model.patient_id}, signatureData length={model.signatureData?.Length}");

                if (!Directory.Exists(signaturesDir))
                {
                    Directory.CreateDirectory(signaturesDir);
                }
                var filename = $"{model.patient_id}.jpeg";
                var savePath = Path.Combine(signaturesDir, filename);

                System.IO.File.WriteAllBytes(savePath, imageData);
                model.signatureValue = model.signatureData;
                model.signatureData = filename;



                if (model.id > 0)
                {
                    _ieService.UpdateSign(model);
                }
                else
                {
                    _ieService.InsertSign(model);
                }
                return Ok(new { FileName = filename });
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SaveSign");
                return StatusCode(500, "Error saving signature: " + ex.Message);
            }
        }
        private string ExtractBase64Data(string signatureData)
        {
            // Example method to extract Base64 data from a comma-separated string
            // Adjust as per your actual data structure
            return signatureData.Contains(",")
                   ? signatureData.Split(',')[1].Trim()
                   : signatureData.Trim();
        }

        //public IActionResult SaveSign([FromBody] tbl_ie_sign model)
        //{
        //   // if (string.IsNullOrEmpty(model.signatureData))
        //  //    return BadRequest("Invalid signature data.");
        //    var data = model.id;   
        //    try
        //    {
        //        var base64Data = model.signatureData.Contains(",")
        //                        ? model.signatureData.Split(',')[1]
        //                        : model.signatureData;
        //        if (!IsBase64String(base64Data))
        //        {
        //            return BadRequest("Invalid Base-64 string.");
        //        }

        //        var imageData = Convert.FromBase64String(base64Data);

        //        var signaturesDir = Path.Combine(Environment.WebRootPath, "signatures");
        //        if (!Directory.Exists(signaturesDir))
        //        {
        //            Directory.CreateDirectory(signaturesDir);
        //        }
        //        var filename = $"{model.ie_id}.jpeg";
        //        var savePath = Path.Combine(signaturesDir, filename);

        //        System.IO.File.WriteAllBytes(savePath, imageData);
        //        model.signatureData = savePath;
        //        if (model.id > 0)
        //        {
        //            data = model.id;
        //            _ieService.UpdateSign(model);
        //        }
        //        else
        //        {
        //            _ieService.InsertSign(model);
        //        }
        //        return Ok(new { FileName = filename });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Error saving signature: " + ex.Message);
        //    }
        //}
        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }


        [HttpPost]
        public IActionResult GetSign(int pId)
        {
            var data = new tbl_ie_sign();
            try
            {
                data = _ieService.GetOnesign(pId);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetSign");
            }
            return Json(data);
        }

        [HttpPost]
        public IActionResult GetDaignoCodeList(string bodyparts, int ieId)
        {
            try
            {

                var page1Data = _ieService.GetOnePage1(ieId);

                string assetment = "";

                if (page1Data != null)
                    assetment = page1Data.assessment;

                bodyparts = bodyparts.Replace("_", " ");
                ViewBag.BodyPart = bodyparts.ToUpper();
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

                string cnd = " and cmp_id=" + cmpid + " and (BodyPart='" + bodyparts + "' or Description like '%" + bodyparts + "%') order by Description desc";

                var data = _diagcodesService.GetAll(cnd);

                var cmpIdInt = Convert.ToInt32(cmpid);

                var datavm = (from c in data
                              select new DaignoCodeVM
                              {
                                  DaignoCodeId = c.Id.Value,
                                  Description = c.Description,
                                  DiagCode = c.DiagCode,
                                  IsSelect = assetment != null ? (assetment.IndexOf(c.Description) > 0 ? true : c.PreSelect) : false,
                                  Display_Order = c.display_order,
                                  cmp_id = c.cmp_id

                              }).ToList().Where(x => x.cmp_id == cmpIdInt).OrderBy(x => x.Display_Order);
                return PartialView("_DaignoCode", datavm);


            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetDaignoCodeList");
            }

            return View();
        }

        [HttpPost]
        public IActionResult GetDefultDaignoCodeList(string bodyparts)
        {
            try
            {
                bodyparts = bodyparts.Replace("_", " ");

                bodyparts = bodyparts.Replace(",", "','");

                ViewBag.BodyPart = bodyparts.ToUpper();
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

                string cnd = " and cmp_id=" + cmpid + " and BodyPart in ('" + bodyparts + "')";

                var data = _diagcodesService.GetAll(cnd);



                var datavm = (from c in data
                              select new DaignoCodeVM
                              {
                                  DaignoCodeId = c.Id.Value,
                                  Description = c.Description,
                                  DiagCode = c.DiagCode,
                                  IsSelect = c.PreSelect,
                                  Display_Order = c.display_order

                              }).ToList().OrderBy(x => x.Display_Order);
                return Json(datavm);

            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetDefultDaignoCodeList");

            }

            return View();
        }

        [HttpGet]
        public JsonResult GetPhysiciansByLocation(int locationId)
        {
            var physicians = _physicianService.GetAll(" AND locationid=" + locationId);
            var physicianList = physicians.Select(p => new SelectListItem
            {
                Text = p.physicianname,
                Value = p.Id.ToString(),
            }).ToList();

            return Json(physicianList);
        }
        #endregion

        #region FU method
        [HttpPost]
        public IActionResult FuList(int ieId)
        {
            var data = _patientFUservices.GetAllByIeId(ieId);
            tbl_users user = new tbl_users();
            for (int i = 0; i < data.Count; i++)
            {
                user.Id = data[i].provider_id;
                var providerData = _userService.GetOne(user);
                if (providerData != null)
                {
                    data[i].providerName = providerData.fullname;
                }


            }

            return Json(data);
        }

        #endregion

        #region POC method

        [HttpPost]
        public IActionResult GetPOC(int patientIEId)
        {
            try
            {

                //var injurbodyparts = _pocService.GetInjuredParts(patientIEId);
                var injurbodyparts = _pocService.GetInjuredPartsPOC(patientIEId);

                if (injurbodyparts != null)
                    injurbodyparts = injurbodyparts.Append("Other").ToArray();
                else
                {
                    injurbodyparts = new string[1];
                    injurbodyparts[0] = "Other";
                }

                string potion = null;
                string iinew = string.Empty;
                StringBuilder html = new StringBuilder();

                foreach (var ii in injurbodyparts)
                {

                    if (ii.ToLower().Contains("left"))
                    {
                        potion = "Left";
                        iinew = ii.Substring(4, ii.Length - 4);
                    }
                    else if (ii.ToLower().Contains("right"))
                    {
                        potion = "Right";
                        iinew = ii.Substring(5, ii.Length - 5);
                    }
                    else
                    {
                        potion = null;
                        iinew = ii;
                    }

                    int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                    var x = _pocService.GetAllProcedures(iinew.Trim(), patientIEId, potion, cmpid.Value);



                    if (x != null)
                    {
                        if (x.Rows.Count > 0)
                        {

                            //Table start.
                            html.Append("<div class='accordion-item'>" +
                                "<h2 class='accordion-header' id='#" + ii + "_HeadingColps'>" +
                                "<button class='accordion-button collapsed' type='button' data-bs-toggle='collapse' data-bs-target='#" + ii.Replace(" ", "_") + "_Colps' aria-expanded='false' aria-controls='flush-collapseTwo'>" + ii.ToUpperInvariant() + "</button></h2>" +
                                "<div id='" + ii.Replace(" ", "_") + "_Colps' class='accordion-collapse collapse' aria-labelledby='" + ii.Replace(" ", "_") + "_HeadingColps' data-bs-parent='#accordionFlushExample'>" +
                                "<div class='accordion-body'><div class='table-responsive'>" +
                                "<table class='table table-hover table-bordered tblpoc'  border = '1' id='" + ii + "_tbl'>");
                            html.Append("<thead>");
                            //Building the Header row.
                            html.Append("<tr>");

                            foreach (DataColumn column in x.Columns)
                            {
                                if (column.ColumnName != "procedureID" && (column.ColumnName != "MCODE") &&
                                (column.ColumnName != "INhouseProc") && (column.ColumnName != "HasPosition") &&
                                (column.ColumnName != "HasLevel") && (column.ColumnName != "HasMuscle") &&
                                (column.ColumnName != "HasSubCode") && (column.ColumnName != "BID") && (column.ColumnName != "HasMedication") &&
                                (column.ColumnName != "PatientProceduresID")
                                && (column.ColumnName != "Medication") && (column.ColumnName != "Muscle") && (column.ColumnName != "Level")
                                && (column.ColumnName != "Req_Pos") && (column.ColumnName != "Sch_Pos") && (column.ColumnName != "FU_Pos")
                                && (column.ColumnName != "Exe_Pos") && (column.ColumnName != "SubProcedureID") && (column.ColumnName != "PatientIEID")
                                && (column.ColumnName != "PatientFuID") && (column.ColumnName != "Sides") && (column.ColumnName != "HasSides")
                                && (column.ColumnName != "Display_Order") && (column.ColumnName != "ProcedureDetailID")
                                && (column.ColumnName != "Consider") && (column.ColumnName != "Consider")
                                && (column.ColumnName != "LevelsDefault") && (column.ColumnName != "SidesDefault") && (column.ColumnName != "CF") && (column.ColumnName != "SignPath"))
                                {
                                    if (column.ColumnName == "bodypart")
                                    {
                                        html.Append("<th  scope=\"col\" style='height: 35px;min-width:130px;background-color:yellow'>");
                                        html.Append(ii.ToUpperInvariant());
                                        html.Append("</th>");
                                    }
                                    else
                                    {

                                        if (column.ColumnName != "Followup" && column.ColumnName != "mcode_desc")
                                        {
                                            html.Append("<th scope=\"col\" style='height: 35px;;min-width:130px;'>");
                                            html.Append(column.ColumnName);
                                            html.Append("</th>");
                                        }
                                        /*else
                                        {
                                            html.Append("<th scope=\"col\"  style='height: 35px; background-color:grey'>");
                                            html.Append(column.ColumnName);
                                            html.Append("</th>");
                                        }*/

                                    }
                                }
                            }
                            html.Append("</tr>");
                            html.Append("</thead>");
                            html.Append("<tbody>");

                            foreach (DataRow row in x.Rows)
                            {
                                html.Append("<tr>");
                                foreach (DataColumn column in x.Columns)
                                {
                                    if ((column.ColumnName != "ProcedureDetail_ID") && (column.ColumnName != "Bodypart") && (column.ColumnName != "patientfu_id") && (column.ColumnName != "patientie_id") && (column.ColumnName != "InHouseProc"))
                                    {
                                        /*if (column.ColumnName == "Consider")
                                         {
                                             string date1 = string.Empty;
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[14])))
                                             {
                                                 date1 = Convert.ToDateTime(row[14]).ToString("MM/dd/yyyy").Replace('-', '/');
                                             }

                                             if (row[10] != null)
                                                 if (Convert.ToInt32(row[10]) != 0)
                                                 {
                                                     if (row[14] != DBNull.Value)
                                                     {
                                                         html.Append("<td >");
                                                         html.Append("<input type='button' class='btn btn-danger'   onclick='boxDisable($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "'   data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-Procedure_Detail_ID='" + row[30] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "'  data-Date='" + date1 + "' data-Position='Consider'   id='" + row[0] + "_C_" + row[1] + "_" + row[2] + "' value='Consider' />");
                                                         html.Append("</td>");
                                                     }
                                                     else
                                                     {
                                                         html.Append("<td >");
                                                         html.Append("<input type='button' class='btn btn-danger' onclick='boxDisable($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-Procedure_Detail_ID='" + row[30] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "'  data-Date='" + date1 + "' data-Position='Consider'   id='" + row[0] + "_C_" + row[1] + "_" + row[2] + "' value='Consider' />");
                                                         html.Append("</td>");
                                                     }
                                                 }
                                                 else
                                                 {
                                                     html.Append("<td >");
                                                     html.Append("<input type='button' class='btn btn-danger' onclick='boxDisable($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-Procedure_Detail_ID='" + row[30] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "'  data-Date='" + date1 + "' data-Position='Consider'   id='" + row[0] + "_C_" + row[1] + "_" + row[2] + "' value='Consider' />");
                                                     html.Append("</td>");
                                                 }
                                         }
                                         else*/
                                        if (column.ColumnName == "Requested")
                                        {

                                            string date1 = string.Empty;
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[15])))
                                            { date1 = Convert.ToDateTime(row[15]).ToString("MM/dd/yy").Replace('-', '/'); }

                                            StringBuilder notify = new StringBuilder();

                                            if (!string.IsNullOrEmpty(Convert.ToString(row[12])))
                                            { notify.AppendLine("muscle:" + row[12]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                            { notify.AppendLine("medication:" + row[11]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[23])))
                                            { notify.AppendLine("SubCode:" + row[23]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[27])))
                                            { notify.AppendLine("side:" + row[27]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[13])))
                                            { notify.AppendLine("level:" + row[13]); }
                                            if (row[10] != null)
                                                if (Convert.ToInt32(row[10]) != 0)
                                                {
                                                    html.Append("<td style='min-width:130px;'> ");
                                                    html.Append("<input type='text' class=\"form-control\"  onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "'  data-toggle='tooltip' title='" + notify + "'  data-Sides='" + row[27] + "' data-Procedure_Detail_ID='" + row[30] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "'data-ReqPos='" + row[19] + "' data-Bodyid='" + row[7] + "' data-Position='Request' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_R_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                    html.Append("</td>");
                                                }
                                                else
                                                {
                                                    html.Append("<td style='min-width:130px;'>");
                                                    html.Append("<input type='text' class=\"form-control\" onclick='Popup($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "'  data-toggle='tooltip' title='" + notify + "'  data-Sides='" + row[27] + "' data-Procedure_Detail_ID='" + row[30] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Request' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_R_" + row[1] + "_" + row[2] + "' value='' />");
                                                    html.Append("</td>");
                                                }
                                        }
                                        else if (column.ColumnName == "Scheduled")
                                        {

                                            string date1 = string.Empty;
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[16])))
                                            { date1 = Convert.ToDateTime(row[16]).ToString("MM/dd/yy").Replace('-', '/'); }


                                            StringBuilder notify = new StringBuilder();
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[12])))
                                            { notify.AppendLine("muscle:" + row[12]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                            { notify.AppendLine("medication:" + row[11]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[23])))
                                            { notify.AppendLine("SubCode:" + row[23]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[27])))
                                            { notify.AppendLine("side:" + row[27]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[13])))
                                            { notify.AppendLine("level:" + row[13]); }

                                            if (row[10] != null)
                                            {
                                                if (Convert.ToInt32(row[10]) != 0)
                                                {
                                                    html.Append("<td style='min-width:130px;'>");
                                                    html.Append("<input type='text' class='ProcText form-control' onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "'data-ReqPos='" + row[20] + "'data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Schedule' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "' data-MCode='" + row[1] + "'   id='" + row[0] + "_S_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                    html.Append("</td>");
                                                }
                                                else
                                                {
                                                    html.Append("<td style='min-width:130px;' >");
                                                    html.Append("<input type='text' class='ProcText form-control' onclick='Popup($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Schedule' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "' data-MCode='" + row[1] + "'   id='" + row[0] + "_S_" + row[1] + "_" + row[2] + "' value='' />");
                                                    html.Append("</td>");
                                                }
                                            }

                                        }
                                        else if (column.ColumnName == "Executed")
                                        {

                                            string date1 = string.Empty;
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[17])))
                                            { date1 = Convert.ToDateTime(row[17]).ToString("MM/dd/yy").Replace('-', '/'); }



                                            StringBuilder notify = new StringBuilder();
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[12])))
                                            { notify.AppendLine("muscle:" + row[12]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                            { notify.AppendLine("medication:" + row[11]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[23])))
                                            { notify.AppendLine("SubCode:" + row[23]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[27])))
                                            { notify.AppendLine("side:" + row[27]); }
                                            if (!string.IsNullOrEmpty(Convert.ToString(row[13])))
                                            { notify.AppendLine("level:" + row[13]); }

                                            if (row[10] != null)
                                                if (Convert.ToInt32(row[10]) != 0)
                                                {
                                                    html.Append("<td style='min-width:130px;'>");
                                                    html.Append("<input type='text' class='form-control dateval' onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "'data-PID='" + row[0] + "'data-ReqPos='" + row[22] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Execute' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "' data-Medication='" + row[9] + "' data-SignPath='" + row[33] + "'    data-Date='" + date1 + "' data-MCode='" + row[1] + "'   class='ProcText' id='" + row[0] + "_E_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                    html.Append("</td>");
                                                }
                                                else
                                                {
                                                    html.Append("<td style='min-width:130px;'>");
                                                    html.Append("<input type='text' class='form-control' onclick='Popup($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-PID='" + row[0] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Execute' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "' data-Medication='" + row[9] + "'  data-Date='" + date1 + "' data-CF='" + row[34] + "' data-MCode='" + row[1] + "'     class='ProcText' id='" + row[0] + "_E_" + row[1] + "_" + row[2] + "' value='' />");
                                                    html.Append("</td>");
                                                }
                                        }
                                        /* else if (column.ColumnName == "Followup")
                                         {

                                             string date1 = string.Empty;
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[18])))
                                             { date1 = Convert.ToDateTime(row[18]).ToString("MM/dd/yyyy").Replace('-', '/'); }

                                             StringBuilder notify = new StringBuilder();
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[12])))
                                             { notify.AppendLine("muscle:" + row[12]); }
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                             { notify.AppendLine("medication:" + row[11]); }
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[23])))
                                             { notify.AppendLine("SubCode:" + row[23]); }
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[27])))
                                             { notify.AppendLine("side:" + row[27]); }
                                             if (!string.IsNullOrEmpty(Convert.ToString(row[13])))
                                             { notify.AppendLine("level:" + row[13]); }

                                             if (row[10] != null)
                                                 if (Convert.ToInt32(row[10]) != 0)
                                                 {
                                                     html.Append("<td style='background:grey'>");
                                                     html.Append("<input style='background:grey' type='text' class='form-control' onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-PID='" + row[0] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "'data-ReqPos='" + row[21] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Follow Up' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_F_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                     html.Append("</td>");
                                                 }
                                                 else
                                                 {
                                                     html.Append("<td style='background:grey'>");
                                                     html.Append("<input style='background:grey' type='text' class='form-control' onclick='Popup($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-PID='" + row[0] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Follow Up' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_F_" + row[1] + "_" + row[2] + "' value='' />");
                                                     html.Append("</td>");
                                                 }
                                         }*/
                                        else if (column.ColumnName == "count")
                                        {
                                            html.Append("<td style='min-width:130px;'>");
                                            html.Append("<input type='button' class='btn btn-warning btn-sm' style='margin-left:25px' onclick='CountPopup($(this));' data-Div='" + ii + "_counttable' data-Procedure_Detail_ID='" + row[30] + "' data-PatientIEID='" + row[24] + "' data-PID='" + row[0] + "' value='" + row[26] + "'  />");
                                            html.Append("</td>");
                                        }
                                        else if (column.ColumnName == "MCODE")
                                        {
                                            html.Append("<td   data-bs-toggle='tooltip' data-bs-placement='top'  title='" + row["mcode_desc"] + "' style='text-align:center; background-color:#3de33d;min-width:130px;'>");
                                            html.Append(row[column.ColumnName]);
                                            html.Append("</td>");
                                        }
                                    }
                                }
                                html.Append("</tr>");
                            }
                            html.Append("</tbody></table></div><div id='" + ii + "_counttable' style='padding-left: 1%' ></div></div></div></div>");

                            string body = html.ToString();

                            //Append the HTML string to Placeholder.
                            //PlaceHolder1.Controls.Add(new Literal { Text = html.ToString() });

                            //Page.ClientScript.RegisterStartupScript(this.GetType(), ii, "tableTransform($('#" + ii + "_tbl'));", true);

                        }
                        else
                        {

                            html.Append("<table border = '1' id='" + ii + "_tbl'>");
                            html.Append("</table>");
                            //Append the HTML string to Placeholder.
                            // PlaceHolder1.Controls.Add(new Literal { Text = html.ToString() });
                        }
                    }



                }

                ViewBag.content = html.ToString();
                return PartialView("_POC");
            }
            catch (Exception ex)
            {
                return View();
            }


        }

        public string GetMuscleFromDB(int ProcedureId)
        {
            string test = _pocService.GetMuscle(ProcedureId);

            DataTable dt = new DataTable();
            string[] lines = test.Split('\n');
            dt.Columns.Add("Muscle");
            for (int i = 0; i < lines.Length; i++)
            {
                DataRow row = dt.NewRow();
                row[0] = lines[i];
                dt.Rows.Add(row);
            }
            string json = JsonConvert.SerializeObject(dt);
            return json;
        }

        public string GetSubCodeFromDB(int ProcedureId)
        {
            string test = _pocService.GetSubCode(ProcedureId);

            DataTable dt = new DataTable();
            string[] lines = test.Split('\n');
            dt.Columns.Add("SubCode");
            for (int i = 0; i < lines.Length; i++)
            {
                DataRow row = dt.NewRow();
                row[0] = lines[i];
                dt.Rows.Add(row);
            }
            string json = JsonConvert.SerializeObject(dt);
            return json;
        }

        public string GetMedicationFromDB(int ProcedureId)
        {
            string test = _pocService.GetMedication(ProcedureId);

            DataTable dt = new DataTable();
            string[] lines = test.Split('\n');
            dt.Columns.Add("Medicaton");
            for (int i = 0; i < lines.Length; i++)
            {
                DataRow row = dt.NewRow();
                row[0] = lines[i];
                dt.Rows.Add(row);
            }
            string json = JsonConvert.SerializeObject(dt);
            return json;
        }

        [HttpPost]
        public JsonResult SavePOC(ProcedureDetailsVMNew model)
        {
            string result = "0";

            string count = "", bodyPartSide = "";

            if (!string.IsNullOrEmpty(model.Position))
                bodyPartSide = model.Position + " " + model.BodyPart;
            else
                bodyPartSide = model.BodyPart;

            try
            {
                var data = new ProcedureDetailsVM();

                data.Backup_Line = model.Backup_Line;
                data.BodyPart = model.BodyPart;
                data.BodypartSide = bodyPartSide;
                data.CreatedBy = "";
                data.CT_AUTH_Date = Common.ConvertDate(model.CT_AUTH_Date);
                data.CT_Note = model.CT_Note;
                data.CT_Report_Status = model.CT_Report_Status;
                data.CT_ReSche_Date = Common.ConvertDate(model.CT_ReSche_Date);
                data.Ins_Note = model.Ins_Note;
                data.Ins_Ver_Status = Convert.ToBoolean(model.Ins_Ver_Status);
                data.IsConsidered = model.IsConsidered == "0" ? false : true;
                data.IsFromNew = Convert.ToInt16(model.IsFromNew);
                data.IsVaccinated = Convert.ToBoolean(model.IsVaccinated);
                data.Level = model.Level;
                data.MC_Date = Common.ConvertDate(model.MC_Date);
                data.MC_Note = model.MC_Note;
                data.MC_Report_Status = model.MC_Report_Status;
                data.MC_ReSche_Date = Common.ConvertDate(model.MC_ReSche_Date);
                data.MC_Type = model.MC_Type;
                data.Medication = model.Medication;
                data.Muscle = model.Muscle;
                data.ProcedureMasterID = string.IsNullOrEmpty(model.ProcedureMasterID) ? null : Convert.ToInt32(model.ProcedureMasterID);
                data.PatientFuID = string.IsNullOrEmpty(model.PatientFuID) ? null : Convert.ToInt32(model.PatientFuID);
                data.PatientIEID = string.IsNullOrEmpty(model.PatientIEID) ? null : Convert.ToInt32(model.PatientIEID);
                data.PatientProceduresID = string.IsNullOrEmpty(model.PatientProcedureID) ? null : Convert.ToInt32(model.PatientProcedureID);
                data.ProcedureDetailID = string.IsNullOrEmpty(model.ProcedureDetailID) ? null : Convert.ToInt32(model.ProcedureDetailID);
                data.ProcedureID = string.IsNullOrEmpty(model.ProcedureID) ? null : Convert.ToInt32(model.ProcedureID);
                data.Side = model.Side;
                data.SignPath = "";
                data.Subcode = model.SubProcedureID;
                data.Vac_Note = model.Vac_Note;
                data.Vac_Status = model.Vac_Status;


                switch (model.req)
                {
                    case "Request":
                        DateTime? Requested = Common.ConvertDate(model.pocdate);
                        data.Req_Pos = model.Position;
                        data.Requested = Requested;
                        result = _pocService.SaveProcedureDetails(data);
                        // count = _bl.savePatientProcedureDetailNew(ProcedureDetailID, ProcedureMasterID, _patientIEID, _patientFUID, BodyPartID, ProcedureID, null, Requested, null, null, null, BodyPart, Medication, Muscle, Level, Req_Pos, Sch_Pos, Exe_Pos, FU_Pos, 0, IsFromNew, PatientProcedureID, req, IsConsidered, Side, SubProcedureID, null, bodyPartSide, MC_Type, MC_Date, MC_Report_Status, MC_Note, MC_ReSche_Date, CT_AUTH_Date, CT_Report_Status, CT_Note, CT_ReSche_Date, Ins_Ver_Status, Backup_Line, Ins_Note, IsVaccinated, Vac_Status, Vac_Note);
                        break;
                    case "Schedule":
                        DateTime? Scheduled = Common.ConvertDate(model.pocdate);
                        data.Sch_Pos = model.Position;
                        data.Scheduled = Scheduled;
                        result = _pocService.SaveProcedureDetails(data);
                        //count = _bl.savePatientProcedureDetailNew(ProcedureDetailID, ProcedureMasterID, _patientIEID, _patientFUID, BodyPartID, ProcedureID, null, null, Scheduled, null, null, BodyPart, Medication, Muscle, Level, Req_Pos, Sch_Pos, Exe_Pos, FU_Pos, 0, IsFromNew, PatientProcedureID, req, IsConsidered, Side, SubProcedureID, null, bodyPartSide, MC_Type, MC_Date, MC_Report_Status, MC_Note, MC_ReSche_Date, CT_AUTH_Date, CT_Report_Status, CT_Note, CT_ReSche_Date, Ins_Ver_Status, Backup_Line, Ins_Note, IsVaccinated, Vac_Status, Vac_Note);
                        break;

                    case "Execute":
                        DateTime? Executed = Common.ConvertDate(model.pocdate);
                        data.Exe_Pos = model.Position;
                        data.Executed = Executed;
                        result = _pocService.SaveProcedureDetails(data);
                        break;

                }

                int ie_id = Convert.ToInt32(model.PatientIEID);
                var pocData = this.UpdatePOCPlan(ie_id);

                if (pocData != null)
                {

                    //var obj = _ieService.GetOnePage1(ie_id);
                    //string strCCdesc = "", strPEdesc = "", strAdesc = "";
                    //if (obj != null)
                    //{
                    //    if (!string.IsNullOrEmpty(pocData.strCCDesc))
                    //        strCCdesc = obj.cc + "<p>" + pocData.strCCDesc + "</p>";
                    //    else
                    //        strCCdesc = obj.cc;

                    //    if (!string.IsNullOrEmpty(pocData.strPEDesc))
                    //        strPEdesc = obj.pe + "<p>" + pocData.strPEDesc + "</p>";
                    //    else
                    //        strPEdesc = obj.pe;

                    //    if (!string.IsNullOrEmpty(pocData.strADesc))
                    //        strAdesc = obj.assessment + "<p>" + pocData.strADesc + "</p>";
                    //    else
                    //        strAdesc = obj.assessment;
                    //}
                    //pocData.strADesc = strAdesc;
                    //pocData.strCCDesc = strCCdesc;
                    //pocData.strPEDesc = strPEdesc;
                    _ieService.UpdatePage1Plan(ie_id, pocData.strPoc);
                }
                return Json(pocData);
            }
            catch (Exception ex)
            {

            }

            //_pocService.SaveProcedureDetails(model);
            return Json(result);
        }

        [HttpPost]
        public string GetProcDetail(int ProcedureDetailID)
        {
            var data = _pocService.GetProcedureDetails(ProcedureDetailID);
            string json = JsonConvert.SerializeObject(data);
            return json;
        }

        [HttpPost]
        public string GetProcedureCountDetail(int ProcedureID, int PatientIEID)
        {
            try
            {
                var x = _pocService.GetProcedureCountDetails(ProcedureID, PatientIEID);
                StringBuilder html = new StringBuilder();
                if (x.Rows.Count > 0)
                {
                    // x = FlipDataTable(x);

                    //Table start.
                    html.Append("<table class='table table-hover table-bordered tblpoc'  border = '1' id='countbl'>");
                    html.Append("<thead>");
                    //Building the Header row.
                    html.Append("<tr>");
                    foreach (DataColumn column in x.Columns)
                    {

                        if (column.ColumnName != "procedureID" && (column.ColumnName != "MCODE") &&
                        (column.ColumnName != "INhouseProc") && (column.ColumnName != "Position") &&
                        (column.ColumnName != "HasLevel") && (column.ColumnName != "HasMuscle") &&
                        (column.ColumnName != "HasMedication") && (column.ColumnName != "PatientProceduresID")
                        && (column.ColumnName != "Medication") && (column.ColumnName != "Muscle") && (column.ColumnName != "Level")
                        && (column.ColumnName != "Req_Pos") && (column.ColumnName != "Sch_Pos") && (column.ColumnName != "FU_Pos")
                        && (column.ColumnName != "Exe_Pos") && (column.ColumnName != "PatientIEID") && (column.ColumnName != "PatientFuID")
                        && (column.ColumnName != "SubProcedureID") && (column.ColumnName != "HasSubCode")
                        && (column.ColumnName != "HasSides") && (column.ColumnName != "sides") && (column.ColumnName != "ProcedureDetailID")
                        && (column.ColumnName != "LevelsDefault") && (column.ColumnName != "SidesDefault"))
                        {
                            if (column.ColumnName == "BodyPart")
                            {
                                html.Append("<th style='height: 35px; background-color:yellow'>");
                                html.Append("");
                                html.Append("</th>");
                            }
                            else
                            {
                                if (column.ColumnName == "Consider" || column.ColumnName == "Followup")
                                {
                                    html.Append("<th style='display:none'>");
                                    html.Append(column.ColumnName);
                                    html.Append("</th>");
                                }
                                else if (column.ColumnName == "Followup")
                                {
                                    html.Append("<th style='height: 35px;background:grey'>");
                                    html.Append(column.ColumnName);
                                    html.Append("</th>");
                                }
                                else
                                {
                                    html.Append("<th  style='height: 35px;'>");
                                    html.Append(column.ColumnName);
                                    html.Append("</th>");
                                }

                            }
                        }
                        //}
                    }
                    html.Append("</tr>");
                    html.Append("</thead>");
                    html.Append("<tbody>");

                    foreach (DataRow row in x.Rows)
                    {
                        html.Append("<tr>");
                        foreach (DataColumn column in x.Columns)
                        {
                            if ((column.ColumnName != "PatientProceduresID") && (column.ColumnName != "BodyPart") && (column.ColumnName != "PatientFuID") && (column.ColumnName != "PatientIEID") && (column.ColumnName != "INhouseProc"))
                            {
                                //|| column.ColumnName == "Consider" || )
                                if (column.ColumnName == "Consider")
                                {
                                    if (row[8] != null)
                                        if (Convert.ToInt32(row[8]) != 0)
                                        {
                                            if (row[12] != DBNull.Value)
                                            {
                                                html.Append("<td style='display:none'>");
                                                html.Append("<div class='checkbox'><input type='checkbox' checked  onclick='boxDisable($(this));' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "' data-Body='" + row[6] + "' data-Date='" + row[12] + "' data-Position='Consider'   id='" + row[0] + "_C_" + row[1] + "_" + row[2] + "' value='Consider' /></div>");
                                                html.Append("</td>");
                                            }
                                            else
                                            {
                                                html.Append("<td style='display:none'>");
                                                html.Append("<div class='checkbox'><input type='checkbox' onclick='boxDisable($(this));' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "' data-Body='" + row[6] + "' data-Date='" + row[12] + "' data-Position='Consider'   id='" + row[0] + "_C_" + row[1] + "_" + row[2] + "' value='Consider' /></div>");
                                                html.Append("</td>");
                                            }
                                        }
                                        else
                                        {
                                            html.Append("<td style='display:none'>");
                                            html.Append("<div class='checkbox'><input type='checkbox' onclick='boxDisable($(this));' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "' data-Body='" + row[6] + "' data-Date='" + row[12] + "' data-Position='Consider'   id='" + row[0] + "_C_" + row[1] + "_" + row[2] + "' value='Consider' /></div>");
                                            html.Append("</td>");
                                        }
                                }
                                else if (column.ColumnName == "Requested")
                                {
                                    string date1 = string.Empty;
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[13])))
                                    { date1 = Convert.ToDateTime(row[13]).ToString("MM/dd/yyyy").Replace('-', '/'); }


                                    StringBuilder notify = new StringBuilder();
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[10])))
                                    { notify.AppendLine("muscle:" + row[10]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[9])))
                                    { notify.AppendLine("medication:" + row[9]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[24])))
                                    { notify.AppendLine("SubCode:" + row[24]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[26])))
                                    { notify.AppendLine("side:" + row[26]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                    { notify.AppendLine("level:" + row[11]); }

                                    if (row[8] != null)
                                        if (Convert.ToInt32(row[8]) != 0)
                                        {
                                            html.Append("<td >");
                                            html.Append("<input type='text'  class=\"form-control\" data-toggle='tooltip' title='" + notify + "' onclick='PopupNE($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-PPID='" + row[8] + "' data-Procedure_Detail_ID='" + row[27] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "' data-Body='" + row[6] + "'data-ReqPos='" + row[17] + "' data-Position='Request' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_R_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                            html.Append("</td>");
                                        }
                                        else
                                        {
                                            html.Append("<td >");
                                            html.Append("<input type='text'  class=\"form-control\" onclick='Popup($(this));'  data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-toggle='tooltip' title='" + notify + "'  data-PPID='" + row[8] + "'  data-Procedure_Detail_ID='" + row[27] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "' data-Body='" + row[6] + "' data-Position='Request' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_R_" + row[1] + "_" + row[2] + "' value='' />");
                                            html.Append("</td>");
                                        }
                                }
                                else if (column.ColumnName == "Scheduled")
                                {

                                    string date1 = string.Empty;
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[14])))
                                    { date1 = Convert.ToDateTime(row[14]).ToString("MM/dd/yyyy").Replace('-', '/'); }

                                    StringBuilder notify = new StringBuilder();
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[10])))
                                    { notify.AppendLine("muscle:" + row[10]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[9])))
                                    { notify.AppendLine("medication:" + row[9]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[24])))
                                    { notify.AppendLine("SubCode:" + row[24]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[26])))
                                    { notify.AppendLine("side:" + row[26]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                    { notify.AppendLine("level:" + row[11]); }
                                    if (row[8] != null)
                                    {
                                        if (Convert.ToInt32(row[8]) != 0)
                                        {
                                            html.Append("<td >");
                                            html.Append("<input type='text'  class=\"form-control\" class='ProcText' data-toggle='tooltip' title='" + notify + "'  onclick='PopupNE($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "'data-ReqPos='" + row[18] + "' data-Body='" + row[6] + "' data-Position='Schedule' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'   id='" + row[0] + "_S_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                            html.Append("</td>");
                                        }
                                        else
                                        {
                                            html.Append("<td >");
                                            html.Append("<input type='text'  class=\"form-control\" class='ProcText' data-toggle='tooltip' title='" + notify + "' onclick='Popup($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-PID='" + row[0] + "' data-Body='" + row[6] + "' data-Position='Schedule' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'   id='" + row[0] + "_S_" + row[1] + "_" + row[2] + "' value='' />");
                                            html.Append("</td>");
                                        }
                                    }

                                }
                                else if (column.ColumnName == "Executed")
                                {
                                    string date1 = string.Empty;
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[15])))
                                    { date1 = Convert.ToDateTime(row[15]).ToString("MM/dd/yyyy").Replace('-', '/'); }

                                    StringBuilder notify = new StringBuilder();
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[10])))
                                    { notify.AppendLine("muscle:" + row[10]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[9])))
                                    { notify.AppendLine("medication:" + row[9]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[24])))
                                    { notify.AppendLine("SubCode:" + row[24]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[26])))
                                    { notify.AppendLine("side:" + row[26]); }
                                    if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                    { notify.AppendLine("level:" + row[11]); }

                                    if (row[8] != null)
                                        if (Convert.ToInt32(row[8]) != 0)
                                        {
                                            html.Append("<td >");
                                            html.Append("<input type='text'  class=\"form-control\" onclick='PopupNE($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "'data-PID='" + row[0] + "'data-ReqPos='" + row[20] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-Body='" + row[6] + "' data-Position='Execute' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "' data-Medication='" + row[9] + "'  data-Date='" + date1 + "' data-MCode='" + row[1] + "'   class='ProcText' id='" + row[0] + "_E_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                            html.Append("</td>");
                                        }
                                        else
                                        {
                                            html.Append("<td >");
                                            html.Append("<input type='text'  class=\"form-control\" onclick='Popup($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-PID='" + row[0] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-Body='" + row[6] + "' data-Position='Execute' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "' data-Medication='" + row[9] + "'  data-Date='" + date1 + "' data-MCode='" + row[1] + "'   class='ProcText' id='" + row[0] + "_E_" + row[1] + "_" + row[2] + "' value='' />");
                                            html.Append("</td>");
                                        }
                                }
                                //else if (column.ColumnName == "Followup")
                                //{
                                //    string date1 = string.Empty;
                                //    if (!string.IsNullOrEmpty(Convert.ToString(row[16])))
                                //    { date1 = Convert.ToDateTime(row[16]).ToString("MM/dd/yyyy").Replace('-', '/'); }

                                //    StringBuilder notify = new StringBuilder();
                                //    if (!string.IsNullOrEmpty(Convert.ToString(row[10])))
                                //    { notify.AppendLine("muscle:" + row[10]); }
                                //    if (!string.IsNullOrEmpty(Convert.ToString(row[9])))
                                //    { notify.AppendLine("medication:" + row[9]); }
                                //    if (!string.IsNullOrEmpty(Convert.ToString(row[24])))
                                //    { notify.AppendLine("SubCode:" + row[24]); }
                                //    if (!string.IsNullOrEmpty(Convert.ToString(row[26])))
                                //    { notify.AppendLine("side:" + row[26]); }
                                //    if (!string.IsNullOrEmpty(Convert.ToString(row[11])))
                                //    { notify.AppendLine("level:" + row[11]); }

                                //    if (row[8] != null)
                                //        if (Convert.ToInt32(row[8]) != 0)
                                //        {
                                //            html.Append("<td style='background:grey'>");
                                //            html.Append("<input type='text' style='background:grey' onclick='PopupNE($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-PID='" + row[0] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "'data-ReqPos='" + row[19] + "' data-Body='" + row[6] + "' data-Position='Follow Up' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_F_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                //            html.Append("</td>");
                                //        }
                                //        else
                                //        {
                                //            html.Append("<td style='background:grey'>");
                                //            html.Append("<input type='text' style='background:grey' onclick='Popup($(this));' data-LevelsDefault='" + row[28] + "' data-SidesDefault='" + row[29] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[27] + "' data-PPID='" + row[8] + "' data-PatientIEID='" + row[21] + "' data-PatientFUID='" + row[22] + "' data-PID='" + row[0] + "' data-Level='" + row[11] + "' data-Medi='" + row[9] + "' data-Musc='" + row[10] + "' data-Body='" + row[6] + "' data-Position='Follow Up' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_F_" + row[1] + "_" + row[2] + "' value='' />");
                                //            html.Append("</td>");
                                //        }
                                //}
                                else if (column.ColumnName == "MCODE")
                                {
                                    html.Append("<td style='text-align:center; background-color:#3de33d;'>");
                                    html.Append(row[column.ColumnName]);
                                    html.Append("</td>");
                                }
                            }
                        }
                        html.Append("</tr>");
                    }
                    html.Append("</tbody>");
                    //Table end.
                    html.Append("</table>");

                }
                return html.ToString();
            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetProcedureCountDetail");
                return "";
            }
        }
        [HttpPost]
        public IActionResult Delete(int ProcedureDetailID, int IeId)
        {


            var data = _pocService.RemoveProcedureCountDetails(ProcedureDetailID);
            var pocData = this.UpdatePOCPlan(IeId);

            if (pocData != null)
            {
                var obj = _ieService.GetOnePage1(IeId);
                string strCCdesc = "", strPEdesc = "", strAdesc = "";
                if (obj != null)
                {
                    if (!string.IsNullOrEmpty(pocData.strCCDesc))
                        strCCdesc = obj.cc + "<p>" + pocData.strCCDesc + "</p>";
                    else
                        strCCdesc = obj.cc;

                    if (!string.IsNullOrEmpty(pocData.strPEDesc))
                        strPEdesc = obj.pe + "<p>" + pocData.strPEDesc + "</p>";
                    else
                        strPEdesc = obj.pe;

                    if (!string.IsNullOrEmpty(pocData.strADesc))
                        strAdesc = obj.assessment + "<p>" + pocData.strADesc + "</p>";
                    else
                        strAdesc = obj.assessment;
                }

                _ieService.UpdatePage1Plan(IeId, pocData.strPoc);

            }
            return Json(pocData.strPoc);
        }


        [HttpPost]
        public IActionResult DeleteIE(int ieId, int pId)
        {
            try
            {
                _ieService.Delete(ieId, pId);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "DeleteIE");
            }
            return Json(1);
        }


        #endregion

        #region POCSumarry
        [HttpPost]
        public IActionResult POCSummary(int patientIEId)
        {
            try
            {
                int? ieId = HttpContext.Session.GetInt32(SessionKeys.SessionIEId);

                var Data = _pocService.GetPOCSummary(patientIEId);

                return PartialView("_POCSummary", Data);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "DeleteIE");
                return View();
            }

        }

        [HttpPost]
        public IActionResult SurgeryPOCSummary(int patientIEId)
        {
            try
            {
                int? ieId = HttpContext.Session.GetInt32(SessionKeys.SessionIEId);

                var Data = _pocService.GetExecutedPOCIE(patientIEId);

                return PartialView("_POCSummary", Data);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SurgeryPOCSummary");
                return View();
            }
        }
        #endregion

        #region PrintIE

        public IActionResult IEPrint(int id)
        {
            try
            {
                string plan = "";
                ViewBag.IsHome = true;
                ViewBag.url = HttpContext.Request.GetEncodedUrl();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string body = string.Empty;
                string bodypart = "";
                string printbody = "<body style='font-size:12px;font-family:'Times New Roman', Times,serif'>";
                //using streamreader for reading my htmltemplate   

                var templateData = _printService.GetTemplate(cmpid, "IE");
                var gender = "";
                var sex = "";

                body = templateData.content;

                var patientData = _ieService.GetOnebyPatientId(id);

                if (patientData != null)
                {
                    sex = Common.GetGenderFromSex(patientData.gender);
                    gender = Common.GetMrMrsFromSex(patientData.gender);
                    //body = body.Replace("#patientname", gender + " " + patientData.fname + " " + patientData.mname + " " + patientData.lname);
                    body = body.Replace("#patientname", gender + " " + patientData.lname + ", " + patientData.fname + " " + patientData.mname);
                    body = body.Replace("#fn", patientData.fname);
                    body = body.Replace("#ln", patientData.lname);
                    body = body.Replace("#gender", gender);

                    body = body.Replace("#doi", Common.commonDate(patientData.doa));
                    body = body.Replace("#age", patientData.age == null ? "0" : patientData.age.Value.ToString());
                    body = body.Replace("#gender", gender);
                    body = body.Replace("#dos", Common.commonDate(patientData.doe, HttpContext.Session.GetString(SessionKeys.SessionDateFormat)));



                    // body = body.Replace("#CT", System.Enum.GetName(typeof(CaseType), Convert.ToInt32(patientData.compensation)));
                    body = body.Replace("#CT", patientData.compensation);
                    body = body.Replace("#casetype", patientData.compensation);
                }

                //header printing

                var locData = _locService.GetAll(" and id=" + patientData.location_id);

                if (locData != null && locData.Count > 0)
                {
                    if (locData[0].nameofpractice != null)
                        body = body.Replace("#drName", locData[0].nameofpractice.ToLower().Contains("dr") ? locData[0].nameofpractice : "Dr. " + locData[0].nameofpractice);
                    else
                        body = body.Replace("#drName", "");
                    body = body.Replace("#address", locData[0].address);
                    //body = body.Replace("#Address", locData[0].address);
                    body = body.Replace("#Address", locData[0].address + "<br/>" + locData[0].city + ", " + locData[0].state + " " + locData[0].zipcode);
                    //  body = body.Replace("#address", locData[0].address + "<br/>" + locData[0].city + ", " + locData[0].state + " " + locData[0].zipcode);
                    body = body.Replace("#loc", locData[0].location);
                    body = body.Replace("#Location", locData[0].location);
                    if (locData[0].nameofpractice != null)
                        body = body.Replace("#Nameofpractice", locData[0].nameofpractice.ToLower().Contains("dr") ? locData[0].nameofpractice : locData[0].nameofpractice);
                    else
                        body = body.Replace("#Nameofpractice", "");
                    body = body.Replace("#Phone", locData[0].telephone);
                    //body = body.Replace("#Location", locData[0].address + "<br/>" + locData[0].city + ", " + locData[0].state + " " + locData[0].zipcode);
                }

                //ADL printing

                var page2Data = _ieService.GetOnePage2(id);

                if (page2Data != null)
                {
                    body = body.Replace("#ADL", this.removePtag(page2Data.aod));
                    body = body.Replace("#Ros", this.removePtag(page2Data.ros));
                }
                else
                {
                    body.Replace("#ADL", "");
                    body = body.Replace("#Ros", "");
                }

                //GET POC 

                //POC printing

                var dataPOC = this.getPOC(id);

                //CC printing

                var page1Data = _ieService.GetOnePage1(id);
                if (page1Data != null)
                {

                    plan = page1Data.plan;
                    if (!string.IsNullOrEmpty(page1Data.bodypart))
                    {
                        bodypart = Common.ReplceCommowithAnd(page1Data.bodypart.ToLower());
                        body = body.Replace("#PC", string.IsNullOrEmpty(bodypart) ? "" : bodypart.Replace(",", ", ") + " pain.");

                    }
                    else
                    {
                        body = body.Replace("#PC", "");
                    }

                    string cc = "";
                    string pe = "";

                    cc = string.IsNullOrEmpty(page1Data.cc) ? "" : this.removePtag(page1Data.cc);
                    pe = string.IsNullOrEmpty(page1Data.pe) ? "" : page1Data.pe;

                    if (dataPOC != null)
                    {
                        cc = string.IsNullOrEmpty(dataPOC.strCCDesc) ? cc : cc + "<br/><br/>" + dataPOC.strCCDesc;
                        pe = string.IsNullOrEmpty(dataPOC.strPEDesc) ? pe : pe + "<br/><br/>" + dataPOC.strPEDesc;
                    }


                    body = body.Replace("#Reason", string.IsNullOrEmpty(page1Data.appt_reason) ? "" : this.removePtag(page1Data.appt_reason));
                    body = body.Replace("#CC", cc);
                    body = body.Replace("#PE", pe);

                    var history = string.IsNullOrEmpty(page1Data.history) ? "" : page1Data.history;


                    history = history.Replace("#dos", Common.commonDate(patientData.doe, HttpContext.Session.GetString(SessionKeys.SessionDateFormat)));
                    history = history.Replace("#doi", Common.commonDate(patientData.doa));
                    //  history.Replace("#patientname", gender + " " + patientData.fname + " " + patientData.mname + " " + patientData.lname);
                    history = history.Replace("#patientname", sex + " " + patientData.lname + " " + patientData.fname + " " + patientData.lname);
                    history = history.Replace("#accidenttype", patientData.accidentType);

                    body = body.Replace("#history", history);

                    body = body.Replace("#DD", string.IsNullOrEmpty(page1Data.dd) ? "" : page1Data.dd);
                    body = body.Replace("#WorkStatus", string.IsNullOrEmpty(page1Data.work_status) ? "" : page1Data.work_status);
                    body = body.Replace("#POPlan", string.IsNullOrEmpty(page1Data.plan) ? "" : page1Data.plan);
                    body = body.Replace("#fn", patientData.fname);
                    body = body.Replace("#ln", patientData.lname);
                    body = body.Replace("#gender", Common.GetMrMrsFromSex(patientData.gender));
                    body = body.Replace("#sex", Common.GetGenderFromSex(patientData.gender));





                    body = body.Replace("#PC", string.IsNullOrEmpty(bodypart) ? "" : bodypart.Replace(",", ", "));
                    body = body.Replace("#bodypart", string.IsNullOrEmpty(bodypart) ? "" : Common.FirstCharToUpper(bodypart).ToString().Replace(",", ", "));

                    string assessment = "";
                    if (!string.IsNullOrEmpty(page1Data.assessment))
                    {
                        if (bodypart != null)
                            assessment = page1Data.assessment.Replace("#PC", bodypart + " pain.");
                        else
                            assessment = page1Data.assessment.Replace("#PC", "");
                        assessment = assessment.Replace("#accidenttype", patientData.accidentType);
                    }



                    if (dataPOC != null)
                    {
                        assessment = string.IsNullOrEmpty(dataPOC.strADesc) ? assessment : assessment + "<br/><br/>" + dataPOC.strADesc;
                    }

                    body = body.Replace("#doi", Common.commonDate(patientData.doa, HttpContext.Session.GetString(SessionKeys.SessionDateFormat)));

                    body = body.Replace("#PastMedicalHistory", this.removePtag(page1Data.pmh));
                    body = body.Replace("#PastSurgicalHistory", this.removePtag(page1Data.psh));
                    body = body.Replace("#SocialHistory", this.removePtag(page1Data.social_history));
                    body = body.Replace("#Allergies", this.removePtag(page1Data.allergies));
                    body = body.Replace("#FamilyHistory", this.removePtag(page1Data.family_history));
                    body = body.Replace("#Vital", this.removePtag(page1Data.vital));
                    body = body.Replace("#Diagnoses", this.removePtag(assessment));
                    body = body.Replace("#Occupation", this.removePtag(page1Data.occupation));
                    body = body.Replace("#PastMedications", this.removePtag(page1Data.medication));
                    body = body.Replace("#DD", this.removePtag(page1Data.dd));
                    body = body.Replace("#WorkStatus", this.removePtag(page1Data.work_status));
                    body = body.Replace("#IR", this.removePtag(page1Data.impairment_rating));
                    body = body.Replace("#age", patientData.age == null ? "0" : patientData.age.Value.ToString());
                    body = body.Replace("#doi", Common.commonDate(patientData.doa, HttpContext.Session.GetString(SessionKeys.SessionDateFormat)));


                }
                else
                {
                    body = body.Replace("#WorkStatus", "");
                    body = body.Replace("#DD", "");
                    body = body.Replace("#PastMedications", "");
                    body = body.Replace("#Occupation", "");
                    body = body.Replace("#Diagnoses", "");
                    body = body.Replace("#Vital", "");
                    body = body.Replace("#Allergies", "");
                    body = body.Replace("#FamilyHistory", "");
                    body = body.Replace("#PastSurgicalHistory", "");
                    body = body.Replace("#SocialHistory", "");
                    body = body.Replace("#PastMedicalHistory", "");
                }

                //last line 
                string lastline = "";
                if (page1Data != null)
                    lastline = "It is my opinion that the injuries and symptoms " + gender + " " + patientData.fname + " " + patientData.mname + " " + patientData.lname + " sustained to " + page1Data.bodypart + " are causally related to the incident that occurred on " + Common.commonDate(patientData.doa) + " as described by the patient.";
                body = body.Replace("#lastline", lastline);

                //NE printing

                var pageNEData = _ieService.GetOneNE(id);
                if (pageNEData != null)
                {

                    body = body.Replace("#Sen_Exm", this.removePtag(pageNEData.sensory));
                    body = body.Replace("#SE", this.removePtag(pageNEData.sensory));
                    body = body.Replace("#MMST", this.removePtag(pageNEData.manual_muscle_strength_testing));
                    body = body.Replace("#NE", this.removePtag(pageNEData.neurological_exam));
                    body = body.Replace("#DTR", this.removePtag(pageNEData.other_content));

                }
                else
                {
                    body = body.Replace("#SE", "");
                    body = body.Replace("#DTR", "");
                    body = body.Replace("#NE", "");
                    body = body.Replace("#MMST", "");
                    body = body.Replace("#Sen_Exm", "");
                }

                //NE printing

                var page3Data = _ieService.GetOnePage3(id);
                if (page3Data != null)
                {
                    body = body.Replace("#Gait", this.removePtag(page3Data.gait));
                    body = body.Replace("#Care", this.removePtag(page3Data.care));
                    body = body.Replace("#Procedures", this.removePtag(page3Data.universal));
                    body = body.Replace("#Goal", this.removePtag(page3Data.goal));
                    body = body.Replace("#DM", this.removePtag(page3Data.discharge_medications));
                }
                else
                {
                    body = body.Replace("#Goal", "");
                    body = body.Replace("#Procedures", "");
                    body = body.Replace("#Care", "");
                    body = body.Replace("#Gait", "");
                    body = body.Replace("#DM", "");
                }

                //Treatment printing

                var pageOtherData = _ieService.GetOneOtherPage(id);
                if (pageOtherData != null)
                {

                    body = body.Replace("#Treatment", this.removePtag(pageOtherData.treatment_details));
                    body = body.Replace("#heshe", Common.GethesheFromSex(patientData.gender));
                    body = body.Replace("#hisher", Common.GethisherFromSex(patientData.gender));
                    body = body.Replace("#note1", this.removePtag(pageOtherData.note1));
                    body = body.Replace("#note2", this.removePtag(pageOtherData.note2));
                    body = body.Replace("#note3", this.removePtag(pageOtherData.note3));


                    string fup_duration = "";
                    if (!string.IsNullOrEmpty(pageOtherData.followup_duration))
                        fup_duration = pageOtherData.followup_duration;
                    else if (pageOtherData.followup_date.HasValue)
                        fup_duration = Common.commonDate(pageOtherData.followup_date);


                    body = body.Replace("#FollowUp", this.removePtag(fup_duration));
                    body = body.Replace("#fup", this.removePtag(fup_duration));


                }
                else
                {
                    body = body.Replace("#fup", "");
                    body = body.Replace("#FollowUp", "");
                    body = body.Replace("#Treatment", "");
                }




                if (string.IsNullOrEmpty(plan))
                    body = body.Replace("#Plan", this.removePtag(dataPOC.strPoc));
                else
                    body = body.Replace("#Plan", this.removePtag(plan));
                body = body.Replace("#ReflexExam", "");
                string injectionHtml = dataPOC.strInjectionDesc;
                //string injectionHtml = "<h2>Injection Test</h2>";

                //string SessionDiffPage = HttpContext.Session.GetString(SessionKeys.SessionPageBreak);
                SessionDiffDoc = HttpContext.Session.GetString(SessionKeys.SessionInjectionAsSeparateBlock);

                if (SessionDiffDoc != "true")
                {

                    if (HttpContext.Session.GetString(SessionKeys.SessionPageBreak) == "true")
                    {
                        // Create HTML with a page break before the injection section
                        string pageBreakHtml = "<div style='page-break-before: always;'>";
                        pageBreakHtml += injectionHtml;
                        pageBreakHtml += "</div>";

                        body = body.Replace("#injection", pageBreakHtml);
                    }
                    else
                    {
                        body = body.Replace("#injection", injectionHtml);
                    }
                }
                else
                {
                    body = body.Replace("#injection", injectionHtml);
                }

                body = body.Replace("#location", patientData.location);
                body = body.Replace("#dob", Common.commonDate(patientData.dob));
                body = body.Replace("#name", gender + " " + patientData.fname + " " + patientData.mname + " " + patientData.lname);
                body = body.Replace("#sex", Common.GetGenderFromSex(patientData.gender));
                body = body.Replace("#gender", Common.GetMrMrsFromSex(patientData.gender));

                var strDiagnostic = this.getDiagnostic(id);

                if (cmpid != "4")
                {
                    if (string.IsNullOrEmpty(strDiagnostic))
                    {
                        if (HttpContext.Session.GetString(SessionKeys.SessionIsDaignosis) == "true")
                        {
                            strDiagnostic = HttpContext.Session.GetString(SessionKeys.SessionDaignosisNotFoundStatment);
                        }
                    }
                    else
                    {
                        if (HttpContext.Session.GetString(SessionKeys.SessionIsDaignosis) == "true")
                        {
                            strDiagnostic = strDiagnostic + "<br/><br/>" + HttpContext.Session.GetString(SessionKeys.SessionDaignosisFoundStatment); ;
                        }
                    }
                }

                body = body.Replace("#Diagnostic", this.removePtag(strDiagnostic));



                var data = _ieService.GetOnePage3(id);

                if (data != null)
                {
                    body = body.Replace("#PrescribedMedications", this.removePtag(data.discharge_medications));
                }
                else
                {
                    body = body.Replace("#PrescribedMedications", "");
                }

                body = body.Replace("#LastNote", "");

                ViewBag.ieId = patientData.id;
                ViewBag.locId = patientData.location_id;
                ViewBag.content = body;

                string signName = "";
                int signUserId = 0;

                int? providorId = HttpContext.Session.GetInt32(SessionKeys.SessionSelectedProviderId);

                if (patientData.provider_id != null)
                {
                    signUserId = patientData.provider_id.Value;
                }
                else if (providorId != null)
                {
                    signUserId = providorId.Value;
                }

                if (signUserId > 0)
                {
                    tbl_users _user = new tbl_users()
                    {
                        Id = signUserId
                    };
                    var userData = _userService.GetOne(_user);
                    signName = userData.signature;

                    if (!string.IsNullOrEmpty(signName))
                    {
                        string signatureUrl = $"/Uploads/Sign/" + cmpid + "/" + signName;
                        //string signatureUrl = "https://paintrax.com/newversionlive/Uploads/Sign/" + cmpid + "/" + signName;
                        string base64Image = ImageToBase64(Environment.WebRootPath + signatureUrl);
                        body = body.Replace("#Sign", $" <img src='data:image/jpg;base64,{base64Image}' alt='My Image' />");
                        // body = body.Replace("#Sign", $"<img crossorigin='anonymous|use-credentials' src='{signatureUrl}' alt='Patient Signature' />");
                    }
                    else
                        body = body.Replace("#Sign", "");

                    body = body.Replace("#ProviderName", userData.providername);
                    body = body.Replace("#AssProviderName", userData.assistant_providername);
                }
                else
                    body = body.Replace("#Sign", "");


                if (SessionDiffDoc == "true")
                {
                    body += "<br><br><!--Diff Doc-->";
                    body += injectionHtml;
                }


                ViewBag.content = body;

            }
            catch (Exception ex)
            {
                SaveLog(ex, "IEPrint");
            }
            return View();
        }

        [HttpPost]
        public IActionResult DownloadPDF(String htmlContent, string ieId)
        {
            //if (!string.IsNullOrEmpty(htmlContent))
            //{
            //    IronPdf.HtmlToPdf Renderer = new IronPdf.HtmlToPdf();
            //    Renderer.RenderHtmlAsPdf(htmlContent).SaveAs("html-string.pdf");
            //    return Ok();
            //}
            //else
            //{
            //    var stream = new FileStream(Path.Combine(_hostingEnvironment.ContentRootPath, "html-string.pdf"), FileMode.Open);
            //    return new FileStreamResult(stream, "application/pdf");
            //}

            return View();

        }

        #endregion

        #region Image to Base64
        public string ImageToBase64(string imagePath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }
        #endregion
        #region private method
        //public MemoryStream ConvertHtmlToWord(string htmlContent)
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    using (WordprocessingDocument doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
        //    {
        //        MainDocumentPart mainPart = doc.AddMainDocumentPart();
        //        mainPart.Document = new Document();
        //        Body body = mainPart.Document.AppendChild(new Body());
        //        HtmlConverter converter = new HtmlConverter(mainPart);
        //        var generatedBody = converter.Parse(htmlContent);
        //        body.Append(generatedBody);
        //    }
        //    return memoryStream;
        //}

        public MemoryStream ConvertHtmlToWord(string htmlContent)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                // Create the MainDocumentPart
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Define the font and size
                RunProperties runProperties = new RunProperties();
                RunFonts runFonts = new RunFonts() { Ascii = "Times New Roman" };
                FontSize fontSize = new FontSize() { Val = "24" }; // Font size 12 (in half-point format)

                // Apply the font settings to the RunProperties
                runProperties.Append(runFonts);
                runProperties.Append(fontSize);

                // Parse the HTML content and append it to the document
                HtmlConverter converter = new HtmlConverter(mainPart);
                IList<OpenXmlCompositeElement> generatedBody = converter.Parse(htmlContent);

                // Iterate over the parsed elements and apply RunProperties
                foreach (var element in generatedBody)
                {
                    foreach (var run in element.Descendants<Run>()) // Find all Run elements in the element
                    {
                        run.PrependChild(runProperties.CloneNode(true)); // Apply the font properties to each Run
                    }

                    // Append each element to the body
                    body.Append(element.CloneNode(true));
                }

                // Save the changes
                mainPart.Document.Save();
            }
            return memoryStream;
        }



        [HttpPost]
        public IActionResult DownloadWord(string htmlContent, int ieId, int id)
        {
            htmlContent = htmlContent.Replace("<p>&nbsp;</p>", "");

            string filePath = "", docName = "", patientName = "", injDocName = "", dos = "";
            string[] splitContent;
            string injHtmlContent = "";
            if (SessionDiffDoc == "true")
            {

                splitContent = htmlContent.Split(new string[] { "<!--Diff Doc-->" }, StringSplitOptions.None);
                htmlContent = splitContent[0];
                if (splitContent.Length > 1)
                    injHtmlContent = splitContent[1];

            }
            // Create a new DOCX package
            using (MemoryStream memStream = new MemoryStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document))
                {

                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    var headerPart = mainPart.AddNewPart<HeaderPart>();
                    var footerPart = mainPart.AddNewPart<FooterPart>();

                    // Create the main document part content
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());


                    // Define the font and size
                    RunProperties runProperties = new RunProperties();
                    RunFonts runFonts = new RunFonts() { Ascii = "Times New Roman" };
                    FontSize fontSize = new FontSize() { Val = "24" }; // Font size 12 (in half-point format)

                    // Apply the font settings to the RunProperties
                    runProperties.Append(runFonts);
                    runProperties.Append(fontSize);

                    // Parse the HTML content and append it to the document
                    HtmlConverter converter = new HtmlConverter(mainPart);
                    IList<OpenXmlCompositeElement> generatedBody = converter.Parse(htmlContent);

                    // Iterate over the parsed elements and apply RunProperties
                    foreach (var element in generatedBody)
                    {
                        foreach (var run in element.Descendants<Run>()) // Find all Run elements in the element
                        {
                            run.PrependChild(runProperties.CloneNode(true)); // Apply the font properties to each Run
                        }

                        // Append each element to the body
                        body.Append(element.CloneNode(true));
                    }


                    // Convert HTML to OpenXML and add to the body
                    //HtmlConverter converter = new HtmlConverter(mainPart);
                    //var generatedBody = converter.Parse(htmlContent);
                    //body.Append(generatedBody);


                    var header = new Header(new Paragraph(new Run(new Text("Header Test"))));
                    HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPart) };
                    //var footer = new Footer(new Paragraph(new Run(new Text("Page"), new SimpleField() { Instruction = "PAGE" })));
                    //FooterReference footerReference = new FooterReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) };

                    headerPart.Header = header;

                    mainPart.Document.Body.Append(new SectionProperties(headerReference));

                }
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

                var patientData = _ieService.GetOnebyPatientId(ieId);

                if (patientData.doa == null)
                {
                    docName = patientData.lname + "," + patientData.fname + "_IE_" + Common.commonDate(patientData.doe).Replace("/", "") + ".docx";
                }
                else if (patientData.account_no != null)
                {
                    docName = patientData.lname + "," + patientData.fname + "_IE_" + Common.commonDate(patientData.doe).Replace("/", "") + "_" + patientData.account_no + "_" + Common.commonDate(patientData.doa).Replace("/", "") + ".docx";
                }
                else
                    docName = patientData.lname + "," + patientData.fname + "_IE_" + Common.commonDate(patientData.doe).Replace("/", "") + "_" + Common.commonDate(patientData.doa).Replace("/", "") + ".docx";

                patientName = patientData.lname + ", " + patientData.fname;

                dos = patientData.doe == null ? "" : patientData.doe.Value.ToShortDateString();

                string subPath = "Report/" + cmpid; // Your code goes here

                bool exists = System.IO.Directory.Exists(subPath);

                if (!exists)
                    System.IO.Directory.CreateDirectory(subPath);

                filePath = subPath + "/" + docName;

                // Save the memory stream to a file
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {

                    memStream.WriteTo(fileStream);
                }
                injDocName = subPath + "/" + patientData.lname + "," + patientData.fname + "_IE_" + Common.commonDate(patientData.doe).Replace("/", "") + "_injection.docx";
                System.IO.File.Delete(injDocName);
                injHtmlContent = Regex.Replace(injHtmlContent, @"</ol>", string.Empty, RegexOptions.IgnoreCase);
                injHtmlContent = injHtmlContent.Trim();

                if (injHtmlContent != "")
                {

                    MemoryStream injMemStream = ConvertHtmlToWord(injHtmlContent);
                    // injDocName = subPath + "/"+ patientData.lname + "," + patientData.fname + "_IE_" + Common.commonDate(patientData.doe).Replace("/", "") + "_injection.docx";
                    using (FileStream fileStream = new FileStream(injDocName, FileMode.Create))
                    {

                        injMemStream.WriteTo(fileStream);
                    }
                }

            }

            return Json(new { filePath = filePath, fileName = docName, patientName = patientName, dos = dos, injFileName = injDocName });

        }

        [HttpGet]
        public virtual ActionResult DownloadFile(string filePath, string fileName, int locId = 0, string patientName = "", string signatureUrl = "", string dos = "", string injFileName = "")
        {

            string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

            var dt = _locService.GetAll(" and cmp_id=" + cmpid + " and id=" + locId);

            if (dt.Count > 0)
            {
                if (!string.IsNullOrEmpty(dt[0].header_template))
                {
                    string filepathFrom = Path.Combine(Environment.WebRootPath, "Uploads/HeaderTemplate") + "//" + dt[0].header_template;


                    string filepathTo = filePath;
                    AddHeaderFromTo(filepathFrom, filepathTo, patientName, dos);
                }
                else
                {
                    try
                    {
                        string filepathFrom = Path.Combine(Environment.WebRootPath, "Uploads/HeaderTemplate") + "//" + HttpContext.Session.GetString(SessionKeys.SessionHeaderTemplate); ;


                        string filepathTo = filePath;
                        AddHeaderFromTo(filepathFrom, filepathTo, patientName, dos);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            byte[] data = System.IO.File.ReadAllBytes(filePath);
            byte[] injdata = new byte[0];
            if (System.IO.File.Exists(injFileName))
                injdata = System.IO.File.ReadAllBytes(injFileName);
            if (SessionDiffDoc == "true" && injdata.Length > 0)
            {

                var files = new List<(string FileName, byte[] FileContent)>
                {
                    (fileName, data),

                    (Path.GetFileName(injFileName), injdata)
                };

                var zipFileName = Path.ChangeExtension(fileName, ".zip"); ;
                var zipFilePath = Path.ChangeExtension(filePath, ".zip");
                System.IO.File.Delete(zipFilePath);
                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        var entry = zip.CreateEntry(file.FileName);
                        using (var entryStream = entry.Open())
                        using (var fileStream = new MemoryStream(file.FileContent))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                byte[] zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);
                return File(zipFileBytes, "application/zip", zipFileName);
            }
            else
            {

                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

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

        private string getTreatment(string val)
        {
            string returnStr = "";
            if (!string.IsNullOrEmpty(val))
            {
                string[] str = val.Split('`');



                for (int i = 0; i < str.Length; i++)
                {

                    if (!string.IsNullOrEmpty(str[i]) && str[i].Substring(0, 1) != "@")
                    {
                        returnStr += "<br/>" + str[i].TrimStart('@');
                    }
                }
            }

            return returnStr;

        }

        private pocDetails getPOC(int PatientIE_ID)
        {
            DataTable dsPOC = _pocService.GetPOCIENew(PatientIE_ID);



            string strPoc = "", pocDesc = "", ccdesc = "", pedesc = "", adesc = "";
            string inject_desc = "";
            if (dsPOC != null && dsPOC.Rows.Count > 0)
            {

                for (int i = 0; i < dsPOC.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(dsPOC.Rows[i]["Heading"].ToString()))
                    {

                        //if (i != dsPOC.Tables[0].Rows.Count - 1)
                        //    strPoc = strPoc + "<b style='text-transform:uppercase'>" + dsPOC.Tables[0].Rows[i]["Heading"].ToString().TrimEnd(':') + ": </b>" + dsPOC.Tables[0].Rows[i]["PDesc"].ToString() + "<br/><br/>";
                        //else

                        //if (dsPOC.Tables[0].Rows[i]["Executed"] != DBNull.Value)
                        //{
                        //    this.generatePNReport(bodypart, dsPOC.Tables[0].Rows[i]["MCODE"].ToString(), ViewState["name"].ToString(),
                        //        ViewState["dob"].ToString(), ViewState["location"].ToString(), "", dsPOC.Tables[0].Rows[i]["Level"].ToString(), "", "", ViewState["doe"].ToString(), PatientIE_ID);
                        //}

                        string heading = dsPOC.Rows[i]["Heading"].ToString();

                        pocDesc = dsPOC.Rows[i]["PDesc"].ToString();

                        string _ccdesc = dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString();
                        if (_ccdesc != "")
                        {
                            _ccdesc = _ccdesc.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                            _ccdesc = _ccdesc.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                            _ccdesc = _ccdesc.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            _ccdesc = _ccdesc.Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                            _ccdesc = _ccdesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            _ccdesc = _ccdesc.Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());

                            ccdesc = ccdesc + _ccdesc;
                        }


                        string _pedesc = dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString();
                        if (_pedesc != "")
                        {
                            _pedesc = _pedesc.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                            _pedesc = _pedesc.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                            _pedesc = _pedesc.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            _pedesc = _pedesc.Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                            _pedesc = _pedesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            _pedesc = _pedesc.Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());

                            pedesc = pedesc + _pedesc;
                        }

                        string _adesc = dsPOC.Rows[i]["ADesc"] == null ? "" : dsPOC.Rows[i]["ADesc"].ToString();

                        if (_adesc != null)
                        {
                            _adesc = _adesc.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                            _adesc = _adesc.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                            _adesc = _adesc.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            _adesc = _adesc.Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                            _adesc = _adesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            _adesc = _adesc.Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());

                            adesc = adesc + _adesc;
                        }

                        if (heading.ToLower().Contains("(side)"))
                        {
                            heading = heading.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString().ToUpper());
                            heading = heading.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString().ToUpper());

                        }

                        if (pocDesc.ToLower().Contains("(side)"))
                        {
                            pocDesc = pocDesc.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                            pocDesc = pocDesc.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                        }

                        if (heading.ToLower().Contains("(levels)"))
                        {
                            heading = heading.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            heading = heading.Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        if (pocDesc.ToLower().Contains("(levels)"))
                        {
                            pocDesc = pocDesc.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            pocDesc = pocDesc.Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        if (heading.ToLower().Contains("(level)"))
                        {
                            heading = heading.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            heading = heading.Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());

                        }

                        if (pocDesc.ToLower().Contains("(level)"))
                        {
                            pocDesc = pocDesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            pocDesc = pocDesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        if (dsPOC.Rows[i]["pn"].ToString() == "1" && dsPOC.Rows[i]["Executed"] != DBNull.Value)
                        {
                            if (!string.IsNullOrEmpty(dsPOC.Rows[i]["injection_description"].ToString()))
                            {
                                inject_desc = inject_desc + "<br/>" + (dsPOC.Rows[i]["injection_description"].ToString());
                                inject_desc = inject_desc.Replace("#Side", dsPOC.Rows[i]["Sides"].ToString());
                                inject_desc = inject_desc.Replace("#Muscle", dsPOC.Rows[i]["Muscle"].ToString().TrimEnd('~').ToString().Replace("~", ", "));
                            }
                        }
                        strPoc = strPoc + "<li><b style='text-transform:uppercase'>" + heading.TrimEnd(':') + ": </b>" + pocDesc + "</li>";
                    }
                }
            }

            pocDetails pocDetails = new pocDetails()
            {
                strInjectionDesc = inject_desc,
                strPoc = strPoc != "" ? "<ol>" + strPoc + "</ol>" : "",
                strCCDesc = ccdesc,
                strPEDesc = pedesc,
                strADesc = adesc
            };

            return pocDetails;
        }

        private pocDetails UpdatePOCPlan(int PatientIE_ID)
        {
            DataTable dsPOC = _pocService.GetPOCIENew(PatientIE_ID);

            string strPoc = "", pocDesc = "", ccdesc = "", pedesc = "", adesc = "";
            string inject_desc = "";
            if (dsPOC != null && dsPOC.Rows.Count > 0)
            {

                for (int i = 0; i < dsPOC.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(dsPOC.Rows[i]["Heading"].ToString()))
                    {


                        string heading = dsPOC.Rows[i]["Heading"].ToString();
                        pocDesc = dsPOC.Rows[i]["PDesc"].ToString();
                        ccdesc = ccdesc + dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString().Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                        ccdesc = ccdesc + dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString().Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                        ccdesc = ccdesc + dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString().Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                        ccdesc = ccdesc + dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString().Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                        ccdesc = ccdesc + dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString().Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                        ccdesc = ccdesc + dsPOC.Rows[i]["CCDesc"] == null ? "" : dsPOC.Rows[i]["CCDesc"].ToString().Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());




                        pedesc = pedesc + dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString().Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                        pedesc = pedesc + dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString().Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                        pedesc = pedesc + dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString().Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                        pedesc = pedesc + dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString().Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                        pedesc = pedesc + dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString().Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                        pedesc = pedesc + dsPOC.Rows[i]["PEDesc"] == null ? "" : dsPOC.Rows[i]["PEDesc"].ToString().Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());


                        adesc = adesc + dsPOC.Rows[i]["ADesc"] == null ? "" : dsPOC.Rows[i]["ADesc"].ToString().Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                        adesc = adesc + dsPOC.Rows[i]["adesc"] == null ? "" : dsPOC.Rows[i]["adesc"].ToString().Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                        adesc = adesc + dsPOC.Rows[i]["adesc"] == null ? "" : dsPOC.Rows[i]["adesc"].ToString().Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                        adesc = adesc + dsPOC.Rows[i]["adesc"] == null ? "" : dsPOC.Rows[i]["adesc"].ToString().Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                        adesc = adesc + dsPOC.Rows[i]["adesc"] == null ? "" : dsPOC.Rows[i]["adesc"].ToString().Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                        adesc = adesc + dsPOC.Rows[i]["adesc"] == null ? "" : dsPOC.Rows[i]["adesc"].ToString().Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());





                        if (heading.ToLower().Contains("(side)"))
                        {
                            heading = heading.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                            heading = heading.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());

                        }

                        if (pocDesc.ToLower().Contains("(side)"))
                        {
                            pocDesc = pocDesc.Replace("(SIDE)", dsPOC.Rows[i]["Sides"].ToString());
                            pocDesc = pocDesc.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                        }

                        if (heading.ToLower().Contains("(levels)"))
                        {
                            heading = heading.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            heading = heading.Replace("(LEVELS)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        if (pocDesc.ToLower().Contains("(levels)"))
                        {
                            pocDesc = pocDesc.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                            pocDesc = pocDesc.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        if (heading.ToLower().Contains("(level)"))
                        {
                            heading = heading.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            heading = heading.Replace("(LEVEL)", dsPOC.Rows[i]["Level"].ToString());

                        }

                        if (pocDesc.ToLower().Contains("(level)"))
                        {
                            pocDesc = pocDesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                            pocDesc = pocDesc.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                        }





                        if (dsPOC.Rows[i]["pn"].ToString() == "1" && dsPOC.Rows[i]["Executed"] != DBNull.Value)
                        {
                            if (!string.IsNullOrEmpty(dsPOC.Rows[i]["injection_description"].ToString()))
                            {
                                inject_desc = inject_desc + "<br/>" + (dsPOC.Rows[i]["injection_description"].ToString());
                                inject_desc = inject_desc.Replace("#Side", dsPOC.Rows[i]["Sides"].ToString());
                                inject_desc = inject_desc.Replace("#Muscle", dsPOC.Rows[i]["Muscle"].ToString().TrimEnd('~').ToString().Replace("~", ", "));
                            }
                        }
                        strPoc = strPoc + "<li><b style='text-transform:uppercase'>" + heading.TrimEnd(':') + ": </b>" + pocDesc + "</li>";
                    }
                }
            }

            pocDetails pocDetails = new pocDetails()
            {
                strInjectionDesc = inject_desc,
                strPoc = strPoc != "" ? "<ol>" + strPoc + "</ol>" : "",
                strCCDesc = ccdesc,
                strPEDesc = pedesc,
                strADesc = adesc
            };

            return pocDetails;
        }

        public void AddHeaderFromTo(string filepathFrom, string filepathTo, string patientName = "", string dos = "")
        {
            // Replace header in target document with header of source document.
            using (WordprocessingDocument
                wdDoc = WordprocessingDocument.Open(filepathTo, true))
            {
                MainDocumentPart mainPart = wdDoc.MainDocumentPart;

                // Delete the existing header part.
                mainPart.DeleteParts(mainPart.HeaderParts);

                // Create a new header part.
                DocumentFormat.OpenXml.Packaging.HeaderPart headerPart =
            mainPart.AddNewPart<HeaderPart>();

                // Get Id of the headerPart.
                string rId = mainPart.GetIdOfPart(headerPart);

                // Feed target headerPart with source headerPart.
                using (WordprocessingDocument wdDocSource =
                    WordprocessingDocument.Open(filepathFrom, true))
                {
                    DocumentFormat.OpenXml.Packaging.HeaderPart firstHeader =
            wdDocSource.MainDocumentPart.HeaderParts.FirstOrDefault();

                    wdDocSource.MainDocumentPart.HeaderParts.FirstOrDefault();

                    if (firstHeader != null)
                    {
                        headerPart.FeedData(firstHeader.GetStream());
                }
                }

                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

                var restheaderPart = mainPart.AddNewPart<HeaderPart>("Rest");
                restheaderPart.Header = CreateHeaderWithPageNumber(patientName, "");
                if (cmpid == 7 || cmpid == 13)
                {
                    if (!string.IsNullOrEmpty(dos))
                    {
                        string _dos = Common.commonDate(Convert.ToDateTime(dos), HttpContext.Session.GetString(SessionKeys.SessionDateFormat));
                        restheaderPart.Header = CreateHeaderWithPageNumber(patientName, _dos);
                    }
                }
                else
                {

                    restheaderPart.Header = CreateHeaderWithPageNumber(patientName, "");
                }


                //  restheaderPart.Header = new Header(new Paragraph("Purav\nSandip"));
                string restId = mainPart.GetIdOfPart(restheaderPart);
                // Get SectionProperties and Replace HeaderReference with new Id.
                IEnumerable<DocumentFormat.OpenXml.Wordprocessing.SectionProperties> sectPrs =
            mainPart.Document.Body.Elements<SectionProperties>();
                foreach (var sectPr in sectPrs)
                {
                    // Delete existing references to headers.
                    sectPr.RemoveAllChildren<HeaderReference>();
                    sectPr.Append(new TitlePage());
                    // Create the new header reference node.
                    sectPr.PrependChild<HeaderReference>(new HeaderReference() { Type = HeaderFooterValues.First, Id = rId });
                    sectPr.PrependChild<HeaderReference>(new HeaderReference() { Type = HeaderFooterValues.Default, Id = restId });
                }
            }
        }

        public Header CreateHeaderWithPageNumber(string text1, string text2)
        {
            if (text2 != "")
            {
                return new Header(
                    new Paragraph(
                        new Run(
                            new Text(text1) // First line
                        ),
                        new Break(), // Line break
                        new Run(
                            new Text(text2) // Second line
                        ),
                        new Break(), // Another line break if needed
                        new Run(
                            new Text("Page ") // Static "Page " text
                        ),
                        new Run(
                            new SimpleField() // Dynamic page number field
                            {
                                Instruction = "PAGE", // Specifies the field type
                            }
                        )
                    )
                );
            }
            else
            {
                return new Header(
                   new Paragraph(
                       new Run(
                           new Text(text1) // First line
                       ),
                       new Break(), // Line break

                       new Run(
                           new Text("Page ") // Static "Page " text
                       ),
                       new Run(
                           new SimpleField() // Dynamic page number field
                           {
                               Instruction = "PAGE", // Specifies the field type
                           }
                       )
                   )
               );
            }
        }
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

        private string removePtag(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                Regex rgx = new Regex("<p>|</p>");
                string res = rgx.Replace(content, "", 2);
                return res;
            }
            else
                return "";
        }

        private string getDiagnostic(int id)
        {
            var data = _ieService.GetOnePage3(id);

            string strDaignosis = "", stradddaigno = "";
            bool isnormal = true;

            if (data != null)
            {

                if (data.diagcervialbulge_date != null)
                {
                    strDaignosis = data.diagcervialbulge_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diagcervialbulge_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagcervialbulge_study));

                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diagcervialbulge_text))
                    {

                        strDaignosis = strDaignosis + " of the cervical spine " + data.diagcervialbulge_text + ", ";

                        stradddaigno = stradddaigno + "Cervical " + data.diagcervialbulge_text.Replace("reveals", "").TrimEnd('.') + ".<br/>";
                        isnormal = false;
                    }

                    if (!string.IsNullOrEmpty(data.diagcervialbulge_hnp1))
                    {
                        strDaignosis = strDaignosis + " HNP at " + data.diagcervialbulge_hnp1.TrimEnd('.') + ".";
                        stradddaigno = stradddaigno + "Cervical herniated nucleus pulposis at " + data.diagcervialbulge_hnp1.TrimEnd('.') + ".<br/>";
                        isnormal = false;
                    }

                    if (!string.IsNullOrEmpty(data.diagcervialbulge_hnp2))
                    {
                        strDaignosis = strDaignosis + data.diagcervialbulge_hnp2.TrimEnd('.') + ".";
                        if (!string.IsNullOrEmpty(data.diagcervialbulge_hnp2))
                        {
                            stradddaigno = stradddaigno + data.diagcervialbulge_hnp2.TrimEnd('.') + ".<br/>";
                        }

                        isnormal = false;
                    }

                    if (isnormal)
                    {
                        strDaignosis = strDaignosis + " of the cervical spine is normal. ";
                    }
                }

                if (data.diagthoracicbulge_date != null)
                {
                    isnormal = true;
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diagthoracicbulge_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diagthoracicbulge_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagthoracicbulge_study));
                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diagthoracicbulge_text))
                    {
                        strDaignosis = strDaignosis + " of the thoracic spine " + data.diagthoracicbulge_text + ", ";

                        stradddaigno = stradddaigno + "Thoracic " + data.diagthoracicbulge_text.ToString().Replace("reveals", "").TrimEnd('.') + ".<br/>";
                        isnormal = false;
                    }

                    if (!string.IsNullOrEmpty(data.diagthoracicbulge_hnp1))
                    {
                        strDaignosis = strDaignosis + " HNP at " + data.diagthoracicbulge_hnp1.TrimEnd('.') + ". ";
                        stradddaigno = stradddaigno + "Thoracic herniated nucleus pulposis at " + data.diagthoracicbulge_hnp1.TrimEnd('.') + ".<br/>";
                        isnormal = false;
                    }

                    if (!string.IsNullOrEmpty(data.diagthoracicbulge_hnp2))
                    {
                        strDaignosis = strDaignosis + data.diagthoracicbulge_hnp2.TrimEnd('.') + ". ";
                        if (!string.IsNullOrEmpty(data.diagthoracicbulge_hnp2))
                        {
                            stradddaigno = stradddaigno + data.diagthoracicbulge_hnp2.TrimEnd('.') + ".<br/>";
                        }

                        isnormal = false;
                    }

                    if (isnormal)
                    {
                        strDaignosis = strDaignosis + " of the thoracic spine is normal. ";
                    }
                }

                if (data.diaglumberbulge_date != null)
                {
                    isnormal = true;
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diaglumberbulge_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diaglumberbulge_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diaglumberbulge_study));
                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diaglumberbulge_text))
                    {
                        strDaignosis = strDaignosis + " of the lumbar spine " + data.diaglumberbulge_text + ", ";

                        stradddaigno = stradddaigno + "Lumbar " + data.diaglumberbulge_text.ToString().Replace("reveals", "").TrimEnd('.') + ".<br/>";
                        isnormal = false;
                    }

                    if (!string.IsNullOrEmpty(data.diaglumberbulge_hnp1))
                    {
                        strDaignosis = strDaignosis + " HNP at " + data.diaglumberbulge_hnp1.TrimEnd('.') + ". ";
                        stradddaigno = stradddaigno + "Lumbar herniated nucleus pulposis at " + data.diaglumberbulge_hnp1.TrimEnd('.') + ".<br/>";
                        isnormal = false;
                    }

                    if (!string.IsNullOrEmpty(data.diaglumberbulge_hnp2))
                    {
                        strDaignosis = strDaignosis + data.diaglumberbulge_hnp2.TrimEnd('.') + ". ";
                        if (!string.IsNullOrEmpty(data.diaglumberbulge_hnp2))
                        {
                            stradddaigno = stradddaigno + data.diaglumberbulge_hnp2.TrimEnd('.') + ".<br/>";
                        }

                        isnormal = false;
                    }

                    if (isnormal)
                    {
                        strDaignosis = strDaignosis + " of the lumbar spine is normal. ";
                    }
                }

                if (data.diagleftshoulder_date != null)
                {

                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diagleftshoulder_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diagleftshoulder_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagleftshoulder_study));
                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diagleftshoulder_text))
                    {
                        strDaignosis = strDaignosis + " of the left shoulder " + data.diagleftshoulder_text.TrimEnd('.') + ". " + "<br/>";
                    }
                    else
                    {
                        strDaignosis = strDaignosis + " of the left shoulder is normal. ";
                    }

                }

                if (data.diagrightshoulder_date != null)
                {

                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diagrightshoulder_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diagrightshoulder_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagrightshoulder_study));
                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diagrightshoulder_text))
                    {
                        strDaignosis = strDaignosis + " of the right shoulder " + data.diagrightshoulder_text.TrimEnd('.') + ". " + "<br/>";
                    }
                    else
                    {
                        strDaignosis = strDaignosis + " of the right shoulder is normal. ";
                    }

                }
                if (data.diagleftknee_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diagleftknee_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diagleftknee_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagleftknee_study));
                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diagleftknee_text))
                    {
                        strDaignosis = strDaignosis + " of the left knee " + data.diagleftknee_text.TrimEnd('.') + ". " + "<br/>";
                    }
                    else
                    {
                        strDaignosis = strDaignosis + " of the left knee is normal. ";
                    }
                }

                if (data.diagrightknee_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diagrightknee_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.diagrightknee_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagrightknee_study));
                        strDaignosis = strDaignosis + " " + study;
                    }

                    if (!string.IsNullOrEmpty(data.diagrightknee_text))
                    {
                        strDaignosis = strDaignosis + " of the right knee " + data.diagrightknee_text.TrimEnd('.') + ". " + "<br/>";
                    }
                    else
                    {
                        strDaignosis = strDaignosis + " of the right knee is normal. ";
                    }
                }



                //if (data.diagrightknee_date != null)
                //{
                //    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.diagrightknee_date.Value.ToString("MM/dd/yyyy") + " - ";

                //    if (!string.IsNullOrEmpty(data.diagrightknee_study))
                //    {
                //        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study1>(data.diagrightknee_study));
                //        strDaignosis = strDaignosis + " " + study;
                //    }

                //    if (!string.IsNullOrEmpty(data.diagrightknee_text))
                //    {
                //        strDaignosis = strDaignosis + " of the right knee " + data.diagrightknee_text.TrimEnd('.') + ". ";
                //    }
                //    else
                //    {
                //        strDaignosis = strDaignosis + " of the right knee is normal. ";
                //    }
                //}

                if (data.other1_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other1_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other1_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other1_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other1_text))
                    {
                        strDaignosis = strDaignosis + data.other1_text.TrimEnd('.') + ". ";
                    }
                }

                if (data.other2_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other2_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other2_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other2_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other2_text))
                    {
                        strDaignosis = strDaignosis + data.other2_text.TrimEnd('.') + ". ";
                    }
                }

                if (data.other3_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other3_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other3_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other3_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other3_text))
                    {
                        strDaignosis = strDaignosis + data.other3_text.TrimEnd('.') + ". ";
                    }
                }

                if (data.other4_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other4_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other4_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other4_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other4_text))
                    {
                        strDaignosis = strDaignosis + data.other4_text.TrimEnd('.') + ". ";
                    }
                }

                if (data.other5_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other5_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other5_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other5_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other5_text))
                    {
                        strDaignosis = strDaignosis + data.other5_text.TrimEnd('.') + ". ";
                    }
                }

                if (data.other6_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other6_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other6_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other6_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other6_text))
                    {
                        strDaignosis = strDaignosis + data.other6_text.TrimEnd('.') + ". ";
                    }
                }

                if (data.other7_date != null)
                {
                    strDaignosis = (!string.IsNullOrEmpty(strDaignosis) ? (strDaignosis + "<br/>") : "") + data.other7_date.Value.ToString("MM/dd/yyyy") + " - ";

                    if (!string.IsNullOrEmpty(data.other7_study))
                    {
                        var study = EnumHelper.GetDisplayName(System.Enum.Parse<EnumHelper.Study2>(data.other7_study));
                        strDaignosis = strDaignosis + " " + study + " ";
                    }

                    if (!string.IsNullOrEmpty(data.other7_text))
                    {
                        strDaignosis = strDaignosis + data.other7_text.TrimEnd('.') + ". ";
                    }
                }

            }

            return strDaignosis;

        }
        #endregion

        #region FU
        public IActionResult CreateFU(int patientIEId, int patientId, string type = "")
        {

            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            int? userid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId);
            int fu_id = 0;

            var fuData = _ieService.GetLastFU(patientIEId, type);

            if (fuData == null)
            {
                var ieData = _ieService.GetOne(patientIEId);

                tbl_patient_fu objFU = new tbl_patient_fu()
                {
                    created_by = userid,
                    doe = System.DateTime.Now,
                    patientIE_ID = patientIEId,
                    cmp_id = cmpid,
                    created_date = System.DateTime.Now,
                    is_active = true,
                    patient_id = patientId,
                    extra_comments = "",
                    type = type,
                    accident_type = ieData.accident_type,
                    provider_id = ieData.provider_id

                };
                fu_id = _patientFUservices.Insert(objFU);

                try
                {
                    _forwardServices.GetOnePage1(patientIEId, fu_id, cmpid.Value, patientId);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }

                try
                {
                    _forwardServices.GetOnePage2(patientIEId, fu_id, cmpid.Value, patientId);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }

                try
                {
                    _forwardServices.GetOneNE(patientIEId, fu_id, cmpid.Value, patientId);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }

                try
                {
                    _forwardServices.GetOneOther(patientIEId, cmpid.Value, fu_id, patientId);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");

                }


                try
                {
                    _forwardServices.GetPage3(patientIEId, cmpid.Value, fu_id, patientId);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");

                }

                try
                {
                    var iepage3 = _ieService.GetOnePage3(patientIEId);

                    var fupage3 = new tbl_fu_page3();

                    if (iepage3 != null)
                        fupage3 = _mapper.Map<tbl_fu_page3>(iepage3);


                    fupage3.fu_id = fu_id;
                    fupage3.cmp_id = cmpid.Value;


                    _fuPage3services.Insert(fupage3);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                    return View("Error");
                }
                /*try
                {
                    _pocService.ForwardPOCIETOFU(patientIEId, fu_id);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                    return View("Error");
                }*/

                try
                {
                    var pageOther = _ieService.GetOneOtherPage(patientIEId);
                    var fupageOther = _mapper.Map<tbl_fu_other>(pageOther);

                    if (fupageOther != null)
                    {
                        fupageOther.fu_id = fu_id;
                    }
                    _fuOtherservices.Insert(fupageOther);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }
            }
            else
            {
                if (fuData.doe != null)
                {
                    if (fuData.doe.Value.ToString("MM/dd/yyyy") == System.DateTime.Now.ToString("MM/dd/yyyy"))
                    {
                        return Json(new { success = false });
                    }
                }

                tbl_patient_fu objFU = new tbl_patient_fu()
                {
                    created_by = userid,
                    doe = System.DateTime.Now,
                    patientIE_ID = patientIEId,
                    cmp_id = cmpid,
                    created_date = System.DateTime.Now,
                    is_active = true,
                    patient_id = patientId,
                    extra_comments = "",
                    type = type,
                    accident_type = fuData.accident_type,
                    provider_id = fuData.provider_id

                };
                fu_id = _patientFUservices.Insert(objFU);

                try
                {
                    _forwardServices.GetOnePage1(patientIEId, fu_id, cmpid.Value, patientId, fuData.id.Value);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }

                try
                {
                    _forwardServices.GetOnePage2(patientIEId, fu_id, cmpid.Value, patientId, fuData.id.Value);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }

                try
                {
                    _forwardServices.GetOneNE(patientIEId, fu_id, cmpid.Value, patientId, fuData.id.Value);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");
                }

                try
                {
                    _forwardServices.GetOneOther(patientIEId, cmpid.Value, fu_id, patientId, fuData.id.Value);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");

                }


                try
                {
                    _forwardServices.GetPage3(patientIEId, cmpid.Value, fu_id, patientId, fuData.id.Value);
                }
                catch (Exception ex)
                {
                    SaveLog(ex, "CreateFU");

                }
            }
            var redirectUrl = Url.Action("Create", "FuVisit", new { patientIEId = patientIEId, patientFUId = fu_id, type = type });
            return Json(new { success = true, redirectUrl = redirectUrl });


        }
        #endregion

        public string getInjectionReport(int id)
        {
            tbl_procedures obj = new tbl_procedures();
            obj.id = id;
            var data = _procedureservices.GetOne(obj);

            if (data.pn)
            {
                return "";
            }
            return "";
        }


        [HttpPost]
        public IActionResult ImportDocumentsData(string selectedParent, IFormFile[] postedFile)
        {
            string PatientID = HttpContext.Session.GetInt32(SessionKeys.SessionPatientId).ToString();
            string FolderPath = "";
            bool folderExists = false;
            try
            {
                string SelectedFolder = selectedParent.Replace('"', ' ').Trim();
                if (postedFile != null && postedFile.Length > 0)
                {
                    foreach (var file in postedFile)
                    {
                        // Get the file name from the browser
                        var fileName = System.IO.Path.GetFileName(file.FileName);


                        // Get file path to be uploaded

                        FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", SelectedFolder, PatientID);


                        folderExists = Directory.Exists(FolderPath);
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

                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "List");
            }

            FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments");
            folderExists = Directory.Exists(FolderPath);
            if (!folderExists)
                Directory.CreateDirectory(FolderPath);

            string[] dirs = Directory.GetDirectories(FolderPath, "*", SearchOption.TopDirectoryOnly);

            List<TreeViewNode> nodes = new List<TreeViewNode>();

            int i = 0;
            foreach (var item in dirs)
            {
                i++;
                string FolderName = System.IO.Path.GetFileName(item);
                var FolderPathFile = Path.Combine(Directory.GetCurrentDirectory(), "PatientDocuments", FolderName.ToString(), PatientID.ToString());
                int j = 0;

                bool folderExistsNew = Directory.Exists(FolderPathFile);
                if (!folderExistsNew)
                    Directory.CreateDirectory(FolderPathFile);

                foreach (var item1 in Directory.GetFiles(FolderPathFile))
                {
                    j++;
                    nodes.Add(new TreeViewNode { id = j.ToString() + "-" + i.ToString() + "~" + FolderName.ToString() + "$" + System.IO.Path.GetFileName(item1), parent = i.ToString(), text = System.IO.Path.GetFileName(item1) });
                }

                nodes.Add(new TreeViewNode { id = i.ToString(), parent = "#", text = FolderName.ToString() + "(" + j + ")" });
            }

            return Json(new { updatedJson = JsonConvert.SerializeObject(nodes, Formatting.Indented) });
        }
    }

    public class pocDetails
    {
        public string strPoc { get; set; }
        public string strInjectionDesc { get; set; }
        public string strPEDesc { get; set; }
        public string strCCDesc { get; set; }
        public string strADesc { get; set; }
    }
}
