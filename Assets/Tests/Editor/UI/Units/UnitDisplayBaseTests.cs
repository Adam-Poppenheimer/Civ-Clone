using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Core;

using Assets.UI.Units;

namespace Assets.Tests.UI.Units {

    [TestFixture]
    public class UnitDisplayBaseTests : ZenjectUnitTestFixture {

        #region internal types

        private class TestDisplay : UnitDisplayBase {

            #region events

            public event EventHandler<EventArgs> RefreshFired;

            #endregion

            #region instance methods

            #region from UnitDisplayBase

            public override void Refresh() {
                if(RefreshFired != null) {
                    RefreshFired(this, EventArgs.Empty);
                }
            }

            #endregion

            #endregion

        }

        #endregion

        #region instance fields and properties

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            UnitSignals = new UnitSignals();

            Container.Bind<UnitSignals>().FromInstance(UnitSignals);

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<TurnBeganSignal>().FromMock();

            Container.Bind<TestDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When UnitSignals.UnitLocationChangedSignal is fired, UnitDisplayBase " +
            "should call its Refresh method")]
        public void UnitLocationChangedFired_DisplayRefreshed() {
            var unit = BuildUnit();
            var newLocation = BuildTile();

            var displayToTest = Container.Resolve<TestDisplay>();
            displayToTest.RefreshFired += (sender, args) => Assert.Pass();

            displayToTest.ObjectToDisplay = unit;
            displayToTest.OnEnable();

            UnitSignals.UnitLocationChangedSignal.OnNext(new Tuple<IUnit, IHexCell>(unit, newLocation));

            Assert.Fail("Refresh was never called");
        }

        [Test(Description = "When UnitSignals.UnitLocationChangedSignal is fired, UnitDisplayBase " +
            "should not call its Refresh method if the passed unit isn't the one it's currently displaying")]
        public void UnitLocationChangedFired_IgnoredIfNotSameUnit() {
            var unit = BuildUnit();
            var newLocation = BuildTile();

            var displayToTest = Container.Resolve<TestDisplay>();
            displayToTest.RefreshFired += (sender, args) => Assert.Fail("DisplayToTest refreshed on an invalid signal");
            displayToTest.OnEnable();

            UnitSignals.UnitLocationChangedSignal.OnNext(new Tuple<IUnit, IHexCell>(unit, newLocation));
        }

        [Test(Description = "When UnitSignals.UnitActivatedAbilitySignal is fired, UnitDisplayBase " +
            "should call its Refresh method")]
        public void UnitActivatedAbilityFired_DisplayRefreshed() {
            var unit = BuildUnit();
            var newAbility = BuildAbility();

            var displayToTest = Container.Resolve<TestDisplay>();
            displayToTest.RefreshFired += (sender, args) => Assert.Pass();

            displayToTest.ObjectToDisplay = unit;
            displayToTest.OnEnable();

            UnitSignals.UnitActivatedAbilitySignal.OnNext(new Tuple<IUnit, IUnitAbilityDefinition>(unit, newAbility));

            Assert.Fail("Refresh was never called");
        }

        [Test(Description = "When UnitSignals.UnitActivatedAbilitySignal is fired, UnitDisplayBase " +
            "should not call its Refresh method if the passed unit isn't the one it's currently displaying")]
        public void UnitActivatedAbilityFired_IgnoredIfNotSameUnit() {
            var unit = BuildUnit();
            var newAbility = BuildAbility();

            var displayToTest = Container.Resolve<TestDisplay>();
            displayToTest.RefreshFired += (sender, args) => Assert.Fail("DisplayToTest refreshed on an invalid signal");
            displayToTest.OnEnable();

            UnitSignals.UnitActivatedAbilitySignal.OnNext(new Tuple<IUnit, IUnitAbilityDefinition>(unit, newAbility));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            var mockUnit = new Mock<IUnit>();

            return mockUnit.Object;
        }

        private IHexCell BuildTile() {
            return new Mock<IHexCell>().Object;
        }

        private IUnitAbilityDefinition BuildAbility() {
            return new Mock<IUnitAbilityDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
