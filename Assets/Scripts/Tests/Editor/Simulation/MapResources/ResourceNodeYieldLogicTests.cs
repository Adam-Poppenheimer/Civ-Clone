using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.MapResources {

    public class ResourceNodeYieldLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ResourceNodeYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetYieldFromNode_ReturnsEmptyIfResourceNotVisible() {
            var resource = BuildResourceDefinition(
                null, new YieldSummary(food: 1), new YieldSummary(production: 2)
            );

            var node = BuildResourceNode(resource);

            var visibleResources = new List<IResourceDefinition>();

            var nodeYieldLogic = Container.Resolve<ResourceNodeYieldLogic>();

            Assert.AreEqual(
                YieldSummary.Empty,
                nodeYieldLogic.GetYieldFromNode(node, visibleResources, null)
            );
        }

        [Test]
        public void GetYieldFromNode_ReturnsBonusYieldBaseIfResourceVisible() {
            var resource = BuildResourceDefinition(
                null, new YieldSummary(food: 1), new YieldSummary(production: 2)
            );

            var node = BuildResourceNode(resource);

            var visibleResources = new List<IResourceDefinition>() { resource };

            var nodeYieldLogic = Container.Resolve<ResourceNodeYieldLogic>();

            Assert.AreEqual(
                resource.BonusYieldBase,
                nodeYieldLogic.GetYieldFromNode(node, visibleResources, null)
            );
        }

        [Test]
        public void GetYieldFromNode_AndImprovementNotExtractor_ReturnsBonusYieldBase() {
            var extractor = BuildImprovementTemplate();
            var otherTemplate = BuildImprovementTemplate();

            var improvement = BuildImprovement(otherTemplate, true, false);

            var resource = BuildResourceDefinition(
                extractor, new YieldSummary(food: 1), new YieldSummary(production: 2)
            );

            var node = BuildResourceNode(resource);

            var visibleResources = new List<IResourceDefinition>() { resource };

            var nodeYieldLogic = Container.Resolve<ResourceNodeYieldLogic>();

            Assert.AreEqual(
                resource.BonusYieldBase,
                nodeYieldLogic.GetYieldFromNode(node, visibleResources, improvement)
            );
        }

        [Test]
        public void GetYieldFromNode_AndImprovementExtractor_AddsBonusYieldWhenImproved() {
            var extractor = BuildImprovementTemplate();

            var improvement = BuildImprovement(extractor, true, false);

            var resource = BuildResourceDefinition(
                extractor, new YieldSummary(food: 1), new YieldSummary(production: 2)
            );

            var node = BuildResourceNode(resource);

            var visibleResources = new List<IResourceDefinition>() { resource };

            var nodeYieldLogic = Container.Resolve<ResourceNodeYieldLogic>();

            Assert.AreEqual(
                resource.BonusYieldBase + resource.BonusYieldWhenImproved,
                nodeYieldLogic.GetYieldFromNode(node, visibleResources, improvement)
            );
        }

        [Test]
        public void GetYieldFromNode_AndExtractorUnconstructed_ReturnsBonusYieldBase() {
            var extractor = BuildImprovementTemplate();

            var improvement = BuildImprovement(extractor, false, false);

            var resource = BuildResourceDefinition(
                extractor, new YieldSummary(food: 1), new YieldSummary(production: 2)
            );

            var node = BuildResourceNode(resource);

            var visibleResources = new List<IResourceDefinition>() { resource };

            var nodeYieldLogic = Container.Resolve<ResourceNodeYieldLogic>();

            Assert.AreEqual(
                resource.BonusYieldBase,
                nodeYieldLogic.GetYieldFromNode(node, visibleResources, improvement)
            );
        }

        [Test]
        public void GetYieldFromNode_AndExtractorPillaged_ReturnsBonusYieldBase() {
            var extractor = BuildImprovementTemplate();

            var improvement = BuildImprovement(extractor, true, true);

            var resource = BuildResourceDefinition(
                extractor, new YieldSummary(food: 1), new YieldSummary(production: 2)
            );

            var node = BuildResourceNode(resource);

            var visibleResources = new List<IResourceDefinition>() { resource };

            var nodeYieldLogic = Container.Resolve<ResourceNodeYieldLogic>();

            Assert.AreEqual(
                resource.BonusYieldBase,
                nodeYieldLogic.GetYieldFromNode(node, visibleResources, improvement)
            );
        }

        #endregion

        #region utilities

        private IResourceNode BuildResourceNode(IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            return mockNode.Object;
        }

        private IResourceDefinition BuildResourceDefinition(
            IImprovementTemplate extractor, YieldSummary bonusYieldBase,
            YieldSummary bonusYieldWhenImproved
        ) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.Extractor)             .Returns(extractor);
            mockResource.Setup(resource => resource.BonusYieldBase)        .Returns(bonusYieldBase);
            mockResource.Setup(resource => resource.BonusYieldWhenImproved).Returns(bonusYieldWhenImproved);

            return mockResource.Object;
        }

        private IImprovementTemplate BuildImprovementTemplate() {
            return new Mock<IImprovementTemplate>().Object;
        }

        private IImprovement BuildImprovement(
            IImprovementTemplate template, bool isConstructed, bool isPillaged
        ) {
            var mockImprovement = new Mock<IImprovement>();
            
            mockImprovement.Setup(improvement => improvement.Template)     .Returns(template);
            mockImprovement.Setup(improvement => improvement.IsConstructed).Returns(isConstructed);
            mockImprovement.Setup(improvement => improvement.IsPillaged)   .Returns(isPillaged);

            return mockImprovement.Object;
        }

        #endregion

        #endregion

    }

}
