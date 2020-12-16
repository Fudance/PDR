using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class NewBookingRequestValidator : INewBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public NewBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(NewBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (BookingTimeInvalid(request, ref result))
                return result;

            return result;
        }

        public bool BookingTimeInvalid(NewBookingRequest request, ref PdrValidationResult result)
        {

            var errors = new List<string>();

            if (request.StartTime < DateTime.Now)
                errors.Add("StartTime cannot be in the past");

            if(request.EndTime < DateTime.Now)
                errors.Add("EndTime cannot be in the past");

            if(request.EndTime < request.StartTime)
                errors.Add("EndTime cannot be before StartTime");

            if(errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

    }
}
