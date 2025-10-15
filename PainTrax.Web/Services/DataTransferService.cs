using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using PainTrax.Web.ViewModel;
using PainTrax.Web.Models;
using System.Reflection;
using DocumentFormat.OpenXml.Drawing.Charts;
using MS.Services;
namespace PainTrax.Web.Services
{
    public class DataTransferService
    {
        private readonly string _sqlServerConn;
        private readonly string _mySqlConn;
        private readonly ProcedureService _procedureService = new ProcedureService();
        private readonly PatientIEService _patientIEService = new PatientIEService();

        public DataTransferService(IConfiguration config)
        {
            _sqlServerConn = config.GetConnectionString("SqlServerConn");
            _mySqlConn = config.GetConnectionString("MySqlConn");
        }

        public int TransferEmployees()
        {
            int insertedCount = 0;
            var pdVM = new DataSet();

            // Step 1: Read from SQL Server
            using (SqlConnection sqlConn = new SqlConnection(_sqlServerConn))
            {
                sqlConn.Open();
                string sqlQuery = "select p.MCODE,pd.* from tblProceduresDetail pd inner join tblProcedures p on pd.Procedure_Master_ID=p.Procedure_ID";

                SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn);
                DataSet dataSet = new DataSet();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                sqlDataAdapter.Fill(dataSet);

                pdVM = dataSet;

            }

            foreach (DataRow row in pdVM.Tables[0].Rows)
            {

                string cnd= " and cmp_id=10 and mcode='"+ row["MCODE"] + "'";

                var ProcedureMasterID = _procedureService.GetId(cnd);

                cnd = " and cmp_id=10 and old_id='" + row["PatientIE_ID"] + "'";

                var PatientIEID = _patientIEService.GetId(cnd);

                cnd = " and cmp_id=10 and old_id='" + row["PatientFU_ID"] + "'";

                var PatientFUID = _patientIEService.GetId(cnd);

                ProcedureDetailsVM vm = new ProcedureDetailsVM
                {
                    BodyPart = row["BodyPart"] != DBNull.Value ? row["BodyPart"].ToString() : string.Empty,
                    BodypartSide = string.Empty,
                    Backup_Line = string.Empty,
                    Category = string.Empty,
                    Cmp_Id = 19,
                    Consider = row["Consider"] != DBNull.Value ? Convert.ToDateTime(row["Consider"].ToString()) : null,
                    CreatedBy = string.Empty,
                    CT_AUTH_Date = null,
                    CT_Note = string.Empty,
                    CT_Report_Status = string.Empty,
                    CT_ReSche_Date = null,
                    Executed = row["Executed"] != DBNull.Value ? Convert.ToDateTime(row["Executed"].ToString()) : null,
                    Exe_Pos = row["Executed_Position"] != DBNull.Value ? row["Executed_Position"].ToString() : null,
                    Followup = row["Followup"] != DBNull.Value ? Convert.ToDateTime(row["Followup"].ToString()) : null,
                    FU_Pos = row["FU_Pos"] != DBNull.Value ? row["FU_Pos"].ToString() : string.Empty,
                    Ins_Note = null,
                    Ins_Ver_Status = null,
                    IsConsidered = row["IsConsidered"] != DBNull.Value ? Convert.ToBoolean(row["IsConsidered"]) : null,
                    IsFromNew = null,
                    IsVaccinated = null,
                    Level = row["Level"] != DBNull.Value ? row["Level"].ToString() : string.Empty,
                    MC_Date = null,
                    MC_Note = null,
                    MC_Report_Status = null,
                    MC_ReSche_Date = null,
                    Medication = row["Medication"] != DBNull.Value ? row["Medication"].ToString() : string.Empty,
                    MC_Type = null,
                    Muscle = row["Muscle"] != DBNull.Value ? row["Muscle"].ToString() : string.Empty,
                    PatientFuID = PatientFUID,
                    PatientIEID = PatientIEID,
                    PatientProceduresID = null,
                    ProcedureDetailID = null,
                    ProcedureDetail_ID = null,
                    ProcedureID = null,
                    ProcedureMasterID = ProcedureMasterID,
                    Requested = row["Requested"] != DBNull.Value ? Convert.ToDateTime(row["Requested"].ToString()) : null,
                    Req_Pos = row["Requested_Position"] != DBNull.Value ? row["Requested_Position"].ToString() : null,
                    Scheduled = row["Scheduled"] != DBNull.Value ? Convert.ToDateTime(row["Scheduled"].ToString()) : null,
                    Sch_Pos = row["Scheduled_Position"] != DBNull.Value ? row["Scheduled_Position"].ToString() : null,
                    Side = row["Sides"] != DBNull.Value ? row["Sides"].ToString() : null,
                    SignPath = row["SignPath"] != DBNull.Value ? row["SignPath"].ToString() : null,
                    Subcode = row["Subcode"] != DBNull.Value ? row["Subcode"].ToString() : null,
                    Vac_Note = null,
                    Vac_Status = null

                };
            }


            return insertedCount;
        }

    }
}
