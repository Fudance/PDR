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
            throw new NotImplementedException();
        }

    }
}
