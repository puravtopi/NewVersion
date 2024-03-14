using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
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
using System.Text.RegularExpressions;
using static PainTrax.Web.Helper.EnumHelper;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using HtmlToOpenXml;

namespace PainTrax.Web.Controllers
{
    public class FUVisitController : Controller
    {

        private readonly Common _commonservices = new Common();
        private readonly PatientService _patientservices = new PatientService();
        private readonly PatientFUService _patientFuservices = new PatientFUService();
        private readonly FUPage1Service _fuPage1services = new FUPage1Service();
        private readonly FUPage2Service _fuPage2services = new FUPage2Service();
        private readonly FUPage3Service _fuPage3services = new FUPage3Service();
        private readonly FUNEService _fuNEservices = new FUNEService();
        private readonly FUOtherService _fuOtherService = new FUOtherService();
        private readonly FUCommentService _fuCommentService = new FUCommentService();

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
        private readonly WebsiteMacrosMasterService _websiteMacrosService = new WebsiteMacrosMasterService();
        private readonly PrintSettingServices _printService = new PrintSettingServices();
        private EnumHelper enumHelper = new EnumHelper();
        private readonly ILogger<VisitController> _logger;
        private IMapper _mapper;

        public FUVisitController(ILogger<VisitController> logger, IMapper mapper)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create(int patientIEId, int patientFUId = 0, string type = "")
        {
            VisitVM obj = new VisitVM();

            try
            {              
                int id = patientIEId;
                obj.vaccinated = false;

                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

                var macroList = _websiteMacrosService.GetAutoComplete(cmpid.Value);
                ViewBag.csList = _commonservices.GetCaseType(cmpid.Value);
                ViewBag.asList = _commonservices.GetAccidenttype(cmpid.Value);

                ViewBag.macroList = JsonConvert.SerializeObject(macroList);

                ViewBag.type = type;

                if (patientFUId > 0)
                {
                    var ieData = _ieService.GetOnebyPatientId(patientIEId);

                    int patientId = 0;

                    if (ieData != null)
                    {
                        obj.id = ieData.id;
                        patientId = ieData.patient_id.Value;
                        obj.locationid = ieData.location_id;
                        obj.location = ieData.location;
                        obj.doa = ieData.doa;
                        obj.dos = ieData.doe;
                        obj.prime_claim_no = ieData.primary_claim_no;
                        obj.prime_policy_no = ieData.primary_policy_no;
                        obj.prime_WCB_group = ieData.primary_wcb_group;

                        obj.sec_claim_no = ieData.secondary_claim_no;
                        obj.sec_policy_no = ieData.secondary_policy_no;
                        obj.sec_WCB_group = ieData.secondary_wcb_group;
                        obj.alert_note = ieData.alert_note;
                        obj.referring_physician = ieData.referring_physician;
                        obj.casetype = ieData.casetype;
                        obj.compensation = ieData.compensation;
                        obj.accidentType = ieData.accidentType;
                    }

                    var fuData = _patientFuservices.GetOne(patientFUId);

                    obj.dos = fuData.doe;
                    obj.dov = fuData.doe;

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

                    var page1Data = _fuPage1services.GetOne(patientFUId);

                    if (page1Data != null)
                    {
                        var page1IEData = _mapper.Map<tbl_ie_page1>(page1Data);
                        obj.Page1 = page1IEData;

                        string daignoLink = "";
                        if (!string.IsNullOrEmpty(page1Data.bodypart))
                        {
                            for (int i = 0; i < page1Data.bodypart.Split(',').Length; i++)
                            {
                                var linkbody = page1Data.bodypart.Split(',')[i].Replace(" ", "_");
                                daignoLink += "<a href=# onclick=openDaignoModel('" + linkbody + "')>" + page1Data.bodypart.Split(',')[i] + "</a>&nbsp;";
                            }

                        }
                        ViewBag.DaignoLink = daignoLink;
                    }
                    else
                        obj.Page1 = new tbl_ie_page1();

                    var page2Data = _fuPage2services.GetOne(patientFUId);

                    if (page2Data != null)
                    {
                        var page2IEData = _mapper.Map<tbl_ie_page2>(page2Data);
                        obj.Page2 = page2IEData;
                    }
                    else
                        obj.Page2 = new tbl_ie_page2();

                    var page3Data = _fuPage3services.GetOne(patientFUId);

                    if (page3Data != null)
                    {
                        var page3IEData = _mapper.Map<tbl_ie_page3>(page3Data);
                        obj.Page3 = page3IEData;
                    }
                    else
                        obj.Page3 = new tbl_ie_page3();


                    var NEData = _fuNEservices.GetOne(patientFUId);

                    if (NEData != null)
                    {
                        var neIEData = _mapper.Map<tbl_ie_ne>(NEData);
                        obj.NE = neIEData;
                    }
                    else
                        obj.NE = new tbl_ie_ne();


                    var pageOther = _fuOtherService.GetOne(patientFUId);

                    if (pageOther != null)
                    {
                        var otherIEData = _mapper.Map<tbl_ie_other>(pageOther);


                        obj.Other = otherIEData;


                        var tretmentdesc = string.IsNullOrEmpty(pageOther.treatment_delimit_desc) ? null : pageOther.treatment_delimit_desc.Split('^');


                        var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);

                        List<tbl_treatment_master> lst = new List<tbl_treatment_master>();
                        int i = 0;
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

                        obj.Other.listTreatmentMaster = lst;
                    }
                    else
                    {
                        obj.Other = new tbl_ie_other();
                        var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);
                        obj.Other.followup_duration = "2 weeks.";
                        obj.Other.listTreatmentMaster = _data;
                    }

                    var pageComment = _fuCommentService.GetOne(patientFUId);

                    if (pageComment != null)
                    {
                        var commentIEData = _mapper.Map<tbl_ie_comment>(pageComment);
                        obj.Comment = commentIEData;
                    }
                    else
                        obj.Comment = new tbl_ie_comment();


                    var patientData = _patientservices.GetOne(patientId);

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
                    }

                }
                else if (id > 0)
                {

                    var ieData = _ieService.GetOnebyPatientId(id);

                    if (ieData != null)
                    {
                        obj.id = ieData.id;

                        obj.locationid = ieData.location_id;
                        obj.location = ieData.location;
                        obj.doa = ieData.doa;
                        obj.dos = ieData.doe;
                        obj.prime_claim_no = ieData.primary_claim_no;
                        obj.prime_policy_no = ieData.primary_policy_no;
                        obj.prime_WCB_group = ieData.primary_wcb_group;

                        obj.sec_claim_no = ieData.secondary_claim_no;
                        obj.sec_policy_no = ieData.secondary_policy_no;
                        obj.sec_WCB_group = ieData.secondary_wcb_group;
                        obj.alert_note = ieData.alert_note;
                        obj.referring_physician = ieData.referring_physician;

                        obj.compensation = ieData.compensation;
                        obj.casetype = ieData.casetype;
                    }
                    //if (obj.compensation != null)
                    //    obj.casetype = System.Enum.GetName(typeof(CaseType), Convert.ToInt32(obj.compensation));

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

                        string daignoLink = "";
                        if (!string.IsNullOrEmpty(page1Data.bodypart))
                        {
                            for (int i = 0; i < page1Data.bodypart.Split(',').Length; i++)
                            {
                                var linkbody = page1Data.bodypart.Split(',')[i].Replace(" ", "_");
                                daignoLink += "<a href=# onclick=openDaignoModel('" + linkbody + "')>" + page1Data.bodypart.Split(',')[i] + "</a>&nbsp;";
                            }

                        }
                        ViewBag.DaignoLink = daignoLink;
                    }
                    else
                        obj.Page1 = new tbl_ie_page1();

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
                        obj.NE = new tbl_ie_ne();

                    var page3Data = _ieService.GetOnePage3(id);

                    if (page3Data != null)
                    {
                        obj.Page3 = page3Data;
                    }
                    else
                        obj.Page3 = new tbl_ie_page3();


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
                        var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);

                        List<tbl_treatment_master> lst = new List<tbl_treatment_master>();
                        foreach (var item in _data)
                        {
                            if (!string.IsNullOrEmpty(pageOther.treatment_delimit))
                            {
                                if (pageOther.treatment_delimit.Contains(item.id.ToString()))
                                    item.pre_select = true;
                                else
                                    item.pre_select = false;
                            }

                            lst.Add(item);
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
                    obj.Other.followup_duration = "2 weeks.";

                    var _data = _treatmentService.GetAll(" and cmp_id=" + cmpid.Value);

                    obj.Other.listTreatmentMaster = _data;

                }

                obj.listWebsiteMacros = macroList;
             
                obj.fu_id = patientFUId;
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
                    emp_id = empId,
                    is_active = true,
                    location_id = model.locationid,
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
                    alert_note = model.alert_note,
                    referring_physician = model.referring_physician

                };
                int ie = 0;
                if (model.id.Value > 0)
                {
                    objIE.id = model.id.Value;
                    _ieService.UpdateFromFU(objIE);
                    ie = model.id.Value;
                }
                else
                    ie = _ieService.Insert(objIE);


                //save FU details 

                tbl_patient_fu objFU = new tbl_patient_fu()
                {
                   
                    created_by = userid,
                    doe = model.dov,
                    patientIE_ID = ie,
                    cmp_id = cmpid,
                    created_date = System.DateTime.Now,
                    is_active = true,
                    patient_id = patientId,
                    extra_comments = model.alert_note,
                    type = model.type
                };
                int fu_id = 0;
                if (model.fu_id.Value > 0)
                {
                    objFU.id = model.fu_id.Value;
                    _patientFuservices.Update(objFU);
                    fu_id = model.fu_id.Value;
                }
                else

                    fu_id = _patientFuservices.Insert(objFU);

                HttpContext.Session.SetInt32(SessionKeys.SessionIEId, ie);
                return Json(new { status = 1, patintid = patientId, ieid = ie, fuid = fu_id });
            }
    
            catch(Exception ex)
            {
                SaveLog(ex, "SaveDetails");
              
            }
            return Json(new { status =0 });
        }

        [HttpPost]
        public IActionResult SavePage1(tbl_fu_page1 model)
        {
            try
            {               
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;

                var obj = _fuPage1services.GetOne(model.fu_id.Value);

                if (obj != null)
                {
                    model.id = obj.id;
                    _fuPage1services.Update(model);
                }
                else
                    _fuPage1services.Insert(model);
               
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SavePage1");              
            }
            return Json(1);
        }

        [HttpPost]
        public IActionResult SavePage2(tbl_fu_page2 model)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;

                var obj = _fuPage2services.GetOne(model.fu_id.Value);

                if (obj != null)
                {
                    model.id = obj.id;
                    _fuPage2services.Update(model);
                }
                else
                    _fuPage2services.Insert(model);             
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SavePage2");
             
            }
            return Json(1);
        }

        [HttpPost]
        public IActionResult SavePage3(tbl_fu_page3 model, int ieid, int fuid, int patientid, int? id, string discharge_medications)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;

                model.ie_id = ieid;
                model.patient_id = patientid;
                model.fu_id = fuid;

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
                int data = 1;


                var obj = _fuPage3services.GetOne(model.fu_id.Value);

                if (obj != null)
                {
                    model.id = obj.id;
                    _fuPage3services.Update(model);
                }
                else
                    _fuPage3services.Insert(model);              
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SavePage3");
            }
            return Json(1);

        }

        [HttpPost]
        public IActionResult SaveNE(tbl_fu_ne model)
        {
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid;
                var data = 1;

                var obj = _fuNEservices.GetOne(model.fu_id.Value);

                if (obj != null)
                {
                    model.id = obj.id;
                    _fuNEservices.Update(model);
                }
                else
                    _fuNEservices.Insert(model);

               
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SaveNE");
            }
            return Json(1);
        }

        [HttpPost]
        public IActionResult SaveOther(tbl_fu_other model)
        {
            try
            {
                var obj = _fuOtherService.GetOne(model.fu_id.Value);

                if (obj != null)
                {
                    model.id = obj.id;
                    _fuOtherService.Update(model);
                }
                else
                    _fuOtherService.Insert(model);

               
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SaveOther");
             
            }
            return Json(1);
        }

        [HttpPost]
        public IActionResult SaveComment(tbl_fu_comment model)
        {

            var data = 1;
            //if (model.id > 0)
            //{
            //    data = model.id.Value;
            //    _ieService.UpdateComment(model);
            //}
            //else
            try
            {
                _fuCommentService.Insert(model);
            }
            catch(Exception ex)
            {
                SaveLog(ex, "SaveComment");
            }
            return Json(data);
        }

        #region POC method

        [HttpPost]
        public IActionResult GetPOC(int patientIEId, int patientFUId)
        {
            try
            {
                var injurbodyparts = _pocService.GetInjuredParts(patientIEId, patientFUId);

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
                    var x = _pocService.GetAllProceduresFU(iinew.Trim(), patientFUId, potion, cmpid.Value);
                    //var x = _pocService.GetAllProcedures(iinew.Trim(), patientIEId, potion);

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
                                        html.Append("<th scope=\"col\" style='height: 35px; background-color:yellow'>");
                                        html.Append(ii.ToUpperInvariant());
                                        html.Append("</th>");
                                    }
                                    else
                                    {

                                        if (column.ColumnName != "Followup")
                                        {
                                            html.Append("<th scope=\"col\" style='height: 35px;'>");
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
                                                    html.Append("<td>");
                                                    html.Append("<input type='text' class=\"form-control\"  onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "'  data-toggle='tooltip' title='" + notify + "'  data-Sides='" + row[27] + "' data-Procedure_Detail_ID='" + row[30] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "'data-ReqPos='" + row[19] + "' data-Bodyid='" + row[7] + "' data-Position='Request' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "'  class='ProcText' id='" + row[0] + "_R_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                    html.Append("</td>");
                                                }
                                                else
                                                {
                                                    html.Append("<td >");
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
                                                    html.Append("<td >");
                                                    html.Append("<input type='text' class='ProcText form-control' onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-PID='" + row[0] + "'data-ReqPos='" + row[20] + "'data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Schedule' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "'  data-Medication='" + row[9] + "'  data-Date='" + date1 + "' data-MCode='" + row[1] + "'   id='" + row[0] + "_S_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                    html.Append("</td>");
                                                }
                                                else
                                                {
                                                    html.Append("<td >");
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
                                                    html.Append("<td>");
                                                    html.Append("<input type='text' class='form-control dateval' onclick='PopupNE($(this));' data-LevelsDefault='" + row[31] + "' data-SidesDefault='" + row[32] + "' data-toggle='tooltip' title='" + notify + "' data-Procedure_Detail_ID='" + row[30] + "' data-Sides='" + row[27] + "' data-HasSides ='" + row[28] + "' data-PPID='" + row[10] + "' data-PatientIEID='" + row[24] + "' data-PatientFUID='" + row[25] + "'data-PID='" + row[0] + "'data-ReqPos='" + row[22] + "' data-Level='" + row[13] + "' data-Medi='" + row[11] + "' data-Musc='" + row[12] + "' data-SubPID='" + row[23] + "' data-Body='" + row[8] + "' data-Bodyid='" + row[7] + "' data-Position='Execute' data-HasLevel='" + row[4] + "' data-Pos='" + row[3] + "' data-Muscle='" + row[5] + "' data-Subcode='" + row[6] + "' data-InhouseProc='" + row[2] + "' data-Medication='" + row[9] + "' data-SignPath='" + row[33] + "'    data-Date='" + date1 + "' data-MCode='" + row[1] + "'   class='ProcText' id='" + row[0] + "_E_" + row[1] + "_" + row[2] + "' value='" + date1 + "' />");
                                                    html.Append("</td>");
                                                }
                                                else
                                                {
                                                    html.Append("<td>");
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
                                            html.Append("<td>");
                                            html.Append("<input type='button' class='btn btn-warning btn-sm' style='margin-left:25px' onclick='CountPopup($(this));' data-Div='" + ii + "_counttable' data-Procedure_Detail_ID='" + row[30] + "' data-PatientIEID='" + row[24] + "' data-PID='" + row[0] + "' value='" + row[26] + "'  />");
                                            html.Append("</td>");
                                        }
                                        else if (column.ColumnName == "MCODE")
                                        {
                                            html.Append("<td  data-bs-toggle='tooltip' data-bs-placement='top'  title='" + row["mcode_desc"] + "' style='text-align:center; background-color:#3de33d'>");
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
                
            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetPOC");
            }
            return PartialView("_POC");
        }

        public string GetMuscleFromDB(int patientIEId)
        {
            string test = _pocService.GetMuscle(patientIEId);

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

        public string GetSubCodeFromDB(int patientIEId)
        {
            DataTable dt = new DataTable();
            try
            {
                string test = _pocService.GetSubCode(patientIEId);               
                string[] lines = test.Split('\n');
                dt.Columns.Add("SubCode");
                for (int i = 0; i < lines.Length; i++)
                {
                    DataRow row = dt.NewRow();
                    row[0] = lines[i];
                    dt.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "GetSubCodeFromDB");
            }
            string json = JsonConvert.SerializeObject(dt);
            return json;
        }

        public string GetMedicationFromDB(int patientIEId)
        {
            DataTable dt = new DataTable();
            try
            {
                string test = _pocService.GetMedication(patientIEId);

                string[] lines = test.Split('\n');
                dt.Columns.Add("Medicaton");
                for (int i = 0; i < lines.Length; i++)
                {
                    DataRow row = dt.NewRow();
                    row[0] = lines[i];
                    dt.Rows.Add(row);
                }
            }
            catch(Exception ex)
            {
                SaveLog(ex, "GetMedicationFromDB");
            }
            string json = JsonConvert.SerializeObject(dt);
            return json;
        }

        [HttpPost]
        public IActionResult SavePOC(ProcedureDetailsVMNew model)
        {
            string Req_Pos = ""; string Sch_Pos = ""; string Exe_Pos = ""; string FU_Pos = "", result = "0";

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
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SavePOC");
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

                            else if (column.ColumnName == "MCODE")
                            {
                                html.Append("<td style='text-align:center; background-color:#3de33d'>");
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
        [HttpPost]
        public IActionResult Delete(int ProcedureDetailID)
        {
            var data = _pocService.RemoveProcedureCountDetails(ProcedureDetailID);
            return Json(data);
        }

        [HttpPost]
        public IActionResult DeleteFU(int fuId)
        {
            try
            {
                _patientFuservices.Delete(fuId);
            }
            catch(Exception ex)
            {
                SaveLog(ex, "DeleteFU");
            }
            return Json(1);
        }


        [HttpPost]
        public IActionResult GetDaignoCodeList(string bodyparts)
        {
               bodyparts = bodyparts.Replace("_", " ");
                ViewBag.BodyPart = bodyparts.ToUpper();
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

                string cnd = " and (cmp_id=" + cmpid + " and BodyPart='" + bodyparts + "') or Description like '%" + bodyparts + "%'";

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
            return PartialView("_DaignoCode", datavm);
        }

        #endregion

        #region POCSumarry
        [HttpPost]
        public IActionResult POCSummary(int patientFUId)
        {
            try
            {
                var Data = _pocService.GetFUPOCSummary(patientFUId);
                return PartialView("_POCSummary", Data);

            }
            catch (Exception ex)
            {
                SaveLog(ex, "POCSummary");
                return View();
            }
            
        }

        [HttpPost]
        public IActionResult SurgeryPOCSummary(int patientIEId)
        {
            var Data = _pocService.GetExecutedPOCIE(patientIEId);
            try
            {
                int? ieId = HttpContext.Session.GetInt32(SessionKeys.SessionIEId);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SurgeryPOCSummary");
             
            }
            return PartialView("_POCSummary", Data);
        }
        #endregion

        #region PrintFU

        public IActionResult FUPrint(int ieid, int fuid)
        {

            try
            {
                ViewBag.url = HttpContext.Request.GetEncodedUrl();

                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();
                string body = string.Empty;
                //using streamreader for reading my htmltemplate   

                var fuData = _patientFuservices.GetOne(fuid);

                var templateData = _printService.GetTemplate(cmpid, fuData.type);
                var gender = "";

                body = templateData.content;

                var patientData = _ieService.GetOnebyPatientId(ieid);

                if (patientData != null)
                {
                    gender = Common.GetMrMrsFromSex(patientData.gender);

                    body = body.Replace("#patientname", gender + " " + patientData.fname + " " + patientData.mname + " " + patientData.lname);
                    body = body.Replace("#dob", Common.commonDate(patientData.dob));
                    body = body.Replace("#doi", Common.commonDate(patientData.doa));
                    body = body.Replace("#dos", Common.commonDate(fuData.doe, HttpContext.Session.GetString(SessionKeys.SessionDateFormat)));
                    body = body.Replace("#location", patientData.location);
                    body = body.Replace("#age", patientData.age == null ? "0" : patientData.age.Value.ToString());
                    body = body.Replace("#gender", Common.GetGenderFromSex(patientData.gender));


                    // body = body.Replace("#CT", System.Enum.GetName(typeof(CaseType), Convert.ToInt32(patientData.compensation)));
                    body = body.Replace("#CT", patientData.compensation);
                }

                //header printing

                var locData = _locService.GetAll(" and id=" + patientData.location_id);

                if (locData != null && locData.Count > 0)
                {
                    body = body.Replace("#drName", locData[0].nameofpractice.ToLower().Contains("dr") ? locData[0].nameofpractice : "Dr. " + locData[0].nameofpractice);
                    body = body.Replace("#address", locData[0].address + "<br/>" + locData[0].city + ", " + locData[0].state + " " + locData[0].zipcode);
                }

                //ADL printing

                var page2Data = _fuPage2services.GetOne(fuid);

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


                var page1DataIE = _ieService.GetOnePage1(ieid);

                //CC printing

                var page1Data = _fuPage1services.GetOne(fuid);
                if (page1Data != null)
                {
                    body = body.Replace("#CC", string.IsNullOrEmpty(page1Data.cc) ? "" : this.removePtag(page1Data.cc));
                    body = body.Replace("#PE", string.IsNullOrEmpty(page1Data.pe) ? "" : page1Data.pe);
                    body = body.Replace("#history", string.IsNullOrEmpty(page1DataIE.history) ? "" : page1DataIE.history);



                    body = body.Replace("#bodypart", string.IsNullOrEmpty(page1Data.bodypart) ? "" : page1Data.bodypart.ToLower());

                    string bodypart = "";

                    if (!string.IsNullOrEmpty(page1Data.bodypart))
                        bodypart = Common.ReplceCommowithAnd(page1Data.bodypart);

                    body = body.Replace("#PC", string.IsNullOrEmpty(bodypart) ? "" : Common.FirstCharToUpper(bodypart) + " Pain.");


                    body = body.Replace("#PastMedicalHistory", this.removePtag(page1DataIE.pmh));
                    body = body.Replace("#PastSurgicalHistory", this.removePtag(page1DataIE.psh));
                    body = body.Replace("#SocialHistory", this.removePtag(page1DataIE.social_history));
                    body = body.Replace("#Allergies", this.removePtag(page1DataIE.allergies));
                    body = body.Replace("#FamilyHistory", this.removePtag(page1DataIE.family_history));
                    body = body.Replace("#Vital", this.removePtag(page1DataIE.vital));
                    body = body.Replace("#Diagnoses", this.removePtag(page1Data.assessment));
                    body = body.Replace("#Occupation", this.removePtag(page1Data.occupation));
                    body = body.Replace("#PastMedications", this.removePtag(page1DataIE.medication));
                    body = body.Replace("#DD", this.removePtag(page1Data.dd));
                    body = body.Replace("#WorkStatus", this.removePtag(page1Data.work_status));
                    // body = body.Replace("#IR", this.removePtag(page1Data.impairment_rating));

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

                var pageNEData = _fuNEservices.GetOne(fuid);
                if (pageNEData != null)
                {

                    body = body.Replace("#Sen_Exm", this.removePtag(pageNEData.sensory));

                    body = body.Replace("#MMST", this.removePtag(pageNEData.manual_muscle_strength_testing));

                    body = body.Replace("#NE", this.removePtag(pageNEData.neurological_exam));

                    body = body.Replace("#DTR", this.removePtag(pageNEData.other_content));

                }
                else
                {
                    body = body.Replace("#DTR", "");
                    body = body.Replace("#NE", "");
                    body = body.Replace("#MMST", "");
                    body = body.Replace("#Sen_Exm", "");
                }

                //NE printing

                var page3Data = _fuPage3services.GetOne(fuid);
                if (page3Data != null)
                {
                    body = body.Replace("#Gait", this.removePtag(page3Data.gait));
                    body = body.Replace("#Care", this.removePtag(page3Data.care));
                    body = body.Replace("#Procedures", this.removePtag(page3Data.universal));
                    body = body.Replace("#Goal", this.removePtag(page3Data.goal));
                }
                else
                {
                    body = body.Replace("#Goal", "");
                    body = body.Replace("#Procedures", "");
                    body = body.Replace("#Care", "");
                    body = body.Replace("#Gait", "");
                }

                //Treatment printing

                var pageOtherData = _fuOtherService.GetOne(fuid);
                if (pageOtherData != null)
                {

                    body = body.Replace("#Treatment", this.removePtag(pageOtherData.treatment_details));
                    body = body.Replace("#note1", this.removePtag(pageOtherData.note1));
                    body = body.Replace("#note2", this.removePtag(pageOtherData.note2));
                    body = body.Replace("#note3", this.removePtag(pageOtherData.note3));


                    string fup_duration = "";
                    if (!string.IsNullOrEmpty(pageOtherData.followup_duration))
                        fup_duration = pageOtherData.followup_duration;
                    else if (pageOtherData.followup_date.HasValue)
                        fup_duration = Common.commonDate(pageOtherData.followup_date);


                    body = body.Replace("#FollowUp", this.removePtag(fup_duration));
                }
                else
                {
                    body = body.Replace("#FollowUp", "");
                    body = body.Replace("#Treatment", "");
                }

                //POC printing

                var strPOC = this.getPOC(fuid);


                body = body.Replace("#Plan", this.removePtag(strPOC));


                body = body.Replace("#ReflexExam", "");


                var strDiagnostic = this.getDiagnostic(fuid);


                if (string.IsNullOrEmpty(strDiagnostic))
                    strDiagnostic = "None Reviewed";
                else
                    strDiagnostic = strDiagnostic + "<br/><br/>The above diagnostic studies were reviewed.";


                body = body.Replace("#Diagnostic", this.removePtag(strDiagnostic));


                var data = _fuPage3services.GetOne(fuid);

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
                ViewBag.content = body;

            }
            catch (Exception ex)
            {
                SaveLog(ex, "FUPrint");
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

        [HttpPost]
        public IActionResult DownloadWord(string htmlContent, int ieId)
        {
            //  string htmlContent = "<p>This is a <strong>sample</strong> HTML content.</p>";
            string filePath = "", docName = "";
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

                    // Convert HTML to OpenXML and add to the body
                    HtmlConverter converter = new HtmlConverter(mainPart);
                    var generatedBody = converter.Parse(htmlContent);
                    body.Append(generatedBody);


                    var header = new Header(new Paragraph(new Run(new Text("Header Test"))));
                    HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPart) };
                    var footer = new Footer(new Paragraph(new Run(new Text("Page"), new SimpleField() { Instruction = "PAGE" })));
                    FooterReference footerReference = new FooterReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) };

                    headerPart.Header = header;
                    footerPart.Footer = footer;
                    mainPart.Document.Body.Append(new SectionProperties(headerReference, footerReference));

                }
                string cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId).ToString();

                var patientData = _ieService.GetOnebyPatientId(ieId);

                docName = patientData.lname + "," + patientData.fname + "_FU_" + Common.commonDate(patientData.doe).Replace("/", "") + ".docx";

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
            }

            return Json(new { filePath = filePath, fileName = docName });
        }

        [HttpGet]
        public virtual ActionResult DownloadFile(string filePath, string fileName)
        {
            string filepathFrom = @"HeaderTemplate/1/IE_Template.dotx";
            string filepathTo = filePath;
            AddHeaderFromTo(filepathFrom, filepathTo);
            byte[] data = System.IO.File.ReadAllBytes(filePath);
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        }

        public static void AddHeaderFromTo(string filepathFrom, string filepathTo)
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

                // Get SectionProperties and Replace HeaderReference with new Id.
                IEnumerable<DocumentFormat.OpenXml.Wordprocessing.SectionProperties> sectPrs =
            mainPart.Document.Body.Elements<SectionProperties>();
                foreach (var sectPr in sectPrs)
                {
                    // Delete existing references to headers.
                    sectPr.RemoveAllChildren<HeaderReference>();

                    // Create the new header reference node.
                    sectPr.PrependChild<HeaderReference>(new HeaderReference() { Id = rId });
                }
            }
        }

        #endregion

        #region Private Method
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
        private string getPOC(int PatientFU_ID)
        {
            DataTable dsPOC = _pocService.GetPOCFU(PatientFU_ID);

            //var data = _pocService.GetPOCSummary(PatientIE_ID);

            string strPoc = "<ol>";
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

                        if (heading.ToLower().Contains("(side)"))
                        {
                            heading = heading.Replace("(side)", dsPOC.Rows[i]["Sides"].ToString());
                        }

                        if (heading.ToLower().Contains("(levels)"))
                        {
                            heading = heading.Replace("(levels)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        if (heading.ToLower().Contains("(level)"))
                        {
                            heading = heading.Replace("(level)", dsPOC.Rows[i]["Level"].ToString());
                        }

                        strPoc = strPoc + "<li><b style='text-transform:uppercase'>" + heading.TrimEnd(':') + ": </b>" + dsPOC.Rows[i]["PDesc"].ToString() + "</li>";
                    }
                }
            }
            return strPoc + "</ol>";
        }

        private string getDiagnostic(int id)
        {
            var data = _fuPage3services.GetOne(id);

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
                        strDaignosis = strDaignosis + " of the left shoulder " + data.diagleftshoulder_text.TrimEnd('.') + ". ";
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
                        strDaignosis = strDaignosis + " of the right shoulder " + data.diagrightshoulder_text.TrimEnd('.') + ". ";
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
                        strDaignosis = strDaignosis + " of the left knee " + data.diagleftknee_text.TrimEnd('.') + ". ";
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
                        strDaignosis = strDaignosis + " of the right knee " + data.diagrightknee_text.TrimEnd('.') + ". ";
                    }
                    else
                    {
                        strDaignosis = strDaignosis + " of the right knee is normal. ";
                    }
                }

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
    }
}
