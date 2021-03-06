﻿using HotelGarage.Core.Model;
using HotelGarage.Core.Repository;
using System.Linq;

namespace HotelGarage.Persistence.Repository
{
    public class CarRepository : ICarRepository
    {
        private IApplicationDbContext _context;

        public CarRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public Car GetCar(Reservation reservation)
        {
            return _context.Cars.FirstOrDefault(c => c.LicensePlate == reservation.Car.LicensePlate);
        }

        public void Add(Car car)
        {
            _context.Cars.Add(car);
        }
    }
}