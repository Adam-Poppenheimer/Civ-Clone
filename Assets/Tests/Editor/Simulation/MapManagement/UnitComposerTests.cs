using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.MapManagement;

namespace Assets.Tests.Simulation.MapManagement {

    public class UnitComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitFactory>                                  MockUnitFactory;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IHexGrid>                                      MockGrid;
        private Mock<ICivilizationFactory>                          MockCivilizationFactory;

        private List<IUnitTemplate> AvailableUnitTemplates = new List<IUnitTemplate>();
        private List<IUnit>         AllUnits               = new List<IUnit>();
        private List<ICivilization> AllCivilizations       = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableUnitTemplates.Clear();
            AllUnits              .Clear();
            AllCivilizations      .Clear();

            MockUnitFactory         = new Mock<IUnitFactory>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockGrid                = new Mock<IHexGrid>();
            MockCivilizationFactory = new Mock<ICivilizationFactory>();

            MockUnitFactory        .Setup(factory => factory.AllUnits)        .Returns(AllUnits);
            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivilizations.AsReadOnly());

            MockUnitFactory.Setup(
                factory => factory.BuildUnit(It.IsAny<IHexCell>(), It.IsAny<IUnitTemplate>(), It.IsAny<ICivilization>())
            ).Returns<IHexCell, IUnitTemplate, ICivilization>(
                (location, template, owner) => BuildUnit(location, owner, template)
            );

            Container.Bind<IUnitFactory>                                 ().FromInstance(MockUnitFactory        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);
            Container.Bind<ICivilizationFactory>                         ().FromInstance(MockCivilizationFactory.Object);

            Container.Bind<IEnumerable<IUnitTemplate>>()
                     .WithId("Available Unit Templates")
                     .FromInstance(AvailableUnitTemplates);

            Container.Bind<UnitComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_AllUnitsRemovedFromPositions() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 0));
            var cellTwo   = BuildHexCell(new HexCoordinates(0, 0));
            var cellThree = BuildHexCell(new HexCoordinates(0, 0));

            var unitOne   = BuildUnit(cellOne,   BuildCivilization(), BuildUnitTemplate());
            var unitTwo   = BuildUnit(cellTwo,   BuildCivilization(), BuildUnitTemplate());
            var unitThree = BuildUnit(cellThree, BuildCivilization(), BuildUnitTemplate());

            var composer = Container.Resolve<UnitComposer>();

            composer.ClearRuntime();

            MockUnitPositionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(unitOne, null),
                Times.Once, "Did not remove UnitOne from position as expected"
            );

            MockUnitPositionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(unitTwo, null),
                Times.Once, "Did not remove UnitTwo from position as expected"
            );

            MockUnitPositionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(unitThree, null),
                Times.Once, "Did not remove UnitThree from position as expected"
            );
        }

        [Test]
        public void ClearRuntime_AllUnitsDestroyed() {
            Mock<IUnit> mockUnitOne, mockUnitTwo, mockUnitThree;

            BuildUnit(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), BuildUnitTemplate(), out mockUnitOne);
            BuildUnit(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), BuildUnitTemplate(), out mockUnitTwo);
            BuildUnit(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), BuildUnitTemplate(), out mockUnitThree);

            var composer = Container.Resolve<UnitComposer>();

            composer.ClearRuntime();

            mockUnitOne.Verify(
                unit => unit.Destroy(), Times.Once,
                "UnitOne.Destroy() was not called as expected"
            );

            mockUnitTwo.Verify(
                unit => unit.Destroy(), Times.Once,
                "UnitTwo.Destroy() was not called as expected"
            );

            mockUnitThree.Verify(
                unit => unit.Destroy(), Times.Once,
                "UnitThree.Destroy() was not called as expected"
            );
        }

        [Test]
        public void ComposeUnits_IgnoresUnitsOfTypeCity() {
            Mock<IUnit> mockUnitOne, mockUnitTwo, mockUnitThree;

            BuildUnit(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), BuildUnitTemplate(), out mockUnitOne);
            BuildUnit(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), BuildUnitTemplate(), out mockUnitTwo);
            BuildUnit(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), BuildUnitTemplate(), out mockUnitThree);
            
            mockUnitOne  .Setup(unit => unit.Type).Returns(UnitType.City);
            mockUnitTwo  .Setup(unit => unit.Type).Returns(UnitType.City);
            mockUnitThree.Setup(unit => unit.Type).Returns(UnitType.Melee);

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            Assert.AreEqual(1, mapData.Units.Count, "Unexpected number of elements in MapData.Units");
        }

        [Test]
        public void ComposeUnits_StoresLocationsByCoordinates() {
            BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate());
            BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate());
            BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate());

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            var dataLikeUnitOne   =  mapData.Units.Where(data => data.Location.Equals(new HexCoordinates(0, 1)));
            var dataLikeUnitTwo   =  mapData.Units.Where(data => data.Location.Equals(new HexCoordinates(2, 3)));
            var dataLikeUnitThree =  mapData.Units.Where(data => data.Location.Equals(new HexCoordinates(4, 5)));

            Assert.AreEqual(3, mapData.Units.Count, "Unexpected number of elements in MapData.Units");

            Assert.AreEqual(
                1, dataLikeUnitOne.Count(),
                "Unexpected number of elements in MapData.Units with coordinates of UnitOne"
            );

            Assert.AreEqual(
                1, dataLikeUnitTwo.Count(),
                "Unexpected number of elements in MapData.Units with coordinates of UnitTwo"
            );

            Assert.AreEqual(
                1, dataLikeUnitThree.Count(),
                "Unexpected number of elements in MapData.Units with coordinates of UnitThree"
            );
        }

        [Test]
        public void ComposeUnits_StoresTemplatesByName() {
            BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate("Template One"));
            BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate("Template Two"));
            BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate("Template Three"));

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            var dataLikeUnitOne = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(0, 1))
                     && data.Template.Equals("Template One")
            );

            var dataLikeUnitTwo = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(2, 3))
                     && data.Template.Equals("Template Two")
            );

            var dataLikeUnitThree = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(4, 5))
                     && data.Template.Equals("Template Three")
            );

            Assert.AreEqual(3, mapData.Units.Count, "Unexpected number of elements in MapData.Units");

            Assert.AreEqual(
                1, dataLikeUnitOne.Count(),
                "Unexpected number of elements in MapData.Units representing UnitOne"
            );

            Assert.AreEqual(
                1, dataLikeUnitTwo.Count(),
                "Unexpected number of elements in MapData.Units representing UnitTwo"
            );

            Assert.AreEqual(
                1, dataLikeUnitThree.Count(),
                "Unexpected number of elements in MapData.Units representing UnitThree"
            );
        }

        [Test]
        public void ComposeUnits_StoresOwnersByName() {
            BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization("Civ One"),   BuildUnitTemplate());
            BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization("Civ Two"),   BuildUnitTemplate());
            BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization("Civ Three"), BuildUnitTemplate());

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            var dataLikeUnitOne = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(0, 1))
                     && data.Owner.Equals("Civ One")
            );

            var dataLikeUnitTwo = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(2, 3))
                     && data.Owner.Equals("Civ Two")
            );

            var dataLikeUnitThree = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(4, 5))
                     && data.Owner.Equals("Civ Three")
            );

            Assert.AreEqual(3, mapData.Units.Count, "Unexpected number of elements in MapData.Units");

            Assert.AreEqual(
                1, dataLikeUnitOne.Count(),
                "Unexpected number of elements in MapData.Units representing UnitOne"
            );

            Assert.AreEqual(
                1, dataLikeUnitTwo.Count(),
                "Unexpected number of elements in MapData.Units representing UnitTwo"
            );

            Assert.AreEqual(
                1, dataLikeUnitThree.Count(),
                "Unexpected number of elements in MapData.Units representing UnitThree"
            );
        }

        [Test]
        public void ComposeUnits_StoresCurrentMovement() {
            var unitOne =   BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate());
            var unitTwo =   BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate());
            var unitThree = BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate());

            unitOne  .CurrentMovement = 2;
            unitTwo  .CurrentMovement = 0;
            unitThree.CurrentMovement = -10;

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            var dataLikeUnitOne = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(0, 1))
                     && data.CurrentMovement == 2
            );

            var dataLikeUnitTwo = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(2, 3))
                     && data.CurrentMovement == 0
            );

            var dataLikeUnitThree = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(4, 5))
                     && data.CurrentMovement == -10
            );

            Assert.AreEqual(3, mapData.Units.Count, "Unexpected number of elements in MapData.Units");

            Assert.AreEqual(
                1, dataLikeUnitOne.Count(),
                "Unexpected number of elements in MapData.Units representing UnitOne"
            );

            Assert.AreEqual(
                1, dataLikeUnitTwo.Count(),
                "Unexpected number of elements in MapData.Units representing UnitTwo"
            );

            Assert.AreEqual(
                1, dataLikeUnitThree.Count(),
                "Unexpected number of elements in MapData.Units representing UnitThree"
            );
        }

        [Test]
        public void ComposeUnits_StoresHitpoints() {
            var unitOne =   BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate());
            var unitTwo =   BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate());
            var unitThree = BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate());

            unitOne  .Hitpoints = 2;
            unitTwo  .Hitpoints = 0;
            unitThree.Hitpoints = -10;

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            var dataLikeUnitOne = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(0, 1))
                     && data.Hitpoints == 2
            );

            var dataLikeUnitTwo = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(2, 3))
                     && data.Hitpoints == 0
            );

            var dataLikeUnitThree = mapData.Units.Where(
                data => data.Location.Equals(new HexCoordinates(4, 5))
                     && data.Hitpoints == -10
            );

            Assert.AreEqual(3, mapData.Units.Count, "Unexpected number of elements in MapData.Units");

            Assert.AreEqual(
                1, dataLikeUnitOne.Count(),
                "Unexpected number of elements in MapData.Units representing UnitOne"
            );

            Assert.AreEqual(
                1, dataLikeUnitTwo.Count(),
                "Unexpected number of elements in MapData.Units representing UnitTwo"
            );

            Assert.AreEqual(
                1, dataLikeUnitThree.Count(),
                "Unexpected number of elements in MapData.Units representing UnitThree"
            );
        }

        [Test]
        public void ComposeUnits_StoresCurrentPathsAsCoordinates() {
            var pathOne = new List<IHexCell>() {
                BuildHexCell(new HexCoordinates(0, 0)), BuildHexCell(new HexCoordinates(1, 1)),
                BuildHexCell(new HexCoordinates(2, 2))
            };

            var pathTwo = new List<IHexCell>() {
                BuildHexCell(new HexCoordinates(3, 3)), BuildHexCell(new HexCoordinates(4, 4)),
                BuildHexCell(new HexCoordinates(5, 5))
            };

            var unitOne =   BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate());
            var unitTwo =   BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate());
            var unitThree = BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate());

            unitOne  .CurrentPath = pathOne;
            unitTwo  .CurrentPath = pathTwo;
            unitThree.CurrentPath = null;

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            CollectionAssert.AreEqual(
                pathOne.Select(hex => hex.Coordinates), mapData.Units[0].CurrentPath,
                "Unexpected path in data representing unitOne"
            );

            CollectionAssert.AreEqual(
                pathTwo.Select(hex => hex.Coordinates), mapData.Units[1].CurrentPath,
                "Unexpected path in data representing unitTwo"
            );

            CollectionAssert.AreEqual(
                null, mapData.Units[2].CurrentPath,
                "Unexpected path in data representing unitThree"
            );
        }

        [Test]
        public void ComposeUnits_StoresIsSetUpToBombard() {
            Mock<IUnit> mockUnitOne, mockUnitTwo, mockUnitThree;

            BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate(), out mockUnitOne);
            BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate(), out mockUnitTwo);
            BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate(), out mockUnitThree);
            
            mockUnitOne  .Setup(unit => unit.IsSetUpToBombard).Returns(true);
            mockUnitTwo  .Setup(unit => unit.IsSetUpToBombard).Returns(false);
            mockUnitThree.Setup(unit => unit.IsSetUpToBombard).Returns(true);

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            Assert.IsTrue (mapData.Units[0].IsSetUpToBombard, "Unexpected isSetUpToBombard value in data representing unitOne");
            Assert.IsFalse(mapData.Units[1].IsSetUpToBombard, "Unexpected isSetUpToBombard value in data representing unitTwo");
            Assert.IsTrue (mapData.Units[2].IsSetUpToBombard, "Unexpected isSetUpToBombard value in data representing unitThree");
        }

        [Test]
        public void ComposeUnits_StoresExperienceAndLevel() {
            var unitOne   = BuildUnit(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), BuildUnitTemplate());
            var unitTwo   = BuildUnit(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), BuildUnitTemplate());
            var unitThree = BuildUnit(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), BuildUnitTemplate());

            unitOne  .Experience = 10;
            unitTwo  .Experience = 20;
            unitThree.Experience = 30;

            unitOne  .Level = 1;
            unitTwo  .Level = 2;
            unitThree.Level = 3;

            var composer = Container.Resolve<UnitComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeUnits(mapData);

            Assert.AreEqual(10, mapData.Units[0].Experience, "Unexpected Experience value in data representing unitOne");
            Assert.AreEqual(20, mapData.Units[1].Experience, "Unexpected Experience value in data representing unitTwo");
            Assert.AreEqual(30, mapData.Units[2].Experience, "Unexpected Experience value in data representing unitThree");

            Assert.AreEqual(1, mapData.Units[0].Level, "Unexpected Level value in data representing unitOne");
            Assert.AreEqual(2, mapData.Units[1].Level, "Unexpected Level value in data representing unitTwo");
            Assert.AreEqual(3, mapData.Units[2].Level, "Unexpected Level value in data representing unitThree");
        }

        [Test]
        public void ComposeUnits_ComposesChosenPromotionsInPromotionTreeProperly() {
            throw new NotImplementedException();
        }




        [Test]
        public void DecomposeUnits_CallsIntoFactoryWithCorrectLocation_Template_AndOwner() {
            var cellOne = BuildHexCell(new HexCoordinates(1, 1));
            var cellTwo = BuildHexCell(new HexCoordinates(2, 2));

            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var templateOne = BuildUnitTemplate("Template One");
            var templateTwo = BuildUnitTemplate("Template Two");

            var mapData = new SerializableMapData() {
                Units = new List<SerializableUnitData>() {
                    new SerializableUnitData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Template = "Template One"
                    },
                    new SerializableUnitData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        Template = "Template Two"
                    }
                }
            };

            var composer = Container.Resolve<UnitComposer>();

            composer.DecomposeUnits(mapData);

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(cellOne, templateOne, civOne), Times.Once,
                "Factory.BuildUnit() was not invoked correctly on the data defining UnitOne"
            );

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(cellTwo, templateTwo, civTwo), Times.Once,
                "Factory.BuildUnit() was not invoked correctly on the data defining UnitTwo"
            );
        }

        [Test]
        public void DecomposeUnits_SetsCurrentMovementProperly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildUnitTemplate("Template One");
            BuildUnitTemplate("Template Two");

            var mapData = new SerializableMapData() {
                Units = new List<SerializableUnitData>() {
                    new SerializableUnitData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Template = "Template One", CurrentMovement = 5
                    },
                    new SerializableUnitData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        Template = "Template Two", CurrentMovement = -2
                    }
                }
            };

            var composer = Container.Resolve<UnitComposer>();

            composer.DecomposeUnits(mapData);

            Assert.AreEqual(5,  AllUnits[0].CurrentMovement, "The first instantiated unit has an unexpected CurrentMovement value");
            Assert.AreEqual(-2, AllUnits[1].CurrentMovement, "The second instantiated unit has an unexpected CurrentMovement value");
        }

        [Test]
        public void DecomposeUnits_SetsHitpointsProperly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildUnitTemplate("Template One");
            BuildUnitTemplate("Template Two");

            var mapData = new SerializableMapData() {
                Units = new List<SerializableUnitData>() {
                    new SerializableUnitData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Template = "Template One", Hitpoints = 5
                    },
                    new SerializableUnitData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        Template = "Template Two", Hitpoints = -2
                    }
                }
            };

            var composer = Container.Resolve<UnitComposer>();

            composer.DecomposeUnits(mapData);

            Assert.AreEqual(5,  AllUnits[0].Hitpoints, "The first instantiated unit has an unexpected Hitpoints value");
            Assert.AreEqual(-2, AllUnits[1].Hitpoints, "The second instantiated unit has an unexpected Hitpoints value");
        }

        [Test]
        public void DecomposeUnits_SetsCurrentPathProperly() {
            var cellOne   = BuildHexCell(new HexCoordinates(1, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 2));
            var cellThree = BuildHexCell(new HexCoordinates(3, 3));

            BuildCivilization("Civ One");

            BuildUnitTemplate("Template One");

            var mapData = new SerializableMapData() {
                Units = new List<SerializableUnitData>() {
                    new SerializableUnitData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Template = "Template One",
                        CurrentPath = new List<HexCoordinates>() {
                            cellThree.Coordinates, cellOne.Coordinates,
                            cellTwo.Coordinates,
                        }
                    }
                }
            };

            var composer = Container.Resolve<UnitComposer>();

            composer.DecomposeUnits(mapData);

            CollectionAssert.AreEqual(
                new List<IHexCell>() { cellThree, cellOne, cellTwo },
                AllUnits[0].CurrentPath,
                "First unit had an unexpected CurrentPath"
            );
        }

        [Test]
        public void DecomposeUnits_SetsUpToBombardCorrectly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildUnitTemplate("Template One");
            BuildUnitTemplate("Template Two");

            var mapData = new SerializableMapData() {
                Units = new List<SerializableUnitData>() {
                    new SerializableUnitData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Template = "Template One", IsSetUpToBombard = true
                    },
                    new SerializableUnitData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        Template = "Template Two", IsSetUpToBombard = false
                    }
                }
            };

            var createdUnitMocks = new List<Mock<IUnit>>();

            MockUnitFactory.Setup(
                factory => factory.BuildUnit(It.IsAny<IHexCell>(), It.IsAny<IUnitTemplate>(), It.IsAny<ICivilization>())
            ).Returns(delegate(IHexCell location, IUnitTemplate template, ICivilization owner) {
                Mock<IUnit> mock;
                var newUnit = BuildUnit(location, owner, template, out mock);

                createdUnitMocks.Add(mock);

                return newUnit;
            });

            var composer = Container.Resolve<UnitComposer>();

            composer.DecomposeUnits(mapData);

            createdUnitMocks[0].Verify(
                unit => unit.SetUpToBombard(), Times.Once,
                "First unit did not have its SetUpToBombard() method called as expected"
            );

            createdUnitMocks[1].Verify(
                unit => unit.SetUpToBombard(), Times.Never,
                "Second unit unexpectedly had its SetUpToBombard() method called"
            );
        }

        [Test]
        public void DecomposeUnits_ExperienceAndLevelSetCorrectly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildUnitTemplate("Template One");
            BuildUnitTemplate("Template Two");

            var mapData = new SerializableMapData() {
                Units = new List<SerializableUnitData>() {
                    new SerializableUnitData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Template = "Template One", Experience = 10, Level = 1
                    },
                    new SerializableUnitData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        Template = "Template Two", Experience = 20, Level = 2
                    }
                }
            };

            var composer = Container.Resolve<UnitComposer>();

            composer.DecomposeUnits(mapData);

            Assert.AreEqual(10, AllUnits[0].Experience, "The first instantiated unit has an unexpected Experience value");
            Assert.AreEqual(20, AllUnits[1].Experience, "The second instantiated unit has an unexpected Experience value");

            Assert.AreEqual(1, AllUnits[0].Level, "The first instantiated unit has an unexpected Level value");
            Assert.AreEqual(2, AllUnits[1].Level, "The second instantiated unit has an unexpected Level value");
        }

        [Test]
        public void DecomposeUnits_PromotionTreesDecomposedProperly() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return newCell;
        }

        private ICivilization BuildCivilization(string name = "") {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Name).Returns(name);

            var newCiv = mockCiv.Object;

            AllCivilizations.Add(newCiv);

            return newCiv;
        }

        private IUnitTemplate BuildUnitTemplate(string name = "") {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.name).Returns(name);

            var newTemplate = mockTemplate.Object;

            AvailableUnitTemplates.Add(newTemplate);

            return newTemplate;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner, IUnitTemplate template) {
            Mock<IUnit> mock;
            return BuildUnit(location, owner, template, out mock);
        }

        private IUnit BuildUnit(
            IHexCell location, ICivilization owner, IUnitTemplate template,
            out Mock<IUnit> mock
        ){
            mock = new Mock<IUnit>();

            mock.SetupAllProperties();
            mock.Setup(unit => unit.Template).Returns(template);

            var newUnit = mock.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            AllUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
