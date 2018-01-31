using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.UI.Cities;

using Assets.Simulation.Core;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityDisplayBaseTests : ZenjectUnitTestFixture {

        #region internal types

        private class TestCityDisplayBase : CityDisplayBase {

            #region events

            public event EventHandler<EventArgs> RefreshFired;

            #endregion

            #region instance methods

            #region from CityDisplayBase

            public override void Refresh() {
                if(RefreshFired != null) {
                    RefreshFired(this, EventArgs.Empty);
                }
            }

            #endregion

            #endregion

        }

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<CoreSignals>().AsSingle();

            Container.DeclareSignal<SlotDisplayClickedSignal>();

            Container.Bind<TestCityDisplayBase>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When SlotDisplayClickedSignal is fired, CityDisplayBase should refresh itself")]
        public void SlotDisplayClickedFired_RefreshCalled() {
            var cityDisplay = Container.Resolve<TestCityDisplayBase>();

            cityDisplay.RefreshFired += delegate(object sender, EventArgs e) {
                Assert.Pass();
            };

            var slotClickedSignal = Container.Resolve<SlotDisplayClickedSignal>();
            slotClickedSignal.Fire(null);
            Assert.Fail("CityDisplayBase.Refresh was never called");
        }

        #endregion

        #endregion

    }

}
