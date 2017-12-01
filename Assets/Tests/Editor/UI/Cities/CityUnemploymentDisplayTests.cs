using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Core;

using Assets.UI.Cities.Distribution;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityUnemploymentDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IWorkerDistributionLogic> MockDistributionLogic;

        private Text UnemployedPeopleField;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockDistributionLogic = new Mock<IWorkerDistributionLogic>();

            Container.Bind<IWorkerDistributionLogic>().FromInstance(MockDistributionLogic.Object);

            UnemployedPeopleField = Container.InstantiateComponentOnNewGameObject<Text>();
            Container.Bind<Text>().WithId("Unemployed People Field").FromInstance(UnemployedPeopleField);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<CityClickedSignal>();
            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<IObservable<Unit>>().WithId("CityDisplay Deselected").FromMock();

            Container.Bind<CityUnemploymentDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and CityToDisplay is null, nothing significant " +
            "should be called")]
        public void RefreshCalled_AndCityToDisplayNull_NothingHappens() {
            var display = Container.Resolve<CityUnemploymentDisplay>();

            display.Refresh();

            MockDistributionLogic.Verify(logic => logic.GetUnemployedPeopleInCity(It.IsAny<ICity>()),
                Times.Never, "DistributionLogic.GetUnemployedPeopleInCity was called an unexpected number of times");

            Assert.AreEqual("", UnemployedPeopleField.text, "UnemployedPeopleField.text has an unexpected value");
        }

        [Test(Description = "When Refresh is called and CityToDisplay is not null, UnemployedPeopleField " +
            "should be given the unemployed person count claimed by DistributionLogic")]
        [TestCase(5)]
        [TestCase(0)]
        [TestCase(-2)]
        public void RefreshCalled_AndCityToDisplayNotNull_FieldRefreshed(int unemployedPeople) {
            var display = Container.Resolve<CityUnemploymentDisplay>();
            
            var city = BuildCity(unemployedPeople);

            Container.Resolve<CityClickedSignal>().Fire(city, new PointerEventData(EventSystem.current));

            MockDistributionLogic.Verify(logic => logic.GetUnemployedPeopleInCity(city),
                Times.Once, "DistributionLogic.GetUnemployedPeopleInCity was called an unexpected number of times");

            Assert.AreEqual(unemployedPeople.ToString(), UnemployedPeopleField.text, 
                "UnemployedPeopleField.text has an unexpected value");
        }

        #endregion

        #region utilities

        private ICity BuildCity(int unemployedPeople) {
            var mockCity = new Mock<ICity>();

            MockDistributionLogic
                .Setup(logic => logic.GetUnemployedPeopleInCity(mockCity.Object))
                .Returns(unemployedPeople);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
