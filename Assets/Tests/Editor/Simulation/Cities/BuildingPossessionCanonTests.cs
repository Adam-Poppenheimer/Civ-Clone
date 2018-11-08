﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.MapResources;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingPossessionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IResourceLockingCanon>                         MockResourceLockingCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IGlobalPromotionCanon>                         MockGlobalPromotionCanon;
        private CitySignals                                         CitySignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourceLockingCanon = new Mock<IResourceLockingCanon>();
            MockCityPossessionCanon  = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockGlobalPromotionCanon = new Mock<IGlobalPromotionCanon>();
            CitySignals              = new CitySignals();

            Container.Bind<IResourceLockingCanon>                        ().FromInstance(MockResourceLockingCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon .Object);
            Container.Bind<IGlobalPromotionCanon>                        ().FromInstance(MockGlobalPromotionCanon.Object);
            Container.Bind<CitySignals>                                  ().FromInstance(CitySignals);

            Container.Bind<BuildingPossessionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When a new possession relationship is established, " +
            "BuildingPossessionCanon reserves all required specialty resources " +
            "marked by the building's template. It does this for the civilization " +
            "that now owns the city")]
        public void OnPossessionEstablished_RequiredSpecialtyResourcesReserved() {
            var resourceOne   = BuildResourceDefinition("Resource One");
            var resourceTwo   = BuildResourceDefinition("Resource Two");
            var resourceThree = BuildResourceDefinition("Resource Three");

            var building = BuildBuilding(BuildTemplate(resourceOne, resourceTwo));

            var civilization = BuildCivilization();

            var city = BuildCity(civilization);

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);

            MockResourceLockingCanon.Verify(
                canon => canon.LockCopyOfResourceForCiv(resourceOne, civilization),
                Times.Once, "Resource One was not locked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.LockCopyOfResourceForCiv(resourceTwo, civilization),
                Times.Once, "Resource Two was not locked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.LockCopyOfResourceForCiv(resourceThree, civilization),
                Times.Never, "Resource Three was locked unexpectedly"
            );
        }

        [Test]
        public void OnPossessionEstablished_GlobalPromotionsAdded() {
            var promotionOne   = BuildPromotion();
            var promotionTwo   = BuildPromotion();
            var promotionThree = BuildPromotion();

            var promotions = new List<IPromotion>() {
                promotionOne, promotionTwo, promotionThree
            };

            var building = BuildBuilding(BuildTemplate(promotions));

            var civ = BuildCivilization();

            var city = BuildCity(civ);

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);

            MockGlobalPromotionCanon.Verify(
                canon => canon.AddGlobalPromotionToCiv(promotionOne, civ),
                Times.Once, "PromotionOne not added as expected"
            );

            MockGlobalPromotionCanon.Verify(
                canon => canon.AddGlobalPromotionToCiv(promotionTwo, civ),
                Times.Once, "PromotionTwo not added as expected"
            );

            MockGlobalPromotionCanon.Verify(
                canon => canon.AddGlobalPromotionToCiv(promotionThree, civ),
                Times.Once, "PromotionThree not added as expected"
            );
        }

        [Test]
        public void OnPossessionEstablished_CityGainedBuildingSignalFired() {
            var building = BuildBuilding(BuildTemplate());

            var city = BuildCity();

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            CitySignals.CityGainedBuildingSignal.Subscribe(delegate(Tuple<ICity, IBuilding> data) {
                Assert.AreEqual(city,     data.Item1, "Incorrect city passed");
                Assert.AreEqual(building, data.Item2, "Incorrect building passed");

                Assert.Pass();
            });

            possessionCanon.ChangeOwnerOfPossession(building, city);

            Assert.Fail("CityGainedBuildingSignal wasn't fired");
        }

        [Test(Description = "When an existing possession relationship is broken, " +
            "BuildingPossessionCanon unreserves all required specialty resource " +
            "marked by the building's template. It does this for the civilization " +
            "that used to own the city")]
        public void OnPossessionBroken_RequiredSpecialtyResourcesUnreserved() {
            var resourceOne   = BuildResourceDefinition("Resource One");
            var resourceTwo   = BuildResourceDefinition("Resource Two");
            var resourceThree = BuildResourceDefinition("Resource Three");

            var building = BuildBuilding(BuildTemplate(resourceOne, resourceTwo));

            var civilization = BuildCivilization();

            var city = BuildCity(civilization);

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);
            possessionCanon.ChangeOwnerOfPossession(building, null);

            MockResourceLockingCanon.Verify(
                canon => canon.UnlockCopyOfResourceForCiv(resourceOne, civilization),
                Times.Once, "Resource One was not unlocked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.UnlockCopyOfResourceForCiv(resourceTwo, civilization),
                Times.Once, "Resource Two was not unlocked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.UnlockCopyOfResourceForCiv(resourceThree, civilization),
                Times.Never, "Resource Three was unlocked unexpectedly"
            );
        }

        [Test]
        public void OnPossessionBroken_GlobalPromotionsRemoved() {
            var promotionOne   = BuildPromotion();
            var promotionTwo   = BuildPromotion();
            var promotionThree = BuildPromotion();

            var promotions = new List<IPromotion>() {
                promotionOne, promotionTwo, promotionThree
            };

            var building = BuildBuilding(BuildTemplate(promotions));

            var civ = BuildCivilization();

            var city = BuildCity(civ);

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);
            possessionCanon.ChangeOwnerOfPossession(building, null);

            MockGlobalPromotionCanon.Verify(
                canon => canon.RemoveGlobalPromotionFromCiv(promotionOne, civ),
                Times.Once, "PromotionOne not removed as expected"
            );

            MockGlobalPromotionCanon.Verify(
                canon => canon.RemoveGlobalPromotionFromCiv(promotionTwo, civ),
                Times.Once, "PromotionTwo not removed as expected"
            );

            MockGlobalPromotionCanon.Verify(
                canon => canon.RemoveGlobalPromotionFromCiv(promotionThree, civ),
                Times.Once, "PromotionThree not removed as expected"
            );
        }

        [Test]
        public void OnPossessionBroken_CityLostBuildingSignalFired() {
            var building = BuildBuilding(BuildTemplate());

            var city = BuildCity();

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);

            CitySignals.CityLostBuildingSignal.Subscribe(delegate(Tuple<ICity, IBuilding> data) {
                Assert.AreEqual(city,     data.Item1, "Incorrect city passed");
                Assert.AreEqual(building, data.Item2, "Incorrect building passed");

                Assert.Pass();
            });

            possessionCanon.ChangeOwnerOfPossession(building, null);

            Assert.Fail("CityLostBuildingSignal wasn't fired");
        }

        #endregion

        #region utilities

        private ICity BuildCity(ICivilization owner = null) {
            var newCity = new Mock<ICity>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private IBuildingTemplate BuildTemplate(params IResourceDefinition[] requiredResources) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.ResourcesConsumed).Returns(requiredResources);

            return mockTemplate.Object;
        }

        private IBuildingTemplate BuildTemplate(IEnumerable<IPromotion> globalPromotions) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.GlobalPromotions).Returns(globalPromotions);

            return mockTemplate.Object;
        }

        private IResourceDefinition BuildResourceDefinition(string name = "") {
            var mockDefinition = new Mock<IResourceDefinition>();

            mockDefinition.Name = name;
            mockDefinition.Setup(definition => definition.name).Returns(name);

            return mockDefinition.Object;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IPromotion BuildPromotion() {
            return new Mock<IPromotion>().Object;
        }

        #endregion

        #endregion

    }

}
