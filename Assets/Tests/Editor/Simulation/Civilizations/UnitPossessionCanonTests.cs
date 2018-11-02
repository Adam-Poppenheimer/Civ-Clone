using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Moq;
using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.Civilizations {

    public class UnitPossessionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IResourceLockingCanon> MockResourceLockingCanon;
        private CivilizationSignals         CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourceLockingCanon = new Mock<IResourceLockingCanon>();
            CivSignals               = new CivilizationSignals();

            Container.Bind<IResourceLockingCanon>().FromInstance(MockResourceLockingCanon.Object);
            Container.Bind<CivilizationSignals>  ().FromInstance(CivSignals);

            Container.Bind<UnitPossessionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PossessionEstablished_RequiredResourcesOfUnitLocked() {
            var resourceOne = BuildResource("Resource One");
            var resourceTwo = BuildResource("Resource Two");

            var unit = BuildUnit(resourceOne, resourceTwo);

            var civ = BuildCiv();

            var possessionCanon = Container.Resolve<UnitPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(unit, civ);

            MockResourceLockingCanon.Verify(canon => canon.LockCopyOfResourceForCiv(resourceOne, civ), "Failed to lock ResourceOne");
            MockResourceLockingCanon.Verify(canon => canon.LockCopyOfResourceForCiv(resourceTwo, civ), "Failed to lock ResourceTwo");
        }

        [Test]
        public void PossessionEstablished_CivGainedUnitSignalFired() {
            var unit = BuildUnit();

            var civ = BuildCiv();

            var possessionCanon = Container.Resolve<UnitPossessionCanon>();

            CivSignals.CivGainedUnitSignal.Subscribe(delegate(Tuple<ICivilization, IUnit> data) {
                Assert.AreEqual(civ,  data.Item1, "Incorrect civ passed");
                Assert.AreEqual(unit, data.Item2, "Incorrect unit passed");

                Assert.Pass();
            });

            possessionCanon.ChangeOwnerOfPossession(unit, civ);

            Assert.Fail("CivGainedUnitSignal not fired");
        }

        [Test]
        public void PossessionBroken_RequiredResourcesOfUnitUnlocked() {
            var resourceOne = BuildResource("Resource One");
            var resourceTwo = BuildResource("Resource Two");

            var unit = BuildUnit(resourceOne, resourceTwo);

            var civ = BuildCiv();

            var possessionCanon = Container.Resolve<UnitPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(unit, civ);
            possessionCanon.ChangeOwnerOfPossession(unit, null);

            MockResourceLockingCanon.Verify(canon => canon.UnlockCopyOfResourceForCiv(resourceOne, civ), "Failed to unlock ResourceOne");
            MockResourceLockingCanon.Verify(canon => canon.UnlockCopyOfResourceForCiv(resourceTwo, civ), "Failed to unlock ResourceTwo");
        }

        [Test]
        public void PossessionBroken_CivLosingUnitSignalFired() {
            var unit = BuildUnit();

            var civ = BuildCiv();

            var possessionCanon = Container.Resolve<UnitPossessionCanon>();

            CivSignals.CivLosingUnitSignal.Subscribe(delegate(Tuple<ICivilization, IUnit> data) {
                Assert.AreEqual(civ,  data.Item1, "Incorrect civ passed");
                Assert.AreEqual(unit, data.Item2, "Incorrect unit passed");

                Assert.Pass();
            });

            possessionCanon.ChangeOwnerOfPossession(unit, civ);
            possessionCanon.ChangeOwnerOfPossession(unit, null);

            Assert.Fail("CivLosingUnitSignal not fired");
        }

        [Test]
        public void PossessionBroken_CivLostUnitSignalFired() {
            var unit = BuildUnit();

            var civ = BuildCiv();

            var possessionCanon = Container.Resolve<UnitPossessionCanon>();

            CivSignals.CivLostUnitSignal.Subscribe(delegate(Tuple<ICivilization, IUnit> data) {
                Assert.AreEqual(civ,  data.Item1, "Incorrect civ passed");
                Assert.AreEqual(unit, data.Item2, "Incorrect unit passed");

                Assert.Pass();
            });

            possessionCanon.ChangeOwnerOfPossession(unit, civ);
            possessionCanon.ChangeOwnerOfPossession(unit, null);

            Assert.Fail("CivLostUnitSignal not fired");
        }

        [Test]
        public void OnCivilizationBeingDestroyedFired_AllUnitsBelongingToCivDestroyed() {
            Mock<IUnit> unitOneMock, unitTwoMock;

            var unitOne = BuildUnit(out unitOneMock);
            var unitTwo = BuildUnit(out unitTwoMock);

            var civ = BuildCiv();

            var possessionCanon = Container.Resolve<UnitPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(unitOne, civ);
            possessionCanon.ChangeOwnerOfPossession(unitTwo, civ);

            CivSignals.CivilizationBeingDestroyedSignal.OnNext(civ);

            unitOneMock.Verify(unit => unit.Destroy(), "UnitOne not destroyed");
            unitTwoMock.Verify(unit => unit.Destroy(), "UnitTwo not destroyed");
        }

        #endregion

        #region utilities

        private IResourceDefinition BuildResource(string name) {
            var mockDefinition = new Mock<IResourceDefinition>();

            mockDefinition.Name = name;
            mockDefinition.Setup(resource => resource.name).Returns(name);

            return mockDefinition.Object;
        }

        private IUnit BuildUnit(params IResourceDefinition[] requiredResources) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.RequiredResources).Returns(requiredResources);

            return mockUnit.Object;
        }

        private IUnit BuildUnit(out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();

            return mock.Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;

        }

        #endregion

        #endregion

    }

}
