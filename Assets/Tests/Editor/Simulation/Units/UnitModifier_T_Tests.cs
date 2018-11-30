using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Tests.Simulation.Units {

    public class UnitModifier_T_Tests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockSocialPolicyCanon = new Mock<ISocialPolicyCanon>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon  .Object);
        }

        #endregion

        #region tests

        [Test]
        public void GetValueForUnit_PullsFromBonusesOfOwner() {
            var civ = BuildCiv(
                BuildBonuses(), BuildBonuses(), BuildBonuses()
            );

            var unit = BuildUnit(civ);

            var modifier = new UnitModifier<int>(
                new UnitModifier<int>.ExtractionData() {
                    PolicyBonusesExtractor = bonuses => 1,
                    Aggregator = (a, b) => a + b,
                    UnitaryValue = 1
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(4, modifier.GetValueForUnit(unit));
        }

        #endregion

        #region utilities

        private ISocialPolicyBonusesData BuildBonuses() {
            return new Mock<ISocialPolicyBonusesData>().Object;
        }

        private ICivilization BuildCiv(params ISocialPolicyBonusesData[] policyBonuses) {
            var newCiv = new Mock<ICivilization>().Object;

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(newCiv)).Returns(policyBonuses);
            
            return newCiv;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
