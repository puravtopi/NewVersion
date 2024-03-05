using MySql.Data.MySqlClient;
using System.Linq;
using System.Data;
using MS.Models;
using PainTrax.Services;
using PainTrax.Web.Models;

namespace MS.Services;
public class PatientIEService : ParentService
{
    #region IE

    public List<vm_patient_ie> GetAll(string cnd = "")
    {
        string query = "select *,CASE WHEN (SELECT COUNT(*) FROM tbl_patient_fu WHERE tbl_patient_fu.patientIE_ID=vm_patient_ie.id)>0 then TRUE ELSE FALSE END AS isFU from vm_patient_ie where 1=1 ";

        if (!string.IsNullOrEmpty(query))
            query = query + cnd;

        query = query + " order by id desc";

        List<vm_patient_ie> dataList = ConvertDataTable<vm_patient_ie>(GetData(query));
        return dataList;
    }
    /*public List<vm_patient_ie> GetAautoComplete(string cnd = "")
    {
        string query = "select *,state as label,state as val from tbl_patient where 1=1 ";
        query += cnd;
        List<vm_patient_ie> dataList = ConvertDataTable<vm_patient_ie>(GetData(query));
        return dataList;
    }*/
    public tbl_patient_ie? GetOne(int data)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_patient_ie where id=@id ", conn);
        cm.Parameters.AddWithValue("@id", data);
        var datalist = ConvertDataTable<tbl_patient_ie>(GetData(cm)).FirstOrDefault();
        return datalist;
    }

    public vm_patient_ie? GetOnebyPatientId(int patient_id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from vm_patient_ie where id=@patient_id ", conn);
        cm.Parameters.AddWithValue("@patient_id", patient_id);
        var datalist = ConvertDataTable<vm_patient_ie>(GetData(cm)).FirstOrDefault();
        return datalist;
    }

    public int Insert(tbl_patient_ie data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_patient_ie
		(patient_id,location_id,attorney_id,primary_ins_cmp_id,secondary_ins_cmp_id,emp_id,adjuster_id,doe,doa,primary_claim_no,secondary_claim_no,primary_policy_no,secondary_policy_no,compensation,note,ins_note,alert_note,created_date,created_by,is_active,secondary_wcb_group,primary_wcb_group,referring_physician,accident_type)Values
				(@patient_id,@location_id,@attorney_id,@primary_ins_cmp_id,@secondary_ins_cmp_id,@emp_id,@adjuster_id,@doe,@doa,@primary_claim_no,@secondary_claim_no,@primary_policy_no,@secondary_policy_no,@compensation,@note,@ins_note,@alert_note,@created_date,@created_by,@is_active,@secondary_wcb_group,@primary_wcb_group,@referring_physician,@accident_type);select @@identity;", conn);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@location_id", data.location_id);
        cm.Parameters.AddWithValue("@attorney_id", data.attorney_id);
        cm.Parameters.AddWithValue("@primary_ins_cmp_id", data.primary_ins_cmp_id);
        cm.Parameters.AddWithValue("@secondary_ins_cmp_id", data.secondary_ins_cmp_id);
        cm.Parameters.AddWithValue("@emp_id", data.emp_id);
        cm.Parameters.AddWithValue("@adjuster_id", data.adjuster_id);
        cm.Parameters.AddWithValue("@doe", data.doe);
        cm.Parameters.AddWithValue("@doa", data.doa);
        cm.Parameters.AddWithValue("@primary_claim_no", data.primary_claim_no);
        cm.Parameters.AddWithValue("@secondary_claim_no", data.secondary_claim_no);
        cm.Parameters.AddWithValue("@primary_policy_no", data.primary_policy_no);
        cm.Parameters.AddWithValue("@secondary_policy_no", data.secondary_policy_no);
        cm.Parameters.AddWithValue("@compensation", data.compensation);
        cm.Parameters.AddWithValue("@primary_wcb_group", data.primary_wcb_group);
        cm.Parameters.AddWithValue("@secondary_wcb_group", data.secondary_wcb_group);
        cm.Parameters.AddWithValue("@note", data.note);
        cm.Parameters.AddWithValue("@ins_note", data.ins_note);
        cm.Parameters.AddWithValue("@alert_note", data.alert_note);
        cm.Parameters.AddWithValue("@created_date", System.DateTime.Now);
        cm.Parameters.AddWithValue("@created_by", data.created_by);
         cm.Parameters.AddWithValue("@is_active", data.is_active);
         cm.Parameters.AddWithValue("@referring_physician", data.referring_physician);
         cm.Parameters.AddWithValue("@accident_type", data.accident_type);
        var result = ExecuteScalar(cm);
        return result;
    }
    public void Update(tbl_patient_ie data)
    {
        MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_patient_ie SET
		patient_id=@patient_id,
		location_id=@location_id,
		attorney_id=@attorney_id,
		primary_ins_cmp_id=@primary_ins_cmp_id,
		secondary_ins_cmp_id=@secondary_ins_cmp_id,
		emp_id=@emp_id,
		adjuster_id=@adjuster_id,
		doe=@doe,
		doa=@doa,
		primary_claim_no=@primary_claim_no,
		secondary_claim_no=@secondary_claim_no,
		primary_policy_no=@primary_policy_no,
		secondary_policy_no=@secondary_policy_no,
		compensation=@compensation,
		primary_wcb_group=@primary_wcb_group,
		referring_physician=@referring_physician,
		note=@note,
		ins_note=@ins_note,
		alert_note=@alert_note,
		secondary_wcb_group=@secondary_wcb_group,
        updated_by=@updated_by,
		updated_date=@updated_date,
        accident_type=@accident_type,
		is_active=@is_active		where id=@id", conn);
        cm.Parameters.AddWithValue("@id", data.id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@location_id", data.location_id);
        cm.Parameters.AddWithValue("@attorney_id", data.attorney_id);
        cm.Parameters.AddWithValue("@primary_ins_cmp_id", data.primary_ins_cmp_id);
        cm.Parameters.AddWithValue("@secondary_ins_cmp_id", data.secondary_ins_cmp_id);
        cm.Parameters.AddWithValue("@emp_id", data.emp_id);
        cm.Parameters.AddWithValue("@adjuster_id", data.adjuster_id);
        cm.Parameters.AddWithValue("@doe", data.doe);
        cm.Parameters.AddWithValue("@doa", data.doa);
        cm.Parameters.AddWithValue("@primary_claim_no", data.primary_claim_no);
        cm.Parameters.AddWithValue("@secondary_claim_no", data.secondary_claim_no);
        cm.Parameters.AddWithValue("@primary_policy_no", data.primary_policy_no);
        cm.Parameters.AddWithValue("@secondary_policy_no", data.secondary_policy_no);
        cm.Parameters.AddWithValue("@compensation", data.compensation);
        cm.Parameters.AddWithValue("@primary_wcb_group", data.primary_wcb_group);
        cm.Parameters.AddWithValue("@secondary_wcb_group", data.secondary_wcb_group);
        cm.Parameters.AddWithValue("@note", data.note);
        cm.Parameters.AddWithValue("@ins_note", data.ins_note);
        cm.Parameters.AddWithValue("@alert_note", data.alert_note);
        cm.Parameters.AddWithValue("@updated_by", data.updated_by);
        cm.Parameters.AddWithValue("@updated_date", data.updated_date);
        cm.Parameters.AddWithValue("@is_active", data.is_active);
        cm.Parameters.AddWithValue("@referring_physician", data.referring_physician);
        cm.Parameters.AddWithValue("@accident_type", data.accident_type);

        Execute(cm);
    }

    public void UpdateFromFU(tbl_patient_ie data)
    {
        MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_patient_ie SET
		patient_id=@patient_id,
		location_id=@location_id,
		attorney_id=@attorney_id,
		primary_ins_cmp_id=@primary_ins_cmp_id,
		secondary_ins_cmp_id=@secondary_ins_cmp_id,
		emp_id=@emp_id,
		adjuster_id=@adjuster_id,
		doa=@doa,
		primary_claim_no=@primary_claim_no,
		secondary_claim_no=@secondary_claim_no,
		primary_policy_no=@primary_policy_no,
		secondary_policy_no=@secondary_policy_no,
		compensation=@compensation,
		primary_wcb_group=@primary_wcb_group,
		referring_physician=@referring_physician,
		note=@note,
		ins_note=@ins_note,
		alert_note=@alert_note,
		secondary_wcb_group=@secondary_wcb_group,
        updated_by=@updated_by,
		updated_date=@updated_date,
		is_active=@is_active		where id=@id", conn);
        cm.Parameters.AddWithValue("@id", data.id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@location_id", data.location_id);
        cm.Parameters.AddWithValue("@attorney_id", data.attorney_id);
        cm.Parameters.AddWithValue("@primary_ins_cmp_id", data.primary_ins_cmp_id);
        cm.Parameters.AddWithValue("@secondary_ins_cmp_id", data.secondary_ins_cmp_id);
        cm.Parameters.AddWithValue("@emp_id", data.emp_id);
        cm.Parameters.AddWithValue("@adjuster_id", data.adjuster_id);
       
        cm.Parameters.AddWithValue("@doa", data.doa);
        cm.Parameters.AddWithValue("@primary_claim_no", data.primary_claim_no);
        cm.Parameters.AddWithValue("@secondary_claim_no", data.secondary_claim_no);
        cm.Parameters.AddWithValue("@primary_policy_no", data.primary_policy_no);
        cm.Parameters.AddWithValue("@secondary_policy_no", data.secondary_policy_no);
        cm.Parameters.AddWithValue("@compensation", data.compensation);
        cm.Parameters.AddWithValue("@primary_wcb_group", data.primary_wcb_group);
        cm.Parameters.AddWithValue("@secondary_wcb_group", data.secondary_wcb_group);
        cm.Parameters.AddWithValue("@note", data.note);
        cm.Parameters.AddWithValue("@ins_note", data.ins_note);
        cm.Parameters.AddWithValue("@alert_note", data.alert_note);
        cm.Parameters.AddWithValue("@updated_by", data.updated_by);
        cm.Parameters.AddWithValue("@updated_date", data.updated_date);
        cm.Parameters.AddWithValue("@is_active", data.is_active);
        cm.Parameters.AddWithValue("@referring_physician", data.referring_physician);

        Execute(cm);
    }

    public void Delete(int id)
    {
        MySqlCommand cm = new MySqlCommand(@"DELETE FROM tbl_patient_ie
		where id=@id", conn);
        cm.Parameters.AddWithValue("@id", id);
        Execute(cm);
    }
    #endregion

    #region Page1
    public int InsertPage1(tbl_ie_page1 data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_page1
		(history,vital,pmh,psh,allergies,medication,family_history,social_history,cc,pe,rom,note,ie_id,cmp_id,patient_id,bodypart,daignosis_delimit,daignosis_desc,assessment,plan,dd,work_status,occupation,impairment_rating)Values
				(@history,@vital,@pmh,@psh,@allergies,@medication,@family_history,@social_history,@cc,@pe,@rom,@note,@ie_id,@cmp_id,@patient_id,@bodypart,@daignosis_delimit,@daignosis_desc,@assessment,@plan,@dd,@work_status,@occupation,@impairment_rating);select @@identity;", conn);
        cm.Parameters.AddWithValue("@history", data.history);
        cm.Parameters.AddWithValue("@vital", data.vital);
        cm.Parameters.AddWithValue("@pmh", data.pmh);
        cm.Parameters.AddWithValue("@psh", data.psh);
        cm.Parameters.AddWithValue("@allergies", data.allergies);
        cm.Parameters.AddWithValue("@medication", data.medication);
        cm.Parameters.AddWithValue("@family_history", data.family_history);
        cm.Parameters.AddWithValue("@social_history", data.social_history);
        cm.Parameters.AddWithValue("@cc", data.cc);
        cm.Parameters.AddWithValue("@pe", data.pe);
        cm.Parameters.AddWithValue("@rom", data.rom);
        cm.Parameters.AddWithValue("@note", data.note);
        cm.Parameters.AddWithValue("@ie_id", data.ie_id);
        cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@bodypart", data.bodypart);
        cm.Parameters.AddWithValue("@daignosis_desc", data.daignosis_desc);
        cm.Parameters.AddWithValue("@daignosis_delimit", data.daignosis_delimit);
        cm.Parameters.AddWithValue("@assessment", data.assessment);
        cm.Parameters.AddWithValue("@plan", data.plan);
        cm.Parameters.AddWithValue("@dd", data.dd);
        cm.Parameters.AddWithValue("@work_status", data.work_status);
        cm.Parameters.AddWithValue("@occupation", data.occupation);
        cm.Parameters.AddWithValue("@impairment_rating", data.impairment_rating);
        var result = ExecuteScalar(cm);
        return result;
    }

    public int UpdatePage1(tbl_ie_page1 data)
    {
        MySqlCommand cm = new MySqlCommand(@"update tbl_ie_page1 set
		history=@history,vital=@vital,pmh=@pmh,psh=@psh,allergies=@allergies,medication=@medication,family_history=@family_history,
        social_history=@social_history,cc=@cc,pe=@pe,rom=@rom,note=@note,
        bodypart=@bodypart,daignosis_desc=@daignosis_desc,daignosis_delimit=@daignosis_delimit,occupation=@occupation,
        assessment=@assessment,plan=@plan,dd=@dd,work_status=@work_status,impairment_rating=@impairment_rating 

        where id=@id
				 ;select 1;", conn);
        cm.Parameters.AddWithValue("@history", data.history);
        cm.Parameters.AddWithValue("@vital", data.vital);
        cm.Parameters.AddWithValue("@pmh", data.pmh);
        cm.Parameters.AddWithValue("@psh", data.psh);
        cm.Parameters.AddWithValue("@allergies", data.allergies);
        cm.Parameters.AddWithValue("@medication", data.medication);
        cm.Parameters.AddWithValue("@family_history", data.family_history);
        cm.Parameters.AddWithValue("@social_history", data.social_history);
        cm.Parameters.AddWithValue("@cc", data.cc);
        cm.Parameters.AddWithValue("@pe", data.pe);
        cm.Parameters.AddWithValue("@rom", data.rom);
        cm.Parameters.AddWithValue("@note", data.note);
        cm.Parameters.AddWithValue("@id", data.id);
        cm.Parameters.AddWithValue("@bodypart", data.bodypart);
        cm.Parameters.AddWithValue("@daignosis_desc", data.daignosis_desc);
        cm.Parameters.AddWithValue("@daignosis_delimit", data.daignosis_delimit);
        cm.Parameters.AddWithValue("@assessment", data.assessment);
        cm.Parameters.AddWithValue("@plan", data.plan);
        cm.Parameters.AddWithValue("@dd", data.dd);
        cm.Parameters.AddWithValue("@work_status", data.work_status);
        cm.Parameters.AddWithValue("@occupation", data.occupation);
        cm.Parameters.AddWithValue("@impairment_rating", data.impairment_rating);
        var result = ExecuteScalar(cm);
        return result;
    }

    public tbl_ie_page1? GetOnePage1(int id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_ie_page1 where ie_id=@id ", conn);
        cm.Parameters.AddWithValue("@id", id);
        var datalist = ConvertDataTable<tbl_ie_page1>(GetData(cm)).FirstOrDefault();
        return datalist;
    }
    #endregion

    #region Page2
    public int InsertPage2(tbl_ie_page2 data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_page2
		(ros,aod,other,ie_id,patient_id,cmp_id)Values
				(@ros,@aod,@other,@ie_id,@patient_id,@cmp_id);select @@identity;", conn);

        cm.Parameters.AddWithValue("@ros", data.ros);
        cm.Parameters.AddWithValue("@aod", data.aod);
        cm.Parameters.AddWithValue("@other", data.other);
        cm.Parameters.AddWithValue("@ie_id", data.ie_id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
        var result = ExecuteScalar(cm);
        return result;
    }

    public int UpdatePage2(tbl_ie_page2 data)
    {
        MySqlCommand cm = new MySqlCommand(@"update tbl_ie_page2
		 set ros=@ros,aod=@aod,other=@other where id=@id;select 1;", conn);

        cm.Parameters.AddWithValue("@ros", data.ros);
        cm.Parameters.AddWithValue("@aod", data.aod);
        cm.Parameters.AddWithValue("@other", data.other);
        cm.Parameters.AddWithValue("@id", data.id);

        var result = ExecuteScalar(cm);
        return result;
    }
    public tbl_ie_page2? GetOnePage2(int id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_ie_page2 where ie_id=@id ", conn);
        cm.Parameters.AddWithValue("@id", id);
        var datalist = ConvertDataTable<tbl_ie_page2>(GetData(cm)).FirstOrDefault();
        return datalist;
    }
    #endregion

    #region Page3

    public int InsertPage3(tbl_ie_page3 data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_page3
		(ie_id,cmp_id,patient_id,gait,diagcervialbulge_date,diagcervialbulge_study,diagcervialbulge,diagcervialbulge_text,diagcervialbulge_hnp1,diagcervialbulge_hnp2,diagthoracicbulge_date,diagthoracicbulge_study,diagthoracicbulge,diagthoracicbulge_text,diagthoracicbulge_hnp1,diagthoracicbulge_hnp2,diaglumberbulge_date,diaglumberbulge_study,diaglumberbulge,diaglumberbulge_text,diaglumberbulge_hnp1,diaglumberbulge_hnp2,diagleftshoulder_date,diagleftshoulder_study,diagleftshoulder,diagleftshoulder_text,diagrightshoulder_date,diagrightshoulder_study,diagrightshoulder,diagrightshoulder_text,diagleftknee_date,diagleftknee_study,diagleftknee,diagleftknee_text,diagrightknee_date,diagrightknee_study,diagrightknee,diagrightknee_text,diagbrain_date,diagbrain_study,diagbrain,diagbrain_text,other1_date,other1_study,other1,other1_text,other2_date,other2_study,other2,other2_text,other3_date,other3_study,other3,other3_text,other4_date,other4_study,other4,other4_text,other5_date,other5_study,other5,other5_text,other6_date,other6_study,other6,other6_text,other7_date,other7_study,other7,other7_text,followupin,followupin_date,goal,care,universal,discharge_medications)Values
				(@ie_id,@cmp_id,@patient_id,@gait,@diagcervialbulge_date,@diagcervialbulge_study,@diagcervialbulge,@diagcervialbulge_text,@diagcervialbulge_hnp1,@diagcervialbulge_hnp2,@diagthoracicbulge_date,@diagthoracicbulge_study,@diagthoracicbulge,@diagthoracicbulge_text,@diagthoracicbulge_hnp1,@diagthoracicbulge_hnp2,@diaglumberbulge_date,@diaglumberbulge_study,@diaglumberbulge,@diaglumberbulge_text,@diaglumberbulge_hnp1,@diaglumberbulge_hnp2,@diagleftshoulder_date,@diagleftshoulder_study,@diagleftshoulder,@diagleftshoulder_text,@diagrightshoulder_date,@diagrightshoulder_study,@diagrightshoulder,@diagrightshoulder_text,@diagleftknee_date,@diagleftknee_study,@diagleftknee,@diagleftknee_text,@diagrightknee_date,@diagrightknee_study,@diagrightknee,@diagrightknee_text,@diagbrain_date,@diagbrain_study,@diagbrain,@diagbrain_text,@other1_date,@other1_study,@other1,@other1_text,@other2_date,@other2_study,@other2,@other2_text,@other3_date,@other3_study,@other3,@other3_text,@other4_date,@other4_study,@other4,@other4_text,@other5_date,@other5_study,@other5,@other5_text,@other6_date,@other6_study,@other6,@other6_text,@other7_date,@other7_study,@other7,@other7_text,@followupin,@followupin_date,@goal,@care,@universal,@discharge_medications);select @@identity;", conn);
        cm.Parameters.AddWithValue("@ie_id", data.ie_id);
        cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@gait", data.gait);
        cm.Parameters.AddWithValue("@diagcervialbulge_date", data.diagcervialbulge_date);
        cm.Parameters.AddWithValue("@diagcervialbulge_study", data.diagcervialbulge_study);
        cm.Parameters.AddWithValue("@diagcervialbulge", data.diagcervialbulge);
        cm.Parameters.AddWithValue("@diagcervialbulge_text", data.diagcervialbulge_text);
        cm.Parameters.AddWithValue("@diagcervialbulge_hnp1", data.diagcervialbulge_hnp1);
        cm.Parameters.AddWithValue("@diagcervialbulge_hnp2", data.diagcervialbulge_hnp2);
        cm.Parameters.AddWithValue("@diagthoracicbulge_date", data.diagthoracicbulge_date);
        cm.Parameters.AddWithValue("@diagthoracicbulge_study", data.diagthoracicbulge_study);
        cm.Parameters.AddWithValue("@diagthoracicbulge", data.diagthoracicbulge);
        cm.Parameters.AddWithValue("@diagthoracicbulge_text", data.diagthoracicbulge_text);
        cm.Parameters.AddWithValue("@diagthoracicbulge_hnp1", data.diagthoracicbulge_hnp1);
        cm.Parameters.AddWithValue("@diagthoracicbulge_hnp2", data.diagthoracicbulge_hnp2);
        cm.Parameters.AddWithValue("@diaglumberbulge_date", data.diaglumberbulge_date);
        cm.Parameters.AddWithValue("@diaglumberbulge_study", data.diaglumberbulge_study);
        cm.Parameters.AddWithValue("@diaglumberbulge", data.diaglumberbulge);
        cm.Parameters.AddWithValue("@diaglumberbulge_text", data.diaglumberbulge_text);
        cm.Parameters.AddWithValue("@diaglumberbulge_hnp1", data.diaglumberbulge_hnp1);
        cm.Parameters.AddWithValue("@diaglumberbulge_hnp2", data.diaglumberbulge_hnp2);
        cm.Parameters.AddWithValue("@diagleftshoulder_date", data.diagleftshoulder_date);
        cm.Parameters.AddWithValue("@diagleftshoulder_study", data.diagleftshoulder_study);
        cm.Parameters.AddWithValue("@diagleftshoulder", data.diagleftshoulder);
        cm.Parameters.AddWithValue("@diagleftshoulder_text", data.diagleftshoulder_text);
        cm.Parameters.AddWithValue("@diagrightshoulder_date", data.diagrightshoulder_date);
        cm.Parameters.AddWithValue("@diagrightshoulder_study", data.diagrightshoulder_study);
        cm.Parameters.AddWithValue("@diagrightshoulder", data.diagrightshoulder);
        cm.Parameters.AddWithValue("@diagrightshoulder_text", data.diagrightshoulder_text);
        cm.Parameters.AddWithValue("@diagleftknee_date", data.diagleftknee_date);
        cm.Parameters.AddWithValue("@diagleftknee_study", data.diagleftknee_study);
        cm.Parameters.AddWithValue("@diagleftknee", data.diagleftknee);
        cm.Parameters.AddWithValue("@diagleftknee_text", data.diagleftknee_text);
        cm.Parameters.AddWithValue("@diagrightknee_date", data.diagrightknee_date);
        cm.Parameters.AddWithValue("@diagrightknee_study", data.diagrightknee_study);
        cm.Parameters.AddWithValue("@diagrightknee", data.diagrightknee);
        cm.Parameters.AddWithValue("@diagrightknee_text", data.diagrightknee_text);
        cm.Parameters.AddWithValue("@diagbrain_date", data.diagbrain_date);
        cm.Parameters.AddWithValue("@diagbrain_study", data.diagbrain_study);
        cm.Parameters.AddWithValue("@diagbrain", data.diagbrain);
        cm.Parameters.AddWithValue("@diagbrain_text", data.diagbrain_text);
        cm.Parameters.AddWithValue("@other1_date", data.other1_date);
        cm.Parameters.AddWithValue("@other1_study", data.other1_study);
        cm.Parameters.AddWithValue("@other1", data.other1);
        cm.Parameters.AddWithValue("@other1_text", data.other1_text);
        cm.Parameters.AddWithValue("@other2_date", data.other2_date);
        cm.Parameters.AddWithValue("@other2_study", data.other2_study);
        cm.Parameters.AddWithValue("@other2", data.other2);
        cm.Parameters.AddWithValue("@other2_text", data.other2_text);
        cm.Parameters.AddWithValue("@other3_date", data.other3_date);
        cm.Parameters.AddWithValue("@other3_study", data.other3_study);
        cm.Parameters.AddWithValue("@other3", data.other3);
        cm.Parameters.AddWithValue("@other3_text", data.other3_text);
        cm.Parameters.AddWithValue("@other4_date", data.other4_date);
        cm.Parameters.AddWithValue("@other4_study", data.other4_study);
        cm.Parameters.AddWithValue("@other4", data.other4);
        cm.Parameters.AddWithValue("@other4_text", data.other4_text);
        cm.Parameters.AddWithValue("@other5_date", data.other5_date);
        cm.Parameters.AddWithValue("@other5_study", data.other5_study);
        cm.Parameters.AddWithValue("@other5", data.other5);
        cm.Parameters.AddWithValue("@other5_text", data.other5_text);
        cm.Parameters.AddWithValue("@other6_date", data.other6_date);
        cm.Parameters.AddWithValue("@other6_study", data.other6_study);
        cm.Parameters.AddWithValue("@other6", data.other6);
        cm.Parameters.AddWithValue("@other6_text", data.other6_text);
        cm.Parameters.AddWithValue("@other7_date", data.other7_date);
        cm.Parameters.AddWithValue("@other7_study", data.other7_study);
        cm.Parameters.AddWithValue("@other7", data.other7);
        cm.Parameters.AddWithValue("@other7_text", data.other7_text);
        cm.Parameters.AddWithValue("@followupin", data.followupin);
        cm.Parameters.AddWithValue("@followupin_date", data.followupin_date);
        cm.Parameters.AddWithValue("@care", data.care);
        cm.Parameters.AddWithValue("@goal", data.goal);
        cm.Parameters.AddWithValue("@universal", data.universal);
        cm.Parameters.AddWithValue("@discharge_medications", data.discharge_medications);
        var result = ExecuteScalar(cm);
        return result;
    }

    public void UpdatePage3(tbl_ie_page3 data)
    {
        MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_ie_page3 SET
		gait=@gait,
        goal=@goal,
        universal=@universal,
        care=@care,
		diagcervialbulge_date=@diagcervialbulge_date,
		diagcervialbulge_study=@diagcervialbulge_study,
		diagcervialbulge=@diagcervialbulge,
		diagcervialbulge_text=@diagcervialbulge_text,
		diagcervialbulge_hnp1=@diagcervialbulge_hnp1,
		diagcervialbulge_hnp2=@diagcervialbulge_hnp2,
		diagthoracicbulge_date=@diagthoracicbulge_date,
		diagthoracicbulge_study=@diagthoracicbulge_study,
		diagthoracicbulge=@diagthoracicbulge,
		diagthoracicbulge_text=@diagthoracicbulge_text,
		diagthoracicbulge_hnp1=@diagthoracicbulge_hnp1,
		diagthoracicbulge_hnp2=@diagthoracicbulge_hnp2,
		diaglumberbulge_date=@diaglumberbulge_date,
		diaglumberbulge_study=@diaglumberbulge_study,
		diaglumberbulge=@diaglumberbulge,
		diaglumberbulge_text=@diaglumberbulge_text,
		diaglumberbulge_hnp1=@diaglumberbulge_hnp1,
		diaglumberbulge_hnp2=@diaglumberbulge_hnp2,
		diagleftshoulder_date=@diagleftshoulder_date,
		diagleftshoulder_study=@diagleftshoulder_study,
		diagleftshoulder=@diagleftshoulder,
		diagleftshoulder_text=@diagleftshoulder_text,
		diagrightshoulder_date=@diagrightshoulder_date,
		diagrightshoulder_study=@diagrightshoulder_study,
		diagrightshoulder=@diagrightshoulder,
		diagrightshoulder_text=@diagrightshoulder_text,
		diagleftknee_date=@diagleftknee_date,
		diagleftknee_study=@diagleftknee_study,
		diagleftknee=@diagleftknee,
		diagleftknee_text=@diagleftknee_text,
		diagrightknee_date=@diagrightknee_date,
		diagrightknee_study=@diagrightknee_study,
		diagrightknee=@diagrightknee,
		diagrightknee_text=@diagrightknee_text,
		diagbrain_date=@diagbrain_date,
		diagbrain_study=@diagbrain_study,
		diagbrain=@diagbrain,
		diagbrain_text=@diagbrain_text,
		other1_date=@other1_date,
		other1_study=@other1_study,
		other1=@other1,
			other1_text=@other1_text,
		other2_date=@other2_date,
		other2_study=@other2_study,
		other2=@other2,
		other2_text=@other2_text,
		other3_date=@other3_date,
		other3_study=@other3_study,
		other3=@other3,
		other3_text=@other3_text,
		other4_date=@other4_date,
		other4_study=@other4_study,
		other4=@other4,
		other4_text=@other4_text,
		other5_date=@other5_date,
		other5_study=@other5_study,
		other5=@other5,
		other5_text=@other5_text,
		other6_date=@other6_date,
		other6_study=@other6_study,
		other6=@other6,
		other6_text=@other6_text,
		other7_date=@other7_date,
		other7_study=@other7_study,
		other7=@other7,
		other7_text=@other7_text,
		followupin=@followupin,
        discharge_medications=@discharge_medications,
		followupin_date=@followupin_date		where id=@id", conn);
        cm.Parameters.AddWithValue("@id", data.id);

        cm.Parameters.AddWithValue("@gait", data.gait);
        cm.Parameters.AddWithValue("@diagcervialbulge_date", data.diagcervialbulge_date);
        cm.Parameters.AddWithValue("@diagcervialbulge_study", data.diagcervialbulge_study);
        cm.Parameters.AddWithValue("@diagcervialbulge", data.diagcervialbulge);
        cm.Parameters.AddWithValue("@diagcervialbulge_text", data.diagcervialbulge_text);
        cm.Parameters.AddWithValue("@diagcervialbulge_hnp1", data.diagcervialbulge_hnp1);
        cm.Parameters.AddWithValue("@diagcervialbulge_hnp2", data.diagcervialbulge_hnp2);
        cm.Parameters.AddWithValue("@diagthoracicbulge_date", data.diagthoracicbulge_date);
        cm.Parameters.AddWithValue("@diagthoracicbulge_study", data.diagthoracicbulge_study);
        cm.Parameters.AddWithValue("@diagthoracicbulge", data.diagthoracicbulge);
        cm.Parameters.AddWithValue("@diagthoracicbulge_text", data.diagthoracicbulge_text);
        cm.Parameters.AddWithValue("@diagthoracicbulge_hnp1", data.diagthoracicbulge_hnp1);
        cm.Parameters.AddWithValue("@diagthoracicbulge_hnp2", data.diagthoracicbulge_hnp2);
        cm.Parameters.AddWithValue("@diaglumberbulge_date", data.diaglumberbulge_date);
        cm.Parameters.AddWithValue("@diaglumberbulge_study", data.diaglumberbulge_study);
        cm.Parameters.AddWithValue("@diaglumberbulge", data.diaglumberbulge);
        cm.Parameters.AddWithValue("@diaglumberbulge_text", data.diaglumberbulge_text);
        cm.Parameters.AddWithValue("@diaglumberbulge_hnp1", data.diaglumberbulge_hnp1);
        cm.Parameters.AddWithValue("@diaglumberbulge_hnp2", data.diaglumberbulge_hnp2);
        cm.Parameters.AddWithValue("@diagleftshoulder_date", data.diagleftshoulder_date);
        cm.Parameters.AddWithValue("@diagleftshoulder_study", data.diagleftshoulder_study);
        cm.Parameters.AddWithValue("@diagleftshoulder", data.diagleftshoulder);
        cm.Parameters.AddWithValue("@diagleftshoulder_text", data.diagleftshoulder_text);
        cm.Parameters.AddWithValue("@diagrightshoulder_date", data.diagrightshoulder_date);
        cm.Parameters.AddWithValue("@diagrightshoulder_study", data.diagrightshoulder_study);
        cm.Parameters.AddWithValue("@diagrightshoulder", data.diagrightshoulder);
        cm.Parameters.AddWithValue("@diagrightshoulder_text", data.diagrightshoulder_text);
        cm.Parameters.AddWithValue("@diagleftknee_date", data.diagleftknee_date);
        cm.Parameters.AddWithValue("@diagleftknee_study", data.diagleftknee_study);
        cm.Parameters.AddWithValue("@diagleftknee", data.diagleftknee);
        cm.Parameters.AddWithValue("@diagleftknee_text", data.diagleftknee_text);
        cm.Parameters.AddWithValue("@diagrightknee_date", data.diagrightknee_date);
        cm.Parameters.AddWithValue("@diagrightknee_study", data.diagrightknee_study);
        cm.Parameters.AddWithValue("@diagrightknee", data.diagrightknee);
        cm.Parameters.AddWithValue("@diagrightknee_text", data.diagrightknee_text);
        cm.Parameters.AddWithValue("@diagbrain_date", data.diagbrain_date);
        cm.Parameters.AddWithValue("@diagbrain_study", data.diagbrain_study);
        cm.Parameters.AddWithValue("@diagbrain", data.diagbrain);
        cm.Parameters.AddWithValue("@diagbrain_text", data.diagbrain_text);
        cm.Parameters.AddWithValue("@other1_date", data.other1_date);
        cm.Parameters.AddWithValue("@other1_study", data.other1_study);
        cm.Parameters.AddWithValue("@other1", data.other1);
        cm.Parameters.AddWithValue("@other1_text", data.other1_text);
        cm.Parameters.AddWithValue("@other2_date", data.other2_date);
        cm.Parameters.AddWithValue("@other2_study", data.other2_study);
        cm.Parameters.AddWithValue("@other2", data.other2);
        cm.Parameters.AddWithValue("@other2_text", data.other2_text);
        cm.Parameters.AddWithValue("@other3_date", data.other3_date);
        cm.Parameters.AddWithValue("@other3_study", data.other3_study);
        cm.Parameters.AddWithValue("@other3", data.other3);
        cm.Parameters.AddWithValue("@other3_text", data.other3_text);
        cm.Parameters.AddWithValue("@other4_date", data.other4_date);
        cm.Parameters.AddWithValue("@other4_study", data.other4_study);
        cm.Parameters.AddWithValue("@other4", data.other4);
        cm.Parameters.AddWithValue("@other4_text", data.other4_text);
        cm.Parameters.AddWithValue("@other5_date", data.other5_date);
        cm.Parameters.AddWithValue("@other5_study", data.other5_study);
        cm.Parameters.AddWithValue("@other5", data.other5);
        cm.Parameters.AddWithValue("@other5_text", data.other5_text);
        cm.Parameters.AddWithValue("@other6_date", data.other6_date);
        cm.Parameters.AddWithValue("@other6_study", data.other6_study);
        cm.Parameters.AddWithValue("@other6", data.other6);
        cm.Parameters.AddWithValue("@other6_text", data.other6_text);
        cm.Parameters.AddWithValue("@other7_date", data.other7_date);
        cm.Parameters.AddWithValue("@other7_study", data.other7_study);
        cm.Parameters.AddWithValue("@other7", data.other7);
        cm.Parameters.AddWithValue("@other7_text", data.other7_text);
        cm.Parameters.AddWithValue("@followupin", data.followupin);
        cm.Parameters.AddWithValue("@followupin_date", data.followupin_date);
        cm.Parameters.AddWithValue("@care", data.care);
        cm.Parameters.AddWithValue("@goal", data.goal);
        cm.Parameters.AddWithValue("@universal", data.universal);
        cm.Parameters.AddWithValue("@discharge_medications", data.discharge_medications);
        Execute(cm);
    }
    public tbl_ie_page3? GetOnePage3(int id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_ie_page3 where ie_id=@id ", conn);
        cm.Parameters.AddWithValue("@id", id);
        var datalist = ConvertDataTable<tbl_ie_page3>(GetData(cm)).FirstOrDefault();
        return datalist;
    }

    #endregion

    #region NE

    public int InsertNE(tbl_ie_ne data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_ne
		(neurological_exam,sensory,manual_muscle_strength_testing,ie_id,patient_id,other_content,cmp_id)Values
				(@neurological_exam,@sensory,@manual_muscle_strength_testing,@ie_id,@patient_id,@other_content,@cmp_id);select @@identity;", conn);
        cm.Parameters.AddWithValue("@neurological_exam", data.neurological_exam);
        cm.Parameters.AddWithValue("@sensory", data.sensory);
        cm.Parameters.AddWithValue("@manual_muscle_strength_testing", data.manual_muscle_strength_testing);
        cm.Parameters.AddWithValue("@ie_id", data.ie_id);
        cm.Parameters.AddWithValue("@other_content", data.other_content);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
        var result = ExecuteScalar(cm);
        return result;
    }
    public int UpdatePageNE(tbl_ie_ne data)
    {
        MySqlCommand cm = new MySqlCommand(@"update tbl_ie_ne
		 set neurological_exam=@neurological_exam,sensory=@sensory,manual_muscle_strength_testing=@manual_muscle_strength_testing,other_content=@other_content
         where id=@id;select 1;", conn);

        cm.Parameters.AddWithValue("@neurological_exam", data.neurological_exam);
        cm.Parameters.AddWithValue("@sensory", data.sensory);
        cm.Parameters.AddWithValue("@other_content", data.other_content);
        cm.Parameters.AddWithValue("@manual_muscle_strength_testing", data.manual_muscle_strength_testing);
        cm.Parameters.AddWithValue("@id", data.id);

        var result = ExecuteScalar(cm);
        return result;
    }

    public tbl_ie_ne GetOneNE(int id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_ie_ne where ie_id=@id ", conn);
        cm.Parameters.AddWithValue("@id", id);
        var datalist = ConvertDataTable<tbl_ie_ne>(GetData(cm)).FirstOrDefault();
        return datalist;
    }

    #endregion

    #region OtherPage
    public int InsertOtherPage(tbl_ie_other data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_other
		(ie_id,patient_id,treatment_details,treatment_delimit,followup_duration,followup_date,treatment_delimit_desc)Values
				(@ie_id,@patient_id,@treatment_details,@note1,@note2,@note3,@treatment_delimit,@followup_duration,@followup_date,@treatment_delimit_desc);select @@identity", conn);
        cm.Parameters.AddWithValue("@ie_id", data.ie_id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@treatment_details", data.treatment_details);
        cm.Parameters.AddWithValue("@note1", data.note1);
        cm.Parameters.AddWithValue("@note2", data.note2);
        cm.Parameters.AddWithValue("@note3", data.note3);
        cm.Parameters.AddWithValue("@followup_date", data.followup_date);
        cm.Parameters.AddWithValue("@followup_duration", data.followup_duration);
        cm.Parameters.AddWithValue("@treatment_delimit_desc", data.treatment_delimit_desc.TrimStart('^'));
        cm.Parameters.AddWithValue("@treatment_delimit", data.treatment_delimit == null ? "" : data.treatment_delimit.TrimStart(','));
        var result = ExecuteScalar(cm);
        return result;
    }

    public void UpdateOtherPage(tbl_ie_other data)
    {
        MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_ie_other SET
		treatment_details=@treatment_details,followup_date=@followup_date,followup_duration=@followup_duration,
        treatment_delimit_desc=@treatment_delimit_desc,note1 = @note1,note2 = @note2,note3= @note3,
		treatment_delimit=@treatment_delimit where id=@id", conn);
        cm.Parameters.AddWithValue("@id", data.id);
        cm.Parameters.AddWithValue("@treatment_details", data.treatment_details);
        cm.Parameters.AddWithValue("@note1", data.note1);
        cm.Parameters.AddWithValue("@note2", data.note2);
        cm.Parameters.AddWithValue("@note3", data.note3);
        cm.Parameters.AddWithValue("@followup_date", data.followup_date);
        cm.Parameters.AddWithValue("@followup_duration", data.followup_duration);
        cm.Parameters.AddWithValue("@treatment_delimit", data.treatment_delimit == null ? "" : data.treatment_delimit.TrimStart(','));
        cm.Parameters.AddWithValue("@treatment_delimit_desc", data.treatment_delimit_desc.TrimStart('^'));
        Execute(cm);
    }

    public tbl_ie_other? GetOneOtherPage(int id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_ie_other where ie_id=@id ", conn);
        cm.Parameters.AddWithValue("@id", id);
        var datalist = ConvertDataTable<tbl_ie_other>(GetData(cm)).FirstOrDefault();
        return datalist;
    }
    #endregion

    #region Comment
    public tbl_ie_comment? GetOneComment(int id)
    {
        DataTable dt = new DataTable();
        MySqlCommand cm = new MySqlCommand("select * from tbl_ie_comment where ie_id=@id ", conn);
        cm.Parameters.AddWithValue("@id", id);
        var datalist = ConvertDataTable<tbl_ie_comment>(GetData(cm)).FirstOrDefault();
        return datalist;
    }
    public int InsertComment(tbl_ie_comment data)
    {
        MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_comment
		(ie_id,patient_id,comment_details)Values
				(@ie_id,@patient_id,@comment_details);select @@identity", conn);
        cm.Parameters.AddWithValue("@ie_id", data.ie_id);
        cm.Parameters.AddWithValue("@patient_id", data.patient_id);
        cm.Parameters.AddWithValue("@comment_details", data.comment_details);
        var result = ExecuteScalar(cm);
        return result;
    }
    public void UpdateComment(tbl_ie_comment data)
    {
        MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_ie_comment SET
				comment_details=@comment_details		where id=@id", conn);
        cm.Parameters.AddWithValue("@id", data.id);
        cm.Parameters.AddWithValue("@comment_details", data.comment_details);
        Execute(cm);
    }
    #endregion

}