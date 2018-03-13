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

            public string   Name;
            public bool     IsAquatic;
            public UnitType Type;
            public bool     BelongsToThisCivilization;

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

        private static IEnumerable CanPlaceUnitAtLocationTestCases {
            get {
                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = false,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("Non-aquatic on dry, unoccupied grassland").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = true,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("Aquatic on dry, unoccupied grassland").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = false,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("Non-aquatic on submerged cell").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = true,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = false
                }).SetName("Aquatic on submerged cell").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = true,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("Aquatic on dry cell with friendly city").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = true,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = false
                }).SetName("Aquatic on dry cell with foreign city").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = false,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "LandMilitary", IsAquatic = false,
                            Type = UnitType.Melee, BelongsToThisCivilization = true
                        }
                    },
                    HasCityOnCell = false
                }).SetName("Land Military supertype on cell with friendly Land Military unit").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test", IsAquatic = false,
                        Type = UnitType.Melee,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "LandCivilian", Type = UnitType.Civilian,
                            BelongsToThisCivilization = true
                        }
                    },
                    HasCityOnCell = false
                }).SetName("Land Military supertype on cell with friendly civilian unit").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        IsAquatic = false,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "WaterMilitary", Type = UnitType.NavalMelee,
                            BelongsToThisCivilization = true
                        }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("Land Military supertype on cell with friendly Water Military unit").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.Civilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "LandMilitary",
                            Type = UnitType.Melee,
                            BelongsToThisCivilization = true
                        }
                    },
                    HasCityOnCell = false
                }).SetName("Civilian on cell with friendly Land Military unit").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.Civilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "Civilian", Type = UnitType.Civilian,
                            BelongsToThisCivilization = true
                        }
                    },
                    HasCityOnCell = false
                }).SetName("Civilian on cell with friendly civilian unit").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.Civilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "WaterMilitary", Type = UnitType.NavalMelee,
                            BelongsToThisCivilization = true
                        }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("Civilian on cell with friendly Water Military").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.NavalMelee,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandMilitary", Type = UnitType.Melee, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("Water Military supertype on cell with friendly Land Military").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.NavalMelee,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "Civilian", Type = UnitType.Civilian, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("Water Military supertype on cell with friendly Civilian").Returns(true);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.NavalMelee,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "Water Military", Type = UnitType.NavalMelee, BelongsToThisCivilization = true }
                    },
                    HasCityOnCell = true,
                    CityBelongsToThisCivilization = true
                }).SetName("Water Military supertype on cell with friendly WaterMilitary").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.Melee,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "LandCivilian", Type = UnitType.Civilian, BelongsToThisCivilization = false }
                    },
                    HasCityOnCell = false
                }).SetName("Land Military supertype on cell with foreign anything").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test",
                        Type = UnitType.Civilian,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(false),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() { Name = "Land Military", Type = UnitType.Melee, BelongsToThisCivilization = false }
                    },
                    HasCityOnCell = false
                }).SetName("Civilian on cell with foreign anything").Returns(false);

                yield return new TestCaseData(new CanChangeOwnerTestData() {
                    UnitToTest = new UnitTestData() {
                        Name = "Unit to test", IsAquatic = true,
                        Type = UnitType.NavalMelee,
                        BelongsToThisCivilization = true
                    },
                    CellToTest = new CellTestData(true),
                    UnitsOnCell = new List<UnitTestData>() {
                        new UnitTestData() {
                            Name = "Civilian", Type = UnitType.Civilian,
                            IsAquatic = true, BelongsToThisCivilization = false
                        }
                    },
                    HasCityOnCell = false
                }).SetName("WaterMilitary on cell with foreign anything").Returns(false);

            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IHexGrid>                                      MockGrid;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockGrid                = new Mock<IHexGrid>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);

            Container.Bind<UnitPositionCanon>().AsSingle();

            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanChangeOwnerOfPossession should consider whether the argued cell is " + 
            "underwater and the UnitType and ownership of other units on the cell when determining " + 
            "whether a particular unit can be moved to a particular cell. It should also consider " + 
            "the placement and ownership of cities.")]
        [TestCaseSource("CanPlaceUnitAtLocationTestCases")]
        public bool CanPlaceUnitAtLocationTests(CanChangeOwnerTestData data) {
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

            return positionCanon.CanPlaceUnitAtLocation(unitToTest, cellToTest, false);
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

            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.Type     ).Returns(data.Type);
            mockTemplate.Setup(template => template.IsAquatic).Returns(data.IsAquatic);

            mockUnit.Setup(unit => unit.Template).Returns(mockTemplate.Object);

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

            var newCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location)).Returns(new List<ICity>() { newCity });

            AllCities.Add(mockCity.Object);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
