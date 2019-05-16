using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.MapManagement;
using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapManagement {

    public class BarbarianComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IEncampmentFactory>       MockEncampmentFactory;
        private Mock<IEncampmentLocationCanon> MockEncampmentLocationCanon;
        private Mock<IHexGrid>                 MockGrid;

        private List<IEncampment> AllEncampments = new List<IEncampment>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllEncampments.Clear();

            MockEncampmentFactory       = new Mock<IEncampmentFactory>();
            MockEncampmentLocationCanon = new Mock<IEncampmentLocationCanon>();
            MockGrid                    = new Mock<IHexGrid>();

            MockEncampmentFactory.Setup(factory => factory.AllEncampments).Returns(AllEncampments.AsReadOnly());

            Container.Bind<IEncampmentFactory>      ().FromInstance(MockEncampmentFactory      .Object);
            Container.Bind<IEncampmentLocationCanon>().FromInstance(MockEncampmentLocationCanon.Object);
            Container.Bind<IHexGrid>                ().FromInstance(MockGrid                   .Object);

            Container.Bind<BarbarianComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_AllEncampmentsDestroyed() {
            var encampments = new List<IEncampment>() {
                BuildEncampment(BuildCell(new HexCoordinates(1, 1)), 0),
                BuildEncampment(BuildCell(new HexCoordinates(2, 2)), 0),
                BuildEncampment(BuildCell(new HexCoordinates(3, 3)), 0)
            };

            var composer = Container.Resolve<BarbarianComposer>();

            composer.ClearRuntime();

            foreach(var encampment in encampments) {
                MockEncampmentFactory.Verify(
                    factory => factory.DestroyEncampment(encampment), Times.Once,
                    "An encampment wasn't destroyed as expected"
                );
            }
        }

        [Test]
        public void ComposeBarbarians_AllLocationsStoredAsCoordinates() {
            BuildEncampment(BuildCell(new HexCoordinates(1, 1)), 0);
            BuildEncampment(BuildCell(new HexCoordinates(2, 2)), 0);
            BuildEncampment(BuildCell(new HexCoordinates(3, 3)), 0);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<BarbarianComposer>();

            composer.ComposeBarbarians(mapData);

            Assert.AreEqual(new HexCoordinates(1, 1), mapData.Encampments[0].Location, "Encampments[0] has an unexpected Location");
            Assert.AreEqual(new HexCoordinates(2, 2), mapData.Encampments[1].Location, "Encampments[1] has an unexpected Location");
            Assert.AreEqual(new HexCoordinates(3, 3), mapData.Encampments[2].Location, "Encampments[2] has an unexpected Location");
        }

        [Test]
        public void ComposeBarbarians_AllSpawnProgressStoredCorrectly() {
            BuildEncampment(BuildCell(new HexCoordinates(1, 1)), 3);
            BuildEncampment(BuildCell(new HexCoordinates(2, 2)), 5);
            BuildEncampment(BuildCell(new HexCoordinates(3, 3)), 11);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<BarbarianComposer>();

            composer.ComposeBarbarians(mapData);

            Assert.AreEqual(3,  mapData.Encampments[0].SpawnProgress, "Encampments[0] has an unexpected SpawnProgress");
            Assert.AreEqual(5,  mapData.Encampments[1].SpawnProgress, "Encampments[1] has an unexpected SpawnProgress");
            Assert.AreEqual(11, mapData.Encampments[2].SpawnProgress, "Encampments[2] has an unexpected SpawnProgress");
        }

        [Test]
        public void DecomposeBarbarians_EncampmentsCreatedFromCorrectLocations() {
            var cellOne   = BuildCell(new HexCoordinates(1, 1));
            var cellTwo   = BuildCell(new HexCoordinates(2, 2));
            var cellThree = BuildCell(new HexCoordinates(3, 3));

            var mapData = new SerializableMapData() {
                Encampments = new List<SerializableEncampmentData>() {
                    new SerializableEncampmentData() { Location = new HexCoordinates(1, 1), SpawnProgress = 3  },
                    new SerializableEncampmentData() { Location = new HexCoordinates(2, 2), SpawnProgress = 5  },
                    new SerializableEncampmentData() { Location = new HexCoordinates(3, 3), SpawnProgress = 11 },
                }
            };

            MockEncampmentFactory.Setup(factory => factory.CreateEncampment(It.IsAny<IHexCell>()))
                                 .Returns<IHexCell>(cell => BuildEncampment(cell, 0));

            var composer = Container.Resolve<BarbarianComposer>();

            composer.DecomposeBarbarians(mapData);

            MockEncampmentFactory.Verify(factory => factory.CreateEncampment(cellOne),   Times.Once, "Encampment not created at cellOne");
            MockEncampmentFactory.Verify(factory => factory.CreateEncampment(cellTwo),   Times.Once, "Encampment not created at cellTwo");
            MockEncampmentFactory.Verify(factory => factory.CreateEncampment(cellThree), Times.Once, "Encampment not created at cellThree");
        }

        [Test]
        public void DecomposeBarbarians_AllEncampmentsSetWithCorrectSpawnProgress() {
            BuildCell(new HexCoordinates(1, 1));
            BuildCell(new HexCoordinates(2, 2));
            BuildCell(new HexCoordinates(3, 3));

            var mapData = new SerializableMapData() {
                Encampments = new List<SerializableEncampmentData>() {
                    new SerializableEncampmentData() { Location = new HexCoordinates(1, 1), SpawnProgress = 3  },
                    new SerializableEncampmentData() { Location = new HexCoordinates(2, 2), SpawnProgress = 5  },
                    new SerializableEncampmentData() { Location = new HexCoordinates(3, 3), SpawnProgress = 11 },
                }
            };

            MockEncampmentFactory.Setup(factory => factory.CreateEncampment(It.IsAny<IHexCell>()))
                                 .Returns<IHexCell>(cell => BuildEncampment(cell, 0));

            var composer = Container.Resolve<BarbarianComposer>();

            composer.DecomposeBarbarians(mapData);

            Assert.AreEqual(3,  AllEncampments[0].SpawnProgress, "AllEncampments[0] has an unexpected SpawnProgress");
            Assert.AreEqual(5,  AllEncampments[1].SpawnProgress, "AllEncampments[1] has an unexpected SpawnProgress");
            Assert.AreEqual(11, AllEncampments[2].SpawnProgress, "AllEncampments[2] has an unexpected SpawnProgress");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return newCell;
        }

        private IEncampment BuildEncampment(IHexCell location, int spawnProgress) {
            var mockEncampment = new Mock<IEncampment>();

            mockEncampment.SetupAllProperties();

            var newEncampment = mockEncampment.Object;

            newEncampment.SpawnProgress = spawnProgress;

            MockEncampmentLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newEncampment)).Returns(location);

            AllEncampments.Add(newEncampment);

            return newEncampment;
        }

        #endregion

        #endregion

    }

}
