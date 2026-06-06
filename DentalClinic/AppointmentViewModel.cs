namespace DentalClinic
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string ServiceName { get; set; }
        public string AppointmentDate { get; set; }
    }
}