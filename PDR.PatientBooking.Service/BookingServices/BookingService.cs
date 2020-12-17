using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly INewBookingRequestValidator _newBookingRequestValidator;

        public BookingService(PatientBookingContext context, INewBookingRequestValidator newBookingRequestValidator)
        {
            _context = context;
            _newBookingRequestValidator = newBookingRequestValidator;
        }

        public bool GetPatientNextAppointment(long identificationNumber, out NextPatientBookingResponse nextPatientBookingResponse)
        {
            nextPatientBookingResponse = new NextPatientBookingResponse();
            var bookings = _context.Order.OrderBy(x => x.StartTime).ToList();

            if (bookings.Where(x => x.Patient.Id == identificationNumber).Count() == 0)
            {
                return false;
            }
            else
            {
                var bookings2 = bookings.Where(x => x.PatientId == identificationNumber);
                if (bookings2.Where(x => x.StartTime > DateTime.Now).Count() == 0)
                {
                    return false;
                }
                else
                {
                    var bookings3 = bookings2.Where(x => x.StartTime > DateTime.Now && x.Cancelled == false);

                    nextPatientBookingResponse.Id = bookings3.First().Id;
                    nextPatientBookingResponse.DoctorId = bookings3.First().DoctorId;
                    nextPatientBookingResponse.StartTime = bookings3.First().StartTime;
                    nextPatientBookingResponse.EndTime = bookings3.First().EndTime; 

                    return true;
                }
            }
        }

        public bool CancelBooking(long patientIdentificationNumber, string bookingId)
        {
            // is the booking valid and does it belong to the patient
            bool validId = Guid.TryParse(bookingId, out Guid bookingIdAsGuid);
            if(true == validId)
            {
                if(false == _context.Order.Any((o => o.Id == bookingIdAsGuid))) {
                    return false;
                }
                var booking = _context.Order.First(o => o.Id == bookingIdAsGuid);

                if(null == booking)
                {
                    return false;
                }
                
                // Ensure that the booking belongs to the patient that requested
                // the cancellation
                if(booking.PatientId == patientIdentificationNumber)
                {
                    var orderToCancel = _context.Order.Where(o => o.Id == bookingIdAsGuid)
                                                      .Where(o => o.PatientId == patientIdentificationNumber)
                                                      .First();
                    orderToCancel.Cancelled = true;

                    _context.SaveChanges();
                    return true;
                }

                return false;
            }

            return false;
        }

        public bool AddBooking(NewBookingRequest newBooking)
        {
            var bookingId = new Guid();
            var bookingStartTime = newBooking.StartTime;
            var bookingEndTime = newBooking.EndTime;
            var bookingPatientId = newBooking.PatientId;
            var bookingPatient = _context.Patient.FirstOrDefault(x => x.Id == newBooking.PatientId);
            var bookingDoctorId = newBooking.DoctorId;
            var bookingDoctor = _context.Doctor.FirstOrDefault(x => x.Id == newBooking.DoctorId);
            var bookingSurgeryType = _context.Patient.FirstOrDefault(x => x.Id == bookingPatientId).Clinic.SurgeryType;

            var validationResult = _newBookingRequestValidator.ValidateRequest(newBooking);

            if(validationResult.PassedValidation != true)
            {
                return false; 
            }

            var myBooking = new Order
            {
                Id = bookingId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime,
                PatientId = bookingPatientId,
                DoctorId = bookingDoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
                SurgeryType = (int)bookingSurgeryType
            };

            _context.Order.AddRange(new List<Order> { myBooking });
            _context.SaveChanges();

            return true;
        }
    }
}
