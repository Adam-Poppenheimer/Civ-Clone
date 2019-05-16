using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    public class ProductionModifierTests : ZenjectUnitTestFixture {

        #region instance methods

        #region tests

        [Test]
        public void JoinTypeOr_NoConditionsMet_ReturnsFalse() {
            var project = BuildProject(BuildUnitTemplate(UnitType.Melee));
            var city    = BuildCity();

            var productionModifier = new ProductionModifier() {
                JoinedTogetherBy = ProductionModifier.JoinType.Or,
                Conditions = new List<ProductionCondition>() {
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Archery }
                    },
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Archery }
                    },
                }
            };

            Assert.IsFalse(productionModifier.DoesModifierApply(project, city));
        }

        [Test]
        public void JoinTypeOr_OneConditionMet_ReturnsTrue() {
            var project = BuildProject(BuildUnitTemplate(UnitType.Melee));
            var city    = BuildCity();

            var productionModifier = new ProductionModifier() {
                JoinedTogetherBy = ProductionModifier.JoinType.Or,
                Conditions = new List<ProductionCondition>() {
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee }
                    },
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Archery }
                    },
                }
            };

            Assert.IsTrue(productionModifier.DoesModifierApply(project, city));
        }

        [Test]
        public void JoinTypeOr_AllConditionsMet_ReturnsTrue() {
            var project = BuildProject(BuildUnitTemplate(UnitType.Melee));
            var city    = BuildCity();

            var productionModifier = new ProductionModifier() {
                JoinedTogetherBy = ProductionModifier.JoinType.Or,
                Conditions = new List<ProductionCondition>() {
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee }
                    },
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee }
                    },
                }
            };

            Assert.IsTrue(productionModifier.DoesModifierApply(project, city));
        }

        [Test]
        public void JoinTypeAnd_NoConditionsMet_ReturnsFalse() {
            var project = BuildProject(BuildUnitTemplate(UnitType.Melee));
            var city    = BuildCity();

            var productionModifier = new ProductionModifier() {
                JoinedTogetherBy = ProductionModifier.JoinType.And,
                Conditions = new List<ProductionCondition>() {
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Archery }
                    },
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Archery }
                    },
                }
            };

            Assert.IsFalse(productionModifier.DoesModifierApply(project, city));
        }

        [Test]
        public void JoinTypeAnd_OneConditionMet_ReturnsFalse() {
            var project = BuildProject(BuildUnitTemplate(UnitType.Melee));
            var city    = BuildCity();

            var productionModifier = new ProductionModifier() {
                JoinedTogetherBy = ProductionModifier.JoinType.And,
                Conditions = new List<ProductionCondition>() {
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee }
                    },
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Archery }
                    },
                }
            };

            Assert.IsFalse(productionModifier.DoesModifierApply(project, city));
        }

        [Test]
        public void JoinTypeAnd_AllConditionsMet_ReturnsTrue() {
            var project = BuildProject(BuildUnitTemplate(UnitType.Melee));
            var city    = BuildCity();

            var productionModifier = new ProductionModifier() {
                JoinedTogetherBy = ProductionModifier.JoinType.And,
                Conditions = new List<ProductionCondition>() {
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee }
                    },
                    new ProductionCondition() {
                        Target             = ProductionCondition.TargetType.Project,
                        Restriction        = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee }
                    },
                }
            };

            Assert.IsTrue(productionModifier.DoesModifierApply(project, city));
        }

        #endregion

        #region utilities

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private IUnitTemplate BuildUnitTemplate(UnitType type) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.Type).Returns(type);

            return mockTemplate.Object;
        }

        private IProductionProject BuildProject(IUnitTemplate unitTemplate) {
            var mockProject = new Mock<IProductionProject>();

            mockProject.Setup(project => project.UnitToConstruct).Returns(unitTemplate);

            return mockProject.Object;
        }

        #endregion

        #endregion

    }

}
