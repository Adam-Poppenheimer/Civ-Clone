using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Cities {

    public class LocalPromotionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<LocalPromotionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetLocalPromotionsOfCity_ReturnsLocalPromotionsOfAllBuildings() {
            var promotionOne   = BuildPromotion();
            var promotionTwo   = BuildPromotion();
            var promotionThree = BuildPromotion();

            var buildingOne = BuildBuilding(BuildTemplate(promotionOne));
            var buildingTwo = BuildBuilding(BuildTemplate(promotionTwo, promotionThree));

            var city = BuildCity(buildingOne, buildingTwo);

            var promotionLogic = Container.Resolve<LocalPromotionLogic>();

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionOne, promotionTwo, promotionThree },
                promotionLogic.GetLocalPromotionsForCity(city)
            );
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion() {
            return new Mock<IPromotion>().Object;
        }

        private IBuildingTemplate BuildTemplate(params IPromotion[] localPromotions) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.LocalPromotions).Returns(localPromotions);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var newCity = new Mock<ICity>().Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        #endregion

        #endregion

    }

}
