using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Data.DataSeed;
using System;
using System.Collections.Generic;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class NewBookingRequestValidatorTests
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private NewBookingRequestValidator _newBookingRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _newBookingRequestValidator = new NewBookingRequestValidator(
                _context
            );
        }

        private void SetupMockDefaults()
        {

        }

        [Test]
        public void ValidateRequest_AllCheckPass_ReturnsPassedValidationResult()
        {
            // arrange
            var request = GetValidRequest();

            // act
            var result = _newBookingRequestValidator.ValidateRequest(request);

            // assert
            result.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequest_StartTimeIsInPast_ReturnsFailedValidationResult()
        {
            // arrange
            var request = GetValidRequest();
            request.StartTime = DateTime.UtcNow.AddHours(-1);

            // act
            var result = _newBookingRequestValidator.ValidateRequest(request);

            // assert
            result.PassedValidation.Should().BeFalse();
            result.Errors.Should().Contain("StartTime cannot be in the past");
        }

        [Test]
        public void ValidateRequest_EndTimeIsInPast_ReturnsFailedValidationResult()
        {
            // arrange
            var request = GetValidRequest();
            request.EndTime = DateTime.UtcNow.AddHours(-1);

            // act
            var result = _newBookingRequestValidator.ValidateRequest(request);

            // assert
            result.PassedValidation.Should().BeFalse();
            result.Errors.Should().Contain("EndTime cannot be in the past");
        }

        [Test]
        public void ValidateRequest_EndTimeIsBeforeStartTime_ReturnsFailedValidationResult()
        {
            // arrange
            var request = GetValidRequest();
            request.StartTime = DateTime.UtcNow.AddHours(2);
            request.EndTime = DateTime.UtcNow.AddHours(1);

            // act
            var result = _newBookingRequestValidator.ValidateRequest(request);

            // assert
            result.PassedValidation.Should().BeFalse();
            result.Errors.Should().Contain("EndTime cannot be before StartTime");
        }

        [TestCase("2100-1-11 12:15:00","2100-11-1 12:30:00")] // full time slot taken
        [TestCase("2100-1-11 12:16:00","2100-11-1 12:29:00")] // inner time slot
        [TestCase("2100-1-11 12:10:00","2100-11-1 12:29:00")] // overlap with start
        [TestCase("2100-1-11 12:20:00","2100-11-1 12:35:00")] // overlap with end
        public void ValidateRequest_DoctorIsAlreadyBooked_ReturnsFailedValidationResult(DateTime startTime, DateTime endTime)
        {
            // arrange
            var request = GetValidRequest();
            request.DoctorId = 1;

            // note that the start times are way into the future so we don't fall foul of the
            // validation that checks the start/end times of the booking against DateTime.Now.
            // This should really be improved.
            request.StartTime = startTime;
            request.EndTime = endTime;

            // act
            var result = _newBookingRequestValidator.ValidateRequest(request);

            // assert
            result.PassedValidation.Should().BeFalse();
            result.Errors.Should().Contain("The Doctor is already booked in this time slot");

        }

        private NewBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<NewBookingRequest>()
                .With(x => x.StartTime, DateTime.UtcNow.AddHours(1))
                .With(x => x.EndTime, DateTime.UtcNow.AddHours(2))
                .Create();

            var orders = new List<Order>
            {
                new Order
                {
                    DoctorId = 1,
                    Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                    StartTime = new DateTime(2100, 1, 11, 12, 15, 00),
                    EndTime = new DateTime(2100, 1, 11, 12, 30, 00)
                },

            };

            _context.Order.AddRange(orders);
            _context.SaveChanges();
            return request;
        }
    }
}
