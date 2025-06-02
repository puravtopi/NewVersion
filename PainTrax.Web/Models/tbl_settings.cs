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
        public string font_family { get; set; }
        public string font_size { get; set; }        
        public string gait_default { get; set; }        
        public string fu_default { get; set; }        
        public bool show_preop { get; set; }        
        public bool show_postop { get; set; }
        public string? sign_content { get; set; }
    }
}
