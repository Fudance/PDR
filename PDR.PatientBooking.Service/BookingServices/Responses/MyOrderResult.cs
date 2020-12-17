using PDR.PatientBooking.Data.Models;
using System.Collections.Generic;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Responses
{
    public class MyOrderResult
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long PatientId { get; set; }
        public long DoctorId { get; set; }
        public int SurgeryType { get; set; }
    }
}
