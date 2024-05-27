using System.ComponentModel.DataAnnotations;

namespace PainTrax.Web.ViewModel
{
    public class ForgotPassword
    {
        [Required]
        public string email {  get; set; }  
    }
}
