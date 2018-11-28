using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    public class ConditionalCombatModifierTests : ZenjectUnitTestFixture {

        #region instance methods

        #region tests

        [Test]
        public void JoinTypeOr_NoConditionsMet_ReturnsFalse() {
            var subject  = BuildUnit();
            var opponent = BuildUnit();
            var location = BuildHexCell();

            var combatModifier = new ConditionalCombatModifier() {
                JoinedTogetherBy = ConditionalCombatModifier.JoinType.Or,
                Conditions = new List<CombatCondition>() {
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Melee
                    },
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Melee
                    },
                }
            };

            Assert.IsFalse(combatModifier.DoesModifierApply(subject, opponent, location, CombatType.Ranged));
        }

        [Test]
        public void JoinTypeOr_OneConditionMet_ReturnsTrue() {
            var subject  = BuildUnit();
            var opponent = BuildUnit();
            var location = BuildHexCell();

            var combatModifier = new ConditionalCombatModifier() {
                JoinedTogetherBy = ConditionalCombatModifier.JoinType.Or,
                Conditions = new List<CombatCondition>() {
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    },
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Melee
                    },
                }
            };

            Assert.IsTrue(combatModifier.DoesModifierApply(subject, opponent, location, CombatType.Ranged));
        }

        [Test]
        public void JoinTypeOr_AllConditionsMet_ReturnsTrue() {
            var subject  = BuildUnit();
            var opponent = BuildUnit();
            var location = BuildHexCell();

            var combatModifier = new ConditionalCombatModifier() {
                JoinedTogetherBy = ConditionalCombatModifier.JoinType.Or,
                Conditions = new List<CombatCondition>() {
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    },
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    },
                }
            };

            Assert.IsTrue(combatModifier.DoesModifierApply(subject, opponent, location, CombatType.Ranged));
        }

        [Test]
        public void JoinTypeAnd_NoConditionsMet_ReturnsFalse() {
            var subject  = BuildUnit();
            var opponent = BuildUnit();
            var location = BuildHexCell();

            var combatModifier = new ConditionalCombatModifier() {
                JoinedTogetherBy = ConditionalCombatModifier.JoinType.And,
                Conditions = new List<CombatCondition>() {
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Melee
                    },
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Melee
                    },
                }
            };

            Assert.IsFalse(combatModifier.DoesModifierApply(subject, opponent, location, CombatType.Ranged));
        }

        [Test]
        public void JoinTypeAnd_OneConditionMet_ReturnsFalse() {
            var subject  = BuildUnit();
            var opponent = BuildUnit();
            var location = BuildHexCell();

            var combatModifier = new ConditionalCombatModifier() {
                JoinedTogetherBy = ConditionalCombatModifier.JoinType.And,
                Conditions = new List<CombatCondition>() {
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Melee
                    },
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    },
                }
            };

            Assert.IsFalse(combatModifier.DoesModifierApply(subject, opponent, location, CombatType.Ranged));
        }

        [Test]
        public void JoinTypeAnd_AllConditionsMet_ReturnsTrue() {
            var subject  = BuildUnit();
            var opponent = BuildUnit();
            var location = BuildHexCell();

            var combatModifier = new ConditionalCombatModifier() {
                JoinedTogetherBy = ConditionalCombatModifier.JoinType.And,
                Conditions = new List<CombatCondition>() {
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    },
                    new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    },
                }
            };

            Assert.IsTrue(combatModifier.DoesModifierApply(subject, opponent, location, CombatType.Ranged));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private IHexCell BuildHexCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
