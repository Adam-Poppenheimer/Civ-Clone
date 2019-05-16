using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Civilizations {

    public class GlobalPromotionLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static List<ICity>                   NoCities      = new List<ICity>();
        private static List<ISocialPolicyDefinition> NoPolicies    = new List<ISocialPolicyDefinition>();
        private static List<IPolicyTreeDefinition>   NoPolicyTrees = new List<IPolicyTreeDefinition>();
        private static List<IPromotion>              NoPromotions  = new List<IPromotion>();

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockSocialPolicyCanon       = new Mock<ISocialPolicyCanon>();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon      .Object);

            Container.Bind<GlobalPromotionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetGlobalPromotionsOfCiv_DefaultsToEmptyCollection() {
            var civ = BuildCiv(NoCities, NoPolicies, NoPolicyTrees);

            var promotionLogic = Container.Resolve<GlobalPromotionLogic>();

            CollectionAssert.IsEmpty(promotionLogic.GetGlobalPromotionsOfCiv(civ));
        }

        [Test]
        public void GetGlobalPromotionsOfCiv_IncludesGlobalPromotionsOfAllOwnedBuildings() {
            var promotionsOne   = new List<IPromotion>() { BuildPromotion() };
            var promotionsTwo   = new List<IPromotion>() { BuildPromotion(), BuildPromotion() };
            var promotionsThree = new List<IPromotion>() { BuildPromotion(), BuildPromotion(), BuildPromotion() };
            var promotionsFour  = new List<IPromotion>() { BuildPromotion(), BuildPromotion() };

            var cities = new List<ICity>() {
                BuildCity(BuildBuilding(BuildBuildingTemplate(promotionsOne))),
                BuildCity(BuildBuilding(BuildBuildingTemplate(promotionsTwo))),
                BuildCity(BuildBuilding(BuildBuildingTemplate(promotionsThree)), BuildBuilding(BuildBuildingTemplate(promotionsFour))),
            };

            var civ = BuildCiv(cities, NoPolicies, NoPolicyTrees);
            
            var promotionLogic = Container.Resolve<GlobalPromotionLogic>();

            CollectionAssert.AreEquivalent(
                promotionsOne.Concat(promotionsTwo).Concat(promotionsThree).Concat(promotionsFour),
                promotionLogic.GetGlobalPromotionsOfCiv(civ)
            );
        }

        [Test]
        public void GetGlobalPromotionsOfCiv_IncludesGlobalPromotionsOfUnlockedPolicies() {
            var promotionsOne   = new List<IPromotion>();
            var promotionsTwo   = new List<IPromotion>() { BuildPromotion() };
            var promotionsThree = new List<IPromotion>() { BuildPromotion(), BuildPromotion() };

            var policies = new List<ISocialPolicyDefinition>() {
                BuildPolicy(BuildBonuses(promotionsOne)), BuildPolicy(BuildBonuses(promotionsTwo)),
                BuildPolicy(BuildBonuses(promotionsThree))
            };

            var civ = BuildCiv(NoCities, policies, NoPolicyTrees);
            
            var promotionLogic = Container.Resolve<GlobalPromotionLogic>();

            CollectionAssert.AreEquivalent(
                promotionsOne.Concat(promotionsTwo).Concat(promotionsThree),
                promotionLogic.GetGlobalPromotionsOfCiv(civ)
            );
        }

        [Test]
        public void GetGlobalPromotionsOfCiv_IncludesGlobalPromotionsFromUnlockingBonusesOfUnlockedPolicyTrees() {
            var promotionsOne   = new List<IPromotion>();
            var promotionsTwo   = new List<IPromotion>() { BuildPromotion() };
            var promotionsThree = new List<IPromotion>() { BuildPromotion(), BuildPromotion() };

            var policyTrees = new List<IPolicyTreeDefinition>() {
                BuildPolicyTree(BuildBonuses(promotionsOne),   null, false),
                BuildPolicyTree(BuildBonuses(promotionsTwo),   null, false),
                BuildPolicyTree(BuildBonuses(promotionsThree), null, false)
            };

            var civ = BuildCiv(NoCities, NoPolicies, policyTrees);
            
            var promotionLogic = Container.Resolve<GlobalPromotionLogic>();

            CollectionAssert.AreEquivalent(
                promotionsOne.Concat(promotionsTwo).Concat(promotionsThree),
                promotionLogic.GetGlobalPromotionsOfCiv(civ)
            );
        }

        [Test]
        public void GetGlobalPromotionsOfCiv_IncludesGlobalPromotionsFromCompletionBonusesOfCompletedPolicyTrees() {
            var promotionsOne   = new List<IPromotion>();
            var promotionsTwo   = new List<IPromotion>() { BuildPromotion() };
            var promotionsThree = new List<IPromotion>() { BuildPromotion(), BuildPromotion() };

            var policyTrees = new List<IPolicyTreeDefinition>() {
                BuildPolicyTree(BuildBonuses(NoPromotions), BuildBonuses(promotionsOne),   true),
                BuildPolicyTree(BuildBonuses(NoPromotions), BuildBonuses(promotionsTwo),   true),
                BuildPolicyTree(BuildBonuses(NoPromotions), BuildBonuses(promotionsThree), false)
            };

            var civ = BuildCiv(NoCities, NoPolicies, policyTrees);
            
            var promotionLogic = Container.Resolve<GlobalPromotionLogic>();

            CollectionAssert.AreEquivalent(
                promotionsOne.Concat(promotionsTwo),
                promotionLogic.GetGlobalPromotionsOfCiv(civ)
            );
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion() {
            return new Mock<IPromotion>().Object;
        }

        private IBuildingTemplate BuildBuildingTemplate(IEnumerable<IPromotion> globalPromotions) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.GlobalPromotions).Returns(globalPromotions);

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

        private ISocialPolicyBonusesData BuildBonuses(IEnumerable<IPromotion> promotions) {
            var mockBonuses = new Mock<ISocialPolicyBonusesData>();

            mockBonuses.Setup(bonuses => bonuses.GlobalPromotions).Returns(promotions);

            return mockBonuses.Object;
        }

        private ISocialPolicyDefinition BuildPolicy(ISocialPolicyBonusesData bonuses) {
            var mockPolicy = new Mock<ISocialPolicyDefinition>();

            mockPolicy.Setup(policy => policy.Bonuses).Returns(bonuses);

            return mockPolicy.Object;
        }

        private IPolicyTreeDefinition BuildPolicyTree(
            ISocialPolicyBonusesData unlockingBonuses, ISocialPolicyBonusesData completionBonuses,
            bool isCompleted
        ) {
            var mockTree = new Mock<IPolicyTreeDefinition>();

            mockTree.Setup(tree => tree.UnlockingBonuses) .Returns(unlockingBonuses);
            mockTree.Setup(tree => tree.CompletionBonuses).Returns(completionBonuses);

            var newTree = mockTree.Object;

            MockSocialPolicyCanon.Setup(
                canon => canon.IsTreeCompletedByCiv(newTree, It.IsAny<ICivilization>())
            ).Returns(isCompleted);

            return newTree;
        }

        private ICivilization BuildCiv(
            IEnumerable<ICity> cities, IEnumerable<ISocialPolicyDefinition> unlockedPolicies,
            IEnumerable<IPolicyTreeDefinition> unlockedPolicyTrees
        ) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            MockSocialPolicyCanon.Setup(canon => canon.GetPoliciesUnlockedFor(newCiv)).Returns(unlockedPolicies);
            MockSocialPolicyCanon.Setup(canon => canon.GetTreesUnlockedFor   (newCiv)).Returns(unlockedPolicyTrees);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
