using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.GameMap;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITilePossessionCanon> MockPossessionCanon;

        private Mock<IMapHexGrid> MockHexGrid;

        private Mock<ICityFactory> MockCityFactory;

        private Mock<ICityConfig> MockConfig;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockPossessionCanon = new Mock<ITilePossessionCanon>();
            MockHexGrid         = new Mock<IMapHexGrid>();
            MockCityFactory     = new Mock<ICityFactory>();
            MockConfig          = new Mock<ICityConfig>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<ITilePossessionCanon>()     .FromInstance(MockPossessionCanon.Object);
            Container.Bind<IMapHexGrid>()              .FromInstance(MockHexGrid.Object);
            Container.Bind<ICityFactory>().FromInstance(MockCityFactory.Object);
            Container.Bind<ICityConfig>()              .FromInstance(MockConfig.Object);

            Container.Bind<CityValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTileValidForCity should return false if the " +
            "argued tile already belongs to another city")]
        public void IsTileValidForCity_FalseIfTileBelongsToCity() {
            var tile = BuildTile(new Mock<ICity>().Object);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsTileValidForCity(tile),
                "IsTileValidForCity returned true on a tile that's already owned");
        }

        [Test(Description = "IsTileValidForCity should return false if the " +
            "distance between this tile and any city is less than Config.MinSeparation")]
        public void IsTileValidForCity_FalseIfTileTooCloseToCity() {
            var tile = BuildTile(null);

            var firstCity  = BuildCity(BuildTile(null));
            var secondCity = BuildCity(BuildTile(null));
            var thirdCity  = BuildCity(BuildTile(null));

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(tile, firstCity .Location)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(tile, secondCity.Location)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(tile, thirdCity .Location)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsTileValidForCity(tile),
                "IsTileValidForCity returns true for a tile too close to existing cities");
        }

        [Test(Description = "IsTileValidForCity should return true if the " +
            "tile is unowned and no city is less than Config.MinSeparation away from it")]
        public void IsTileValidForCity_TrueOtherwise() {
            var tile = BuildTile(null);

            var firstCity  = BuildCity(BuildTile(null));
            var secondCity = BuildCity(BuildTile(null));

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(tile, firstCity .Location)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(tile, secondCity.Location)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsTrue(validityLogic.IsTileValidForCity(tile),
                "IsTileValidForCity returns false for a tile that should be valid");
        }

        #endregion

        #region utilities

        private IMapTile BuildTile(ICity owner) {
            var mockTile = new Mock<IMapTile>();

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(mockTile.Object)).Returns(owner);

            return mockTile.Object;
        }

        private ICity BuildCity(IMapTile location) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Location).Returns(location);

            AllCities.Add(mockCity.Object);
            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
