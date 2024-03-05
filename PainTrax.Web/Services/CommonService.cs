using MS.Models;
using PainTrax.Services;
using PainTrax.Web.Models;

namespace PainTrax.Web.Services
{
    public class CommonService : ParentService
    {
        public List<tbl_pages> GetPagesAll(string cnd = "")
        {
            string query = "select * from tbl_pages where 1=1 ";
            query += cnd;

            List<tbl_pages> dataList = ConvertDataTable<tbl_pages>(GetData(query));
            return dataList;
        }

        public List<tbl_reports> GetReportsAll(string cnd = "")
        {
            string query = "select * from tbl_reports where 1=1 ";
            query += cnd;

            List<tbl_reports> dataList = ConvertDataTable<tbl_reports>(GetData(query));
            return dataList;
        }

        public List<tbl_role> GetRoleAll(string cnd = "")
        {
            string query = "select * from tbl_role where 1=1 ";
            query += cnd;

            List<tbl_role> dataList = ConvertDataTable<tbl_role>(GetData(query));
            return dataList;
        }

        public List<tbl_treatment_master> GetTreatmentAll(string cnd = "")
        {
            string query = "select * from tbl_treatment_master where 1=1 ";

            query += cnd;
            List<tbl_treatment_master> dataList = ConvertDataTable<tbl_treatment_master>(GetData(query));
            return dataList;
        }
    }
}
