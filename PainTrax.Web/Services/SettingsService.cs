using MySql.Data.MySqlClient;
using PainTrax.Services;
using PainTrax.Web.Models;
using System.Data;

namespace PainTrax.Web.Services
{
    public class SettingsService : ParentService
    {
        public List<tbl_settings> GetAll(string cnd = "")
        {

            string query = "select * from tbl_settings where 1=1 ";

            if (!string.IsNullOrEmpty(cnd))
                query = query + cnd;

            List<tbl_settings> dataList = ConvertDataTable<tbl_settings>(GetData(query));
            return dataList;
        }
        public tbl_settings? GetOne(int id)
        {
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select * from tbl_settings where cmp_id=@id ", conn);
            cm.Parameters.AddWithValue("@id", id);
            var datalist = ConvertDataTable<tbl_settings>(GetData(cm)).FirstOrDefault();
            return datalist;
        }
        public void Insert(tbl_settings data)
        {
            MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_settings
		(page_size,location,dateformat,cmp_id,pageBreakForInjection,injectionAsSeparateBlock)Values
				(@page_size,@location,@dateformat,@cmp_id,@pageBreakForInjection,@injectionAsSeparateBlock)", conn);
            cm.Parameters.AddWithValue("@page_size", data.page_size);
            cm.Parameters.AddWithValue("@location", data.location);
            cm.Parameters.AddWithValue("@dateformat",data.dateformat);
            cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
            cm.Parameters.AddWithValue("@pageBreakForInjection", data.pageBreakForInjection);
            cm.Parameters.AddWithValue("@injectionAsSeparateBlock", data.injectionAsSeparateBlock);
            Execute(cm);

        }
        public void Update(tbl_settings data)
        {
            MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_settings SET
		page_size=@page_size,
        dateformat=@dateformat,
		location=@location,
        pageBreakForInjection=@pageBreakForInjection,
        injectionAsSeparateBlock=@injectionAsSeparateBlock
      
			where cmp_id=@Id", conn);
            cm.Parameters.AddWithValue("@Id", data.cmp_id);
            cm.Parameters.AddWithValue("@page_size", data.page_size);
            cm.Parameters.AddWithValue("@location", data.location);
            cm.Parameters.AddWithValue("@dateformat", data.dateformat);
            cm.Parameters.AddWithValue("@pageBreakForInjection", data.pageBreakForInjection);
            cm.Parameters.AddWithValue("@injectionAsSeparateBlock", data.injectionAsSeparateBlock);

            Execute(cm);
        }
        public void Delete(tbl_settings data)
        {
            MySqlCommand cm = new MySqlCommand(@"DELETE FROM tbl_settings
		where Id=@Id", conn);
            cm.Parameters.AddWithValue("@Id", data.id);
            Execute(cm);
        }
    }
}
