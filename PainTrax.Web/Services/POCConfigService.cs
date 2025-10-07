using MySql.Data.MySqlClient;
using System.Linq;
using System.Data;
using PainTrax.Services;
using PainTrax.Web.Models;

namespace PainTrax.Web.Services
{
    public class POCConfigService : ParentService
    {
        public List<tbl_pocconfig> GetAll(string cnd = "")
        {
            string query = "select * from tbl_pocconfig where 1=1 ";

            if (!string.IsNullOrEmpty(cnd))
                query = query + cnd;

            var datalist = ConvertDataTable<tbl_pocconfig>(GetData(query)).FirstOrDefault();

            List<tbl_pocconfig> configList = new List<tbl_pocconfig>();

            string[] entries = datalist.columns.Split(',');

            foreach (string entry in entries)
            {
                //  string[] parts = entry.Split(',');

                if (entry.Length >= 1)
                {
                    tbl_pocconfig config = new tbl_pocconfig
                    {
                        id = entry.TrimEnd(),
                        columns = entry.TrimEnd(),
                        //Value = parts[1]
                    };

                    configList.Add(config);
                }
            }


            return configList;
        }


        public List<tbl_pocconfig> GetAllone(string cnd = "")
        {
            string query = "select * from tbl_pocconfig where 1=1 ";

            if (!string.IsNullOrEmpty(cnd))
                query = query + cnd;

            var datalist = ConvertDataTable<tbl_pocconfig>(GetData(query)).FirstOrDefault();

            List<tbl_pocconfig> configList = new List<tbl_pocconfig>();

            string[] entries = datalist.columns.Split(',');

            foreach (string entry in entries)
            {
                //  string[] parts = entry.Split(',');

                if (entry.Length >= 1)
                {
                    tbl_pocconfig config = new tbl_pocconfig
                    {
                        id = entry.TrimEnd(),
                        columns = entry.TrimEnd(),
                        //Value = parts[1]
                    };

                    configList.Add(config);
                }
            }


            return configList;
        }

        public tbl_pocconfig? GetOne(tbl_pocconfig data)
        {
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select * from tbl_pocconfig ", conn);
            //cm.Parameters.AddWithValue("@id", data.id);
            var datalist = ConvertDataTable<tbl_pocconfig>(GetData(cm)).FirstOrDefault();
            return datalist;
        }

        public void Insert(tbl_pocconfig data)
        {
            MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_pocconfig
		(id,columns)Values
				(@columns);select @@identity;", conn);
            cm.Parameters.AddWithValue("@id", data.id);
            cm.Parameters.AddWithValue("@columns", data.columns);

            //cm.Parameters.AddWithValue("@cmp_id", data.cmp_id);
            //cm.Parameters.AddWithValue("@createddate", data.createddate);
            //cm.Parameters.AddWithValue("@createdby", data.createdby);
            //cm.Parameters.AddWithValue("@header_template", data.header_template);

            int id = ExecuteScalar(cm);

            //addlocationintogroup(data.cmp_id.Value, data.location, id);
        }
        public void Update(tbl_pocconfig data)
        {
            MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_pocconfig SET
            id = @id,		
            columns=@columns ", conn);
            cm.Parameters.AddWithValue("@id", data.id);
            cm.Parameters.AddWithValue("@columns", data.columns);
            Execute(cm);
        }
  

    }
}