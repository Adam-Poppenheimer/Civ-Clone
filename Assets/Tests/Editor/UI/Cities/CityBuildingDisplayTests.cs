using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Core;

using Assets.UI;
using Assets.UI.Cities;
using Assets.UI.Cities.Buildings;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityBuildingDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Transform BuildingDisplayParent;

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockPossessionCanon;

        private List<IBuilding> AllBuildings = new List<IBuilding>();

        private List<Mock<IBuildingDisplay>> AllBuildingDisplays = new List<Mock<IBuildingDisplay>>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllBuildings.Clear();
            AllBuildingDisplays.Clear();

            BuildingDisplayParent = new GameObject().transform;

            MockPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            MockPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICity>()))
                .Returns(() => AllBuildings.AsReadOnly());

            Container.BindFactory<IBuildingDisplay, BuildingDisplayFactory>().FromMethod(delegate(DiContainer container) {
                var mockDisplay = new Mock<IBuildingDisplay>();

                mockDisplay.SetupAllProperties();

                mockDisplay.Setup(display => display.gameObject).Returns(new GameObject());

                AllBuildingDisplays.Add(mockDisplay);
                return mockDisplay.Object;
            });

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockPossessionCanon.Object);

            Container.Bind<Transform>().WithId("Building Display Parent").FromInstance(BuildingDisplayParent);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<SlotDisplayClickedSignal>();

            Container.Bind<CityBuildingDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and ObjectToDisplay is null, no exceptions " +
            "should be thrown and nothing significant should happen")]
        public void OnRefresh_DoesNothingOnNullCity() {
            var displayToTest = Container.Resolve<CityBuildingDisplay>();

            Assert.DoesNotThrow(() => displayToTest.Refresh(),
                "Refresh threw an unexpected exception on a null ObjectToDisplay");

            Assert.AreEqual(0, AllBuildingDisplays.Count, "A nonzero number of building " +
                "displays were instantiated on a null ObjectToDisplay");
        }

        [Test(Description = "When Refresh is called and ObjectToDisplay is not null, " +
            "CityBuildingDisplay should instantiate an IBuildingDisplay for every building " +
            "in that city, and pass each building to exactly one display. All displays " +
            "instantiated this way should also be refreshed")]
        public void OnRefresh_InstantiatesAndConfiguresBuildingDisplays() {
            BuildBuilding();
            BuildBuilding();
            BuildBuilding();

            var displayToTest = Container.Resolve<CityBuildingDisplay>();

            displayToTest.ObjectToDisplay = BuildCity().Object;
            displayToTest.Refresh();

            var buildingsOfDisplay = AllBuildingDisplays.Select(display => display.Object.BuildingToDisplay);

            CollectionAssert.AllItemsAreUnique(buildingsOfDisplay,
                "All building displays should be displaying a different building");

            CollectionAssert.AreEquivalent(AllBuildings, buildingsOfDisplay, 
                "Not all buildings are represented in all instantiated BuildingDisplays");

            foreach(var buildingDisplay in AllBuildingDisplays) {
                buildingDisplay.Verify(display => display.Refresh(),
                    "A building display was not refreshed as expected");
            }
        }

        [Test(Description = "When Refresh is called, all instantiated IBuildingDisplays " +
            "should become children of BuildingDisplayParent")]
        public void OnRefresh_DisplaysPlacedUnderDisplayParent() {
            BuildBuilding();
            BuildBuilding();
            BuildBuilding();

            var displayToTest = Container.Resolve<CityBuildingDisplay>();

            displayToTest.ObjectToDisplay = BuildCity().Object;
            displayToTest.Refresh();

            foreach(var displayMock in AllBuildingDisplays) {
                Assert.AreEqual(BuildingDisplayParent, displayMock.Object.gameObject.transform.parent,
                    "A BuildingDisplay has an unexpected parent");
            }
        }

        [Test(Description = "When Refresh is called more than once, CityBuildingDisplay " +
            "should try to reuse as many previously instantiated IBuildingDisplays as it can")]
        public void OnRefresh_ReusesExistingDisplays() {
            BuildBuilding();
            BuildBuilding();
            BuildBuilding();

            var displayToTest = Container.Resolve<CityBuildingDisplay>();

            displayToTest.ObjectToDisplay = BuildCity().Object;
            displayToTest.Refresh();

            AllBuildings.Clear();

            AllBuildingDisplays.ForEach(display => display.ResetCalls());

            BuildBuilding();
            BuildBuilding();
            BuildBuilding();
            BuildBuilding();

            displayToTest.Refresh();

            Assert.AreEqual(4, AllBuildingDisplays.Count, "There are an unexpected number of building displays");

            var buildingsOfDisplay = AllBuildingDisplays.Select(display => display.Object.BuildingToDisplay);

            CollectionAssert.AllItemsAreUnique(buildingsOfDisplay,
                "All building displays should be displaying a different building");

            CollectionAssert.AreEquivalent(AllBuildings, buildingsOfDisplay, 
                "Not all buildings are represented in all instantiated BuildingDisplays");

            foreach(var displayMock in AllBuildingDisplays) {
                displayMock.Verify(display => display.Refresh(), "A building display was not refreshed");
            }
        }

        [Test(Description = "When Refresh is called more than once, CityBuildingDisplay " +
            "should deactivate any building displays that aren't being used")]
        public void OnRefresh_UnusedDisplaysDeactivated() {
            BuildBuilding();
            BuildBuilding();
            BuildBuilding();

            var displayToTest = Container.Resolve<CityBuildingDisplay>();

            displayToTest.ObjectToDisplay = BuildCity().Object;
            displayToTest.Refresh();

            AllBuildings.Clear();

            BuildBuilding();

            displayToTest.Refresh();

            Assert.AreEqual(
                1, AllBuildingDisplays.Select(mock => mock.Object).Where(display => display.gameObject.activeSelf).Count(),
                "An unexpected number of building displays are still active"
            );
        }

        #endregion

        #region utilities

        private Mock<IBuilding> BuildBuilding() {
            var mockBuilding = new Mock<IBuilding>();

            AllBuildings.Add(mockBuilding.Object);

            return mockBuilding;
        }

        private Mock<ICity> BuildCity() {
            return new Mock<ICity>();
        }

        #endregion

        #endregion

    }

}
