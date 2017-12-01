using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.UI.Cities;

using Assets.Simulation;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class WorkerSlotDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Image SlotImage;

        private Mock<ICityUIConfig> MockConfig;

        #endregion 

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            SlotImage = Container.InstantiateComponentOnNewGameObject<Image>();

            Container.Bind<Image>().WithId("Slot Image").FromInstance(SlotImage);

            MockConfig = new Mock<ICityUIConfig>();
            MockConfig.Setup(config => config.OccupiedSlotMaterial)  .Returns(Resources.Load<Material>("Test Occupied Slot Material" ));
            MockConfig.Setup(config => config.UnoccupiedSlotMaterial).Returns(Resources.Load<Material>("Test Unoccupied Slot Material"));
            MockConfig.Setup(config => config.LockedSlotMaterial)    .Returns(Resources.Load<Material>("Test Locked Slot Material"   ));

            Container.Bind<ICityUIConfig>().FromInstance(MockConfig.Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<SlotDisplayClickedSignal>();

            Container.Bind<WorkerSlotDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and SlotToDisplay is null, nothing significant should happen")]
        public void RefreshCalled_AndSlotToDisplayNull_DoesNothing() {
            var slotDisplay = Container.Resolve<WorkerSlotDisplay>();

            slotDisplay.Refresh();

            MockConfig.VerifyGet(config => config.UnoccupiedSlotMaterial, Times.Never,
                "Config.UnoccupiedSlotMaterial was accessed an unexpected number of times");

            MockConfig.VerifyGet(config => config.OccupiedSlotMaterial, Times.Never,
                "Config.OccupiedSlotMaterial was accessed an unexpected number of times");

            MockConfig.VerifyGet(config => config.LockedSlotMaterial, Times.Never,
                "Config.LockedSlotMaterial was accessed an unexpected number of times");
        }

        [Test(Description = "When Refresh is called and SlotToDisplay is not null, " + 
            "SlotImage.material is set to Config.UnoccupiedSlotMaterial if the slot isn't occupied, " +
            "Config.OccupiedSlotMaterial if the slot is occupied but isn't locked, and " +
            "Config.LockedSlotMaterial if the slot is both occupied and locked")]
        public void RefreshCalled_AndSlotToDisplayNotNull_ImageMaterialSetCorrectly() {
            var unoccupiedSlot       = BuildWorkerSlot(false, false);
            var unoccupiedLockedSlot = BuildWorkerSlot(false, true);
            var occupiedSlot         = BuildWorkerSlot(true, false);
            var occupiedLockedSlot   = BuildWorkerSlot(true, true);

            var config = Container.Resolve<ICityUIConfig>();

            var slotDisplay = Container.Resolve<WorkerSlotDisplay>();

            slotDisplay.SlotToDisplay = unoccupiedSlot;
            slotDisplay.Refresh();
            Assert.AreEqual(config.UnoccupiedSlotMaterial, SlotImage.material, "SlotImage.material has an unexpected value");

            slotDisplay.SlotToDisplay = unoccupiedLockedSlot;
            slotDisplay.Refresh();
            Assert.AreEqual(config.UnoccupiedSlotMaterial, SlotImage.material, "SlotImage.material has an unexpected value");

            slotDisplay.SlotToDisplay = occupiedSlot;
            slotDisplay.Refresh();
            Assert.AreEqual(config.OccupiedSlotMaterial, SlotImage.material, "SlotImage.material has an unexpected value");

            slotDisplay.SlotToDisplay = occupiedLockedSlot;
            slotDisplay.Refresh();
            Assert.AreEqual(config.LockedSlotMaterial, SlotImage.material, "SlotImage.material has an unexpected value");
        }

        [Test(Description = "When OnPointerClick is called and SlotToDisplay is null, " +
            "SlotDisplayClickedSignal should not be fired")]
        public void OnPointerClickCalled_AndSlotToDisplayNull_DoesNotFireSignal() {
            var slotDisplay = Container.Resolve<WorkerSlotDisplay>();

            Container.Resolve<SlotDisplayClickedSignal>().Listen(delegate(IWorkerSlotDisplay signalDisplay) {
                Assert.Fail("SlotDisplayClickedSignal was fired");
            });

            slotDisplay.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        [Test(Description = "When OnPointerClick is called and SlotToDisplay is not null, " +
            "WorkerSlotDisplay should fire SlotDisplayClickedSignal on its SlotToDisplay")]
        public void OnPointerClickCalled_AndSlotToDisplayNotNull_FiresClickedSignal() {
            var slot = BuildWorkerSlot(true, false);

            var slotDisplay = Container.Resolve<WorkerSlotDisplay>();

            Container.Resolve<SlotDisplayClickedSignal>().Listen(delegate(IWorkerSlotDisplay signalDisplay) {
                Assert.AreEqual(slotDisplay, signalDisplay, "SlotDisplayClickedSignal was fired with the wrong IWorkerSlotDisplay");
                Assert.Pass();
            });

            slotDisplay.SlotToDisplay = slot;
            slotDisplay.OnPointerClick(new PointerEventData(EventSystem.current));
            Assert.Fail("SlotDisplayClickedSignal was never fired");
        }

        #endregion

        #region utilities

        private IWorkerSlot BuildWorkerSlot(bool isOccupied, bool isLocked) {
            var mockSlot = new Mock<IWorkerSlot>();
            mockSlot.SetupAllProperties();

            mockSlot.Object.IsOccupied = isOccupied;
            mockSlot.Object.IsLocked = isLocked;

            return mockSlot.Object;
        }

        #endregion

        #endregion

    }

}
