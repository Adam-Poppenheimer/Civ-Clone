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

        private Subject<Foo> SelectRequestedSignal;
        private Subject<Foo> DeselectRequestedSignal;
        private TurnBeganSignal TurnBeganSignal;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            SelectRequestedSignal = new Subject<Foo>();
            DeselectRequestedSignal = new Subject<Foo>();

            Container.Bind<IObservable<Foo>>().WithId("Select Requested Signal")  .FromInstance(SelectRequestedSignal);
            Container.Bind<IObservable<Foo>>().WithId("Deselect Requested Signal").FromInstance(DeselectRequestedSignal);

            Container.Bind<SignalManager>().AsSingle();
            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<TestDisplayBase>().FromNewComponentOnNewGameObject().AsSingle();

            TurnBeganSignal = Container.Resolve<TurnBeganSignal>();
        }

        #endregion

        #region tests

        [Test(Description = "When Select Requested Signal fires, DisplayBase should " +
            "set ObjectToDisplay to the provided value")]
        public void OnSelectRequested_ObjectToDisplaySet(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            SelectRequestedSignal.OnNext(foo);

            Assert.AreEqual(foo, testDisplay.ObjectToDisplay, "ObjectToDisplay has an unexpected value");
        }

        [Test(Description = "When Select Requested Signal fires, DisplayBase should " +
            "activate its GameObject")]
        public void OnSelectRequested_GameObjectActivated(){
            var testDisplay = Container.Resolve<TestDisplayBase>();
            testDisplay.gameObject.SetActive(false);

            var foo = new Foo();
            
            SelectRequestedSignal.OnNext(foo);

            Assert.IsTrue(testDisplay.gameObject.activeInHierarchy, "GameObject was not activated as expected");
        }

        [Test(Description = "When Select Requested Signal fires, DisplayBase should " +
            "call its own Refresh method")]
        public void OnSelectRequested_RefreshCalled(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            testDisplay.RefreshCalled += (x, y) => Assert.Pass();

            var foo = new Foo();            
            
            SelectRequestedSignal.OnNext(foo);

            Assert.Fail("Refresh was never called");
        }

        [Test(Description = "When Deselect Requested Signal fires, DisplayBase should " +
            "create a subscription to Deselect Requested Signal, and should not do so beforehand")]
        public void OnSelectRequested_DeselectionSubscriptionCreated(){
            Container.Resolve<TestDisplayBase>();

            Assert.IsFalse(DeselectRequestedSignal.HasObservers, 
                "DeselectRequestSignal should not have any observers when DisplayBase is instantiated");

            var foo = new Foo();            
            
            SelectRequestedSignal.OnNext(foo);

            Assert.IsTrue(DeselectRequestedSignal.HasObservers,
                "DeselectRequestedSignal has no observers after SelectRequestedSignal has fired");
        }

        [Test(Description = "When Deselect Requested Signal fires, DisplayBase should " +
            "set ObjectToDisplay to null")]
        public void OnDeselectRequested_ObjectToDisplayCleared(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            SelectRequestedSignal.OnNext(foo);
            DeselectRequestedSignal.OnNext(foo);

            Assert.Null(testDisplay.ObjectToDisplay, "ObjectToDisplay was not set to null");
        }

        [Test(Description = "When Deselect Requested Signal fires, DisplayBase should " +
            "deactivate its GameObject")]
        public void OnDeselectRequested_GameObjectDeactivated(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            SelectRequestedSignal.OnNext(foo);
            DeselectRequestedSignal.OnNext(foo);

            Assert.IsFalse(testDisplay.gameObject.activeSelf,
                "TestDisplay's GameObject should not be active after the deselection event");
        }

        [Test(Description = "When Deselect Requested Signal fires, DisplayBase should " +
            "unsubscribe its subscription to Deselect Requested Signal")]
        public void OnDeselectRequested_DeselectionSubscriptionCleared(){
            Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            SelectRequestedSignal.OnNext(foo);
            DeselectRequestedSignal.OnNext(foo);

            Assert.IsFalse(DeselectRequestedSignal.HasObservers, 
                "Deselect Requested Signal still has observers even after being fired");
        }

        [Test(Description = "When TurnBegan signal fires, DisplayBase should " +
            "call Refresh if and only if its GameObject is active")]
        public void OnTurnBegan_RefreshedOnlyIfActive(){
            var testDisplay = Container.Resolve<TestDisplayBase>();
            bool refreshedWhenActive = false;
            bool refreshedWhenInactive = false;

            testDisplay.RefreshCalled += (x, y) => refreshedWhenActive = true;
            TurnBeganSignal.Fire(0);

            testDisplay.gameObject.SetActive(false);
            testDisplay.RefreshCalled += (x, y) => refreshedWhenInactive = true;
            TurnBeganSignal.Fire(0);

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
