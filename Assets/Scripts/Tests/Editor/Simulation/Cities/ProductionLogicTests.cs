using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class ProductionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IYieldGenerationLogic>                         MockGenerationLogic;
        private Mock<ICityConfig>                                   MockConfig;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ICityModifiers>                                MockCityModifiers;
        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGenerationLogic         = new Mock<IYieldGenerationLogic>();
            MockConfig                  = new Mock<ICityConfig>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityModifiers           = new Mock<ICityModifiers>();
            MockSocialPolicyCanon       = new Mock<ISocialPolicyCanon>();

            Container.Bind<IYieldGenerationLogic>                        ().FromInstance(MockGenerationLogic        .Object);
            Container.Bind<ICityConfig>                                  ().FromInstance(MockConfig                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<ICityModifiers>                               ().FromInstance(MockCityModifiers          .Object);
            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon      .Object);

            Container.Bind<ProductionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetProductionProgressPerTurnOnProject_DefaultsToProductionYield() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city    = BuildCity();
            var project = BuildProject();

            var civ = BuildCiv(city);

            var totalYield = new YieldSummary(food: 5, gold: 12, production: 13);
            MockGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(city, It.IsAny<YieldSummary>())).Returns(totalYield);

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(civ)).Returns(new List<ISocialPolicyBonusesData>());

            Assert.AreEqual(
                totalYield[YieldType.Production], productionLogic.GetProductionProgressPerTurnOnProject(city, project),
                "GetProductionProgressPerTurnOnProject returned an unexpected value"
            );
        }

        [Test]
        public void GetProductionProgressPerTurnOnProject_ChangedByBuildingProductionModifiers() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = BuildCity(
                BuildBuilding(BuildBuildingTemplate(BuildModifier(2f, true))),
                BuildBuilding(BuildBuildingTemplate(BuildModifier(3f, true))),
                BuildBuilding(BuildBuildingTemplate(BuildModifier(4f, false)))
            );
            var project = BuildProject();

            BuildCiv(city);

            var totalYield = new YieldSummary(production: 10);
            MockGenerationLogic.Setup(
                logic => logic.GetTotalYieldForCity(city, It.Is<YieldSummary>(summary => summary[YieldType.Production] == 2f + 3f))
            ).Returns(totalYield);

            Assert.AreEqual(10, productionLogic.GetProductionProgressPerTurnOnProject(city, project));
        }

        [Test]
        public void GetProductionProgressPerTurnOnProject_ChangedByProductionModifiersOfPolicyBonuses() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = BuildCity();
            var project = BuildProject();

            var civ = BuildCiv(city);

            var policyBonuses = new List<ISocialPolicyBonusesData>() {
                BuildBonusData(BuildModifier(2f, true)),
                BuildBonusData(BuildModifier(3f, true)),
                BuildBonusData(BuildModifier(4f, false)),
            };

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(civ)).Returns(policyBonuses);

            var totalYield = new YieldSummary(production: 10);
            MockGenerationLogic.Setup(
                logic => logic.GetTotalYieldForCity(city, It.Is<YieldSummary>(summary => summary[YieldType.Production] == 2f + 3f))
            ).Returns(totalYield);

            Assert.AreEqual(10, productionLogic.GetProductionProgressPerTurnOnProject(city, project));
        }

        [Test(Description = "When GetGoldCostToHurryProject is called, it returns the production left to " +
            "complete the project times Config.HurryGoldPerProduction, rounded down")]
        public void GetGoldCostToHurryProject_DeterminedByProjectAndConfig() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;

            var projectMock = new Mock<IProductionProject>();
            projectMock.SetupGet(project => project.ProductionToComplete).Returns(100);
            projectMock.SetupGet(project => project.Progress).Returns(27);

            MockConfig.SetupGet(config => config.HurryCostPerProduction).Returns(3.15f);

            Assert.AreEqual(229, productionLogic.GetGoldCostToHurryProject(city, projectMock.Object), 
                "GetGoldCostToHurryProject returned an unexpected value");
        }

        [Test(Description = "All methods should throw NullArgumentExceptions whenever they are passed any null argument")]
        public void AllMethods_ThrowOnNullArguments() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;
            var project = new Mock<IProductionProject>().Object;

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetGoldCostToHurryProject(null, project),
                "GetGoldCostToHurryProject fails to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetGoldCostToHurryProject(city, null),
                "GetGoldCostToHurryProject fails to throw on a null project argument");

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetProductionProgressPerTurnOnProject(null, project),
                "GetProductionProgressPerTurnOnProject fails to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetProductionProgressPerTurnOnProject(city, null),
                "GetProductionProgressPerTurnOnProject fails to throw on a null project argument");
        }

        #endregion

        #region utilities

        private IProductionProject BuildProject() {
            return new Mock<IProductionProject>().Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var newCity = new Mock<ICity>().Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private ICivilization BuildCiv(params ICity[] cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        private IBuildingTemplate BuildBuildingTemplate(IProductionModifier modifier) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.ProductionModifier).Returns(modifier);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private ISocialPolicyBonusesData BuildBonusData(IProductionModifier modifier) {
            var mockBonuses = new Mock<ISocialPolicyBonusesData>();

            mockBonuses.Setup(bonuses => bonuses.ProductionModifier).Returns(modifier);

            return mockBonuses.Object;
        }

        private IProductionModifier BuildModifier(float modifierValue, bool doesApply) {
            var mockModifier = new Mock<IProductionModifier>();

            mockModifier.Setup(modifier => modifier.Value).Returns(modifierValue);
            mockModifier.Setup(modifier => modifier.DoesModifierApply(It.IsAny<IProductionProject>(), It.IsAny<ICity>())).Returns(doesApply);

            return mockModifier.Object;
        }

        #endregion

        #endregion

    }

}
