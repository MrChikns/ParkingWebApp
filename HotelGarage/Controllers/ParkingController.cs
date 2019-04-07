﻿using HotelGarage.Core;
using HotelGarage.Core.Models;
using HotelGarage.Core.ViewModels;
using System;
using System.Web.Mvc;

namespace HotelGarage.Controllers
{

    public class ParkingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ParkingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public ActionResult Parking()
        {
            return View(new ParkingViewModel(_unitOfWork));
        }

        public ActionResult CheckIn(int pPlaceId, int reservationId)
        {
            var reservation = _unitOfWork.Reservations.GetReservationCar(reservationId)
                ?? throw new ArgumentOutOfRangeException("Wrong reservation ID passed");

            var parkingPlace = _unitOfWork.ParkingPlaces.GetParkingPlace(pPlaceId)
                ?? throw new ArgumentOutOfRangeException("Parking place with such an id does not exist");

            if (parkingPlace.StateOfPlaceId != StateOfPlace.Reserved)
                throw new ArgumentException("Parking place has a wrong state to check in");

            if (reservation.Arrival.Date != DateTime.Today.Date && reservation.StateOfReservationId != StateOfReservation.TemporaryLeave)
                throw new ArgumentException("Reservation not arriving today and is no Temporary Leave");

            reservation.CheckIn();

            parkingPlace.Occupy(_unitOfWork.StatesOfPlaces.GetOccupiedStateOfPlace(), reservation);

            _unitOfWork.Complete();
            
            return RedirectToAction("Parking");
        }

        public ActionResult CheckOut(int pPlaceId)
        {
            var pPlace = _unitOfWork.ParkingPlaces.GetParkingPlaceReservationCar(pPlaceId)
                ?? throw new ArgumentOutOfRangeException("Wrong parking place ID passed"); 

            pPlace.Reservation.CheckOut();
            pPlace.Release(_unitOfWork.StatesOfPlaces.GetFreeStateOfPlace());

            _unitOfWork.Complete();

            return RedirectToAction("Parking");
        }

        public ActionResult TemporaryLeave(int pPlaceId)
        {
            var pPlace = _unitOfWork.ParkingPlaces.GetParkingPlaceReservationCar(pPlaceId)
                ?? throw new ArgumentOutOfRangeException("Wrong parking place ID passed");

            pPlace.Reservation.TemporaryLeave();
            pPlace.Reserve(_unitOfWork.StatesOfPlaces.GetReservedStateOfPlace(), pPlace.Reservation);

            _unitOfWork.Complete();

            return RedirectToAction("Parking");
        }

        [HttpPost]
        public ActionResult Reserve(string ParkingPlaceName, int ReservationId)
        {
            var reservation = _unitOfWork.Reservations.GetReservation(ReservationId)
                ?? throw new ArgumentOutOfRangeException("Wrong reservation ID passed");

            ReleasePreviouslyReservedPlace(reservation);
            MoveOrDirectlyReserveParkingPlace(reservation, ParkingPlaceName);

            _unitOfWork.Complete();

            return RedirectToAction("Parking");
        }

        public void ReleasePreviouslyReservedPlace(Reservation reservation) {
            if (reservation.ParkingPlaceId != 0)
            {
                _unitOfWork.ParkingPlaces.GetParkingPlace(reservation)
                    .Release(_unitOfWork.StatesOfPlaces.GetFreeStateOfPlace());
            }
        }

        public void MoveOrDirectlyReserveParkingPlace(Reservation reservation, string ParkingPlaceName) {
            if (reservation.StateOfReservationId == StateOfReservation.Inhouse)
            {
                _unitOfWork.ParkingPlaces.GetParkingPlace(ParkingPlaceName)
                .MoveInhouseReservation(_unitOfWork.StatesOfPlaces.GetOccupiedStateOfPlace(), reservation);
            }
            else
            {
                _unitOfWork.ParkingPlaces.GetParkingPlace(ParkingPlaceName)
                    .Reserve(_unitOfWork.StatesOfPlaces.GetReservedStateOfPlace(), reservation);
            }
        }
    }
}