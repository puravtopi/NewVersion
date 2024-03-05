namespace MS.Models;

public class tbl_patient_fu{

	public int? id {get;set;}
	public int? patient_id {get;set;}
	
	public int? patientIE_ID {get;set;}
	public DateTime? doe {get;set;}

	public DateTime? created_date {get;set;}
	public int? created_by {get;set;}
	public DateTime? updated_date {get;set;}
	public int? updated_by {get;set;}
	public bool? is_active {get;set;}
	public int? cmp_id {get;set;}
	public string? extra_comments {get;set;}
	public string? type { get;set;}
	

}