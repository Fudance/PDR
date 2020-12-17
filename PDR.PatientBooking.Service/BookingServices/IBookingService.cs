using PDR.PatientBooking.Service.BookingServices.Requests;
//using PDR.PatientBooking.Service.BookingServices.Responses;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        void CancelBooking(long patientIdentificationNumber, string bookingId);
    }
}