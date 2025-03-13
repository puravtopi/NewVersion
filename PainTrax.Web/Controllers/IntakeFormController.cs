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
    [SessionCheckFilter]
    public class IntakeFormController : Controller
    {
        private readonly Common _commonservices = new Common();
        private readonly IntakeService service=new IntakeService();
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
                cc_neck = "", cc_neck_radiates = "", cc_neck_tingling = "", cc_neck_increase = "", cc_midback = "", cc_midback_increase = "",
                cc_lowback = "", cc_lowback_radiates = "", cc_lowback_tingling = "", cc_lowback_increase = "", cc_l_shoulder = "", cc_l_shoulder_increase = "",
                cc_r_shoulder = "", cc_r_shoulder_increase = "", cc_l_knee = "", cc_l_knee_increase = "",
                cc_r_knee = "", cc_r_knee_increase = "", cc_other_1 = "", cc_other_2 = "";

            if (model.what_tests_xray=="true")
                what_test = what_test + "," + model.what_tests_xray;
            if (model.what_tests_ct=="true")
                what_test = what_test + "," + model.what_tests_ct;
            if (model.what_tests_mri=="true")
                what_test = what_test + "," + model.what_tests_mri;

            model.what_test = what_test.TrimStart(',');

            if (model.any_medical_conditions_Diabeties=="true")
                medical_condition = medical_condition + "," + model.any_medical_conditions_Diabeties;
            if (model.any_medical_conditions_bp=="true")
                medical_condition = medical_condition + "," + model.any_medical_conditions_bp;
            if (model.any_medical_conditions_ashthma=="true")
                medical_condition = medical_condition + "," + model.any_medical_conditions_ashthma;
            if (model.any_medical_conditions_heart=="true")
                medical_condition = medical_condition + "," + model.any_medical_conditions_heart;
            if (model.any_medical_conditions_none == "true")
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

            if (!string.IsNullOrEmpty(model.txt_describe_neck))
                cc_neck = "The patient complains of neck pain that is " + model.txt_describe_neck + "/10, with 10 being the worst , which is ";

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

            model.cc_neck = cc_neck.TrimStart(',') + ".";

            if (model.neck_pain_radiates_RUE == "true")
                cc_neck_radiates = cc_neck_radiates + ",RUE";
            if (model.neck_pain_radiates_LUE == "true")
                cc_neck_radiates = cc_neck_radiates + ",LUE";
            if (model.neck_pain_radiates_BUE == "true")
                cc_neck_radiates = cc_neck_radiates + ",BUE";
            if (model.neck_pain_numbness == "true")
                cc_neck_radiates = cc_neck_radiates + ",numbness";

            if (!string.IsNullOrEmpty(cc_neck_radiates))
                model.cc_neck = model.cc_neck + " Radiates To " + cc_neck_radiates.TrimStart(',') + ".";

            if (model.neck_pain_bodypart_shoulder == "true")
                cc_neck_tingling = cc_neck_tingling + ",shoulder";
            if (model.neck_pain_bodypart_elbow == "true")
                cc_neck_tingling = cc_neck_tingling + ",elbow";
            if (model.neck_pain_bodypart_hand == "true")
                cc_neck_tingling = cc_neck_tingling + ",hand";
            if (model.neck_pain_bodypart_wrist == "true")
                cc_neck_tingling = cc_neck_tingling + ",wrist";
            if (model.neck_pain_bodypart_finger == "true")
                cc_neck_tingling = cc_neck_tingling + ",finger";

            if (!string.IsNullOrEmpty(cc_neck_tingling))
                model.cc_neck = model.cc_neck + " Tingling To " + cc_neck_tingling.TrimStart(',') + ".";


            if (model.increase_neck_pain_lookingup == "true")
                cc_neck_increase = cc_neck_increase + ",looking up";
            if (model.increase_neck_pain_lookingdown == "true")
                cc_neck_increase = cc_neck_increase + ",looking down";
            if (model.increase_neck_pain_turningheadright == "true")
                cc_neck_increase = cc_neck_increase + ",turning head to right";
            if (model.increase_neck_pain_turningheadleft == "true")
                cc_neck_increase = cc_neck_increase + ",turning head to left";
            if (model.increase_neck_pain_driving == "true")
                cc_neck_increase = cc_neck_increase + ",driving";
            if (model.increase_neck_pain_twisting == "true")
                cc_neck_increase = cc_neck_increase + ",twisting";

            if (!string.IsNullOrEmpty(cc_neck_increase))
                model.cc_neck = model.cc_neck + " Pain increases by " + cc_neck_increase.TrimStart(',') + ".";

            if (!string.IsNullOrEmpty(model.txt_describe_midback))
                cc_midback = "The patient complains of midback pain that is " + model.txt_describe_midback + "/10, with 10 being the worst , which is ";


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


            if (model.increase_midback_pain_sitting == "true")
                cc_midback_increase = cc_midback_increase + ",sitting";
            if (model.increase_midback_pain_standing == "true")
                cc_midback_increase = cc_midback_increase + ",standing";
            if (model.increase_midback_pain_bendingforward == "true")
                cc_midback_increase = cc_midback_increase + ",bending forward";
            if (model.increase_midback_pain_bendingbackwards == "true")
                cc_midback_increase = cc_midback_increase + ", bending backwards";
            if (model.increase_midback_pain_sleeping == "true")
                cc_midback_increase = cc_midback_increase + ",sleeping";
            if (model.increase_midback_pain_twisting == "true")
                cc_midback_increase = cc_midback_increase + ",twisting";
            if (model.increase_midback_pain_lifting == "true")
                cc_midback_increase = cc_midback_increase + ",lifting";





            if (!string.IsNullOrEmpty(cc_lowback_increase))
                model.cc_midback = model.cc_midback + " Pain increases by " + cc_midback_increase.TrimStart(',') + ".";


            if (!string.IsNullOrEmpty(model.txt_describe_lowback))
                cc_lowback = "The patient complains of lowback pain that is " + model.txt_describe_lowback + "/10, with 10 being the worst , which is ";

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




            if (model.lowback_pain_radiates_RLE == "true")
                cc_lowback_radiates = cc_lowback_radiates + ",RLE";
            if (model.lowback_pain_radiates_LLE == "true")
                cc_lowback_radiates = cc_lowback_radiates + ",LLE";
            if (model.lowback_pain_radiates_BLE == "true")
                cc_lowback_radiates = cc_lowback_radiates + ",BLE";
            if (model.lowback_pain_numbness == "true")
                cc_lowback_radiates = cc_lowback_radiates + ",numbness";

            if (!string.IsNullOrEmpty(cc_lowback_radiates))
                model.cc_lowback = model.cc_lowback + " Radiates To " + cc_lowback_radiates.TrimStart(',') + ".";

            if (model.lowback_pain_bodypart_thigh == "true")
                cc_lowback_tingling = cc_lowback_tingling + ",thigh";
            if (model.lowback_pain_bodypart_knee == "true")
                cc_lowback_tingling = cc_lowback_tingling + ",knee";
            if (model.lowback_pain_bodypart_leg == "true")
                cc_lowback_tingling = cc_lowback_tingling + ",leg";
            if (model.lowback_pain_bodypart_ankle == "true")
                cc_lowback_tingling = cc_lowback_tingling + ",ankle";
            if (model.lowback_pain_bodypart_foot == "true")
                cc_lowback_tingling = cc_lowback_tingling + ",foot";
            if (model.lowback_pain_bodypart_toe == "true")
                cc_lowback_tingling = cc_lowback_tingling + ",toe";


            if (!string.IsNullOrEmpty(cc_lowback_tingling))
                model.cc_lowback = model.cc_lowback + " Tingling To " + cc_lowback_tingling.TrimStart(',') + ".";


            if (model.increase_lowback_pain_sitting == "true")
                cc_lowback_increase = cc_lowback_increase + ",sitting";
            if (model.increase_lowback_pain_standing == "true")
                cc_lowback_increase = cc_lowback_increase + ",standing";
            if (model.increase_lowback_pain_bending_forward == "true")
                cc_lowback_increase = cc_lowback_increase + ",bending forward";
            if (model.increase_lowback_pain_bending_backwards == "true")
                cc_lowback_increase = cc_lowback_increase + ",bending backwards";
            if (model.increase_lowback_pain_sleeping == "true")
                cc_lowback_increase = cc_lowback_increase + ",sleeping";
            if (model.increase_lowback_pain_twisting_right == "true")
                cc_lowback_increase = cc_lowback_increase + ",twisting right";
            if (model.increase_lowback_pain_twisting_left == "true")
                cc_lowback_increase = cc_lowback_increase + ",twisting left";
            if (model.increase_lowback_pain_lifting == "true")
                cc_lowback_increase = cc_lowback_increase + ",lifting";



            if (!string.IsNullOrEmpty(cc_lowback_increase))
                model.cc_lowback = model.cc_lowback + " Pain increases by " + cc_lowback_increase.TrimStart(',') + ".";

            if (!string.IsNullOrEmpty(model.describe_leftshoulder))
                cc_l_shoulder = "The patient complains of left shoulder pain that is " + model.describe_leftshoulder + "/10, with 10 being the worst , which is ";


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



            if (model.increase_leftshoulder_pain_Raising_arm == "true")
                cc_l_shoulder_increase = cc_l_shoulder_increase + ",Raising arm";
            if (model.increase_leftshoulder_pain_Lifting == "true")
                cc_l_shoulder_increase = cc_l_shoulder_increase + ",Lifting";
            if (model.increase_leftshoulder_pain_Working == "true")
                cc_l_shoulder_increase = cc_l_shoulder_increase + ",Working";
            if (model.increase_leftshoulder_pain_Rotation == "true")
                cc_l_shoulder_increase = cc_l_shoulder_increase + ",Rotation";
            if (model.increase_leftshoulder_pain_Overhead_activities == "true")
                cc_l_shoulder_increase = cc_l_shoulder_increase + ",Overhead activities";


            if (!string.IsNullOrEmpty(cc_l_shoulder_increase))
                model.cc_l_shoulder = model.cc_l_shoulder + " Pain increases by " + cc_l_shoulder_increase.TrimStart(',') + ".";

            if (!string.IsNullOrEmpty(model.describe_rightshoulder))
                cc_r_shoulder = "The patient complains of right shoulder pain that is " + model.describe_rightshoulder + "/10, with 10 being the worst , which is ";


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

            if (model.increase_rightshoulder_pain_Raising_arm == "true")
                cc_r_shoulder_increase = cc_r_shoulder_increase + ",Raising arm";
            if (model.increase_rightshoulder_pain_Lifting == "true")
                cc_r_shoulder_increase = cc_r_shoulder_increase + ",Lifting";
            if (model.increase_rightshoulder_pain_Working == "true")
                cc_r_shoulder_increase = cc_r_shoulder_increase + ",Working";
            if (model.increase_rightshoulder_pain_Rotation == "true")
                cc_r_shoulder_increase = cc_r_shoulder_increase + ",Rotation";
            if (model.increase_rightshoulder_pain_Overhead_activities == "true")
                cc_r_shoulder_increase = cc_r_shoulder_increase + ",Overhead activities";


            if (!string.IsNullOrEmpty(cc_r_shoulder_increase))
                model.cc_r_shoulder = model.cc_r_shoulder + " Pain increases by " + cc_r_shoulder_increase.TrimStart(',') + ".";


            if (!string.IsNullOrEmpty(model.describe_leftknee))
                cc_l_knee = "The patient complains of left knee pain that is " + model.describe_leftknee + "/10, with 10 being the worst , which is ";


            if (model.txt_describe_leftknee_Constant == "true")
                cc_l_knee = cc_l_knee + ",Constant";
            if (model.txt_describe_leftknee_Intermittent == "true")
                cc_l_knee = cc_l_knee + ",Intermittent";
            if (model.txt_describe_leftknee_Sharp == "true")
                cc_l_knee = cc_l_knee + ",Sharp";
            if (model.txt_describe_leftknee_Electric == "true")
                cc_l_knee = cc_l_knee + ",Electric";
            if (model.txt_describe_leftknee_Shooting == "true")
                cc_l_knee = cc_l_knee + ",Shooting";
            if (model.txt_describe_leftknee_Throbbing == "true")
                cc_l_knee = cc_l_knee + ",Throbbing";
            if (model.txt_describe_leftknee_Pulsating == "true")
                cc_l_knee = cc_l_knee + ",Pulsating";
            if (model.txt_describe_leftknee_Dull == "true")
                cc_l_knee = cc_l_knee + ",Dull";
            if (model.txt_describe_leftknee_Achy == "true")
                cc_l_knee = cc_l_knee + ",Achy";

            model.cc_l_knee = cc_l_knee.TrimStart(',');

            if (model.increase_leftknee_pain_Squatting == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",Squatting";
            if (model.increase_leftknee_pain_Walking == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",Walking";
            if (model.increase_leftknee_pain_Climb == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",Climb stairs";
            if (model.increase_leftknee_pain_goingdown_stairs == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",going down stairs";
            if (model.increase_leftknee_pain_Standing == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",Standing";
            if (model.increase_leftknee_pain_getupfrom_chair == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",get up from chair";
            if (model.increase_leftknee_pain_getoutof_car == "true")
                cc_l_knee_increase = cc_l_knee_increase + ",get out of car";



            if (!string.IsNullOrEmpty(cc_l_knee_increase))
                model.cc_l_knee = model.cc_l_knee + " Pain increases by " + cc_l_knee_increase.TrimStart(',') + ".";


            if (!string.IsNullOrEmpty(model.describe_rightknee))
                cc_l_knee = "The patient complains of right right pain that is " + model.describe_rightknee + "/10, with 10 being the worst , which is ";


            if (model.txt_describe_rightknee_Constant == "true")
                cc_r_knee = cc_r_knee + ",Constant";
            if (model.txt_describe_rightknee_Intermittent == "true")
                cc_r_knee = cc_r_knee + ",Intermittent";
            if (model.txt_describe_rightknee_Sharp == "true")
                cc_r_knee = cc_r_knee + ",Sharp";
            if (model.txt_describe_rightknee_Electric == "true")
                cc_r_knee = cc_r_knee + ",Electric";
            if (model.txt_describe_rightknee_Shooting == "true")
                cc_r_knee = cc_r_knee + ",Shooting";
            if (model.txt_describe_rightknee_Throbbing == "true")
                cc_r_knee = cc_r_knee + ",Throbbing";
            if (model.txt_describe_rightknee_Pulsating == "true")
                cc_r_knee = cc_r_knee + ",Pulsating";
            if (model.txt_describe_rightknee_Dull == "true")
                cc_r_knee = cc_r_knee + ",Dull";
            if (model.txt_describe_rightknee_Achy == "true")
                cc_r_knee = cc_r_knee + ",Achy";

            model.cc_r_knee = cc_r_knee.TrimStart(',');

            if (model.increase_rightknee_pain_Squatting == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",Squatting";
            if (model.increase_rightknee_pain_Walking == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",Walking";
            if (model.increase_rightknee_pain_Climb == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",Climb stairs";
            if (model.increase_rightknee_pain_goingdownstairs == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",going down stairs";
            if (model.increase_rightknee_pain_Standing == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",Standing";
            if (model.increase_rightknee_pain_getupfromchair == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",get up from chair";
            if (model.increase_rightknee_pain_getoutofcar == "true")
                cc_r_knee_increase = cc_r_knee_increase + ",get out of car";

            if (!string.IsNullOrEmpty(cc_r_knee_increase))
                model.cc_r_knee = model.cc_r_knee + " Pain increases by " + cc_r_knee_increase.TrimStart(',') + ".";

            if (!string.IsNullOrEmpty(model.other_describe_part_value))
                cc_other_1 = "The patient complains of " + model.txt_other_describe_part + " pain that is " + model.other_describe_part_value + "/10, with 10 being the worst , which is ";


            if (model.txt_other_describe_part_Constant == "true")
                cc_other_1 = cc_other_1 + ",Constant";
            if (model.txt_other_describe_part_Intermittent == "true")
                cc_other_1 = cc_other_1 + ",Intermittent";
            if (model.txt_other_describe_part_Sharp == "true")
                cc_other_1 = cc_other_1 + ",Sharp";
            if (model.txt_other_describe_part_Electric == "true")
                cc_other_1 = cc_other_1 + ",Electric";
            if (model.txt_other_describe_part_Shooting == "true")
                cc_other_1 = cc_other_1 + ",Shooting";
            if (model.txt_other_describe_part_Throbbing == "true")
                cc_other_1 = cc_other_1 + ",Throbbing";
            if (model.txt_other_describe_part_Pulsating == "true")
                cc_other_1 = cc_other_1 + ",Pulsating";
            if (model.txt_other_describe_part_Dull == "true")
                cc_other_1 = cc_other_1 + ",Dull";
            if (model.txt_other_describe_part_Achy == "true")
                cc_other_1 = cc_other_1 + ",Achy";

            model.cc_other_1 = cc_other_1.TrimStart(',');


            if (!string.IsNullOrEmpty(model.other_describe_part_1_value))
                cc_other_2 = "The patient complains of " + model.txt_other_describe_part_1 + " pain that is " + model.other_describe_part_1_value + "/10, with 10 being the worst , which is ";


            if (model.txt_other_describe_part_1_Constant == "true")
                cc_other_2 = cc_other_2 + ",Constant";
            if (model.txt_other_describe_part_1_Intermittent == "true")
                cc_other_2 = cc_other_2 + ",Intermittent";
            if (model.txt_other_describe_part_1_Sharp == "true")
                cc_other_2 = cc_other_2 + ",Sharp";
            if (model.txt_other_describe_part_1_Electric == "true")
                cc_other_2 = cc_other_2 + ",Electric";
            if (model.txt_other_describe_part_1_Shooting == "true")
                cc_other_2 = cc_other_2 + ",Shooting";
            if (model.txt_other_describe_part_1_Throbbing == "true")
                cc_other_2 = cc_other_2 + ",Throbbing";
            if (model.txt_other_describe_part_1_Pulsating == "true")
                cc_other_2 = cc_other_2 + ",Pulsating";
            if (model.txt_other_describe_part_1_Dull == "true")
                cc_other_2 = cc_other_2 + ",Dull";
            if (model.txt_other_describe_part_1_Achy == "true")
                cc_other_2 = cc_other_2 + ",Achy";

            model.cc_other_2 = cc_other_2.TrimStart(',');

            //PE for Neck

            string pe_neck = "";


            service.Insert(model);

            return RedirectToAction("Create", "IntakeForm");
        }
    }
}
