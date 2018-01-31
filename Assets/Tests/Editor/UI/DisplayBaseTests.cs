using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.UI;

using Assets.Simulation.Core;

namespace Assets.Tests.UI {

    [TestFixture]
    public class DisplayBaseTests : ZenjectUnitTestFixture {

        #region internal types

        private class Foo { }

        private class TestDisplayBase : DisplayBase<Foo> {

            #region events

            public event EventHandler<EventArgs> RefreshCalled;

            #endregion

            #region instance methods

            #region from DisplayBase<Foo>

            public override void Refresh() {
                if(RefreshCalled != null) {
                    RefreshCalled(this, EventArgs.Empty);
                }                
            }

            #endregion

            #endregion

        }

        #endregion

        #region instance fields and properties

        private CoreSignals CoreSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            CoreSignals = new CoreSignals();

            Container.Bind<CoreSignals>().FromInstance(CoreSignals);

            Container.Bind<TestDisplayBase>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When TurnBegan signal fires, DisplayBase should " +
            "call Refresh if and only if its GameObject is active")]
        public void OnTurnBegan_RefreshedOnlyIfActive(){
            var testDisplay = Container.Resolve<TestDisplayBase>();
            bool refreshedWhenActive = false;
            bool refreshedWhenInactive = false;

            testDisplay.RefreshCalled += (x, y) => refreshedWhenActive = true;
            CoreSignals.RoundBeganSignal.OnNext(0);

            testDisplay.gameObject.SetActive(false);
            testDisplay.RefreshCalled += (x, y) => refreshedWhenInactive = true;
            CoreSignals.RoundBeganSignal.OnNext(0);

            Assert.IsTrue(refreshedWhenActive, "Refresh was not called on DisplayBase when " + 
                "OnTurnBegan fired and DisplayBase was active");

            Assert.IsFalse(refreshedWhenInactive, "Refresh was falsely called on DisplayBase when " + 
                "OnTurnBegan fired and DisplayBase was inactive");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
