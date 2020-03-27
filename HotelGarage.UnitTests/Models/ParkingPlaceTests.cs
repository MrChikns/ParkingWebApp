﻿using HotelGarage.Core.Models;
using HotelGarage.Helpers;
using NUnit.Framework;
using System;

namespace HotelGarage.UnitTests.Models
{
    [TestFixture]
    public class ParkingPlaceTests
    {
        private ParkingPlace _parkingPlace;
        private StateOfPlaceEnum _stateOfPlace;
        private Reservation _reservation;

        [SetUp]
        public void SetUp()
        {
            _stateOfPlace = StateOfPlaceEnum.Occupied;
            _reservation = new Reservation() { Id = 1};
            _parkingPlace = new ParkingPlace() { Id = 1};
            _parkingPlace.AssignStateOfPlace(_stateOfPlace);
            _parkingPlace.AssignReservation(_reservation);
        }

        [Test]
        public void GetStateOfParkingPlace_NameIsObsazenoAndReservationUnregistered_Neregistrovan()
        {
            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.NotRegisteredStateOfPlaceLabel));
        }

        [Test]
        public void GetStateOfParkingPlaceName_NameIsObsazenoAndDepartureToday_Odjezd()
        {
            _reservation.IsRegistered = true;

            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.DepartureStateOfPlaceLabel));
        }

        [Test]
        public void GetStateOfParkingPlaceName_NameIsVolnoAndStaffOnly_VolnoStaff()
        {
            _parkingPlace.Id = Constants.NumberOfStandardParkingPlaces + 1;

            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.FreeStaffStateOfPlaceLabel));
        }

        [Test]
        public void GetStateOfParkingPlaceName_NameIsRezervovano_Rezervovano()
        {
            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.ReservedStateOfPlaceLabel));
        }

        [Test]
        public void Release_ParkingPlaceStateIsNotFreeAndReservationIsAssigned()
        {
            _reservation.State = StateOfReservationEnum.Inhouse;
            _parkingPlace.Release();

            Assert.That(_reservation.ParkingPlaceId, Is.EqualTo(0));
            Assert.That(_parkingPlace.Reservation, Is.EqualTo(null));
            Assert.That(_parkingPlace.State, Is.EqualTo(StateOfPlaceEnum.Free));
            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.FreeStateOfPlaceLabel));
        }

        [Test]
        public void Reserve_ParkingPlaceFreeAndReservationNotAssigned()
        {
            _stateOfPlace = StateOfPlaceEnum.Free;
            _parkingPlace.AssignStateOfPlace(StateOfPlaceEnum.Free);

            var reservation = new Reservation() { Id = 5};
            reservation.SetParkingPlaceId(0);
            _parkingPlace.Reserve(reservation);

            Assert.That(reservation.ParkingPlaceId, Is.EqualTo(1));
            Assert.That(_parkingPlace.State, Is.EqualTo(StateOfPlaceEnum.Reserved));
            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.ReservedStateOfPlaceLabel));
            Assert.That(_parkingPlace.Reservation, Is.EqualTo(reservation));
        }

        [Test]
        public void Occupy_ParkingPlaceReserved_OccupiedByReservation()
        {
            _stateOfPlace = StateOfPlaceEnum.Reserved;
            _parkingPlace.Reservation.SetParkingPlaceId(0);
            _parkingPlace.AssignStateOfPlace(StateOfPlaceEnum.Reserved);
            _parkingPlace.Id = 5;

            var reservation = new Reservation() { Id = 5 };
            reservation.SetParkingPlaceId(5);
            _parkingPlace.Occupy(reservation);

            Assert.That(reservation.ParkingPlaceId, Is.EqualTo(5));
            Assert.That(_parkingPlace.State, Is.EqualTo(StateOfPlace.Occupied));
            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.OccupiedStateOfPlaceLabel));
            Assert.That(_parkingPlace.Reservation, Is.EqualTo(reservation));
        }

        [Test]
        public void Occupy_WrongParametersPassed_ThrowsException()
        {
            var reservation = new Reservation() { Id = 5 };

            Assert.That(() => _parkingPlace.Occupy(reservation), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Release_ParkingPlaceNotFree_ReservationNullAndStateOfPlaceFree()
        {
            var reservation = _reservation;
            _parkingPlace.AssignStateOfPlace(StateOfPlaceEnum.Reserved);
            _stateOfPlace = StateOfPlaceEnum.Reserved;
            _parkingPlace.AssingnFreeParkingPlace(reservation);

            Assert.That(reservation.ParkingPlaceId, Is.EqualTo(0));
            Assert.That(_parkingPlace.Reservation, Is.EqualTo(null));
            Assert.That(_parkingPlace.State, Is.EqualTo(StateOfPlaceEnum.Free));
            Assert.That(_parkingPlace.Label, Is.EqualTo(Constants.FreeStateOfPlaceLabel));
        }

        [Test]
        public void MoveInhouseReservation_ReservationInHouse_SetValuesAfterMove()
        {
            _reservation.State = StateOfReservationEnum.Inhouse;
            _parkingPlace.MoveInhouseReservation(_reservation);

            Assert.That(_reservation.ParkingPlaceId, Is.EqualTo(_reservation.Id));
            Assert.That(_parkingPlace.State, Is.EqualTo(StateOfPlaceEnum.Occupied));
            Assert.That(_parkingPlace.Reservation, Is.EqualTo(_reservation));
        }

        [Test]
        public void MoveInhouseReservation_ReservationNotInHouse_ThrowsException()
        {
            _reservation.State = StateOfReservationEnum.Departed;

            Assert.That(() => _parkingPlace.MoveInhouseReservation(_reservation), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
