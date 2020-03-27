﻿using HotelGarage.Core.Repositories;

namespace HotelGarage.Core
{
    public interface IUnitOfWork
    {
        ICarRepository Cars { get; }
        IParkingPlaceRepository ParkingPlaces { get; }
        IReservationRepository Reservations { get; }

        void Complete();
    }
}