using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        bool GetPatientNextAppointment(long identificationNumber, out NextPatientBookingResponse nextPatientBookingResponse);
        bool AddBooking(NewBookingRequest newBooking);
        bool CancelBooking(long patientIdentificationNumber, string bookingId);
    }
}