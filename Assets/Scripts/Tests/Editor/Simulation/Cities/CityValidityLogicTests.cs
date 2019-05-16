using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IHexCell>> MockPossessionCanon;
        private Mock<IHexGrid>                                 MockHexGrid;
        private Mock<ICityFactory>                             MockCityFactory;
        private Mock<ICityConfig>                              MockConfig;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockPossessionCanon   = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockHexGrid           = new Mock<IHexGrid>();
            MockCityFactory       = new Mock<ICityFactory>();
            MockConfig            = new Mock<ICityConfig>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<IPossessionRelationship<ICity, IHexCell>>().FromInstance(MockPossessionCanon  .Object);
            Container.Bind<IHexGrid>                                ().FromInstance(MockHexGrid          .Object);
            Container.Bind<ICityFactory>                            ().FromInstance(MockCityFactory      .Object);
            Container.Bind<ICityConfig>                             ().FromInstance(MockConfig           .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);

            Container.Bind<CityValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsCellValidForCity should return false if the " +
            "argued cell already belongs to another city")]
        public void IsCellValidForCity_FalseIfCellBelongsToCity() {
            var cell = BuildCell(new Mock<ICity>().Object);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(cell),
                "IsCellValidForCity returned true on a cell that's already owned");
        }

        [Test(Description = "IsCellValidForCity should return false if the " +
            "distance between this cell and any city is less than Config.MinSeparation")]
        public void IsCellValidForCity_FalseIfCellTooCloseToCity() {
            var cell = BuildCell(null);

            var firstLocation  = BuildCell(null);
            var secondLocation = BuildCell(null);
            var thirdlLocation = BuildCell(null);

            BuildCity(firstLocation);
            BuildCity(secondLocation);
            BuildCity(thirdlLocation);

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(cell, firstLocation)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(cell, secondLocation)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(cell, thirdlLocation)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(cell),
                "IsCellValidForCity returns true for a cell too close to existing cities");
        }

        [Test(Description = "IsCellValidForCity should return true if the " +
            "cell is unowned and no city is less than Config.MinSeparation away from it")]
        public void IsCellValidForCity_TrueOtherwise() {
            var cell = BuildCell(null);

            var firstLocation  = BuildCell(null);
            var secondLocation = BuildCell(null);

            BuildCity(firstLocation);
            BuildCity(secondLocation);

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(cell, firstLocation)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(cell, secondLocation)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsTrue(validityLogic.IsCellValidForCity(cell),
                "IsCellValidForCity returns false for a cell that should be valid");
        }

        public void IsCellValidForCity_FalseIfCellIsWater() {
            var waterCell = BuildCell(null, true);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(waterCell));
        }


        [Test]
        public void IsCellValidForCity_FalseIfCellFeatureOasis() {
            var cell = BuildCell(null, feature: CellFeature.Oasis);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(cell));
        }

        [Test]
        public void IsCellValidForCity_FalseIfCellFeatureRuins() {
            var cell = BuildCell(null, feature: CellFeature.CityRuins);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(cell));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(
            ICity owner, bool isUnderwater = false, CellFeature feature = CellFeature.None
        ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(isUnderwater ? CellTerrain.FreshWater : CellTerrain.Grassland);
            mockCell.Setup(cell => cell.Feature).Returns(feature);

            MockPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(mockCell.Object)).Returns(owner);

            return mockCell.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            AllCities.Add(newCity);

            return newCity;
        }

        #endregion

        #endregion

    }

}
