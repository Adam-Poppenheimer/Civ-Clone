using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.SocialPolicies {

    [TestFixture]
    public class SocialPolicyCostLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetCostOfNextPolicyForCivTestData {

            public int CityCount;            
            public int PolicyCount;

            public CivConfigTestData Config = new CivConfigTestData();

            public  List<SocialPolicyBonusesDataTestData> SocialPolicyBonuses =
                new List<SocialPolicyBonusesDataTestData>();

        }

        public class CivConfigTestData {

            public int   BasePolicyCost;
            public float PolicyCostPerPolicyCoefficient;
            public float PolicyCostPerPolicyExponent;
            public float PolicyCostPerCityCoefficient;

        }

        public class SocialPolicyBonusesDataTestData {

            public float PolicyCostFromCityCountModifier;

        }

        #endregion

        #region static fields and properties

        private static CivConfigTestData DefaultData = new CivConfigTestData() {
            BasePolicyCost                 = 25,
            PolicyCostPerPolicyCoefficient = 21.03f,
            PolicyCostPerPolicyExponent    = 1.7f,
            PolicyCostPerCityCoefficient   = 0.3f,            
        };

        public static IEnumerable GetCostOfNextPolicyForCivTestCases {
            get {
                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 1, PolicyCount = 0
                }).SetName("1 city and 0 policies").Returns(25);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 1, PolicyCount = 1
                }).SetName("1 city and 1 policy").Returns(45);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 1, PolicyCount = 2
                }).SetName("1 city and 2 policies").Returns(90);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 1, PolicyCount = 3
                }).SetName("1 city and 3 policies").Returns(160);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 2, PolicyCount = 1
                }).SetName("2 cities and 1 policy").Returns(55);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 3, PolicyCount = 1
                }).SetName("3 cities and 1 policy").Returns(70);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 4, PolicyCount = 1
                }).SetName("4 cities and 1 policy").Returns(85);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 4, PolicyCount = 2
                }).SetName("4 cities and 2 policies").Returns(175);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 4, PolicyCount = 3
                }).SetName("4 cities and 3 policies").Returns(305);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 10, PolicyCount = 10
                }).SetName("10 cities and 10 policies").Returns(3990);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 7, PolicyCount = 12
                }).SetName("7 cities and 12 policies").Returns(4090);

                yield return new TestCaseData(new GetCostOfNextPolicyForCivTestData() {
                    Config = DefaultData,
                    CityCount = 10, PolicyCount = 10,
                    SocialPolicyBonuses = new List<SocialPolicyBonusesDataTestData>() {
                        new SocialPolicyBonusesDataTestData() { PolicyCostFromCityCountModifier = -0.3f },
                        new SocialPolicyBonusesDataTestData() { PolicyCostFromCityCountModifier = -0.2f },
                    }
                }).SetName("10 cities and 10 policies, city cost modifier of -0.5 from policies").Returns(1995);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;
        private Mock<ICivilizationConfig>                           MockCivConfig;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockSocialPolicyCanon   = new Mock<ISocialPolicyCanon>();
            MockCivConfig           = new Mock<ICivilizationConfig>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon  .Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig          .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<SocialPolicyCostLogic>().AsSingle();
        }

        private void SetUpConfig(CivConfigTestData configData) {
            MockCivConfig.Setup(config => config.BasePolicyCost)                .Returns(configData.BasePolicyCost);
            MockCivConfig.Setup(config => config.PolicyCostPerPolicyCoefficient).Returns(configData.PolicyCostPerPolicyCoefficient);
            MockCivConfig.Setup(config => config.PolicyCostPerPolicyExponent)   .Returns(configData.PolicyCostPerPolicyExponent);
            MockCivConfig.Setup(config => config.PolicyCostPerCityCoefficient)  .Returns(configData.PolicyCostPerCityCoefficient);
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetCostOfNextPolicyForCivTestCases")]
        public int GetCostOfNextPolicyForCivTests(GetCostOfNextPolicyForCivTestData testData) {
            SetUpConfig(testData.Config);

            var civ = BuildCivilization();

            var cities = new List<ICity>();

            for(int i = 0; i < testData.CityCount; i++) {
                cities.Add(BuildCity());
            }

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(civ)).Returns(cities);

            var policies = new List<ISocialPolicyDefinition>();

            for(int i = 0; i < testData.PolicyCount; i++) {
                policies.Add(BuildPolicy());
            }

            var policyBonuses = testData.SocialPolicyBonuses.Select(bonusData => BuildSocialPolicyBonuses(bonusData));

            MockSocialPolicyCanon.Setup(canon => canon.GetPoliciesUnlockedFor(civ)).Returns(policies);
            
            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(civ)).Returns(policyBonuses);

            var costLogic = Container.Resolve<SocialPolicyCostLogic>();

            return costLogic.GetCostOfNextPolicyForCiv(civ);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private ISocialPolicyDefinition BuildPolicy() {
            return new Mock<ISocialPolicyDefinition>().Object;
        }

        private ISocialPolicyBonusesData BuildSocialPolicyBonuses(SocialPolicyBonusesDataTestData testData) {
            var mockBonuses = new Mock<ISocialPolicyBonusesData>();

            mockBonuses.Setup(bonuses => bonuses.PolicyCostFromCityCountModifier)
                       .Returns(testData.PolicyCostFromCityCountModifier);

            return mockBonuses.Object;
        }

        #endregion

        #endregion

    }

}
