using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.MapManagement;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class PromotionTreeComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<IPromotion>             AvailablePromotions = new List<IPromotion>();
        private List<IPromotionTreeTemplate> AvailableTemplates  = new List<IPromotionTreeTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailablePromotions.Clear();
            AvailableTemplates .Clear();

            Container.Bind<IEnumerable<IPromotion>>()
                     .WithId("Available Promotions")
                     .FromInstance(AvailablePromotions);

            Container.Bind<IEnumerable<IPromotionTreeTemplate>>()
                     .WithId("Available Promotion Tree Templates")
                     .FromInstance(AvailableTemplates);

            Container.Bind<PromotionTreeComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ComposePromotionTree_StoresTemplateByName() {
            var promotionTree = BuildPromotionTree(
                BuildPromotionTemplate("Template One"), new List<IPromotion>()
            );

            var composer = Container.Resolve<PromotionTreeComposer>();

            var serialTree = composer.ComposePromotionTree(promotionTree);

            Assert.AreEqual("Template One", serialTree.Template);
        }

        [Test]
        public void ComposePromotionTree_StoresChosenPromotionsByName() {
            var promotionTree = BuildPromotionTree(
                BuildPromotionTemplate("Template One"), new List<IPromotion>() {
                    BuildPromotion("Promotion One"), BuildPromotion("Promotion Two"),
                    BuildPromotion("Promotion Three"),
                }
            );

            var composer = Container.Resolve<PromotionTreeComposer>();

            var serialTree = composer.ComposePromotionTree(promotionTree);

            CollectionAssert.AreEquivalent(
                new List<string>() { "Promotion One", "Promotion Two", "Promotion Three" },
                serialTree.ChosenPromotions
            );
        }

        [Test]
        public void DecomposePromotionTree_ConstructsOnProperTemplateAndChosenPromotions() {
            BuildPromotionTemplate("Template One");
            var templateTwo = BuildPromotionTemplate("Template Two");

            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");
            BuildPromotion("Promotion Four");

            var serialTree = new SerializablePromotionTreeData() {
                Template = "Template Two", ChosenPromotions = new List<string>() {
                    "Promotion One", "Promotion Two", "Promotion Three"
                }
            };

            var composer = Container.Resolve<PromotionTreeComposer>();

            var promotionTree = composer.DecomposePromotionTree(serialTree);

            Assert.AreEqual(templateTwo, promotionTree.Template, "PromotionTree has an unexpected template");

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionOne, promotionTwo, promotionThree },
                promotionTree.GetChosenPromotions(),
                "PromotionTree has an unexpected collection of chosen promotions"
            );
        }

        #endregion

        #region utilities

        private IPromotionTree BuildPromotionTree(
            IPromotionTreeTemplate template, IEnumerable<IPromotion> chosenPromotions
        ){
            var mockTree = new Mock<IPromotionTree>();

            mockTree.Setup(tree => tree.Template)             .Returns(template);
            mockTree.Setup(tree => tree.GetChosenPromotions()).Returns(chosenPromotions);

            return mockTree.Object;
        }

        private IPromotionTreeTemplate BuildPromotionTemplate(string name) {
            var mockTemplate = new Mock<IPromotionTreeTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.name).Returns(name);

            var newTemplate = mockTemplate.Object;

            AvailableTemplates.Add(newTemplate);

            return newTemplate;
        }

        private IPromotion BuildPromotion(string name) {
            var mockPromotion = new Mock<IPromotion>();

            mockPromotion.Name = name;
            mockPromotion.Setup(promotion => promotion.name).Returns(name);

            var newPromotion = mockPromotion.Object;

            AvailablePromotions.Add(newPromotion);

            return newPromotion;
        }

        #endregion

        #endregion

    }

}
