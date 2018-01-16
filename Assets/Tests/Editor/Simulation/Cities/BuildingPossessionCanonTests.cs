using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingPossessionCanonTests : ZenjectUnitTestFixture {

        [SetUp]
        public void CommonInstall() {
            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<BuildingPossessionCanon>().AsSingle();
        }

        [Test(Description = "CanChangeOwnerOfPossession should return true if that building " +
            "is not part of any city")]
        public void CanChangeOwnerOfPossession_TrueIfNowhere() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            Assert.IsTrue(canon.CanChangeOwnerOfPossession(building, city), 
                "CanChangeOwnerOfPossession returns false on a building that is in no city");
        }

        [Test(Description = "CanChangeOwnerOfPossession should return false if that building " +
            "is already in the city")]
        public void CanChangeOwnerOfPossession_FalseIfAlreadyThere() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, city);

            Assert.IsFalse(canon.CanChangeOwnerOfPossession(building, city), 
                "CanChangeOwnerOfPossession returns true on a building that is already in the argued city");
        }

        [Test(Description = "CanChangeOwnerOfPossession should return false if that building " +
            "is in some other city")]
        public void CanChangeOwnerOfPossession_FalseIfSomewhereElse() {
            var building = new Mock<IBuilding>().Object;

            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, cityOne);

            Assert.IsFalse(canon.CanChangeOwnerOfPossession(building, cityTwo), 
                "CanChangeOwnerOfPossession returns true on a building that is already in a different city");
        }

        [Test(Description = "PlaceBuildingInCity should throw an InvalidOperationException when " +
            "CanChangeOwnerOfPossession on its arguments would return false")]
        public void PlaceBuildingInCity_ThrowsIfNotPermitted() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, city);

            Assert.Throws<InvalidOperationException>(() => canon.ChangeOwnerOfPossession(building, city),
                "PlaceBuildingInCity failed to throw when placement was not permitted");
        }

        [Test(Description = "When PlaceBuildingInCity is called, GetBuildingsOfCity should reflect " +
            "the presence of the argued building in the argued city")]
        public void PlaceBuildingInCity_ReflectedInGetBuildingsOfCity() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, city);

            CollectionAssert.Contains(canon.GetPossessionsOfOwner(city), building, 
                "GetPossessionsOfOwner failed to reflect the placement of a building into a city");
        }

        [Test(Description = "When PlaceBuildingInCity is called, GetOwnerOfPossession should reflect " +
            "the presence of the argued building in the argued city")]
        public void PlaceBuildingInCity_ReflectedInGetOwnerOfPossession() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, city);

            Assert.AreEqual(city, canon.GetOwnerOfPossession(building), 
                "GetOwnerOfPossession failed to reflect the placement of a building into a city");
        }

        [Test(Description = "When RemoveBuildingFromCurrentCity is called, GetBuildingsOfCity " +
            "should reflect the removal of the argued building from the argued city")]
        public void RemoveBuildingFromCurrentCity_ReflectedInGetBuildingsOfCity() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, city);

            canon.ChangeOwnerOfPossession(building, null);

            CollectionAssert.DoesNotContain(canon.GetPossessionsOfOwner(city), building,
                "GetPossessionsOfOwner failed to reflect the removal of a building from a city");
        }

        [Test(Description = "When RemoveBuildingFromCurrentCity is called, GetOwnerOfPossession " +
            "should reflect the removal of the argued building from the argued city")]
        public void RemoveBuildingFromCurrentCity_ReflectedInGetOwnerOfPossession() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            canon.ChangeOwnerOfPossession(building, city);

            canon.ChangeOwnerOfPossession(building, null);

            Assert.IsNull(canon.GetOwnerOfPossession(building), "GetOwnerOfPossession failed to reflect "+
                "the removal of a building from a city");
        }

        [Test(Description = "GetPossessionsOfOwner should return an empty collection if the city has no buildings")]
        public void GetPossessionsOfOwner_ReturnsEmptyCollectionIfNone() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            var buildingsInCity = canon.GetPossessionsOfOwner(city);

            Assert.IsNotNull(buildingsInCity, "GetPossessionsOfOwner returned a null collection");
            CollectionAssert.IsEmpty(buildingsInCity, "The collection returned by GetPossessionsOfOwner is not empty");
        }

        [Test(Description = "GetOwnerOfPossession should return null if the building is part of no city")]
        public void GetOwnerOfPossession_ReturnsNullIfNone() {
            var building = new Mock<IBuilding>().Object;
            var city = new Mock<ICity>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            Assert.IsNull(canon.GetOwnerOfPossession(building), "GetOwnerOfPossession did not return null");
        }

        [Test(Description = "All methods should throw a NullArgumentException when passed " +
            "any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var city = new Mock<ICity>().Object;
            var building = new Mock<IBuilding>().Object;

            var canon = Container.Resolve<BuildingPossessionCanon>();

            Assert.Throws<ArgumentNullException>(() => canon.CanChangeOwnerOfPossession(null, city),
                "CanChangeOwnerOfPossession fails to throw when given a null building argument");

            Assert.Throws<ArgumentNullException>(() => canon.GetPossessionsOfOwner(null),
                "GetPossessionsOfOwner fails to throw when given a null city argument");

            Assert.Throws<ArgumentNullException>(() => canon.GetOwnerOfPossession(null),
                "GetOwnerOfPossession fails to throw when given a null building argument");

            Assert.Throws<ArgumentNullException>(() => canon.ChangeOwnerOfPossession(null, city),
                "PlaceBuildingInCity fails to throw when given a null building argument");

            Assert.Throws<ArgumentNullException>(() => canon.ChangeOwnerOfPossession(null, null),
                "RemoveBuildingFromCurrentCity fails to throw when given a null building argument");
        }

    }

}
