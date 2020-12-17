using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly PatientBookingContext _context;
        private readonly IBookingService _bookingService;
        private readonly INewBookingRequestValidator _newBookingRequestValidator;

        public BookingController(PatientBookingContext context, INewBookingRequestValidator newBookingRequestValidator, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
            _newBookingRequestValidator = newBookingRequestValidator;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointnemtn(long identificationNumber)
        {

            bool result = _bookingService.GetPatientNextAppointment(identificationNumber, out NextPatientBookingResponse bookingResponse);
            if(true == result)
            {
                return Ok(new
                {
                    bookingResponse.Id,
                    bookingResponse.DoctorId,
                    bookingResponse.StartTime,
                    bookingResponse.EndTime
                });
            } 

            return StatusCode(502);
        }

        [HttpDelete("patient/{identificationNumber}/{bookingId}")]
        public IActionResult CancelBooking(long identificationNumber, string bookingId)
        {
            _bookingService.CancelBooking(identificationNumber, bookingId);
            throw new NotImplementedException();
        }

        [HttpPost()]
        public IActionResult AddBooking(NewBookingRequest newBooking)
        {
            bool result = _bookingService.AddBooking(newBooking);

            if(true == result)
            {
                return StatusCode(200);
            }

            return BadRequest();
        }

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            MyOrderResult latestBooking;
            latestBooking = new MyOrderResult();
            latestBooking.Id = bookings2[i].Id;
            latestBooking.DoctorId = bookings2[i].DoctorId;
            latestBooking.StartTime = bookings2[i].StartTime;
            latestBooking.EndTime = bookings2[i].EndTime;
            latestBooking.PatientId = bookings2[i].PatientId;
            latestBooking.SurgeryType = (int)bookings2[i].GetSurgeryType();

            return latestBooking;
        }

    }
}