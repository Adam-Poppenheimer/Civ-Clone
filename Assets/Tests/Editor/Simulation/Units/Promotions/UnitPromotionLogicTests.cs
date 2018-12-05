using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Units.Promotions {

    public class UnitPromotionLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private List<IPromotion> NoPromotions = new List<IPromotion>();

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IGlobalPromotionLogic>                         MockGlobalPromotionLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon  = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockGlobalPromotionLogic = new Mock<IGlobalPromotionLogic>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon .Object);
            Container.Bind<IGlobalPromotionLogic>                        ().FromInstance(MockGlobalPromotionLogic.Object);

            Container.Bind<UnitPromotionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetPromotionsForUnit_GetsStartingPromotionsFromUnitTemplate() {
            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var unit = BuildUnit(
                BuildUnitTemplate(promotions), BuildPromotionTree(NoPromotions),
                BuildCiv(NoPromotions)
            );

            var promotionLogic = Container.Resolve<UnitPromotionLogic>();

            CollectionAssert.AreEquivalent(
                promotions, promotionLogic.GetPromotionsForUnit(unit)
            );
        }

        [Test]
        public void GetPromotionsForUnit_GetsPromotionsFromPromotionTree() {
            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var unit = BuildUnit(
                BuildUnitTemplate(NoPromotions), BuildPromotionTree(promotions),
                BuildCiv(NoPromotions)
            );

            var promotionLogic = Container.Resolve<UnitPromotionLogic>();

            CollectionAssert.AreEquivalent(
                promotions, promotionLogic.GetPromotionsForUnit(unit)
            );
        }

        [Test]
        public void GetPromotionsForUnit_GetsGlobalPromotionsOfOwner() {
            var treePromotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var globalPromotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var civ = BuildCiv(globalPromotions);

            var unit = BuildUnit(BuildUnitTemplate(NoPromotions), BuildPromotionTree(treePromotions), civ);

            var promotionLogic = Container.Resolve<UnitPromotionLogic>();

            CollectionAssert.AreEquivalent(
                treePromotions.Concat(globalPromotions), promotionLogic.GetPromotionsForUnit(unit)
            );
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion() {
            return new Mock<IPromotion>().Object;
        }

        private IUnitTemplate BuildUnitTemplate(IEnumerable<IPromotion> startingPromotions) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.StartingPromotions).Returns(startingPromotions);

            return mockTemplate.Object;
        }

        private IPromotionTree BuildPromotionTree(IEnumerable<IPromotion> allPromotions) {
            var mockTree = new Mock<IPromotionTree>();

            mockTree.Setup(tree => tree.GetAllPromotions()).Returns(allPromotions);

            return mockTree.Object;
        }

        private ICivilization BuildCiv(IEnumerable<IPromotion> globalPromotions) {
            var newCiv = new Mock<ICivilization>().Object;

            MockGlobalPromotionLogic.Setup(logic => logic.GetGlobalPromotionsOfCiv(newCiv))
                                    .Returns(globalPromotions);

            return newCiv;
        }

        private IUnit BuildUnit(IUnitTemplate template, IPromotionTree promotionTree, ICivilization owner) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Template)     .Returns(template);
            mockUnit.Setup(unit => unit.PromotionTree).Returns(promotionTree);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
