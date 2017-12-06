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

        private Subject<Foo> OpenDisplayRequestedSignal;
        private Subject<Foo> CloseDisplayRequestedSignal;
        private TurnBeganSignal TurnBeganSignal;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            OpenDisplayRequestedSignal = new Subject<Foo>();
            CloseDisplayRequestedSignal = new Subject<Foo>();

            var mockSignalLogic = new Mock<IDisplaySignalLogic<Foo>>();
            mockSignalLogic.Setup(logic => logic.CloseDisplayRequested).Returns(CloseDisplayRequestedSignal);
            mockSignalLogic.Setup(logic => logic.OpenDisplayRequested) .Returns(OpenDisplayRequestedSignal);

            Container.Bind<IDisplaySignalLogic<Foo>>().FromInstance(mockSignalLogic.Object);

            Container.Bind<SignalManager>().AsSingle();
            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<TestDisplayBase>().FromNewComponentOnNewGameObject().AsSingle();

            TurnBeganSignal = Container.Resolve<TurnBeganSignal>();
        }

        #endregion

        #region tests

        [Test(Description = "When OpenDisplayRequested fires, DisplayBase should " +
            "set ObjectToDisplay to the provided value")]
        public void OnOpenDisplayRequested_ObjectToDisplaySet(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            OpenDisplayRequestedSignal.OnNext(foo);

            Assert.AreEqual(foo, testDisplay.ObjectToDisplay, "ObjectToDisplay has an unexpected value");
        }

        [Test(Description = "When OpenDisplayRequested fires, DisplayBase should " +
            "activate its GameObject")]
        public void OnOpenDisplayRequested_GameObjectActivated(){
            var testDisplay = Container.Resolve<TestDisplayBase>();
            testDisplay.gameObject.SetActive(false);

            var foo = new Foo();
            
            OpenDisplayRequestedSignal.OnNext(foo);

            Assert.IsTrue(testDisplay.gameObject.activeInHierarchy, "GameObject was not activated as expected");
        }

        [Test(Description = "When OpenDisplayRequested fires, DisplayBase should " +
            "call its own Refresh method")]
        public void OnOpenDisplayRequested_RefreshCalled(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            testDisplay.RefreshCalled += (x, y) => Assert.Pass();

            var foo = new Foo();            
            
            OpenDisplayRequestedSignal.OnNext(foo);

            Assert.Fail("Refresh was never called");
        }

        [Test(Description = "When OpenDisplayRequested fires, DisplayBase should " +
            "create a subscription to CloseDisplayRequested, and should not do so beforehand")]
        public void OnOpenDisplayRequested_DeselectionSubscriptionCreated(){
            Container.Resolve<TestDisplayBase>();

            Assert.IsFalse(CloseDisplayRequestedSignal.HasObservers, 
                "DeselectRequestSignal should not have any observers when DisplayBase is instantiated");

            var foo = new Foo();            
            
            OpenDisplayRequestedSignal.OnNext(foo);

            Assert.IsTrue(CloseDisplayRequestedSignal.HasObservers,
                "CloseDisplayRequested has no observers after OpenDisplayRequested has fired");
        }

        [Test(Description = "When CloseDisplayRequested fires, DisplayBase should " +
            "set ObjectToDisplay to null")]
        public void OnCloseDisplayRequested_ObjectToDisplayCleared(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            OpenDisplayRequestedSignal.OnNext(foo);
            CloseDisplayRequestedSignal.OnNext(foo);

            Assert.Null(testDisplay.ObjectToDisplay, "ObjectToDisplay was not set to null");
        }

        [Test(Description = "When CloseDisplayRequested fires, DisplayBase should " +
            "deactivate its GameObject")]
        public void OnCloseDisplayRequested_GameObjectDeactivated(){
            var testDisplay = Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            OpenDisplayRequestedSignal.OnNext(foo);
            CloseDisplayRequestedSignal.OnNext(foo);

            Assert.IsFalse(testDisplay.gameObject.activeSelf,
                "TestDisplay's GameObject should not be active after CloseDisplayRequested fires");
        }

        [Test(Description = "When CloseDisplayRequested fires, DisplayBase should " +
            "unsubscribe its subscription to Deselect Requested Signal")]
        public void OnCloseDisplayRequested_DeselectionSubscriptionCleared(){
            Container.Resolve<TestDisplayBase>();

            var foo = new Foo();
            
            OpenDisplayRequestedSignal.OnNext(foo);
            CloseDisplayRequestedSignal.OnNext(foo);

            Assert.IsFalse(CloseDisplayRequestedSignal.HasObservers, 
                "CloseDisplayRequested still has observers even after being fired");
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
