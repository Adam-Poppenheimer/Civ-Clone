using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;

using Assets.UI;
using Assets.UI.Cities;
using Assets.UI.Cities.Distribution;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CitySlotChoosingDisplayTests : ZenjectUnitTestFixture {

        private Mock<IWorkerDistributionLogic> DistributionMock;
        private Mock<IWorkerSlotDisplay> SlotDisplayMock;

        private List<IWorkerSlot> AllSlots = new List<IWorkerSlot>();

        [SetUp]
        public void CommonInstall() {
            AllSlots.Clear();

            Container.Bind<SignalManager>().AsSingle();

            var mockSignalLogic = new Mock<IDisplaySignalLogic<ICity>>();
            mockSignalLogic.Setup(logic => logic.OpenDisplayRequested) .Returns(new Mock<IObservable<ICity>>().Object);
            mockSignalLogic.Setup(logic => logic.CloseDisplayRequested).Returns(new Mock<IObservable<ICity>>().Object);

            Container.Bind<IDisplaySignalLogic<ICity>>().FromInstance(mockSignalLogic.Object);

            Container.DeclareSignal<TurnBeganSignal>();

            Container.DeclareSignal<SlotDisplayClickedSignal>();

            DistributionMock = new Mock<IWorkerDistributionLogic>();
            DistributionMock
                .Setup(logic => logic.GetSlotsAvailableToCity(It.IsAny<ICity>()))
                .Returns(() => AllSlots);
            DistributionMock
                .Setup(logic => logic.GetUnemployedPeopleInCity(It.IsAny<ICity>()))
                .Returns<ICity>(city => city.Population - AllSlots.Count);

            Container.Bind<IWorkerDistributionLogic>().FromInstance(DistributionMock.Object);

            Container.Bind<CitySlotChoosingDisplay>().FromNewComponentOnNewGameObject().AsSingle();

            SlotDisplayMock = new Mock<IWorkerSlotDisplay>();
            SlotDisplayMock.SetupAllProperties();
        }

        [Test(Description = "When SlotDisplayClickedSignal is fired on an unlocked but occupied slot, " +
            "that slot should become locked")]
        public void SignalFired_UnlockedOccupiedSlotIsLocked() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest = BuildSlot(true, false);

            var slotDisplay = SlotDisplayMock.Object;

            slotDisplay.SlotToDisplay = slotToTest;

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            signal.Fire(slotDisplay);

            AssertSlot(slotToTest, "SlotToTest", true, true);
        }

        [Test(Description = "When SlotDisplayClickedSignal is fired on a locked and occupied slot, " +
            "that slot should get unlocked and become unoccupied")]
        public void SignalFired_LockedOccupiedSlotIsCleared() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest = BuildSlot(true, true);

            var slotDisplay = SlotDisplayMock.Object;

            slotDisplay.SlotToDisplay = slotToTest;

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            signal.Fire(slotDisplay);

            AssertSlot(slotToTest, "SlotToTest", false, false);
        }

        [Test(Description = "When SlotDisplayClickedSignal is fired on an unoccupied slot " +
            "and there are unemployed people in CityToDisplay, the slot should be filled and locked")]
        public void SignalFired_SlotFilledWithUnemployed() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest = BuildSlot(false, false);

            var slotDisplay = SlotDisplayMock.Object;

            slotDisplay.SlotToDisplay = slotToTest;

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            DistributionMock
                .Setup(logic => logic.GetUnemployedPeopleInCity(It.IsAny<ICity>()))
                .Returns(1);

            signal.Fire(slotDisplay);

            AssertSlot(slotToTest, "SlotToTest", true, true);
        }

        [Test(Description = "When SlotDisplayClickedSignal is fired on an unoccupied slot " +
            "and there are no unemployed people in CityToDisplay, some other slot should be " +
            "unassigned, and the clicked slot should be filled and locked")]
        public void SignalFired_ExistingOccupationReplacedWithSlot() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest      = BuildSlot(false, false);
            var occupiedSlotOne = BuildSlot(true,  false);

            var slotDisplay = SlotDisplayMock.Object;
            slotDisplay.SlotToDisplay = slotToTest;

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            DistributionMock
                .Setup(logic => logic.GetSlotsAvailableToCity(It.IsAny<ICity>()))
                .Returns(new List<IWorkerSlot>() { slotToTest, occupiedSlotOne });

            signal.Fire(slotDisplay);

            AssertSlot(slotToTest, "SlotToTest", true, true);
            AssertSlot(occupiedSlotOne, "OccupiedSlotOne", false, false);
        }

        [Test(Description = "When SlotDisplayClickedSignal causes occupation reassignment, " +
            "it should reassign all unlocked slots before reassigning any locked slots")]
        public void SignalFired_ReassignmentTakesUnlockedSlotsFirst() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var firstSlotToSet = BuildSlot(false, false);
            var secondSlotToSet = BuildSlot(false, false);

            var firstOccupiedSlot = BuildSlot(true, false);
            var secondOccupiedSlot = BuildSlot(true, false);
            var lockedOccupiedSlot = BuildSlot(true, true);

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            DistributionMock
                .Setup(logic => logic.GetSlotsAvailableToCity(It.IsAny<ICity>()))
                .Returns(new List<IWorkerSlot>() {
                    firstSlotToSet, secondSlotToSet, firstOccupiedSlot, secondOccupiedSlot, lockedOccupiedSlot
                });

            var slotDisplay = SlotDisplayMock.Object;

            slotDisplay.SlotToDisplay = firstSlotToSet;
            signal.Fire(slotDisplay);

            slotDisplay.SlotToDisplay = secondSlotToSet;
            signal.Fire(slotDisplay);

            AssertSlot(firstSlotToSet, "FirstSlotToSet",  true, true);
            AssertSlot(firstSlotToSet, "SecondSlotToSet", true, true);

            AssertSlot(firstOccupiedSlot,  "FirstOccupiedSlot",  false, false);
            AssertSlot(secondOccupiedSlot, "SecondOccupiedSlot", false, false);
            AssertSlot(lockedOccupiedSlot, "LockedOccupiedSlot", true,  true);
        }

        [Test(Description = "When SlotDisplayClickedSignal causes occupation reassignment, " +
            "CitySlotChoosingDisplay should reassign locked slots if no unlocked slots are available")]
        public void SignalFired_LockedSlotsCanGetReassigned() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest         = BuildSlot(false, false);
            var lockedOccupiedSlot = BuildSlot(true,  true);

            var slotDisplay = SlotDisplayMock.Object;
            slotDisplay.SlotToDisplay = slotToTest;

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            DistributionMock
                .Setup(logic => logic.GetSlotsAvailableToCity(It.IsAny<ICity>()))
                .Returns(new List<IWorkerSlot>() { slotToTest, lockedOccupiedSlot });

            signal.Fire(slotDisplay);

            AssertSlot(slotToTest,         "SlotToTest",         true,  true);
            AssertSlot(lockedOccupiedSlot, "LockedOccupiedSlot", false, false);
        }

        [Test(Description = "When SlotDisplayClickedSignal is fired, the passed display should " +
            "always be refreshed")]
        public void SignalFired_DisplayRefreshed() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest = BuildSlot(false, false);

            var slotDisplay = SlotDisplayMock.Object;

            slotDisplay.SlotToDisplay = slotToTest;

            choosingDisplay.ObjectToDisplay = BuildNewCity().Object;

            signal.Fire(slotDisplay);

            SlotDisplayMock.Verify(display => display.Refresh(), Times.AtLeastOnce,
                "SlotDisplay.Refresh was not called at least once");
        }

        [Test(Description = "When SlotDisplayClickedSignal is fired, the display should always " +
            "call CityToDisplay.PerformDistribution")]
        public void SignalFired_DistributionPerformed() {
            var choosingDisplay = Container.Resolve<CitySlotChoosingDisplay>();
            var signal = Container.Resolve<SlotDisplayClickedSignal>();

            var slotToTest = BuildSlot(false, false);

            var slotDisplay = SlotDisplayMock.Object;

            slotDisplay.SlotToDisplay = slotToTest;

            var cityMock = BuildNewCity();

            choosingDisplay.ObjectToDisplay = cityMock.Object;

            signal.Fire(slotDisplay);

            cityMock.Verify(city => city.PerformDistribution(),
                Times.Once, "City.PerformDistribution was not called exactly once");
        }

        #region utilities

        private IWorkerSlot BuildSlot(bool isOccupied, bool isLocked) {
            var mockSlot = new Mock<IWorkerSlot>();
            mockSlot.SetupAllProperties();

            mockSlot.Object.IsOccupied = isOccupied;
            mockSlot.Object.IsLocked = isLocked;

            AllSlots.Add(mockSlot.Object);

            return mockSlot.Object;
        }

        private Mock<ICity> BuildNewCity() {
            var cityMock = new Mock<ICity>();

            return cityMock;
        }

        private void AssertSlot(IWorkerSlot slot, string slotName, bool expectedOccupied, bool expectedLock) {
            Assert.That(slot.IsOccupied == expectedOccupied, string.Format("{0}.IsOccupied has an unexpected value", slotName));
            Assert.That(slot.IsLocked   == expectedLock,     string.Format("{0}.IsLocked has an unexpected value", slotName));
        }

        #endregion

    }

}
