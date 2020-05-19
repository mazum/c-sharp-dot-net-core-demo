using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var cars = ProcessCars("fuel.csv");
            var manufacturers = ProcessManufacturers("manufacturers.csv");*/

            /*var query = cars.Where(c => c.Manufacture.Equals("BMW") && c.Year == 2016)
                .OrderByDescending(c => c.Combined)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    c.Manufacture,
                    c.Name,
                    c.Combined
                });

            foreach (var car in query.Take(10))
            {
                Console.WriteLine($"{car.Name} : {car.Combined}");
            }*/

            /*var top = cars
                .OrderByDescending(c => c.Combined)
                .ThenBy(c => c.Name)
                .First(c => c.Manufacture.Equals("BMW") && c.Year == 2016);

            Console.WriteLine(top.Name);*/

            /*var query = cars.Join(manufacturers,
                    c => c.Manufacture,
                    m => m.Name,
                    (c, m) => new
                    {
                        m.Headquarters,
                        c.Name,
                        c.Combined
                    })
                .OrderByDescending(c => c.Combined)
                .ThenBy(c => c.Name);

            foreach (var car in query.Take(10))
            {
                Console.WriteLine($"{car.Headquarters} : {car.Name} : {car.Combined}");
            }*/

            /*var query =
                cars.GroupBy(c => c.Manufacture.ToUpper())
                    .OrderBy(g => g.Key);
            foreach (var group in query)
            {
                Console.WriteLine(group.Key);
                foreach (var car in group.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }*/

            /*var query =
                manufacturers.GroupJoin(cars,
                    m => m.Name,
                    c => c.Manufacture,
                    (m, g) => new
                    {
                        Manufacturer = m,
                        Cars = g
                    })
                    .OrderBy(m => m.Manufacturer.Name);

            foreach (var group in query)
            {
                Console.WriteLine($"{group.Manufacturer.Name} : {group.Manufacturer.Headquarters}");
                foreach (var car in group.Cars.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }*/

            /*var query =
                manufacturers.GroupJoin(cars,
                        m => m.Name,
                        g => g.Manufacture,
                        (m, g) => new
                        {
                            Manufacturer = m,
                            Cars = g
                        })
                    .GroupBy(m => m.Manufacturer.Headquarters)
                    .OrderBy(m => m.Key);

            foreach (var group in query)
            {
                Console.WriteLine($"{group.Key}");
                foreach (var car in group.SelectMany(g => g.Cars)
                                         .OrderByDescending(c => c.Combined)
                                         .Take(3))
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }*/

            /*var query =
                cars.GroupBy(c => c.Manufacture)
                    .Select(c => new
                    {
                        Name = c.Key,
                        Max = c.Max(c => c.Combined),
                        Min = c.Min(c => c.Combined),
                        Average = c.Average(c => c.Combined)
                    })
                    .OrderByDescending(g => g.Max);

            foreach (var carGroup in query)
            {
                Console.WriteLine(carGroup.Name);
                Console.WriteLine($"\tMax: {carGroup.Max}");
                Console.WriteLine($"\tMin: {carGroup.Min}");
                Console.WriteLine($"\tAverage: {carGroup.Average}");
            }*/

            /*var query =
                cars.GroupBy(c => c.Manufacture)
                    .Select(g =>
                    {
                        var result = g.Aggregate(new CarStatistics(),
                            (acc, c) => acc.Accumulate(c),
                            acc => acc.Compute());
                        return new
                        {
                            Name = g.Key,
                            Max = result.Max,
                            Min = result.Min,
                            Average = result.Average
                        };

                    })
                    .OrderByDescending(g => g.Max);

            foreach (var carGroup in query)
            {
                Console.WriteLine(carGroup.Name);
                Console.WriteLine($"\tMax: {carGroup.Max}");
                Console.WriteLine($"\tMin: {carGroup.Min}");
                Console.WriteLine($"\tAverage: {carGroup.Average}");
            }*/

            /*CreateXml();
            QueryXml();*/

            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CarDb>());
            InsertData();
            QueryData();
        }

        private static void QueryData()
        {
            var db = new CarDb();
            db.Database.Log = Console.WriteLine;

            var query = 
                db.Cars.Where(c => c.Manufacturer == "BMW")
                    .OrderByDescending(c => c.Combined)
                    .ThenBy(c => c.Name)
                    .Take(10);

            foreach (var car in query)
            {
                Console.WriteLine($"{car.Name}: {car.Combined}");
            }
        }

        private static void InsertData()
        {
            var cars = ProcessCars("fuel.csv");
            var db = new CarDb();

            if (!db.Cars.Any())
            {
                foreach (var car in cars)
                {
                    db.Cars.Add(car);
                }
                db.SaveChanges();
            }
        }

        private static void QueryXml()
        {
            var ns = (XNamespace)"http://pluralsight.com/cars/2016";
            var ex = (XNamespace)"http://pluralsight.com/cars/2016/ex";
            var document = XDocument.Load("fuel.xml");
            var query = document.Element(ns + "Cars").Elements(ex + "Car")
                .Where(e => e.Attribute("Manufacturer").Value == "BMW")
                .Select(e => e.Attribute("Name").Value);

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }
        }

        private static void CreateXml()
        {
            var records = ProcessCars("fuel.csv");

            var ns = (XNamespace) "http://pluralsight.com/cars/2016";
            var ex = (XNamespace)"http://pluralsight.com/cars/2016/ex";

            var document = new XDocument();
            var cars = new XElement(ns + "Cars", records.Select(c => new XElement(ex + "Car",
                new XAttribute("Name", c.Name),
                new XAttribute("Combined", c.Combined),
                new XAttribute("Manufacturer", c.Manufacture)
            )));

            cars.Add(new XAttribute(XNamespace.Xmlns + "ex", ex));
            document.Add(cars);
            document.Save("fuel.xml");
        }

        private static List<Manufacturer> ProcessManufacturers(string path)
        {
            return
                File.ReadAllLines(path)
                    .Where(line => line.Length > 1)
                    .ToManufacturers()
                    .ToList();
        }

        private static List<Car> ProcessCars(string path)
        {
            return
                File.ReadAllLines(path)
                    .Skip(1)
                    .Where(line => line.Length > 1)
                    .ToCars()
                    .ToList();
        }
    }

    public class CarStatistics
    {
        public CarStatistics()
        {
            Max = Int32.MinValue;
            Min = Int32.MaxValue;
        }

        public CarStatistics Accumulate(Car car)
        {
            Count += 1;
            Total += car.Combined;
            Max = Math.Max(Max, car.Combined);
            Min = Math.Min(Max, car.Combined);
            return this;
        }

        public CarStatistics Compute()
        {
            Average = Total / Count;
            return this;
        }

        public int Max { get; set; }
        public int Min { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public double Average { get; set; }
    }

    public static class CarExtensions
    {
        public static IEnumerable<Car> ToCars(this IEnumerable<string> source)
        {
            foreach (var line in source)
            {
                var columns = line.Split(',');

                yield return new Car
                {
                    Year = int.Parse(columns[0]),
                    Manufacture = columns[1],
                    Name = columns[2],
                    Displacement = double.Parse(columns[3]),
                    Cylinders = int.Parse(columns[4]),
                    City = int.Parse(columns[5]),
                    Highway = int.Parse(columns[6]),
                    Combined = int.Parse(columns[7])
                };
            }
        }
    }

    public static class ManufacturerExtension
    {
        public static IEnumerable<Manufacturer> ToManufacturers(this IEnumerable<string> source)
        {
            foreach (var line in source)
            {
                var columns = line.Split(',');

                yield return new Manufacturer
                {
                    Name = columns[0],
                    Headquarters = columns[1],
                    Year = int.Parse(columns[2])
                };
            }
        }
    }
}
