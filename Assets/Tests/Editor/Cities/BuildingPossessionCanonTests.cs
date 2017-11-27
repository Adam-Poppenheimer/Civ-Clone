using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Cities {

    [TestFixture]
    public class BuildingPossessionCanonTests : ZenjectUnitTestFixture {

        [SetUp]
        public void CommonInstall() {
            Container.Bind<BuildingPossessionCanon>().AsSingle();
        }

        [Test(Description = "CanPlaceBuildingInCity should return true if that building " +
            "is not part of any city")]
        public void CanPlaceBuildingInCity_TrueIfNowhere() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            Assert.IsTrue(canon.CanPlaceBuildingInCity(building, city), 
                "CanPlaceBuildingInCity returns false on a building that is in no city");
        }

        [Test(Description = "CanPlaceBuildingInCity should return false if that building " +
            "is already in the city")]
        public void CanPlaceBuildingInCity_FalseIfAlreadyThere() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, city);

            Assert.IsFalse(canon.CanPlaceBuildingInCity(building, city), 
                "CanPlaceBuildingInCity returns true on a building that is already in the argued city");
        }

        [Test(Description = "CanPlaceBuildingInCity should return false if that building " +
            "is in some other city")]
        public void CanPlaceBuildingInCity_FalseIfSomewhereElse() {
            var building = new Mock<IBuilding>().Object;

            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, cityOne);

            Assert.IsFalse(canon.CanPlaceBuildingInCity(building, cityTwo), 
                "CanPlaceBuildingInCity returns true on a building that is already in a different city");
        }

        [Test(Description = "PlaceBuildingInCity should throw an InvalidOperationException when " +
            "CanPlaceBuildingInCity on its arguments would return false")]
        public void PlaceBuildingInCity_ThrowsIfNotPermitted() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, city);

            Assert.Throws<InvalidOperationException>(() => canon.PlaceBuildingInCity(building, city),
                "PlaceBuildingInCity failed to throw when placement was not permitted");
        }

        [Test(Description = "When PlaceBuildingInCity is called, GetBuildingsOfCity should reflect " +
            "the presence of the argued building in the argued city")]
        public void PlaceBuildingInCity_ReflectedInGetBuildingsOfCity() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, city);

            CollectionAssert.Contains(canon.GetBuildingsInCity(city), building, 
                "GetBuildingsInCity failed to reflect the placement of a building into a city");
        }

        [Test(Description = "When PlaceBuildingInCity is called, GetCityOfBuilding should reflect " +
            "the presence of the argued building in the argued city")]
        public void PlaceBuildingInCity_ReflectedInGetCityOfBuilding() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, city);

            Assert.AreEqual(city, canon.GetCityOfBuilding(building), 
                "GetCityOfBuilding failed to reflect the placement of a building into a city");
        }

        [Test(Description = "When RemoveBuildingFromCurrentCity is called, GetBuildingsOfCity " +
            "should reflect the removal of the argued building from the argued city")]
        public void RemoveBuildingFromCurrentCity_ReflectedInGetBuildingsOfCity() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, city);

            canon.RemoveBuildingFromCurrentCity(building);

            CollectionAssert.DoesNotContain(canon.GetBuildingsInCity(city), building,
                "GetBuildingsInCity failed to reflect the removal of a building from a city");
        }

        [Test(Description = "When RemoveBuildingFromCurrentCity is called, GetCityofBuilding " +
            "should reflect the removal of the argued building from the argued city")]
        public void RemoveBuildingFromCurrentCity_ReflectedInGetCityOfBuilding() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.PlaceBuildingInCity(building, city);

            canon.RemoveBuildingFromCurrentCity(building);

            Assert.IsNull(canon.GetCityOfBuilding(building), "GetCityOfBuilding failed to reflect "+
                "the removal of a building from a city");
        }

        [Test(Description = "GetBuildingsInCity should return an empty collection if the city has no buildings")]
        public void GetBuildingsInCity_ReturnsEmptyCollectionIfNone() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            var buildingsInCity = canon.GetBuildingsInCity(city);

            Assert.IsNotNull(buildingsInCity, "GetBuildingsInCity returned a null collection");
            CollectionAssert.IsEmpty(buildingsInCity, "The collection returned by GetBuildingsInCity is not empty");
        }

        [Test(Description = "GetCityOfBuilding should return null if the building is part of no city")]
        public void GetCityOfBuilding_ReturnsNullIfNone() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            Assert.IsNull(canon.GetCityOfBuilding(building), "GetCityOfBuilding did not return null");
        }

        [Test(Description = "All methods should throw a NullArgumentException when passed " +
            "any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var city = new Mock<ICity>().Object;
            var building = new Mock<IBuilding>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            Assert.Throws<ArgumentNullException>(() => canon.CanPlaceBuildingInCity(null, city),
                "CanPlaceBuildingInCity fails to throw when given a null building argument");

            Assert.Throws<ArgumentNullException>(() => canon.CanPlaceBuildingInCity(building, null),
                "CanPlaceBuildingInCity fails to throw when given a null city argument");

            Assert.Throws<ArgumentNullException>(() => canon.GetBuildingsInCity(null),
                "GetBuildingsInCity fails to throw when given a null city argument");

            Assert.Throws<ArgumentNullException>(() => canon.GetCityOfBuilding(null),
                "GetCityOfBuilding fails to throw when given a null building argument");

            Assert.Throws<ArgumentNullException>(() => canon.PlaceBuildingInCity(null, city),
                "PlaceBuildingInCity fails to throw when given a null building argument");

            Assert.Throws<ArgumentNullException>(() => canon.PlaceBuildingInCity(building, null),
                "PlaceBuildingInCity fails to throw when given a null city argument");

            Assert.Throws<ArgumentNullException>(() => canon.RemoveBuildingFromCurrentCity(null),
                "RemoveBuildingFromCurrentCity fails to throw when given a null building argument");
        }

    }

}
