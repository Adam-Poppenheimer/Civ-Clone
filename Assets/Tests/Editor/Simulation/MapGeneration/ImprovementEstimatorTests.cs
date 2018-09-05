using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapGeneration;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Tests.Simulation.MapGeneration {

    public class ImprovementEstimatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IFreshWaterCanon>          MockFreshWaterCanon;
        private Mock<IImprovementValidityLogic> MockImprovementValidityLogic;
        private Mock<IImprovementYieldLogic>    MockImprovementYieldLogic;
        private Mock<IMapScorer>                MockMapScorer;

        private List<IResourceDefinition>  AvailableResources    = new List<IResourceDefinition>();
        private List<ITechDefinition>      AvailableTechs        = new List<ITechDefinition>();
        private List<IImprovementTemplate> AvailableImprovements = new List<IImprovementTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableResources   .Clear();
            AvailableTechs       .Clear();
            AvailableImprovements.Clear();

            MockFreshWaterCanon          = new Mock<IFreshWaterCanon>();
            MockImprovementValidityLogic = new Mock<IImprovementValidityLogic>();
            MockImprovementYieldLogic    = new Mock<IImprovementYieldLogic>();
            MockMapScorer                = new Mock<IMapScorer>();

            Container.Bind<IFreshWaterCanon>         ().FromInstance(MockFreshWaterCanon         .Object);
            Container.Bind<IImprovementValidityLogic>().FromInstance(MockImprovementValidityLogic.Object);
            Container.Bind<IImprovementYieldLogic>   ().FromInstance(MockImprovementYieldLogic   .Object);
            Container.Bind<IMapScorer>               ().FromInstance(MockMapScorer               .Object);

            var mockTechCanon = new Mock<ITechCanon>();

            mockTechCanon.Setup(canon => canon.AvailableTechs).Returns(AvailableTechs.AsReadOnly());

            Container.Bind<ITechCanon>().FromInstance(mockTechCanon.Object);

            Container.Bind<IEnumerable<IResourceDefinition>> ().WithId("Available Resources")            .FromInstance(AvailableResources);
            Container.Bind<IEnumerable<IImprovementTemplate>>().WithId("Available Improvement Templates").FromInstance(AvailableImprovements);

            Container.Bind<ImprovementEstimator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetExpectedImprovementForCell_ReturnsExtractorOfNodeAtCellIfOneExists() {
            var cell = BuildHexCell(false);

            var improvement = BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3));

            var resource = BuildResourceDefinition(improvement);
            var node = BuildResourceNode(resource);

            var improvementEstimator = Container.Resolve<ImprovementEstimator>();

            Assert.AreEqual(improvement, improvementEstimator.GetExpectedImprovementForCell(cell, node));
        }

        [Test]
        public void GetExpectedImprovementForCell_IgnoresNodeAtCellIfNullExtractor() {
            var cell = BuildHexCell(false);

            BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3));

            var resource = BuildResourceDefinition(null);
            var node = BuildResourceNode(resource);

            var improvementEstimator = Container.Resolve<ImprovementEstimator>();

            Assert.IsNull(improvementEstimator.GetExpectedImprovementForCell(cell, node));
        }

        [Test]
        public void GetExpectedImprovementForCell_ReturnsImprovementWhoseYieldHasTheHighestScore() {
            var cell = BuildHexCell(false);

                                   BuildImprovementTemplate(new YieldSummary(food: 1, gold: 1), cell);
                                   BuildImprovementTemplate(new YieldSummary(food: 2, gold: 1), cell);
            var improvementThree = BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3), cell);

            MockMapScorer.Setup(scorer => scorer.GetScoreOfYield(It.IsAny<YieldSummary>()))
                         .Returns<YieldSummary>(yield => yield.Total);

            var resource = BuildResourceDefinition(null);
            var node = BuildResourceNode(resource);

            var improvementEstimator = Container.Resolve<ImprovementEstimator>();

            Assert.AreEqual(
                improvementThree, improvementEstimator.GetExpectedImprovementForCell(cell, node)
            );
        }

        [Test]
        public void GetExpectedImprovementForCell_IgnoresImprovementsThatAreInvalidForCell() {
            var cell = BuildHexCell(false);

            var improvementOne = BuildImprovementTemplate(new YieldSummary(food: 1, gold: 1), cell);
                                 BuildImprovementTemplate(new YieldSummary(food: 2, gold: 1));
                                 BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3));

            MockMapScorer.Setup(scorer => scorer.GetScoreOfYield(It.IsAny<YieldSummary>()))
                         .Returns<YieldSummary>(yield => yield.Total);

            var resource = BuildResourceDefinition(null);
            var node = BuildResourceNode(resource);

            var improvementEstimator = Container.Resolve<ImprovementEstimator>();

            Assert.AreEqual(
                improvementOne, improvementEstimator.GetExpectedImprovementForCell(cell, node)
            );
        }

        [Test]
        public void GetExpectedImprovementForCell_ReturnsNullIfNoValidImprovement() {
            var cell = BuildHexCell(false);

            BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3));
            BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3));
            BuildImprovementTemplate(new YieldSummary(food: 1, gold: 3));

            var improvementEstimator = Container.Resolve<ImprovementEstimator>();

            Assert.IsNull(improvementEstimator.GetExpectedImprovementForCell(cell, null));
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(bool hasAccessToFreshWater) {
            var newCell = new Mock<IHexCell>().Object;

            MockFreshWaterCanon.Setup(canon => canon.HasAccessToFreshWater(newCell)).Returns(hasAccessToFreshWater);

            return newCell;
        }

        private IResourceNode BuildResourceNode(IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            return mockNode.Object;
        }

        private IResourceDefinition BuildResourceDefinition(IImprovementTemplate extractor) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.Extractor).Returns(extractor);

            var newResource = mockResource.Object;

            AvailableResources.Add(newResource);

            return newResource;
        }

        private IImprovementTemplate BuildImprovementTemplate(
            YieldSummary yield, params IHexCell[] validCells
        ) {
            var newTemplate = new Mock<IImprovementTemplate>().Object;

            MockImprovementYieldLogic.Setup(
                logic => logic.GetYieldOfImprovementTemplate(
                    newTemplate, It.IsAny<IResourceNode>(), AvailableResources, AvailableTechs,
                    It.IsAny<bool>()
                )
            ).Returns(yield);

            foreach(var cell in validCells) {
                MockImprovementValidityLogic.Setup(
                    logic => logic.IsTemplateValidForCell(newTemplate, cell, true)
                ).Returns(true);
            }

            AvailableImprovements.Add(newTemplate);

            return newTemplate;
        }

        #endregion

        #endregion

    }

}
