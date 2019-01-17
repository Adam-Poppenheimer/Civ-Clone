using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CommonAttackValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IWarCanon>                                     MockWarCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockWarCanon = new Mock<IWarCanon>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IWarCanon>                                    ().FromInstance(MockWarCanon           .Object);

            Container.Bind<CommonAttackValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void DoesAttackMeetCommonConditions_FalseIfAttackerAndDefenderAreIdentical() {
            var attackerOwner = BuildCiv();
            var defenderOwner = BuildCiv();

            var attacker = BuildUnit(attackerOwner, true);
                           BuildUnit(defenderOwner, true);

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(true);

            var validityLogic = Container.Resolve<CommonAttackValidityLogic>();

            Assert.IsFalse(validityLogic.DoesAttackMeetCommonConditions(attacker, attacker));
        }

        [Test]
        public void DoesAttackMeetCommonConditions_FalseIfAttackerAndDefenderHaveSameOwner() {
            var attackerOwner = BuildCiv();
            var defenderOwner = attackerOwner;

            var attacker = BuildUnit(attackerOwner, true);
            var defender = BuildUnit(defenderOwner, true);

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(true);

            var validityLogic = Container.Resolve<CommonAttackValidityLogic>();

            Assert.IsFalse(validityLogic.DoesAttackMeetCommonConditions(attacker, defender));
        }

        [Test]
        public void DoesAttackMeetCommonConditions_FalseIfAttackerAndDefenderOwnersNotAtWar() {
            var attackerOwner = BuildCiv();
            var defenderOwner = BuildCiv();

            var attacker = BuildUnit(attackerOwner, true);
            var defender = BuildUnit(defenderOwner, true);

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(false);

            var validityLogic = Container.Resolve<CommonAttackValidityLogic>();

            Assert.IsFalse(validityLogic.DoesAttackMeetCommonConditions(attacker, defender));
        }

        [Test]
        public void DoesAttackMeetCommonConditions_FalseIfAttackerCannotAttack() {
            var attackerOwner = BuildCiv();
            var defenderOwner = BuildCiv();

            var attacker = BuildUnit(attackerOwner, false);
            var defender = BuildUnit(defenderOwner, true);

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(true);

            var validityLogic = Container.Resolve<CommonAttackValidityLogic>();

            Assert.IsFalse(validityLogic.DoesAttackMeetCommonConditions(attacker, defender));
        }

        [Test]
        public void DoesAttackerMeetCommonConditions_TrueIfInvalidatingConditionsNotMet() {
            var attackerOwner = BuildCiv();
            var defenderOwner = BuildCiv();

            var attacker = BuildUnit(attackerOwner, true);
            var defender = BuildUnit(defenderOwner, true);

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(true);

            var validityLogic = Container.Resolve<CommonAttackValidityLogic>();

            Assert.IsTrue(validityLogic.DoesAttackMeetCommonConditions(attacker, defender));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IUnit BuildUnit(ICivilization owner, bool canAttack) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CanAttack).Returns(canAttack);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
