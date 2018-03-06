using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitPositionCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public struct UnitTestData {

            public string Name;
            public UnitType Type;
            public bool BelongsToThisCivilization;

        }

        public struct CellTestData {

            public bool IsUnderwater;

            public CellTestData(bool isUnderwater) {
                IsUnderwater = isUnderwater;
            }

        }

        public struct CanChangeOwnerTestData {

            public UnitTestData UnitToTest;

            public CellTestData CellToTest;

            public List<UnitTestData> UnitsOnCell;

            public bool HasCityOnCell;

            public bool CityBelongsToThisCivilization;

        }

        public struct CanPlaceUnitOfTypeTestData {

            public UnitType Type;

            public CellTestData CellToTest;

            public bool IgnoreOccupancy;

            public List<UnitTestData> UnitsOnCell;

            public bool HasCityOnCell;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable CanChangeOwnerOfPossessionTestCases {
            get {
                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on dry, unoccupied grassland").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on dry, unoccupied grassland").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("WaterMilitary on dry, unoccupied grassland").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("WaterCivilian on dry, unoccupied grassland").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on submerged cell").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on submerged cell").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("WaterMilitary on submerged cell").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("WaterCivilian on submerged cell").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterMilitary on dry cell with friendly city").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterCivilian on dry cell with friendly city").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = false
                }).SetName("WaterMilitary on dry cell with foreign city").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = false
                }).SetName("WaterCivilian on dry cell with foreign city").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on cell with friendly LandMilitary").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on cell with friendly LandCivilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("LandMilitary on cell with friendly WaterMilitary").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("LandMilitary on cell with friendly WaterCivilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on cell with friendly LandMilitary").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on cell with friendly LandCivilian").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("LandCivilian on cell with friendly WaterMilitary").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("LandCivilian on cell with friendly WaterCivilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterMilitary on cell with friendly LandMilitary").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterMilitary on cell with friendly LandCivilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterMilitary on cell with friendly WaterMilitary").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterMilitary on cell with friendly WaterCivilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterCivilian on cell with friendly LandMilitary").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterCivilian on cell with friendly LandCivilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterCivilian on cell with friendly WaterMilitary").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("WaterCivilian on cell with friendly WaterCivilian").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian, BelongsToThisCivilization = false }
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on cell with foreign anything").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.LandCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandMilitary, BelongsToThisCivilization = false }
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on cell with foreign anything").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterMilitary,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian, BelongsToThisCivilization = false }
                    },
                    HasCityOnCell = false
                }).SetName("WaterMilitary on cell with foreign anything").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.WaterCivilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary, BelongsToThisCivilization = false }
                    },
                    HasCityOnCell = false
                }).SetName("WaterCivilian on cell with foreign anything").Returns(false);

            }
        }

        public static IEnumerable CanPlaceUnitOfTypeAtLocationTestCases {
            get {
                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on dry cell").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandMilitary,
                    CellToTest = new CellTestData(true),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on submerged cell").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary }
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on cell with LandMilitary").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian }
                    },
                    HasCityOnCell = false
                }).SetName("LandMilitary on cell with LandCivilian").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary }
                    },
                    HasCityOnCell = true
                }).SetName("LandMilitary on cell with WaterMilitary").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian }
                    },
                    HasCityOnCell = true
                }).SetName("LandMilitary on cell with WaterCivilian").Returns(true);




                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on dry cell").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandCivilian,
                    CellToTest = new CellTestData(true),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on submerged cell").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary }
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on cell with LandMilitary").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian }
                    },
                    HasCityOnCell = false
                }).SetName("LandCivilian on cell with LandCivilian").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary }
                    },
                    HasCityOnCell = true
                }).SetName("LandCivilian on cell with WaterMilitary").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.LandCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian }
                    },
                    HasCityOnCell = true
                }).SetName("LandCivilian on cell with WaterCivilian").Returns(true);





                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("WaterMilitary on dry cell").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(true),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("WaterMilitary on submerged cell").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = true
                }).SetName("WaterMilitary on dry cell with a city").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary }
                    },
                    HasCityOnCell = true
                }).SetName("WaterMilitary on a cell with LandMilitary").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian }
                    },
                    HasCityOnCell = true
                }).SetName("WaterMilitary on a cell with LandCivilian").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary }
                    },
                    HasCityOnCell = true
                }).SetName("WaterMilitary on a cell with WaterMilitary").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterMilitary,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian }
                    },
                    HasCityOnCell = true
                }).SetName("WaterMilitary on a cell with WaterCivilian").Returns(true);



                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("WaterCivilian on dry cell").Returns(false);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(true),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = false
                }).SetName("WaterCivilian on submerged cell").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {

                    },
                    HasCityOnCell = true
                }).SetName("WaterCivilian on dry cell with a city").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.LandMilitary }
                    },
                    HasCityOnCell = true
                }).SetName("WaterCivilian on a cell with LandMilitary").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.LandCivilian }
                    },
                    HasCityOnCell = true
                }).SetName("WaterCivilian on a cell with LandCivilian").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterMilitary", Type = UnitType.WaterMilitary }
                    },
                    HasCityOnCell = true
                }).SetName("WaterCivilian on a cell with WaterMilitary").Returns(true);

                yield return new TestCaseData(new CanPlaceUnitOfTypeTestData() {
                    Type = UnitType.WaterCivilian,
                    CellToTest = new CellTestData(false),
                    IgnoreOccupancy = false,
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "WaterCivilian", Type = UnitType.WaterCivilian }
                    },
                    HasCityOnCell = true
                }).SetName("WaterCivilian on a cell with WaterCivilian").Returns(false);

            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityFactory> MockCityFactory;

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private Mock<IHexGrid> MockGrid;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory         = new Mock<ICityFactory>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockGrid                = new Mock<IHexGrid>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(() => AllCities.AsReadOnly());

            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);

            Container.Bind<UnitPositionCanon>().AsSingle();

            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanChangeOwnerOfPossession should consider whether the argued cell is " + 
            "underwater and the UnitType and ownership of other units on the cell when determining " + 
            "whether a particular unit can be moved to a particular cell. It should also consider " + 
            "the placement and ownership of cities.")]
        [TestCaseSource("CanChangeOwnerOfPossessionTestCases")]
        public bool CanChangeOwnerOfPossessionTests(CanChangeOwnerTestData data) {
            var thisCiv = BuildCivilization("This Civilization");
            var otherCiv = BuildCivilization("Other Civilization");

            var unitToTest = BuildUnit(data.UnitToTest, null, thisCiv, otherCiv);

            var cellToTest = BuildCell(data.CellToTest);

            if(data.HasCityOnCell) {
                var cityAtLocation = BuildCity(cellToTest);

                MockCityPossessionCanon
                    .Setup(canon => canon.GetOwnerOfPossession(cityAtLocation))
                    .Returns(data.CityBelongsToThisCivilization ? thisCiv : otherCiv);
            }

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            foreach(var unitData in data.UnitsOnCell) {
                var unitAtLocation = BuildUnit(unitData, cellToTest, thisCiv, otherCiv);
                positionCanon.ChangeOwnerOfPossession(unitAtLocation, cellToTest);
            }

            return positionCanon.CanChangeOwnerOfPossession(unitToTest, cellToTest);
        }

        [Test(Description = "")]
        [TestCaseSource("CanPlaceUnitOfTypeAtLocationTestCases")]
        public bool CanPlaceUnitOfTypeAtLocationTests(CanPlaceUnitOfTypeTestData data) {
            var cellToTest = BuildCell(data.CellToTest);

            if(data.HasCityOnCell) {
                BuildCity(cellToTest);
            }

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            foreach(var unitData in data.UnitsOnCell) {
                var unitAtLocation = BuildUnit(unitData, cellToTest, null, null);
                positionCanon.ChangeOwnerOfPossession(unitAtLocation, cellToTest);
            }

            return positionCanon.CanPlaceUnitOfTypeAtLocation(data.Type, cellToTest, data.IgnoreOccupancy);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.Name).Returns(name);

            return mockCiv.Object;
        }

        private IUnit BuildUnit(
            UnitTestData data,  IHexCell location,
            ICivilization thisCivilization, ICivilization otherCivilization
        ){
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Name).Returns(data.Name);
            mockUnit.Setup(unit => unit.Type).Returns(data.Type);
            mockUnit.Setup(unit => unit.gameObject).Returns(new GameObject(data.Name));

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon
                .Setup(canon => canon.GetOwnerOfPossession(newUnit))
                .Returns(data.BelongsToThisCivilization ? thisCivilization : otherCivilization);            

            return newUnit;
        }

        private IHexCell BuildCell(CellTestData data) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.IsUnderwater).Returns(data.IsUnderwater);
            mockCell.Setup(cell => cell.transform   ).Returns(new GameObject().transform);

            return mockCell.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Location).Returns(location);

            AllCities.Add(mockCity.Object);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
