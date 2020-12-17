using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using System;
using System.Collections.Generic;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointment(long identificationNumber)
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
            bool result = _bookingService.CancelBooking(identificationNumber, bookingId);
            if(false == result)
            {
                return StatusCode(502);
            }

            return Ok();
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