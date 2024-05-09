using MySql.Data.MySqlClient;
using System.Linq;
using System.Data;
using PainTrax.Services;
using PainTrax.Web.Models;
using PainTrax.Web.Helper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PainTrax.Web.Services
{
    public class UserService : ParentService
    {


        public List<vm_cm_user> GetAll(string cnd = "")
        {
            // List<vm_cm_user> dataList = null;


            string query = "select * from vm_cm_user where 1=1 ";

            if (!string.IsNullOrEmpty(query))
                query = query + cnd;

            List<vm_cm_user> dataList = ConvertDataTable<vm_cm_user>(GetData(query));

            return dataList;
        }

        public tbl_users? GetOne(tbl_users data)
        {
            DataTable dt = new DataTable();
            MySqlCommand cm = new MySqlCommand("select * from tbl_users where Id=@id ", conn);
            cm.Parameters.AddWithValue("@Id", data.Id);
            var datalist = ConvertDataTable<tbl_users>(GetData(cm)).FirstOrDefault();
            return datalist;
        }

        public void Insert(tbl_users data)
        {
            string fullName = data.fname + " " + data.lname;
            data.fullname = fullName;
            //data.password = EncryptionHelper.Encrypt(data.password);
            MySqlCommand cm = new MySqlCommand(@"INSERT INTO tbl_users
		(fname,lname,emailid,address,fullname,uname,password,groupid,desigid,cmp_id,createdby,createddate,phoneno,signature)Values
				(@fname,@lname,@emailid,@address,@fullname,@uname,@password,@groupid,@desigid,@cmpid,@createdby,@createddate,@phoneno,@signature)", conn);
            cm.Parameters.AddWithValue("@fname", data.fname);
            cm.Parameters.AddWithValue("@lname", data.lname);
            cm.Parameters.AddWithValue("@emailid", data.emailid);
            cm.Parameters.AddWithValue("@address", data.address);
            cm.Parameters.AddWithValue("@fullname", fullName);
            cm.Parameters.AddWithValue("@uname", data.uname);
            cm.Parameters.AddWithValue("@password", data.password);
            cm.Parameters.AddWithValue("@groupid", data.groupid);
            cm.Parameters.AddWithValue("@desigid", data.desigid);
            cm.Parameters.AddWithValue("@cmpid", data.cmp_id);
            cm.Parameters.AddWithValue("@createdby", data.createdby);
            cm.Parameters.AddWithValue("@createddate", data.createddate);
            cm.Parameters.AddWithValue("@phoneno", data.phoneno);
            cm.Parameters.AddWithValue("@signature", data.signature);

            Execute(cm);
        }
        public void Update(tbl_users data)
        {
            MySqlCommand cm = new MySqlCommand(@"UPDATE tbl_users SET
		fname=@fname,
		lname=@lname,
		emailid=@emailid,
		address=@address,
		fullname=@fullname,
		uname=@uname,
		password=@password,
		groupid=@groupid,
		desigid=@desigid,
		phoneno=@phoneno,
	    updateddate=@updateddate,
		updatedby=@updatedby,
        signature=@signature
        where Id=@Id", conn);
            cm.Parameters.AddWithValue("@Id", data.Id);
            cm.Parameters.AddWithValue("@fname", data.fname);
            cm.Parameters.AddWithValue("@lname", data.lname);
            cm.Parameters.AddWithValue("@emailid", data.emailid);
            cm.Parameters.AddWithValue("@address", data.address);
            cm.Parameters.AddWithValue("@fullname", data.fullname);
            cm.Parameters.AddWithValue("@uname", data.uname);
            cm.Parameters.AddWithValue("@password", data.password);
            cm.Parameters.AddWithValue("@groupid", data.groupid);
            cm.Parameters.AddWithValue("@desigid", data.desigid);
            cm.Parameters.AddWithValue("@updateddate", data.updateddate);
            cm.Parameters.AddWithValue("@updatedby", data.updatedby);
            cm.Parameters.AddWithValue("@phoneno", data.phoneno);
            cm.Parameters.AddWithValue("@signature", data.signature);
            Execute(cm);
        }
        public void Delete(tbl_users data)
        {
            MySqlCommand cm = new MySqlCommand(@"DELETE FROM tbl_users
		where Id=@Id", conn);
            cm.Parameters.AddWithValue("@Id", data.Id);
            Execute(cm);
        }
        
        public List<SelectListItem> GetProviders(int cmpid)
        {
            string query = "SELECT fullname FROM vm_cm_user WHERE desig_name = 'provider' and cmp_id=" + cmpid;
            DataTable dataTable = GetData(query);

            List<SelectListItem> providers = new List<SelectListItem>();

            // Add default option for dropdown
            providers.Add(new SelectListItem { Text = "--Select Provider--", Value = "" });

            foreach (DataRow row in dataTable.Rows)
            {
                providers.Add(new SelectListItem
                {
                    Text = row["fullname"].ToString(),
                    Value = row["fullname"].ToString()  // Assuming fullname is unique
                });
            }

            return providers;
        }



    }
}
