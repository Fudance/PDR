using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;

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
        }

        [Test]
        public void ValidateRequest_EndTimeIsBeforeStartTime_ReturnsFailedValidationResult()
        {
            // arrange
            var request = GetValidRequest();
            request.EndTime = DateTime.UtcNow;
            request.EndTime = DateTime.UtcNow.AddHours(1);

            // act
            var result = _newBookingRequestValidator.ValidateRequest(request);

            // assert
            result.PassedValidation.Should().BeFalse();
        }

        private NewBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<NewBookingRequest>()
                .With(x => x.StartTime, DateTime.UtcNow)
                .With(x => x.EndTime, DateTime.UtcNow.AddHours(1))
                .Create();
            return request;
        }
    }
}
