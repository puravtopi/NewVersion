namespace PainTrax.Web.ViewModel
{
    public class AIIntakeFormModel
    {
        public string Id { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public string DominantHand { get; set; }
        public string DOB { get; set; }
        public string DOA { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string IsWorking { get; set; }
        public string JobTitle { get; set; }
        public string StoppedAfterAccident { get; set; }
        public string NotWorkingReason { get; set; }
        public List<string> Complaints { get; set; }
        public string InjuryType { get; set; }
        public string Activity { get; set; }
        public string Incident { get; set; }
        public string IncidentType { get; set; }
        public string Mechanism { get; set; }
        public string SymptomOnset { get; set; }
        public string BodyPart { get; set; }
        public string SymptomPattern { get; set; }
        public string DailyActivities { get; set; }
    }
}
