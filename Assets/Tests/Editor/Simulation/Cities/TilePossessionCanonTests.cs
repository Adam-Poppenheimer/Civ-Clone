using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class TilePossessionCanonTests : ZenjectUnitTestFixture {

        [SetUp]
        public void CommonInstall() {
            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<CityDistributionPerformedSignal>();
            Container.DeclareSignal<CityProjectChangedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<CellPossessionCanon>().AsSingle();
        }

        [Test(Description = "CanChangeOwnerOfTile should return true if GetCityOfTile " +
            "returns a different city than the argued city")]
        public void CanChangeOwnerOfTile_TrueIfHasOtherOwner() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;
            var oldCity = new Mock<ICity>().Object;
            var newCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfPossession(tile, oldCity);

            Assert.IsTrue(possessionCanon.CanChangeOwnerOfPossession(tile, newCity),
                "CanChangeOwnerOfTile fails to permit the assignment of a tile with a city to a different city");
        }

        [Test(Description = "CanChangeOwnerOfTile should return true if GetCityOfTile " +
            "returns null")]
        public void CanChangeOwnerOfTile_TrueIfHasNoOwner() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;
            var city = new Mock<ICity>().Object;

            Assert.IsTrue(possessionCanon.CanChangeOwnerOfPossession(tile, city),
                "CanChangeOwnerOfTile fails to permit assignment of a tile that has no owner");
        }

        [Test(Description = "CanChangeOwnerOfTile should return false if GetCityOfTile " +
            "returns the argued city")]
        public void CanChangeOwnerOfTile_FalseIfAlreadyOwnedBy() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;
            var city = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfPossession(tile, city);

            Assert.IsFalse(possessionCanon.CanChangeOwnerOfPossession(tile, city),
                "CanChangeOwnerOfTile falsely permits reassignment of a tile to its current owner");
        }

        [Test(Description = "CanChangeOwnerOfTile should return false if the argued tile " +
            "is the location of a different city, and that different city already possesses the tile")]
        public void CanChangeOwnerOfTile_FalseIfLocationOfDifferentCity() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;

            var firstCityMock = new Mock<ICity>();
            firstCityMock.SetupGet(city => city.Location).Returns(tile);

            var secondCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfPossession(tile, firstCityMock.Object);
            
            Assert.IsFalse(possessionCanon.CanChangeOwnerOfPossession(tile, secondCity),
                "CanChangeOwnerOfTile falsely permitted the assignment of a city's location to a different city");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, GetCityOfTile should return the " +
            "argued city when passed the argued tile")]
        public void ChangeOwnerOfTile_ChangeReflectedInGetCityOfTile() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;
            var oldCity = new Mock<ICity>().Object;
            var newCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfPossession(tile, oldCity);

            Assert.AreEqual(oldCity, possessionCanon.GetOwnerOfPossession(tile),
                "GetCityOfTile failed to return oldCity after the first ownership change");

            possessionCanon.ChangeOwnerOfPossession(tile, newCity);

            Assert.AreEqual(newCity, possessionCanon.GetOwnerOfPossession(tile),
                "GetCityOfTile failed to return newCity after the second ownership change");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, GetTilesOfCity should return " +
            "a set containing the argued tile when passed the argued city")]
        public void ChangeOwnerOfTile_ChangeReflectedInGetTilesOfCity() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;
            var oldCity = new Mock<ICity>().Object;
            var newCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfPossession(tile, oldCity);

            CollectionAssert.Contains(possessionCanon.GetPossessionsOfOwner(oldCity), tile,
                "GetTilesOfCity(newCity) failed to return a collection containing tile after the first ownership change");

            possessionCanon.ChangeOwnerOfPossession(tile, newCity);

            CollectionAssert.DoesNotContain(possessionCanon.GetPossessionsOfOwner(oldCity), tile,
                "GetTilesOfCity(oldCity) falsely returned a collection containing tile after the second ownership change");

            CollectionAssert.Contains(possessionCanon.GetPossessionsOfOwner(newCity), tile,
                "GetTilesOfCity(newCity) failed to return a collection containing tile after the second ownership change");
        }

        [Test(Description = "When ChangeOwnerOfTile is called on a null city, it should not " +
            "throw an exception")]
        public void ChangeOwnerOfTile_NullOwnerValid() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;
            var city = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfPossession(tile, city);

            Assert.DoesNotThrow(() => possessionCanon.ChangeOwnerOfPossession(tile, null),
                "ChangeOwnerOfTile threw an unexpected exception on a null city argument");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, but CanChangeOwnerOfTile would return " +
            "false on the passed arguments, an InvalidOperationException should be thrown")]
        public void ChangeOwnerOfTile_ThrowsIfNotPermitted() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var tile = new Mock<IHexCell>().Object;

            Assert.Throws<InvalidOperationException>(() => possessionCanon.ChangeOwnerOfPossession(tile, null),
                "ChangeOwnerOfTile failed to throw on an invalid change operation");
        }

        [Test(Description = "All methods should throw an ArgumentNullException when passed a null tile argument")]
        public void AllMethods_ThrowOnNullTileArgument() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();
            
            var city = new Mock<ICity>().Object;

            Assert.Throws<ArgumentNullException>(() => possessionCanon.CanChangeOwnerOfPossession(null, city),
                "CanChangeOwnerOfTile failed to throw an ArgumentNullException on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => possessionCanon.ChangeOwnerOfPossession(null, city),
                "ChangeOwnerOfTile failed to throw an ArgumentNullException on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => possessionCanon.GetOwnerOfPossession(null),
                "GetCityOfTile failed to throw an ArgumentNullException on a null tile argument");
        }

        [Test(Description = "GetTilesOfCity should throw an ArgumentNullException when passed a null city argument")]
        public void GetTilesOfCity_ThrowsOnNullArgument() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();
            Assert.Throws<ArgumentNullException>(() => possessionCanon.GetPossessionsOfOwner(null),
                "GetTilesOfCity failed to throw an ArgumentNullException on a null city argument");
        }

    }

}
