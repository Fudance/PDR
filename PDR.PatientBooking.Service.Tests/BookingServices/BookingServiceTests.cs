using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Mock<INewBookingRequestValidator> _validator;

        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _validator = _mockRepository.Create<INewBookingRequestValidator>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _validator.Object
            );
        }

        [Test]
        public void CancelBooking_Success_ReturnsTrue()
        {
            // arrange
            SetupTestData();

            // act
            bool result = _bookingService.CancelBooking(100, "683074b8-44c9-468b-9288-dfafa1e533c9");

            bool cancellationStatus = _context.Order.Where(o => o.Id == Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"))
                                                    .Select(o => o.Cancelled)
                                                    .First();
            // assert
            result.Should().BeTrue();
            cancellationStatus.Should().BeTrue();
        }

        [Test]
        public void CancelBooking_BookingIdNotFound_ReturnsFalse()
        {
            // arrange
            SetupTestData();

            // act
            bool result = _bookingService.CancelBooking(100, "11111111-1111-1111-1111-111111111111");

            // assert
            result.Should().BeFalse();
        }

        [Test]
        public void CancelBooking_PatientIdDoesNotMatchBookingId_ReturnsFalse()
        {
            // arrange
            SetupTestData();

            // act
            bool result = _bookingService.CancelBooking(999, "683074b8-44c9-468b-9288-dfafa1e533c9");

            // assert
            result.Should().BeFalse();
        }

        [Test]
        public void GetPatientNextAppointment_NoCancellations_ReturnsFirstBooking()
        {
            // arrange
            SetupTestData();

            // act
            _bookingService.GetPatientNextAppointment(100, out NextPatientBookingResponse result);

            // assert
            result.Id.Should().Equals("683074b8-44c9-468b-9288-dfafa1e533c9");

        }

        [Test]
        public void GetPatientNextAppointment_SingleCancelledBooking_ReturnsNextBooking()
        {
            // arrange
            SetupTestData();

            // act
            _bookingService.CancelBooking(100, "683074b8-44c9-468b-9288-dfafa1e533c9");
            _bookingService.GetPatientNextAppointment(100, out NextPatientBookingResponse result);

            // assert
            result.Id.Should().Equals("57706153-7600-41fd-8a5e-dc60a584e21c");

        }

        private void SetupTestData()
        {

            var orders = new List<Order>
            {
                new Order
                {
                    Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                    StartTime = new DateTime(2020, 1, 11, 12, 15, 00),
                    EndTime = new DateTime(2020, 1, 11, 12, 30, 00)
                },
                new Order
                {
                    Id = Guid.Parse("57706153-7600-41fd-8a5e-dc60a584e21c"),
                    StartTime = new DateTime(2020, 1, 11, 12, 30, 00),
                    EndTime = new DateTime(2020, 1, 11, 12, 45, 00)
                },
                new Order
                {
                    Id = Guid.Parse("b6aad474-5b5d-42b7-a274-1a94f74ff69a"),
                    StartTime = new DateTime(2020, 1, 11, 14, 15, 00),
                    EndTime = new DateTime(2020, 1, 11, 14, 30, 00)
                }
            };

            var patients = new List<Patient>
            {
                new Patient
                {
                    Id = 100,
                    Gender = 1,
                    FirstName = "Bill",
                    LastName = "Bagly",
                    Email = "BToTheB@gmail.com",
                    DateOfBirth = new DateTime(1912, 1, 17),
                    Created = DateTime.UnixEpoch
                }
            };

            var doctors = new List<Doctor>
            {
                new Doctor()
                {
                    Id = 1,
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Email = "DrMg@docworld.com",
                    FirstName = "Mac",
                    LastName = "Guffin",
                    Gender = 1,
                    Created = DateTime.UtcNow
                }
            };

            foreach(var order in orders)
            {
                order.DoctorId = 1;
                order.PatientId = 100;
            }

            _context.Order.AddRange(orders);
            _context.Patient.AddRange(patients);
            _context.Doctor.AddRange(doctors);
            _context.SaveChanges();
        }

        private void SetupMockDefaults()
        {
            _validator.Setup(x => x.ValidateRequest(It.IsAny<NewBookingRequest>()))
                .Returns(new PdrValidationResult(true));
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
