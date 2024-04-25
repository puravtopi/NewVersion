﻿using PainTrax.Services;
using MS.Services;
using PainTrax.Web.ViewModel;
using MS.Models;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;

namespace PainTrax.Web.Services
{
    public class POCServices : ParentService
    {
        #region local variables
        PatientIEService patientIEService = new PatientIEService();
        FUPage1Service page1FUService = new FUPage1Service();
        #endregion

        public string[] GetInjuredParts(int patientIE, int patientFU = 0)
        {
            try
            {

                string[] result = null;

                if (patientFU == 0)
                {
                    var data = patientIEService.GetOnePage1(patientIE);


                    if (data != null)
                    {
                        if (data.bodypart != null)
                            result = data.bodypart.Split(',');
                    }
                }
                else
                {
                    var data = page1FUService.GetOne(patientFU);


                    if (data != null)
                    {
                        if (data.bodypart != null)
                            result = data.bodypart.Split(',');
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetAllProcedures(string bodyParts, int patientIEID, string potion, int cmp_id)
        {
            try
            {
                DataTable dt = new DataTable();
                MySqlCommand cm = new MySqlCommand("CALL sp_GetAllProceduress('" + bodyParts + "'," + patientIEID + ",'" + potion + "'," + cmp_id + ")", conn);

                var datalist = GetData(cm);

                return datalist;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetAllProceduresFU(string bodyParts, int patientFUID, string potion, int cmp_id)
        {
            try
            {




                DataTable dt = new DataTable();
                MySqlCommand cm = new MySqlCommand("CALL sp_GetAllProcedures_FU('" + bodyParts + "','" + potion + "'," + patientFUID + "," + cmp_id + ")", conn);

                var datalist = GetData(cm);

                return datalist;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetMuscle(int patientIEID)
        {
            string strresult = "";
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select  HasMuscle from tbl_Procedures where id =@Procedure_ID ", conn);
            cm.Parameters.AddWithValue("@Procedure_ID", patientIEID);
            var result = GetData(cm);

            if (result != null && result.Rows.Count > 0)
            {
                strresult = result.Rows[0]["HasMuscle"].ToString();
            }

            return strresult;
        }

        public string GetSubCode(int patientIEID)
        {
            string strresult = "";
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select HasSubProcedure from tblProcedures where Procedure_ID=@Procedure_ID ", conn);
            cm.Parameters.AddWithValue("@Procedure_ID", patientIEID);
            var result = GetData(cm);

            if (result != null && result.Rows.Count > 0)
            {
                strresult = result.Rows[0]["HasSubProcedure"].ToString();
            }

            return strresult;
        }

        public string GetMedication(int patientIEID)
        {
            string strresult = "";
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select HasMedication from tblProcedures where Procedure_ID=@Procedure_ID ", conn);
            cm.Parameters.AddWithValue("@Procedure_ID", patientIEID);
            var result = GetData(cm);

            if (result != null && result.Rows.Count > 0)
            {
                strresult = result.Rows[0]["HasMedication"].ToString();
            }

            return strresult;
        }

        public string SaveProcedureDetails(ProcedureDetailsVM model)
        {
            List<MySqlParameter> param = new List<MySqlParameter>();

            param.Add(new MySqlParameter("v_Backup_Line", model.Backup_Line));
            param.Add(new MySqlParameter("v_BodyPart", model.BodyPart));
            param.Add(new MySqlParameter("v_BodypartSide", model.BodypartSide));
            param.Add(new MySqlParameter("v_Category", model.Category));
            param.Add(new MySqlParameter("v_Consider", model.Consider));
            param.Add(new MySqlParameter("v_CreatedBy", model.CreatedBy));
            param.Add(new MySqlParameter("v_CT_AUTH_Date", model.CT_AUTH_Date));
            param.Add(new MySqlParameter("v_CT_Note", model.CT_Note));
            param.Add(new MySqlParameter("v_CT_Report_Status", model.CT_Report_Status));
            param.Add(new MySqlParameter("v_CT_ReSche_Date", model.CT_ReSche_Date));
            param.Add(new MySqlParameter("v_Executed", model.Executed));
            param.Add(new MySqlParameter("v_Exe_Pos", model.Exe_Pos));
            param.Add(new MySqlParameter("v_Followup", model.Followup));
            param.Add(new MySqlParameter("v_FU_Pos", model.FU_Pos));
            param.Add(new MySqlParameter("v_Ins_Note", model.Ins_Note));
            param.Add(new MySqlParameter("v_Ins_Ver_Status", model.Ins_Ver_Status));
            param.Add(new MySqlParameter("v_IsConsidered", model.IsConsidered));
            param.Add(new MySqlParameter("v_IsFromNew", model.IsFromNew));
            param.Add(new MySqlParameter("v_IsVaccinated", model.IsVaccinated));
            param.Add(new MySqlParameter("v_Level", model.Level));
            param.Add(new MySqlParameter("v_MC_Date", model.MC_Date));
            param.Add(new MySqlParameter("v_MC_Note", model.MC_Note));
            param.Add(new MySqlParameter("v_MC_Report_Status", model.MC_Report_Status));
            param.Add(new MySqlParameter("v_MC_ReSche_Date", model.MC_ReSche_Date));
            param.Add(new MySqlParameter("v_MC_Type", model.MC_Type));
            param.Add(new MySqlParameter("v_Medication", model.Medication));
            param.Add(new MySqlParameter("v_Muscle", model.Muscle));
            param.Add(new MySqlParameter("v_PatientFuID", model.PatientFuID));
            param.Add(new MySqlParameter("v_PatientIEID", model.PatientIEID));
            param.Add(new MySqlParameter("v_PatientProceduresID", model.PatientProceduresID));
            param.Add(new MySqlParameter("v_ProcedureDetailID", model.ProcedureDetailID));
            param.Add(new MySqlParameter("v_ProcedureID", model.ProcedureID));
            param.Add(new MySqlParameter("v_ProcedureMasterID", model.ProcedureMasterID));
            param.Add(new MySqlParameter("v_Requested", model.Requested));
            param.Add(new MySqlParameter("v_Req_Pos", model.Req_Pos));
            param.Add(new MySqlParameter("v_Scheduled", model.Scheduled));
            param.Add(new MySqlParameter("v_Sch_Pos", model.Sch_Pos));
            param.Add(new MySqlParameter("v_Side", model.Side));
            param.Add(new MySqlParameter("v_SignPath", model.SignPath));
            param.Add(new MySqlParameter("v_Subcode", model.Subcode));
            param.Add(new MySqlParameter("v_Vac_Note", model.Vac_Note));
            param.Add(new MySqlParameter("v_Vac_Status", model.Vac_Status));

            var data = ExecuteSP(CommonSp.SavePatientProceduresDetails, param);

            return data;
        }

        public ProcedureDetailsVM? GetProcedureDetails(int ProcedurrDetail_ID)
        {
            try
            {

                MySqlCommand cm = new MySqlCommand("CALL sp_GetProcedureDetails(" + ProcedurrDetail_ID + ")", conn);

                var datalist = ConvertDataTable<ProcedureDetailsVM>(GetData(cm)).FirstOrDefault();
                return datalist;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetProcedureCountDetails(int ProcedureID, int PatientIEID)
        {
            try
            {

                MySqlCommand cm = new MySqlCommand("CALL sp_GetProcBycount(" + ProcedureID + "," + PatientIEID + ")", conn);

                var datalist = (GetData(cm));
                return datalist;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetPOCIE(int PatientIEID)
        {
            try
            {

                MySqlCommand cm = new MySqlCommand("CALL sp_GetPOC(" + PatientIEID + ")", conn);

                var datalist = (GetData(cm));
                return datalist;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetPOCFU(int PatientFUID)
        {
            try
            {

                MySqlCommand cm = new MySqlCommand("CALL sp_GetPOC_FU(" + PatientFUID + ")", conn);

                var datalist = (GetData(cm));
                return datalist;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<POCSumarryVM> GetExecutedPOCIE(int PatientIEID)
        {
            try
            {

                string query = "CALL sp_GetExecutedPOC(" + PatientIEID + ")";

                List<POCSumarryVM> dataList = ConvertDataTable<POCSumarryVM>(GetData(query));
                return dataList;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<POCSumarryVM> GetPOCSummary(int PatientIEID)
        {
            try
            {

                string query = "CALL sp_GetPOC(" + PatientIEID + ")";

                List<POCSumarryVM> dataList = ConvertDataTable<POCSumarryVM>(GetData(query));
                return dataList;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<POCSumarryVM> GetFUPOCSummary(int PatientFUID)
        {
            try
            {

                string query = "CALL sp_GetPOC_FU(" + PatientFUID + ")";

                List<POCSumarryVM> dataList = ConvertDataTable<POCSumarryVM>(GetData(query));
                return dataList;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool RemoveProcedureCountDetails(int ProcedureDetailsID)
        {
            try
            {

                MySqlCommand cm = new MySqlCommand(@"DELETE FROM tbl_procedures_details where ProcedureDetail_ID=@id", conn);
                cm.Parameters.AddWithValue("@id", ProcedureDetailsID);
                Execute(cm);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int ForwardPOCIETOFU(int PatientIEID, int PatientFUID)
        {
            try
            {
                DataTable dt = new DataTable();
                MySqlCommand cm = new MySqlCommand("CALL sp_ForwardPOC_IETOFU('" + PatientIEID + "','" + PatientFUID + "')", conn);

                var datalist = GetData(cm);

                return 1;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
