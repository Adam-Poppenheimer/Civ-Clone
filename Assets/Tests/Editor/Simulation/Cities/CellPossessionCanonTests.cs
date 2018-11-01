using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CellPossessionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;

        private HexCellSignals CellSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            CellSignals = new HexCellSignals();

            Container.Bind<CitySignals>().AsSingle();
            Container.Bind<HexCellSignals>().FromInstance(CellSignals);

            Container.Bind<CellPossessionCanon>().AsSingle();

            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
        }

        #endregion

        #region tests

        [Test(Description = "CanChangeOwnerOfPossession should return true if GetOwnerOfPossession " +
            "returns a different city than the argued city")]
        public void CanChangeOwnerOfPossession_TrueIfCellHasOtherOwner() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell    = BuildCell();
            var oldCity = BuildCity();
            var newCity = BuildCity();

            possessionCanon.ChangeOwnerOfPossession(cell, oldCity);

            Assert.IsTrue(possessionCanon.CanChangeOwnerOfPossession(cell, newCity),
                "CanChangeOwnerOfPossession fails to permit the assignment of a cell with a city to a different city");
        }

        [Test(Description = "CanChangeOwnerOfPossession should return true if GetOwnerOfPossession " +
            "returns null")]
        public void CanChangeOwnerOfPossession_TrueIfHasNoOwner() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell = BuildCell();
            var city = BuildCity();

            Assert.IsTrue(possessionCanon.CanChangeOwnerOfPossession(cell, city),
                "CanChangeOwnerOfPossession fails to permit assignment of a cell that has no owner");
        }

        [Test(Description = "CanChangeOwnerOfPossession should return false if GetOwnerOfPossession " +
            "returns the argued city")]
        public void CanChangeOwnerOfPossession_FalseIfAlreadyOwnedBy() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell = BuildCell();
            var city = BuildCity();

            possessionCanon.ChangeOwnerOfPossession(cell, city);

            Assert.IsFalse(possessionCanon.CanChangeOwnerOfPossession(cell, city),
                "CanChangeOwnerOfPossession falsely permits reassignment of a cell to its current owner");
        }

        [Test(Description = "CanChangeOwnerOfPossession should return false if the argued cell " +
            "is the location of a different city")]
        public void CanChangeOwnerOfPossession_FalseIfLocationOfDifferentCity() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var locationOfFirstCity = BuildCell();
            var firstCity  = BuildCity();
            var secondCity = BuildCity();

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(locationOfFirstCity)).Returns(new List<ICity>() { firstCity });
            
            Assert.IsFalse(possessionCanon.CanChangeOwnerOfPossession(locationOfFirstCity, secondCity),
                "CanChangeOwnerOfPossession falsely permitted the assignment of a city's location to a different city");
        }

        [Test(Description = "When ChangeOwnerOfPossession is called, GetCityOfTile should return the " +
            "argued city when passed the argued tile")]
        public void ChangeOwnerOfPossession_ChangeReflectedInGetCityOfTile() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell    = BuildCell();
            var oldCity = BuildCity();
            var newCity = BuildCity();

            possessionCanon.ChangeOwnerOfPossession(cell, oldCity);

            Assert.AreEqual(oldCity, possessionCanon.GetOwnerOfPossession(cell),
                "GetOwnerOfPossession failed to return oldCity after the first ownership change");

            possessionCanon.ChangeOwnerOfPossession(cell, newCity);

            Assert.AreEqual(newCity, possessionCanon.GetOwnerOfPossession(cell),
                "GetOwnerOfPossession failed to return newCity after the second ownership change");
        }

        [Test(Description = "When ChangeOwnerOfPossession is called, GetPossessionsOfOwner should return " +
            "a set containing the argued cell when passed the argued city")]
        public void ChangeOwnerOfPossession_ChangeReflectedInGetPossessionsOfOwner() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell    = BuildCell();
            var oldCity = BuildCity();
            var newCity = BuildCity();

            possessionCanon.ChangeOwnerOfPossession(cell, oldCity);

            CollectionAssert.Contains(possessionCanon.GetPossessionsOfOwner(oldCity), cell,
                "GetPossessionsOfOwner(newCity) failed to return a collection containing cell after the first ownership change");

            possessionCanon.ChangeOwnerOfPossession(cell, newCity);

            CollectionAssert.DoesNotContain(possessionCanon.GetPossessionsOfOwner(oldCity), cell,
                "GetPossessionsOfOwner(oldCity) falsely returned a collection containing cell after the second ownership change");

            CollectionAssert.Contains(possessionCanon.GetPossessionsOfOwner(newCity), cell,
                "GetPossessionsOfOwner(newCity) failed to return a collection containing cell after the second ownership change");
        }

        [Test(Description = "When ChangeOwnerOfPossession is called on a null city, it should not " +
            "throw an exception")]
        public void ChangeOwnerOfTile_NullOwnerValid() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell = BuildCell();
            var city = BuildCity();

            possessionCanon.ChangeOwnerOfPossession(cell, city);

            Assert.DoesNotThrow(() => possessionCanon.ChangeOwnerOfPossession(cell, null),
                "ChangeOwnerOfPossession threw an unexpected exception on a null city argument");
        }

        [Test(Description = "When ChangeOwnerOfTile is called, but CanChangeOwnerOfTile would return " +
            "false on the passed arguments, an InvalidOperationException should be thrown")]
        public void ChangeOwnerOfCell_ThrowsIfNotPermitted() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            var cell = BuildCell();

            Assert.Throws<InvalidOperationException>(() => possessionCanon.ChangeOwnerOfPossession(cell, null),
                "ChangeOwnerOfPossession failed to throw on an invalid change operation");
        }

        [Test(Description = "All methods should throw an ArgumentNullException when passed a null tile argument")]
        public void AllMethods_ThrowOnNullCellArgument() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();
            
            var city = BuildCity();

            Assert.Throws<ArgumentNullException>(() => possessionCanon.CanChangeOwnerOfPossession(null, city),
                "CanChangeOwnerOfPossession failed to throw an ArgumentNullException on a null cell argument");

            Assert.Throws<ArgumentNullException>(() => possessionCanon.ChangeOwnerOfPossession(null, city),
                "ChangeOwnerOfPossession failed to throw an ArgumentNullException on a null cell argument");

            Assert.Throws<ArgumentNullException>(() => possessionCanon.GetOwnerOfPossession(null),
                "GetOwnerOfPossession failed to throw an ArgumentNullException on a null cell argument");
        }

        [Test(Description = "GetTilesOfCity should throw an ArgumentNullException when passed a null city argument")]
        public void GetTilesOfCity_ThrowsOnNullArgument() {
            var possessionCanon = Container.Resolve<CellPossessionCanon>();
            Assert.Throws<ArgumentNullException>(() => possessionCanon.GetPossessionsOfOwner(null),
                "GetTilesOfCity failed to throw an ArgumentNullException on a null city argument");
        }

        [Test]
        public void MapBeingClearedSignalFired_CanonCleared() {
            var cellOne = BuildCell();
            var cellTwo = BuildCell();

            var cityOne = BuildCity();
            var cityTwo = BuildCity();

            var possessionCanon = Container.Resolve<CellPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(cellOne, cityOne);
            possessionCanon.ChangeOwnerOfPossession(cellTwo, cityTwo);

            CellSignals.MapBeingClearedSignal.OnNext(new UniRx.Unit());

            CollectionAssert.IsEmpty(possessionCanon.GetPossessionsOfOwner(cityOne), "CityOne incorrectly has possessions");
            CollectionAssert.IsEmpty(possessionCanon.GetPossessionsOfOwner(cityTwo), "CityTwo incorrectly has possessions");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        #endregion

        #endregion

    }

}
