using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Core;

using Assets.UI;
using Assets.UI.Cities;
using Assets.UI.Cities.Distribution;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class DistributionPreferencesDisplayTests : ZenjectUnitTestFixture {

        [SetUp]
        public void CommonInstall() {
            Container.Bind<SignalManager>().AsSingle();

            var focusDropdown = new GameObject().AddComponent<Dropdown>();

            focusDropdown.options = new List<Dropdown.OptionData>() {
                new Dropdown.OptionData("Food"),
                new Dropdown.OptionData("Gold"),
                new Dropdown.OptionData("Production"),
                new Dropdown.OptionData("Culture"),
                new Dropdown.OptionData("TotalYield"),
            };                        

            Container.Bind<Dropdown>().FromInstance(focusDropdown);

            var mockSignalLogic = new Mock<IDisplaySignalLogic<ICity>>();
            mockSignalLogic.Setup(logic => logic.OpenDisplayRequested) .Returns(new Mock<IObservable<ICity>>().Object);
            mockSignalLogic.Setup(logic => logic.CloseDisplayRequested).Returns(new Mock<IObservable<ICity>>().Object);

            Container.Bind<IDisplaySignalLogic<ICity>>().FromInstance(mockSignalLogic.Object);

            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<DistributionPreferencesDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        [Test(Description = "When Refresh is called, DistributionPreferenceDisplay should " +
            "set the value on its ResourceFocusDropdown to correspond to the resource focus of its " +
            "CityToDisplay")]
        public void CityClickedFired_DropdownSetToCorrectValue() {
            var preferencesDisplay = Container.Resolve<DistributionPreferencesDisplay>();

            var dropdown = Container.Resolve<Dropdown>();

            var cityMock = new Mock<ICity>();

            preferencesDisplay.ObjectToDisplay = cityMock.Object;

            cityMock.Setup(city => city.ResourceFocus).Returns(ResourceFocusType.Culture);                     
            preferencesDisplay.Refresh();
            Assert.AreEqual(3, dropdown.value, "Dropdown was assigned the wrong value for ResourceFocusType.Culture");

            cityMock.Setup(city => city.ResourceFocus).Returns(ResourceFocusType.Food);                    
            preferencesDisplay.Refresh();
            Assert.AreEqual(0, dropdown.value, "Dropdown was assigned the wrong value for ResourceFocusType.Food");

            cityMock.Setup(city => city.ResourceFocus).Returns(ResourceFocusType.TotalYield);                    
            preferencesDisplay.Refresh();
            Assert.AreEqual(4, dropdown.value, "Dropdown was assigned the wrong value for ResourceFocusType.TotalYield");
        }

        [Test(Description = "When ResourceFocusDropdown's value is changed, DistributionPreferenceDisplay " +
            "should parse the new option and change CityToDisplay's ResourceFocus accordingly")]
        public void DropdownValueChanged_CityResourceFocusChanged() {
            var preferencesDisplay = Container.Resolve<DistributionPreferencesDisplay>();
            var dropdown = Container.Resolve<Dropdown>();

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();
            var city = cityMock.Object;

            preferencesDisplay.ObjectToDisplay = city;
            preferencesDisplay.Refresh();

            dropdown.onValueChanged.Invoke(4);
            Assert.AreEqual(ResourceFocusType.TotalYield, city.ResourceFocus,
                "Dropdown value of 4 did not result in the correct ResourceFocus");

            dropdown.onValueChanged.Invoke(1);
            Assert.AreEqual(ResourceFocusType.Gold, city.ResourceFocus,
                "Dropdown value of 1 did not result in the correct ResourceFocus");

            dropdown.onValueChanged.Invoke(3);
            Assert.AreEqual(ResourceFocusType.Culture, city.ResourceFocus,
                "Dropdown value of 3 did not result in the correct ResourceFocus");
        }

        [Test(Description = "When ResourceFocusDropdown's value is changed, DistributionPreferenceDisplay " +
            "should call CityToDisplay.PerformDistribution")]
        public void DropdownValueChanged_CallsPerformDistribution() {
            var preferencesDisplay = Container.Resolve<DistributionPreferencesDisplay>();
            var dropdown = Container.Resolve<Dropdown>();

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            preferencesDisplay.ObjectToDisplay = cityMock.Object;

            preferencesDisplay.Refresh();

            cityMock.ResetCalls();

            dropdown.onValueChanged.Invoke(0);

            cityMock.Verify(city => city.PerformDistribution(), Times.Once,
                "City did not have its PerformDistribution method called");
        }

    }

}
