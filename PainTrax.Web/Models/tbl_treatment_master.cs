using System.ComponentModel.DataAnnotations;

namespace MS.Models;

public class tbl_treatment_master
{
    [Required(ErrorMessage = "Please Enter Tratment Details")]
    public string? treatment_details {get;set;}
    public bool? pre_select { get; set; }
    public int? id { get; set; }
    public string? note1 { get; set; }
    public string? note2 { get; set; }
    public string? note3 { get; set; }
    public int? cmp_id {get;set;}
	
}	