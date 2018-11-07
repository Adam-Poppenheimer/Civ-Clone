using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapManagement {

    public class CapitalCityComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory>                     MockCivFactory;
        private Mock<ICapitalCityCanon>                        MockCapitalCityCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<IHexGrid>                                 MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCivFactory        = new Mock<ICivilizationFactory>();
            MockCapitalCityCanon  = new Mock<ICapitalCityCanon>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockGrid              = new Mock<IHexGrid>();

            Container.Bind<ICivilizationFactory>                    ().FromInstance(MockCivFactory       .Object);
            Container.Bind<ICapitalCityCanon>                       ().FromInstance(MockCapitalCityCanon .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IHexGrid>                                ().FromInstance(MockGrid             .Object);

            Container.Bind<CapitalCityComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ComposeCapitalCities_AssignsCapitalCoordinatesToCorrespondingCivData() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() { Name = "Civ One"   },
                    new SerializableCivilizationData() { Name = "Civ Two"   },
                    new SerializableCivilizationData() { Name = "Civ Three" }
                }
            };

            var capitalOne   = BuildCity(BuildCell(new HexCoordinates(1, 1)));
            var capitalTwo   = BuildCity(BuildCell(new HexCoordinates(2, 2)));
            var capitalThree = BuildCity(BuildCell(new HexCoordinates(3, 3)));

            var civOne   = BuildCiv("Civ One",   capitalOne);
            var civTwo   = BuildCiv("Civ Two",   capitalTwo);
            var civThree = BuildCiv("Civ Three", capitalThree);

            MockCivFactory.Setup(factory => factory.AllCivilizations)
                .Returns(new List<ICivilization>(){ civOne, civTwo, civThree }.AsReadOnly());

            var composer = Container.Resolve<CapitalCityComposer>();

            composer.ComposeCapitalCities(mapData);

            Assert.AreEqual(new HexCoordinates(1, 1), mapData.Civilizations[0].CapitalLocation, "Incorrect capital location for Civ One");
            Assert.AreEqual(new HexCoordinates(2, 2), mapData.Civilizations[1].CapitalLocation, "Incorrect capital location for Civ Two");
            Assert.AreEqual(new HexCoordinates(3, 3), mapData.Civilizations[2].CapitalLocation, "Incorrect capital location for Civ Three");
        }

        [Test]
        public void DecomposeCapitalCities_AssignsCapitalCityFromCorrespondingCoordinatesInCivData() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() { Name = "Civ One",   CapitalLocation = new HexCoordinates(1, 1) },
                    new SerializableCivilizationData() { Name = "Civ Two",   CapitalLocation = new HexCoordinates(2, 2) },
                    new SerializableCivilizationData() { Name = "Civ Three", CapitalLocation = new HexCoordinates(3, 3) }
                }
            };

            var capitalOne   = BuildCity(BuildCell(new HexCoordinates(1, 1)));
            var capitalTwo   = BuildCity(BuildCell(new HexCoordinates(2, 2)));
            var capitalThree = BuildCity(BuildCell(new HexCoordinates(3, 3)));

            var civOne   = BuildCiv("Civ One",   capitalOne);
            var civTwo   = BuildCiv("Civ Two",   capitalTwo);
            var civThree = BuildCiv("Civ Three", capitalThree);

            MockCivFactory.Setup(factory => factory.AllCivilizations)
                .Returns(new List<ICivilization>(){ civOne, civTwo, civThree }.AsReadOnly());

            var composer = Container.Resolve<CapitalCityComposer>();

            composer.DecomposeCapitalCities(mapData);

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civOne,   capitalOne),   Times.Once, "Capital One one not assigned to Civ One");
            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civTwo,   capitalTwo),   Times.Once, "Capital Two one not assigned to Civ Two");
            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civThree, capitalThree), Times.Once, "Capital Three one not assigned to Civ Three");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCoordinates coords) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coords);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coords)).Returns(newCell);

            return newCell;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity))
                                 .Returns(location);

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<ICity>() { newCity });

            return newCity;
        }

        private ICivilization BuildCiv(string name, ICity capital) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Name).Returns(name);

            var newCiv = mockCiv.Object;

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
