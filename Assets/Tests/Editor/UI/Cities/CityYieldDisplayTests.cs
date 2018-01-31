using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.UI.Cities;
using Assets.UI.Cities.ResourceGeneration;
using Assets.UI;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityYieldDisplayTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(new ResourceSummary(food: 1, production: 2, gold: 3, culture: 4));
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IResourceGenerationLogic> MockGenerationLogic;
        private Mock<IResourceSummaryDisplay>  MockYieldDisplay;        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGenerationLogic = new Mock<IResourceGenerationLogic>();
            Container.Bind<IResourceGenerationLogic>().FromInstance(MockGenerationLogic.Object);

            MockYieldDisplay = new Mock<IResourceSummaryDisplay>();
            Container.Bind<IResourceSummaryDisplay>().WithId("City Yield Display").FromInstance(MockYieldDisplay.Object);

            Container.Bind<CityYieldDisplay>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<CoreSignals>().AsSingle();
            Container.DeclareSignal<SlotDisplayClickedSignal>();
        }

        #endregion

        #region tests

        [   Test(Description = "When Refresh is called and CityToDisplay is null, nothing should happen"),
            TestCaseSource("TestCases")
        ]
        public void OnRefresh_NothingDoneOnNullCityToDisplay(ResourceSummary yieldSummary) {
            var yieldDisplay = Container.Resolve<CityYieldDisplay>();

            yieldDisplay.Refresh();

            MockGenerationLogic.Verify(logic => logic.GetTotalYieldForCity(It.IsAny<ICity>()),
                Times.Never, "ResourceGenerationLogic.GetTotalYieldForCity was called unexpectedly");

            MockYieldDisplay.Verify(display => display.DisplaySummary(It.IsAny<ResourceSummary>()),
                Times.Never, "YieldDisplay.DisplaySummary was called unexpectedly");
        }

        [   Test(Description = "When Refresh is called and CityToDisplay is not null, " +
                "YieldDisplay.DisplaySummary should be called with an argument informed by ResourceGenerationLogic"),
            TestCaseSource("TestCases")
        ]
        public void OnRefresh_YieldDisplayCalledIfCityToDisplayNotNull(ResourceSummary yieldSummary) {
            var yieldDisplay = Container.Resolve<CityYieldDisplay>();

            var mockCity = BuildCity(yieldSummary);

            yieldDisplay.ObjectToDisplay = mockCity.Object;
            yieldDisplay.Refresh();

            MockGenerationLogic.ResetCalls();
            MockYieldDisplay.ResetCalls();

            yieldDisplay.Refresh();

            MockGenerationLogic.Verify(logic => logic.GetTotalYieldForCity(mockCity.Object), Times.Once,
                "ResourceGenerationLogic.GetTotalYieldForCity did not receive the expected call");

            MockYieldDisplay.Verify(display => display.DisplaySummary(yieldSummary), Times.Once,
                "YieldDisplay.DisplaySummary did not receive the expected call");
        }

        #endregion

        #region utilities

        private Mock<ICity> BuildCity(ResourceSummary lastIncome) {
            var mockCity = new Mock<ICity>();
            mockCity.Setup(city => city.LastIncome).Returns(lastIncome);

            MockGenerationLogic
                .Setup(logic => logic.GetTotalYieldForCity(mockCity.Object))
                .Returns(lastIncome);

            return mockCity;
        }

        #endregion

        #endregion

    }

}
