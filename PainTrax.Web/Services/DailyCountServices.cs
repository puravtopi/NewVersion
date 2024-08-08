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
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

namespace PainTrax.Web.Services
{
    public class DailyCountServices : ParentService
    {
        #region local variables
        PatientIEService patientIEService = new PatientIEService();
        FUPage1Service page1FUService = new FUPage1Service();
        #endregion


        public List<DailyCountReportVM> GetDailyCountReport(string cnd)
        {
            string query = "";

            if (!string.IsNullOrEmpty(cnd))
            {
                query = "SELECT DOE as DOE, Location,max(NoOfIE) as NoOfIE,max(NoOfFU) as NoOfFU from(select count(tblpat.id) as NoOfIE, 0 AS NoOfFU, tblpat.DOE as DOE, tblLoc.Location from tbl_patient_ie tblpat inner " +
                        " join tbl_locations tblLoc on tblpat.location_id = tblLoc.id " +
                        " where ( tblpat.doe BETWEEN " + cnd + " ) group by tblLoc.Location,tblpat.DOE union all select 0  AS NoOfIE, count(tblFUPat.id) as NoOfFU ,tblFUPat.DOE as DOE, tblLoc.Location from tbl_patient_fu tblFUPat " +
                        " INNER JOIN tbl_patient_ie ie ON ie.id = tblFUPat.patientIE_ID inner join tbl_locations tblLoc on ie.location_id = tblLoc.id " +
                        " where(tblFUPat.doe BETWEEN " + cnd + ") group by tblLoc.Location,tblFUPat.DOE  )t " +
                        " group by DOE, Location order by DOE asc ";
            }

            //if (!string.IsNullOrEmpty(cmpid))
            //{
            //    query = query + " " + cmpid;
            //}

            MySqlCommand cm = new MySqlCommand(query, conn);

            var datalist = ConvertDataTable<DailyCountReportVM>(GetData(cm));
            return datalist;
        }

    }
}
