using MS.Models;
using MySql.Data.MySqlClient;
using PainTrax.Services;
using System.Data;

namespace PainTrax.Web.Services
{
    public class PatientImportService : ParentService
    {
        #region Page1
        public int InsertPage1(tbl_ie_page1 data)
        {

            var obj = this.GetOne(data.ie_id.Value);

            if (obj == null)
            {
                MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_page1
		(history,pmh,psh,allergies,medication,family_history,social_history,cc,pe,ie_id,patient_id,cmp_id)Values
				(@history,@pmh,@psh,@allergies,@medication,@family_history,@social_history,@cc,@pe,@ie_id,@patient_id,@cmp_id);select @@identity;", conn);
                cm.Parameters.AddWithValue("@history", data.history);
                cm.Parameters.AddWithValue("@pmh", data.pmh);
                cm.Parameters.AddWithValue("@psh", data.psh);
                cm.Parameters.AddWithValue("@allergies", data.allergies);
                cm.Parameters.AddWithValue("@medication", data.medication);
                cm.Parameters.AddWithValue("@family_history", data.family_history);
                cm.Parameters.AddWithValue("@social_history", data.social_history);
                cm.Parameters.AddWithValue("@cc", data.cc);
                cm.Parameters.AddWithValue("@pe", data.pe);
                cm.Parameters.AddWithValue("@ie_id", data.ie_id);
                cm.Parameters.AddWithValue("@patient_id", data.patient_id);
                cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
                var result = ExecuteScalar(cm);
                return result;
            }
            else
            {
                MySqlCommand cm = new MySqlCommand(@"Update tbl_ie_page1 set
		history=@history,pmh=@pmh,psh=@psh,allergies=@allergies,medication=@medication,family_history=@family_history,social_history=@social_history,
        cc=@cc,pe=@pe where patient_id=@patient_id;select 1;", conn); ;
                cm.Parameters.AddWithValue("@history", data.history);
                cm.Parameters.AddWithValue("@pmh", data.pmh);
                cm.Parameters.AddWithValue("@psh", data.psh);
                cm.Parameters.AddWithValue("@allergies", data.allergies);
                cm.Parameters.AddWithValue("@medication", data.medication);
                cm.Parameters.AddWithValue("@family_history", data.family_history);
                cm.Parameters.AddWithValue("@social_history", data.social_history);
                cm.Parameters.AddWithValue("@cc", data.cc);
                cm.Parameters.AddWithValue("@pe", data.pe);

                cm.Parameters.AddWithValue("@patient_id", data.patient_id);

                var result = ExecuteScalar(cm);
                return result;
            }
        }




        public tbl_patient_ie? GetOne(int data)
        {
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select * from tbl_ie_page1 where ie_id=@id ", conn);
            cm.Parameters.AddWithValue("@id", data);
            var datalist = ConvertDataTable<tbl_patient_ie>(GetData(cm)).FirstOrDefault();
            return datalist;
        }
        #endregion

        #region Page2
        public int InsertPage2(tbl_ie_page2 data)
        {

            var obj = this.GetPage2One(data.ie_id.Value);

            if (obj == null)
            {
                MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_ie_page2
		(ros,aod,ie_id,patient_id,cmp_id)Values
				(@ros,@aod,@ie_id,@patient_id,@cmp_id);select @@identity;", conn);
                cm.Parameters.AddWithValue("@ros", data.ros);
                cm.Parameters.AddWithValue("@aod", data.aod);
                cm.Parameters.AddWithValue("@ie_id", data.ie_id);
                cm.Parameters.AddWithValue("@patient_id", data.patient_id);
                cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
                var result = ExecuteScalar(cm);
                return result;
            }
            else
            {
                MySqlCommand cm = new MySqlCommand(@"update tbl_ie_page2 set
		ros=@ros,aod=@aod where ie_id=@ie_id;select 1;", conn);
                cm.Parameters.AddWithValue("@ros", data.ros);
                cm.Parameters.AddWithValue("@aod", data.aod);
                cm.Parameters.AddWithValue("@ie_id", data.ie_id);
              
                var result = ExecuteScalar(cm);
                return result;
            }
        }




        public tbl_ie_page2? GetPage2One(int data)
        {
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select * from tbl_ie_page2 where ie_id=@id ", conn);
            cm.Parameters.AddWithValue("@id", data);
            var datalist = ConvertDataTable<tbl_ie_page2>(GetData(cm)).FirstOrDefault();
            return datalist;
        }
        #endregion
    }
}
