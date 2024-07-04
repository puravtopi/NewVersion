namespace PainTrax.Web.Models

{
	public class tbl_users
	{

		public int? Id { get; set; }
		public string? fname { get; set; }
		public string? lname { get; set; }
		public string? emailid { get; set; }
		public string? address { get; set; }
		public string? fullname { get; set; }
		public string? phoneno { get; set; }
		public string? uname { get; set; }
		public string? password { get; set; }
		public int? groupid { get; set; }
		public int? desigid { get; set; }
		public int? cmp_id { get; set; }
		public int? createdby { get; set; }
		public DateTime? createddate { get; set; }
		public DateTime? updateddate { get; set; }
		public int? updatedby { get; set; }
        public string? signature { get; set; }
        public string? signature_hidden { get; set; }
		public string? providername { get; set; }
		public string? assistant_providername {  get; set; }

    }
}