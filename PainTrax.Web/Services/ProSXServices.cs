using PainTrax.Services;
using MS.Services;
using PainTrax.Web.ViewModel;
using MS.Models;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;

using DocumentFormat.OpenXml.Math;
using Humanizer;
using Microsoft.CodeAnalysis;
using Org.BouncyCastle.Asn1.Ocsp;

namespace PainTrax.Web.Services
{
    public class ProSXServices : ParentService
    {
        #region local variables
        PatientIEService patientIEService = new PatientIEService();
        FUPage1Service page1FUService = new FUPage1Service();
        #endregion


        public List<ProSXReportVM> GetProSXReport(string cnd)
        {
            string query = "SELECT DISTINCT CONCAT(pm.lname,' ',pm.fname)as 'Name',IFNULL(pm.MC,'') AS MC,ie.Compensation AS 'CaseType' ," +
           "lc.location,CASE when pm.Vaccinated = 1 THEN 'Yes' ELSE 'No' END AS Vaccinated,tp.MCODE, " +
            "CASE when pm.gender = '1' THEN 'M' when pm.gender = '2' then 'F' when pm.gender = '3' then 'O'  ELSE '' END AS gender" +
           ",tp.Scheduled  FROM tbl_Procedures_Details tp" +
           " inner join tbl_patient_ie ie on tp.PatientIE_ID = ie.id" +
           " inner join tbl_Patient pm on pm.id = ie.Patient_ID" +
           " inner join tbl_locations lc ON ie.Location_ID = lc.id" +
           " inner join tbl_attorneys a on a.id = ie.attorney_id";

            if (!string.IsNullOrEmpty(cnd))
            {
                query = query + " " + cnd;
            }

            MySqlCommand cm = new MySqlCommand(query, conn);

            var datalist = ConvertDataTable<ProSXReportVM>(GetData(cm));
            return datalist;
        }

        public List<string> GetProSXReportDate(string cmpid)
        {
            string query = "SELECT DISTINCT DATE_FORMAT(pd.Scheduled, '%m/%d/%Y') AS Scheduled FROM tbl_procedures_details pd WHERE pd.Scheduled IS NOT NULL and pd.cmp_id=" + cmpid + " ORDER BY pd.Scheduled desc";

            MySqlCommand cm = new MySqlCommand(query, conn);

            var datalist = GetData(cm);


            List<string> list = datalist.AsEnumerable()
                             .Select(row => row["Scheduled"].ToString())
                             .ToList();

            return list;
        }

    }
}
