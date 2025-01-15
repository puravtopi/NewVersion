using MS.Models;
using MySql.Data.MySqlClient;
using PainTrax.Services;
using PainTrax.Web.Models;

namespace PainTrax.Web.Services
{
    public class IntakeService : ParentService
    {
        public int Insert(IntakeForm data)
        {
            MySqlCommand cm = new MySqlCommand(@"INSERT INTO `tbl_intake` (`LoadTs_src`, 
            `ProcessedFlag_src`, `Cmp_Id`, `Date_src`, `Practitioner_src`, `Clinic_Name__src`, `IE___Visit_Type__src`, 
            `Location___src`, `DOS___src`, `DOB__src`, `Age___src`, `DOA___src`, `Sex__src`, `Case_Type___src`, 
            `Insurance_Co_Name___src`, `Claim_____src`, `WCB_Group__src`, `Last_Name__src`, `Middle_Name__Initial_src`, 
            `First_name__src`, `Mobile___src`, `Vital___Weight__src`, `Vital___Height___src`, `Vital___Handedness___src`, 
            `Occupation__src`, `Occupation___Still_Working___src`, `History___Description_of_the_accident_src`, 
            `History___Did_you_lose_consciousness___src`, `History___If_yes___then_how_long___src`, 
            `History___Did_you_go_to_the_hospital___src`, `History___If_yes___which_hospital___src`, 
            `History___In_the_hospital_did_they_do_any_test___src`, `History___What_tests___src`, 
            `Body_parts__src`, `History___Where_you_prescribed_any_medication___src`, 
            `History___If_yes___what_medication___src`, `PMH___Any_medical_conditions_src`, `PMH___Others_src`, 
            `Notes___Providers_seen___src`, `PSH___Have_you_ever_had_any_surgeries____src`, `PSH___When___src`, 
            `PSH___Type_of_Surgery___src`, `Medication___Are_you_taking_any_medications___src`, `Medication___What_medications___src`, 
            `ALLERGIES___src`, `Allergies___To_what_src`, `Social_History___src`, `History___Physical_therapy_src`, `History___How_often___src`, 
            `History___Chiropractic_src`, `History___How_often___1_src`, `History___Symptoms_since_the_accident___src`, 
            `ADL___personal_hygiene__grooming__dressing__toileting_src`, `ADL___walking__transferring__ambulating_src`, 
            `ADL___eating_independently_src`, `ADL___cook__clean__maintain_households_src`, `ADL___Shop_for_food_src`, 
            `ADL___Sleeping_src`, `Degree_of_Disability_______in____src`, `Work_Status___Last_day_you_worked___src`, 
            `Work_Status___Date_you_returned_to_work__src`, `CC___Neck_Pain_Level___10_src`, `CC___Neck_Pain_Type_src`, 
            `CC___Midback_Pain_Level___10_src`, `CC___Midback_Pain_Type_src`, `CC___Lowback_Pain_Level___10_src`, `CC___Lowback_Pain_Type_src`, 
            `CC___L_Shoulder_Pain_Level___10_src`, `CC___L_Shoulder_Pain_Type_src`,`CC___R_Shoulder_Pain_Level___10_src`, `CC___R_Shoulder_Pain_Type_src`,
            `CC___L_Knee_Pain_Level___10_src`, `CC___L_Knee_Pain_Type_src`, `CC___R_Knee_Pain_Level___10_src`, `CC___R_Knee_Pain_Type_src`, 
            `CC___Other_Body_Part_src`, 
            `CC___Other_Body_Part_Pain_Level___10_src`, `CC___Other_Body_Part_Pain_Type_src`, `CC___src`, `CC___Pain_Level___10_src`, 
            `CC___Pain_Type_src`, `CC___Neck_Pain_radiates_src`, `CC___1_src`, `CC___2_src`, `CC___3_src`, `CC___Low_Back_Pain_radiates_src`,
            `CC___4_src`, `CC___5_src`, `CC___6_src`, `CC___What_relieves_your_pain___src`, `CC___Others_src`, 
            `CC___Neck___What_increases_your_pain___src`, `CC___Mid_Back___What_increases_your_pain___src`, 
            `CC___Low__Back___What_increases_your_pain___src`, `CC___Shoulder___What_increases_your_pain___src`, 
            `CC___Knee___What_increases_your_pain___src`, `PE___Neck___FF___60__src`, `PE___Neck___Ext__50__src`, 
            `PE___Neck_Rot____80__L_src`, `PE___Neck_Rot____80__R_src`, `PE___Neck___Spurling_s_src`, `PE___Mid_back__src`, 
            `PE___Low_Back___FF__90__src`, `PE__Low_Back___Ext__40__src`, `PE___Low_back___Rot___60__L_src`, `PE___Low_back___Rot___60__R_src`, 
            `PE___Low_back___SLR_src`, `PE___Shoulder___Abd__180__L_src`, `PE___Shoulder____Abd__180__R_src`, `PE___Shoulder___Flex___180__L_src`, 
            `PE___Shoulder___Flex___180__R_src`, `PE___Shoulder___Ext_Rot__90__L_src`, `PE___Shoulder___Ext_Rot__90__R_src`, 
            `PE___Shoulder___Int_Rot__90__L_src`, `PE___Shoulder___Int_Rot__90__R_src`, `PE___Knee___Flex__135___L_src`, 
            `PE___Knee___Flex__135___R_src`, `PE___Knee___Exten___0__L_src`, `PE___Knee___Exten___0__R_src`, 
            `PE__Others__src`, `GAIT__src`, `GAIT___Others_src`, `NE___DTRs_2+_src`, `NE___Sensory_Exam___Decr_Paresthesia_LUE_src`,  
            `NE___Sensory_Exam___RUE_src`, `NE___Sensory_Exam___LLE_src`, `NE___Sensory_Exam___RLE_src`, `NE___MMST_src`, 
            `NE___Upper_Ext____Normal_5_5___expect_src`, `NE___Lower_Ext____Normal_5_5___expect_src`, 
            `NE___Please_specify_pain__extremity_in_which_region__src`, `Assessment___DIAGNOSES__src`, `Treatment___Diag__Test_Referral___please_specify____src`, 
            `Treatment___Providers_Seen__src`, `Treatment___Others_src`, `Treatment___Referral__src`, `Treatment___Referral_Others_src`, 
            `Treatment___Request___Schedule_src`, `Treatment___Request___Schedule___Others_src`, `Treatment___DME_Dispensed_at_today_s_office_visit__src`, 
            `Treatment___DME_Meadowbrook_Medical_Supplies__Other_DME_Procedur`, `Follow__up_____Weeks__Months___src`, `Follow_up___Other__src`, `Notes___src`, 
            `Archived_src`, `FirstOpened_src`, `History_Same_Day`,`History_How_Many_Days_Later`,`History_Hospital_Via`)Values
			(@LoadTs_src,
            @ProcessedFlag_src,@Cmp_Id,@Date_src,@Practitioner_src,@Clinic_Name__src,@IE___Visit_Type__src,
            @Location___src,@DOS___src,@DOB__src,@Age___src,@DOA___src,@Sex__src,@Case_Type___src,
            @Insurance_Co_Name___src,@Claim_____src,@WCB_Group__src,@Last_Name__src,@Middle_Name__Initial_src,
            @First_name__src,@Mobile___src,@Vital___Weight__src,@Vital___Height___src,@Vital___Handedness___src,
            @Occupation__src,@Occupation___Still_Working___src,@History___Description_of_the_accident_src,
            @History___Did_you_lose_consciousness___src,@History___If_yes___then_how_long___src,
            @History___Did_you_go_to_the_hospital___src,@History___If_yes___which_hospital___src,
            @History___In_the_hospital_did_they_do_any_test___src,@History___What_tests___src,
            @Body_parts__src,@History___Where_you_prescribed_any_medication___src,
            @History___If_yes___what_medication___src,@PMH___Any_medical_conditions_src,@PMH___Others_src,
            @Notes___Providers_seen___src,@PSH___Have_you_ever_had_any_surgeries____src,@PSH___When___src,
            @PSH___Type_of_Surgery___src,@Medication___Are_you_taking_any_medications___src,@Medication___What_medications___src,
            @ALLERGIES___src,@Allergies___To_what_src,@Social_History___src,@History___Physical_therapy_src,@History___How_often___src,
            @History___Chiropractic_src,@History___How_often___1_src,@History___Symptoms_since_the_accident___src,
            @ADL___personal_hygiene__grooming__dressing__toileting_src,@ADL___walking__transferring__ambulating_src,
            @ADL___eating_independently_src,@ADL___cook__clean__maintain_households_src,@ADL___Shop_for_food_src,
            @ADL___Sleeping_src,@Degree_of_Disability_______in____src,@Work_Status___Last_day_you_worked___src,
            @Work_Status___Date_you_returned_to_work__src,@CC___Neck_Pain_Level___10_src,@CC___Neck_Pain_Type_src,
            @CC___Midback_Pain_Level___10_src,@CC___Midback_Pain_Type_src,@CC___Lowback_Pain_Level___10_src,@CC___Lowback_Pain_Type_src,
            @CC___L_Shoulder_Pain_Level___10_src, @CC___L_Shoulder_Pain_Type_src,@CC___R_Shoulder_Pain_Level___10_src,@CC___R_Shoulder_Pain_Type_src,
            @CC___L_Knee_Pain_Level___10_src, @CC___L_Knee_Pain_Type_src, @CC___R_Knee_Pain_Level___10_src, @CC___R_Knee_Pain_Type_src, 
            @CC___Other_Body_Part_src, 
            @CC___Other_Body_Part_Pain_Level___10_src, @CC___Other_Body_Part_Pain_Type_src, @CC___src, @CC___Pain_Level___10_src, 
            @CC___Pain_Type_src, @CC___Neck_Pain_radiates_src, @CC___1_src, @CC___2_src, @CC___3_src, @CC___Low_Back_Pain_radiates_src,
            @CC___4_src, @CC___5_src, @CC___6_src, @CC___What_relieves_your_pain___src, @CC___Others_src, 
            @CC___Neck___What_increases_your_pain___src, @CC___Mid_Back___What_increases_your_pain___src, 
            @CC___Low__Back___What_increases_your_pain___src, @CC___Shoulder___What_increases_your_pain___src, 
            @CC___Knee___What_increases_your_pain___src, @PE___Neck___FF___60__src, @PE___Neck___Ext__50__src, 
            @PE___Neck_Rot____80__L_src, @PE___Neck_Rot____80__R_src, @PE___Neck___Spurling_s_src, @PE___Mid_back__src, 
            @PE___Low_Back___FF__90__src, @PE__Low_Back___Ext__40__src, @PE___Low_back___Rot___60__L_src, @PE___Low_back___Rot___60__R_src, 
            @PE___Low_back___SLR_src, @PE___Shoulder___Abd__180__L_src, @PE___Shoulder____Abd__180__R_src, @PE___Shoulder___Flex___180__L_src, 
            @PE___Shoulder___Flex___180__R_src, @PE___Shoulder___Ext_Rot__90__L_src, @PE___Shoulder___Ext_Rot__90__R_src, 
            @PE___Shoulder___Int_Rot__90__L_src, @PE___Shoulder___Int_Rot__90__R_src, @PE___Knee___Flex__135___L_src, 
            @PE___Knee___Flex__135___R_src, @PE___Knee___Exten___0__L_src, @PE___Knee___Exten___0__R_src, 
            @PE__Others__src, GAIT__src, @GAIT___Others_src, @NE___DTRs_2+_src, @NE___Sensory_Exam___Decr_Paresthesia_LUE_src,  
            @NE___Sensory_Exam___RUE_src, @NE___Sensory_Exam___LLE_src, @NE___Sensory_Exam___RLE_src, @NE___MMST_src, 
            @NE___Upper_Ext____Normal_5_5___expect_src, @NE___Lower_Ext____Normal_5_5___expect_src, 
            @NE___Please_specify_pain__extremity_in_which_region__src, @Assessment___DIAGNOSES__src, @Treatment___Diag__Test_Referral___please_specify____src, 
            @Treatment___Providers_Seen__src, @Treatment___Others_src, @Treatment___Referral__src, @Treatment___Referral_Others_src, 
            @Treatment___Request___Schedule_src, @Treatment___Request___Schedule___Others_src, @Treatment___DME_Dispensed_at_today_s_office_visit__src, 
            @Treatment___DME_Meadowbrook_Medical_Supplies__Other_DME_Procedur, @Follow__up_____Weeks__Months___src, @Follow_up___Other__src, @Notes___src, 
            @Archived_src, @FirstOpened_src, @History_Same_Day,@History_How_Many_Days_Later,@History_Hospital_Via);select @@identity;", conn);
            cm.Parameters.AddWithValue("@LoadTs_src", System.DateTime.Now.ToString("yyyy-MM-dd"));
            cm.Parameters.AddWithValue("@ProcessedFlag_src", 'N');
            cm.Parameters.AddWithValue("@Cmp_Id", data.Cmp_Id);
            cm.Parameters.AddWithValue("@Date_src", System.DateTime.Now.ToString("yyyy-MM-dd"));
            cm.Parameters.AddWithValue("@Practitioner_src", "Charles DMello");
            cm.Parameters.AddWithValue("@Clinic_Name__src", "");
            cm.Parameters.AddWithValue("@IE___Visit_Type__src", data.visit_type);
            cm.Parameters.AddWithValue("@Location___src", data.location_id);
            cm.Parameters.AddWithValue("@DOS___src", data.dov);
            cm.Parameters.AddWithValue("@DOB__src", data.dob);
            cm.Parameters.AddWithValue("@Age___src", data.age);
            cm.Parameters.AddWithValue("@DOA___src", data.doa);
            cm.Parameters.AddWithValue("@Sex__src", data.gender);
            cm.Parameters.AddWithValue("@Case_Type___src", data.CaseType);
            cm.Parameters.AddWithValue("@Insurance_Co_Name___src", "");
            cm.Parameters.AddWithValue("@Claim_____src", "");
            cm.Parameters.AddWithValue("@WCB_Group__src", data.wcb);
            cm.Parameters.AddWithValue("@Last_Name__src", data.l_name);
            cm.Parameters.AddWithValue("@Middle_Name__Initial_src", "");
            cm.Parameters.AddWithValue("@First_name__src", data.f_name);
            cm.Parameters.AddWithValue("@Mobile___src", "");
            cm.Parameters.AddWithValue("@Vital___Weight__src", data.weight);
            cm.Parameters.AddWithValue("@Vital___Height___src", data.height);
            cm.Parameters.AddWithValue("@Vital___Handedness___src", data.handedness);
            cm.Parameters.AddWithValue("@Occupation__src", data.occupation);
            cm.Parameters.AddWithValue("@Occupation___Still_Working___src", data.txt_still_working);
            cm.Parameters.AddWithValue("@History___Description_of_the_accident_src", data.description_of_the_accident);
            cm.Parameters.AddWithValue("@History___Did_you_lose_consciousness___src", data.loss_consciousness);
            cm.Parameters.AddWithValue("@History___If_yes___then_how_long___src", data.if_yes_how_long);
            cm.Parameters.AddWithValue("@History___Did_you_go_to_the_hospital___src", data.did_go_to_the_hospital);
            cm.Parameters.AddWithValue("@History___If_yes___which_hospital___src", data.if_yes_which_hospital);
            cm.Parameters.AddWithValue("@History___In_the_hospital_did_they_do_any_test___src", data.did_they_did_any_test);
            cm.Parameters.AddWithValue("@History___What_tests___src", data.what_test);
            cm.Parameters.AddWithValue("@Body_parts__src", data.bodyparts_injured_inaccident);
            cm.Parameters.AddWithValue("@History___Where_you_prescribed_any_medication___src", data.prescribed_any_medication_yes);
            cm.Parameters.AddWithValue("@History___If_yes___what_medication___src", data.what_medication);
            cm.Parameters.AddWithValue("@History_Same_Day", data.go_to_the_sameday);
            cm.Parameters.AddWithValue("@History_How_Many_Days_Later", data.if_no_how_many_days_later);
            cm.Parameters.AddWithValue("@History_Hospital_Via", data.rdo_ambulance);
            cm.Parameters.AddWithValue("@PMH___Any_medical_conditions_src", data.any_medical_conditions_bp);
            cm.Parameters.AddWithValue("@PMH___Others_src", data.medical_conditions_others);
            cm.Parameters.AddWithValue("@Notes___Providers_seen___src", "");
            cm.Parameters.AddWithValue("@PSH___Have_you_ever_had_any_surgeries____src", data.had_any_surgeries);
            cm.Parameters.AddWithValue("@PSH___When___src", data.had_any_surgeries_when);
            cm.Parameters.AddWithValue("@PSH___Type_of_Surgery___src", data.type_of_surgery);
            cm.Parameters.AddWithValue("@Medication___Are_you_taking_any_medications___src", data.taking_medications_surgery);
            cm.Parameters.AddWithValue("@Medication___What_medications___src", data.taking_medications_surgery);
            cm.Parameters.AddWithValue("@ALLERGIES___src", data.allergies);
            cm.Parameters.AddWithValue("@Allergies___To_what_src", data.what_allergies);
            cm.Parameters.AddWithValue("@Social_History___src", data.social_history);
            cm.Parameters.AddWithValue("@History___Physical_therapy_src", data.physical_therapy);
            cm.Parameters.AddWithValue("@History___How_often___src", data.txt_physical_therapy);
            cm.Parameters.AddWithValue("@History___Chiropractic_src", data.chiropractic);
            cm.Parameters.AddWithValue("@History___How_often___1_src", data.txt_chiropractic);
            cm.Parameters.AddWithValue("@History___Symptoms_since_the_accident___src", data.symptoms_since_accident);
            cm.Parameters.AddWithValue("@ADL___personal_hygiene__grooming__dressing__toileting_src", data.adl_personal);
            cm.Parameters.AddWithValue("@ADL___walking__transferring__ambulating_src", data.adl_walking);
            cm.Parameters.AddWithValue("@ADL___eating_independently_src", data.adl_eating);
            cm.Parameters.AddWithValue("@ADL___cook__clean__maintain_households_src", data.adl_cooking);
            cm.Parameters.AddWithValue("@ADL___Shop_for_food_src", data.adl_Shop_for_food);
            cm.Parameters.AddWithValue("@ADL___Sleeping_src", data.adl_sleeping);
            cm.Parameters.AddWithValue("@Degree_of_Disability_______in____src", data.degree_of_disability);
            cm.Parameters.AddWithValue("@Work_Status___Last_day_you_worked___src", data.last_day_worked);
            cm.Parameters.AddWithValue("@Work_Status___Date_you_returned_to_work__src", data.return_to_work);
            cm.Parameters.AddWithValue("@CC___Neck_Pain_Level___10_src", data.txt_describe_neck);
            cm.Parameters.AddWithValue("@CC___Neck_Pain_Type_src", data.cc_neck);
            cm.Parameters.AddWithValue("@CC___Midback_Pain_Level___10_src", data.txt_describe_midback);
            cm.Parameters.AddWithValue("@CC___Midback_Pain_Type_src", data.cc_midback);
            cm.Parameters.AddWithValue("@CC___Lowback_Pain_Level___10_src", data.txt_describe_lowback);
            cm.Parameters.AddWithValue("@CC___Lowback_Pain_Type_src", data.cc_lowback);
            cm.Parameters.AddWithValue("@CC___L_Shoulder_Pain_Level___10_src", null);
            cm.Parameters.AddWithValue("@CC___L_Shoulder_Pain_Type_src", data.cc_l_shoulder);
            cm.Parameters.AddWithValue("@CC___R_Shoulder_Pain_Level___10_src",null);
            cm.Parameters.AddWithValue("@CC___R_Shoulder_Pain_Type_src", data.cc_r_shoulder);
            cm.Parameters.AddWithValue("@CC___L_Knee_Pain_Level___10_src", null);
            cm.Parameters.AddWithValue("@CC___L_Knee_Pain_Type_src", data.cc_l_knee);
            cm.Parameters.AddWithValue("@CC___R_Knee_Pain_Level___10_src", null);
            cm.Parameters.AddWithValue("@CC___R_Knee_Pain_Type_src", data.cc_r_knee);


            var result = ExecuteScalar(cm);
            return result;
        }
    }
}
