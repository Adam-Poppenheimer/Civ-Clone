using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.GameMap;

namespace Assets.Cities.Editor {

    [TestFixture]
    public class TilePossessionCanonTests : ZenjectUnitTestFixture {

        [SetUp]
        public void CommonInstall() {
            Container.Bind<TilePossessionCanon>().AsSingle();
        }

        [Test(Description = "CanChangeOwnerOfTile should return true if GetCityOfTile " +
            "returns a different city than the argued city")]
        public void CanChangeOwnerOfTile_TrueIfHasOtherOwner() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;
            var oldCity = new Mock<ICity>().Object;
            var newCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfTile(tile, oldCity);

            Assert.IsTrue(possessionCanon.CanChangeOwnerOfTile(tile, newCity),
                "CanChangeOwnerOfTile fails to permit the assignment of a tile with a city to a different city");
        }

        [Test(Description = "CanChangeOwnerOfTile should return true if GetCityOfTile " +
            "returns null")]
        public void CanChangeOwnerOfTile_TrueIfHasNoOwner() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;
            var city = new Mock<ICity>().Object;

            Assert.IsTrue(possessionCanon.CanChangeOwnerOfTile(tile, city),
                "CanChangeOwnerOfTile fails to permit assignment of a tile that has no owner");
        }

        [Test(Description = "CanChangeOwnerOfTile should return false if GetCityOfTile " +
            "returns the argued city")]
        public void CanChangeOwnerOfTile_FalseIfAlreadyOwnedBy() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;
            var city = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfTile(tile, city);

            Assert.IsFalse(possessionCanon.CanChangeOwnerOfTile(tile, city),
                "CanChangeOwnerOfTile falsely permits reassignment of a tile to its current owner");
        }

        [Test(Description = "CanChangeOwnerOfTile should return false if the argued tile " +
            "is the location of a different city, and that different city already possesses the tile")]
        public void CanChangeOwnerOfTile_FalseIfLocationOfDifferentCity() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;

            var firstCityMock = new Mock<ICity>();
            firstCityMock.SetupGet(city => city.Location).Returns(tile);

            var secondCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfTile(tile, firstCityMock.Object);
            
            Assert.IsFalse(possessionCanon.CanChangeOwnerOfTile(tile, secondCity),
                "CanChangeOwnerOfTile falsely permitted the assignment of a city's location to a different city");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, GetCityOfTile should return the " +
            "argued city when passed the argued tile")]
        public void ChangeOwnerOfTile_ChangeReflectedInGetCityOfTile() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;
            var oldCity = new Mock<ICity>().Object;
            var newCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfTile(tile, oldCity);

            Assert.AreEqual(oldCity, possessionCanon.GetCityOfTile(tile),
                "GetCityOfTile failed to return oldCity after the first ownership change");

            possessionCanon.ChangeOwnerOfTile(tile, newCity);

            Assert.AreEqual(newCity, possessionCanon.GetCityOfTile(tile),
                "GetCityOfTile failed to return newCity after the second ownership change");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, GetTilesOfCity should return " +
            "a set containing the argued tile when passed the argued city")]
        public void ChangeOwnerOfTile_ChangeReflectedInGetTilesOfCity() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;
            var oldCity = new Mock<ICity>().Object;
            var newCity = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfTile(tile, oldCity);

            CollectionAssert.Contains(possessionCanon.GetTilesOfCity(oldCity), tile,
                "GetTilesOfCity(newCity) failed to return a collection containing tile after the first ownership change");

            possessionCanon.ChangeOwnerOfTile(tile, newCity);

            CollectionAssert.DoesNotContain(possessionCanon.GetTilesOfCity(oldCity), tile,
                "GetTilesOfCity(oldCity) falsely returned a collection containing tile after the second ownership change");

            CollectionAssert.Contains(possessionCanon.GetTilesOfCity(newCity), tile,
                "GetTilesOfCity(newCity) failed to return a collection containing tile after the second ownership change");
        }

        [Test(Description = "When ChangeOwnerOfTile is called on a null city, it should not " +
            "throw an exception")]
        public void ChangeOwnerOfTile_NullOwnerValid() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;
            var city = new Mock<ICity>().Object;

            possessionCanon.ChangeOwnerOfTile(tile, city);

            Assert.DoesNotThrow(() => possessionCanon.ChangeOwnerOfTile(tile, null),
                "ChangeOwnerOfTile threw an unexpected exception on a null city argument");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, but CanChangeOwnerOfTile would return " +
            "false on the passed arguments, an InvalidOperationException should be thrown")]
        public void ChangeOwnerOfTile_ThrowsIfNotPermitted() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();

            var tile = new Mock<IMapTile>().Object;

            Assert.Throws<InvalidOperationException>(() => possessionCanon.ChangeOwnerOfTile(tile, null),
                "ChangeOwnerOfTile failed to throw on an invalid change operation");
        }

        [Test(Description = "All methods should throw an ArgumentNullException when passed a null tile argument")]
        public void AllMethods_ThrowOnNullTileArgument() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();
            
            var city = new Mock<ICity>().Object;

            Assert.Throws<ArgumentNullException>(() => possessionCanon.CanChangeOwnerOfTile(null, city),
                "CanChangeOwnerOfTile failed to throw an ArgumentNullException on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => possessionCanon.ChangeOwnerOfTile(null, city),
                "ChangeOwnerOfTile failed to throw an ArgumentNullException on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => possessionCanon.GetCityOfTile(null),
                "GetCityOfTile failed to throw an ArgumentNullException on a null tile argument");
        }

        [Test(Description = "GetTilesOfCity should throw an ArgumentNullException when passed a null city argument")]
        public void GetTilesOfCity_ThrowsOnNullArgument() {
            var possessionCanon = Container.Resolve<TilePossessionCanon>();
            Assert.Throws<ArgumentNullException>(() => possessionCanon.GetTilesOfCity(null),
                "GetTilesOfCity failed to throw an ArgumentNullException on a null city argument");
        }

    }

}
