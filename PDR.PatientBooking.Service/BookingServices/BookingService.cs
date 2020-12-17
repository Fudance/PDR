using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
//using PDR.PatientBooking.Service.BookingServices.Responses;
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
            var bockings = _context.Order.OrderBy(x => x.StartTime).ToList();

            if (bockings.Where(x => x.Patient.Id == identificationNumber).Count() == 0)
            {
                return false;
            }
            else
            {
                var bookings2 = bockings.Where(x => x.PatientId == identificationNumber);
                if (bookings2.Where(x => x.StartTime > DateTime.Now).Count() == 0)
                {
                    return false;
                }
                else
                {
                    var bookings3 = bookings2.Where(x => x.StartTime > DateTime.Now);

                    nextPatientBookingResponse.Id = bookings3.First().Id;
                    nextPatientBookingResponse.DoctorId = bookings3.First().DoctorId;
                    nextPatientBookingResponse.StartTime = bookings3.First().StartTime;
                    nextPatientBookingResponse.EndTime = bookings3.First().EndTime; 

                    return true;
                }
            }
        }

        public void CancelBooking(long patientIdentificationNumber, string bookingId)
        {
            throw new NotImplementedException();
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
