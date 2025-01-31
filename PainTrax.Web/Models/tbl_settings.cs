namespace PainTrax.Web.Models
{
    public class tbl_settings
    {
        public int id { get; set; }
        public int page_size { get; set; }
        public int location { get; set; }        
        public int cmp_id { get; set; }
        public string dateformat { get; set; }
        public bool pageBreakForInjection { get; set; }
        public bool injectionAsSeparateBlock { get; set; }
        public bool isdaignosisshow { get; set; }
        public string foundStatment { get; set; }
        public string notfoundStatment { get; set; }
        public string header_template { get; set; }
        public string header_template_hidden { get; set; }
    }
}
