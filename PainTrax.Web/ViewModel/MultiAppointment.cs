﻿namespace PainTrax.Web.ViewModel
{
    public class MultiAppointment
    {
			public int? appointment_id { get; set; }
			public int? patientIE_id { get; set; }
			public int? patient_id { get; set; }
			public int? location_id { get; set; }
			public string? appointmentDate { get; set; }
			public string? appointmentStart { get; set; }
			public string? appointmentEnd { get; set; }
			public string? appointmentNote { get; set; }
			public int? status_id { get; set; }
			public string? appointmentFromDate { get; set; }
			public string? appointmentToDate { get; set; }
		    public string? days { get; set; }
		    public string? providers { get; set; }
	}
}
