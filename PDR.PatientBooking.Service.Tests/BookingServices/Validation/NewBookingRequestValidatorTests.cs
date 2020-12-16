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


        private NewBookingRequest GetValidRequest()
        {
            var request = _fixture.Create<NewBookingRequest>();
            return request;
        }
    }
}
