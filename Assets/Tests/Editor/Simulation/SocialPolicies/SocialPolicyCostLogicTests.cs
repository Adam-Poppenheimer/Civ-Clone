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

        }

        public class CivConfigTestData {

            public int   BasePolicyCost;
            public float PolicyCostPerPolicyCoefficient;
            public float PolicyCostPerPolicyExponent;
            public float PolicyCostPerCityCoefficient;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetCostOfNexPolicyForCivTestCases {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;
        private Mock<ICivilizationConfig>                           MockCivConfig;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private List<ICity>                   AllCities   = new List<ICity>();
        private List<ISocialPolicyDefinition> AllPolicies = new List<ISocialPolicyDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities  .Clear();
            AllPolicies.Clear();

            MockSocialPolicyCanon   = new Mock<ISocialPolicyCanon>();
            MockCivConfig           = new Mock<ICivilizationConfig>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            MockSocialPolicyCanon.Setup(canon => canon.GetPoliciesUnlockedFor(It.IsAny<ICivilization>()))
                                 .Returns(AllPolicies);

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICivilization>()))
                                   .Returns(AllCities);

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
        public void GetCostOfNexPolicyForCivTests() {
            Assert.Ignore(
                "Formula is currently a facsimile of that used in Civ 5, nor is the specific number " + 
                "this class returns particularly important. There are no clearly productive test cases"
            );
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private ICity BuildCity() {
            var newCity = new Mock<ICity>().Object;

            AllCities.Add(newCity);

            return newCity;
        }

        private ISocialPolicyDefinition BuildPolicy() {
            var newPolicy = new Mock<ISocialPolicyDefinition>().Object;

            AllPolicies.Add(newPolicy);

            return newPolicy;
        }

        #endregion

        #endregion

    }

}
