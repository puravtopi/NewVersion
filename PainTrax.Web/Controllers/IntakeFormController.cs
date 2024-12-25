using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Asn1.Sec;
using PainTrax.Web.Filter;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using System.Data;
using System.Text.RegularExpressions;

namespace PainTrax.Web.Controllers
{
    public class IntakeFormController : Controller
    {
        private readonly Common _commonservices = new Common();
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

            ViewBag.locList = _commonservices.GetLocations(cmpid.Value);

            IntakeForm obj = new IntakeForm();
            return View(obj);
        }
        [HttpPost]
        public IActionResult Create(IntakeForm model)
        {

            string what_test = "", medical_condition = "", social_history = "", symptoms_since_accident = "",
                cc_neck = "",cc_midback="",cc_lowback="",cc_l_shoulder="",cc_r_shoulder="";

            if (!string.IsNullOrEmpty(model.what_tests_xray))
                what_test = what_test + "," + model.what_tests_xray;
            if (!string.IsNullOrEmpty(model.what_tests_ct))
                what_test = what_test + "," + model.what_tests_ct;
            if (!string.IsNullOrEmpty(model.what_tests_mri))
                what_test = what_test + "," + model.what_tests_mri;

            model.what_test = what_test.TrimStart(',');

            if (!string.IsNullOrEmpty(model.any_medical_conditions_Diabeties))
                medical_condition = medical_condition + "," + model.any_medical_conditions_Diabeties;
            if (!string.IsNullOrEmpty(model.any_medical_conditions_bp))
                medical_condition = medical_condition + "," + model.any_medical_conditions_bp;
            if (!string.IsNullOrEmpty(model.any_medical_conditions_ashthma))
                medical_condition = medical_condition + "," + model.any_medical_conditions_ashthma;
            if (!string.IsNullOrEmpty(model.any_medical_conditions_heart))
                medical_condition = medical_condition + "," + model.any_medical_conditions_heart;
            if (!string.IsNullOrEmpty(model.any_medical_conditions_none))
                medical_condition = medical_condition + "," + model.any_medical_conditions_none;

            model.any_medical_conditions = medical_condition.TrimStart(',');

            if (!string.IsNullOrEmpty(model.smoke))
            {
                if (model.smoke == "Yes")
                {
                    if (!string.IsNullOrEmpty(model.txt_smoke))
                        social_history = social_history + ", Smoke for " + model.txt_smoke;
                    else
                        social_history = social_history + ", Smoke";
                }
                else if (model.smoke == "No")
                {
                    social_history = social_history + ", No Smoke";
                }
            }

            if (!string.IsNullOrEmpty(model.marijuana))
            {
                if (model.marijuana == "Yes")
                {
                    if (!string.IsNullOrEmpty(model.txt_marijuana))
                        social_history = social_history + ", marijuana for " + model.txt_marijuana;
                    else
                        social_history = social_history + ", marijuana";
                }
                else if (model.marijuana == "No")
                {
                    social_history = social_history + ", No marijuana";
                }
            }

            if (!string.IsNullOrEmpty(model.marijuana))
            {
                if (model.marijuana == "Yes")
                {
                    if (!string.IsNullOrEmpty(model.txt_marijuana))
                        social_history = social_history + ", marijuana for " + model.txt_marijuana;
                    else
                        social_history = social_history + ", marijuana";
                }
                else if (model.marijuana == "No")
                {
                    social_history = social_history + ", No marijuana";
                }
            }

            if (!string.IsNullOrEmpty(model.alcohol))
            {
                if (model.alcohol == "Yes")
                {
                    if (!string.IsNullOrEmpty(model.alcohol))
                        social_history = social_history + ", alcohol for " + model.alcohol;
                    else
                        social_history = social_history + ", alcohol";
                }
                else if (model.alcohol == "No")
                {
                    social_history = social_history + ", No alcohol";
                }
            }


            model.social_history = social_history.TrimStart(',');


            if (model.symptoms_of_accident_Headaches == "true")
                symptoms_since_accident = symptoms_since_accident + ",Headaches";
            if (model.symptoms_of_accident_ChestPain == "true")
                symptoms_since_accident = symptoms_since_accident + ",Chest Pain/Short of Breath";
            if (model.symptoms_of_accident_Abdominal == "true")
                symptoms_since_accident = symptoms_since_accident + ",Abdominal Pain";
            if (model.symptoms_of_accident_Muscle == "true")
                symptoms_since_accident = symptoms_since_accident + ",Muscle Spasms";
            if (model.symptoms_of_accident_Dizziness == "true")
                symptoms_since_accident = symptoms_since_accident + ",Dizziness";
            if (model.symptoms_of_accident_Nausea == "true")
                symptoms_since_accident = symptoms_since_accident + ",Nausea";
            if (model.symptoms_of_accident_Ringing_in_ears == "true")
                symptoms_since_accident = symptoms_since_accident + ",Ringing in Ears";
            if (model.symptoms_of_accident_Bladder == "true")
                symptoms_since_accident = symptoms_since_accident + ",Bladder Incontinence";
            if (model.symptoms_of_accident_Bowel == "true")
                symptoms_since_accident = symptoms_since_accident + ",Bowel Incontinence";
            if (model.symptoms_of_accident_Seizure == "true")
                symptoms_since_accident = symptoms_since_accident + ",Seizure";
            if (model.symptoms_of_accident_Sleep_issues == "true")
                symptoms_since_accident = symptoms_since_accident + ",Sleep Issues/Difficulty";
            if (model.symptoms_of_accident_Anxiety == "true")
                symptoms_since_accident = symptoms_since_accident + ",Anxiety/Depression";


            model.symptoms_since_accident = symptoms_since_accident.TrimStart(',');


            if (model.describe_neck_Constant == "true")
                cc_neck = cc_neck + ",Constant";
            if (model.describe_neck_Intermittent == "true")
                cc_neck = cc_neck + ",Intermittent";
            if (model.describe_neck_Sharp == "true")
                cc_neck = cc_neck + ",Sharp";
            if (model.describe_neck_Electric == "true")
                cc_neck = cc_neck + ",Electric";
            if (model.describe_neck_Shooting == "true")
                cc_neck = cc_neck + ",Shooting";
            if (model.describe_neck_Throbbing == "true")
                cc_neck = cc_neck + ",Throbbing";
            if (model.describe_neck_Pulsating == "true")
                cc_neck = cc_neck + ",Pulsating";
            if (model.describe_neck_Dull == "true")
                cc_neck = cc_neck + ",Dull";
            if (model.describe_neck_Achy == "true")
                cc_neck = cc_neck + ",Achy";
          
            model.cc_neck = cc_neck.TrimStart(',');

            if (model.describe_midback_Constant == "true")
                cc_midback = cc_midback + ",Constant";
            if (model.describe_midback_Intermittent == "true")
                cc_midback = cc_midback + ",Intermittent";
            if (model.describe_midback_Sharp == "true")
                cc_midback = cc_midback + ",Sharp";
            if (model.describe_midback_Electric == "true")
                cc_midback = cc_midback + ",Electric";
            if (model.describe_midback_Shooting == "true")
                cc_midback = cc_midback + ",Shooting";
            if (model.describe_midback_Throbbing == "true")
                cc_midback = cc_midback + ",Throbbing";
            if (model.describe_midback_Pulsating == "true")
                cc_midback = cc_midback + ",Pulsating";
            if (model.describe_midback_Dull == "true")
                cc_midback = cc_midback + ",Dull";
            if (model.describe_midback_Achy == "true")
                cc_midback = cc_midback + ",Achy";


            model.cc_midback = cc_midback.TrimStart(',');

            if (model.describe_midback_Constant == "true")
                cc_lowback = cc_lowback + ",Constant";
            if (model.describe_midback_Intermittent == "true")
                cc_lowback = cc_lowback + ",Intermittent";
            if (model.describe_midback_Sharp == "true")
                cc_lowback = cc_lowback + ",Sharp";
            if (model.describe_midback_Electric == "true")
                cc_lowback = cc_lowback + ",Electric";
            if (model.describe_midback_Shooting == "true")
                cc_lowback = cc_lowback + ",Shooting";
            if (model.describe_midback_Throbbing == "true")
                cc_lowback = cc_lowback + ",Throbbing";
            if (model.describe_midback_Pulsating == "true")
                cc_lowback = cc_lowback + ",Pulsating";
            if (model.describe_midback_Dull == "true")
                cc_lowback = cc_lowback + ",Dull";
            if (model.describe_midback_Achy == "true")
                cc_lowback = cc_lowback + ",Achy";


            model.cc_midback = cc_midback.TrimStart(',');


            if (model.describe_lowback_Constant == "true")
                cc_lowback = cc_lowback + ",Constant";
            if (model.describe_lowback_Intermittent == "true")
                cc_lowback = cc_lowback + ",Intermittent";
            if (model.describe_lowback_Sharp == "true")
                cc_lowback = cc_lowback + ",Sharp";
            if (model.describe_lowback_Electric == "true")
                cc_lowback = cc_lowback + ",Electric";
            if (model.describe_lowback_Shooting == "true")
                cc_lowback = cc_lowback + ",Shooting";
            if (model.describe_lowback_Throbbing == "true")
                cc_lowback = cc_lowback + ",Throbbing";
            if (model.describe_lowback_Pulsating == "true")
                cc_lowback = cc_lowback + ",Pulsating";
            if (model.describe_lowback_Dull == "true")
                cc_lowback = cc_lowback + ",Dull";
            if (model.describe_lowback_Achy == "true")
                cc_lowback = cc_lowback + ",Achy";


            model.cc_lowback = cc_lowback.TrimStart(',');


            if (model.txt_describe_leftshoulder_Constant == "true")
                cc_l_shoulder = cc_l_shoulder + ",Constant";
            if (model.txt_describe_leftshoulder_Intermittent == "true")
                cc_l_shoulder = cc_l_shoulder + ",Intermittent";
            if (model.txt_describe_leftshoulder_Sharp == "true")
                cc_l_shoulder = cc_l_shoulder + ",Sharp";
            if (model.txt_describe_leftshoulder_Electric == "true")
                cc_l_shoulder = cc_l_shoulder + ",Electric";
            if (model.txt_describe_leftshoulder_Shooting == "true")
                cc_l_shoulder = cc_l_shoulder + ",Shooting";
            if (model.txt_describe_leftshoulder_Throbbing == "true")
                cc_l_shoulder = cc_l_shoulder + ",Throbbing";
            if (model.txt_describe_leftshoulder_Pulsating == "true")
                cc_l_shoulder = cc_l_shoulder + ",Pulsating";
            if (model.txt_describe_leftshoulder_Dull == "true")
                cc_l_shoulder = cc_l_shoulder + ",Dull";
            if (model.txt_describe_leftshoulder_Achy == "true")
                cc_l_shoulder = cc_l_shoulder + ",Achy";


            model.cc_l_shoulder = cc_l_shoulder.TrimStart(',');

            if (model.txt_describe_rightshoulder_Constant == "true")
                cc_r_shoulder = cc_r_shoulder + ",Constant";
            if (model.txt_describe_rightshoulder_Intermittent == "true")
                cc_r_shoulder = cc_r_shoulder + ",Intermittent";
            if (model.txt_describe_rightshoulder_Sharp == "true")
                cc_r_shoulder = cc_r_shoulder + ",Sharp";
            if (model.txt_describe_rightshoulder_Electric == "true")
                cc_r_shoulder = cc_r_shoulder + ",Electric";
            if (model.txt_describe_rightshoulder_Shooting == "true")
                cc_r_shoulder = cc_r_shoulder + ",Shooting";
            if (model.txt_describe_rightshoulder_Throbbing == "true")
                cc_r_shoulder = cc_r_shoulder + ",Throbbing";
            if (model.txt_describe_rightshoulder_Pulsating == "true")
                cc_r_shoulder = cc_r_shoulder + ",Pulsating";
            if (model.txt_describe_rightshoulder_Dull == "true")
                cc_r_shoulder = cc_r_shoulder + ",Dull";
            if (model.txt_describe_rightshoulder_Achy == "true")
                cc_r_shoulder = cc_r_shoulder + ",Achy";


            model.cc_r_shoulder = cc_r_shoulder.TrimStart(',');





            return RedirectToAction("Create", "IntakeForm");
        }
    }
}
