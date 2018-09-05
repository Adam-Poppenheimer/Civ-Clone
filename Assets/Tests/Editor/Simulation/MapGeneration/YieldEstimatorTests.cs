using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.MapGeneration;

namespace Assets.Tests.Simulation.MapGeneration {

    public class YieldEstimatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IInherentCellYieldLogic>                          MockInherentYieldLogic;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodeLocationCanon;
        private Mock<IResourceNodeYieldLogic>                          MockNodeYieldLogic;
        private Mock<IImprovementYieldLogic>                           MockImprovementYieldLogic;
        private Mock<IFreshWaterCanon>                                 MockFreshWaterCanon;
        private Mock<ICellYieldFromBuildingsLogic>                     MockYieldFromBuildingsLogic;
        private Mock<IImprovementEstimator>                            MockImprovementEstimator;

        private List<IResourceDefinition>  AvailableResources    = new List<IResourceDefinition>();
        private List<ITechDefinition>      AvailableTechs        = new List<ITechDefinition>();
        private List<IBuildingTemplate>    AvailableBuildings    = new List<IBuildingTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockInherentYieldLogic      = new Mock<IInherentCellYieldLogic>();
            MockNodeLocationCanon       = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockNodeYieldLogic          = new Mock<IResourceNodeYieldLogic>();
            MockImprovementYieldLogic   = new Mock<IImprovementYieldLogic>();
            MockFreshWaterCanon         = new Mock<IFreshWaterCanon>();
            MockYieldFromBuildingsLogic = new Mock<ICellYieldFromBuildingsLogic>();
            MockImprovementEstimator    = new Mock<IImprovementEstimator>();

            AvailableResources   .Clear();
            AvailableTechs       .Clear();
            AvailableBuildings   .Clear();

            Container.Bind<IInherentCellYieldLogic>                         ().FromInstance(MockInherentYieldLogic      .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodeLocationCanon       .Object);
            Container.Bind<IResourceNodeYieldLogic>                         ().FromInstance(MockNodeYieldLogic          .Object);
            Container.Bind<IImprovementYieldLogic>                          ().FromInstance(MockImprovementYieldLogic   .Object);
            Container.Bind<IFreshWaterCanon>                                ().FromInstance(MockFreshWaterCanon         .Object);
            Container.Bind<ICellYieldFromBuildingsLogic>                    ().FromInstance(MockYieldFromBuildingsLogic .Object);
            Container.Bind<IImprovementEstimator>                           ().FromInstance(MockImprovementEstimator    .Object);

            var mockTechCanon = new Mock<ITechCanon>();
            mockTechCanon.Setup(canon => canon.AvailableTechs).Returns(AvailableTechs.AsReadOnly());

            Container.Bind<ITechCanon>().FromInstance(mockTechCanon.Object);
            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableBuildings);
            Container.Bind<IEnumerable<IResourceDefinition>>().WithId("Available Resources").FromInstance(AvailableResources);

            Container.Bind<YieldEstimator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetYieldEstimateForCell_AlwaysIncludesOneExtraScience() {
            var cell = BuildCell();

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(science: 1f), yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
        }

        [Test]
        public void GetYieldEstimateForCell_AddsInherentYield() {
            var cell = BuildCell();

            MockInherentYieldLogic.Setup(logic => logic.GetInherentCellYield(cell, false))
                                  .Returns(new YieldSummary(food: 1, production: 20));

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 20, science: 1),
                yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
        }

        [Test]
        public void GetYieldEstimateForCell_IgnoresVegetationIfExpectedImprovementRequestsIt() {
            var cell = BuildCell();

            var improvement = BuildImprovementTemplate(clearsVegetationWhenBuilt: true);

            MockImprovementEstimator.Setup(
                estimator => estimator.GetExpectedImprovementForCell(cell, It.IsAny<IResourceNode>())
            ).Returns(improvement);

            MockInherentYieldLogic.Setup(logic => logic.GetInherentCellYield(cell, true))
                                  .Returns(new YieldSummary(food: 1, production: 20));

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 20, science: 1),
                yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
        }

        [Test]
        public void GetYieldEstimateForCell_AddsYieldFromNodeIfNodeNotNull() {
            var cell = BuildCell();

            var node = BuildResourceNode(cell);

            var improvementTemplate = BuildImprovementTemplate(clearsVegetationWhenBuilt: false);

            MockImprovementEstimator.Setup(
                estimator => estimator.GetExpectedImprovementForCell(cell, node)
            ).Returns(improvementTemplate);

            MockNodeYieldLogic.Setup(logic => logic.GetYieldFromNode(node, AvailableResources, It.IsAny<IImprovement>()))
                              .Returns(new YieldSummary(food: 1, production: 20));

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 20, science: 1),
                yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
        }

        [Test]
        public void GetYieldEstimateForCell_IgnoresYieldFromNodeIfNodeNull() {
            var cell = BuildCell();

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(science: 1),
                yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );

            MockNodeYieldLogic.Verify(
                logic => logic.GetYieldFromNode(
                    It.IsAny<IResourceNode>(), It.IsAny<IEnumerable<IResourceDefinition>>(),
                    It.IsAny<IImprovement>()
                ),
                Times.Never
            );
        }

        [Test]
        public void GetYieldEstimateForCell_AddsYieldFromExpectedImprovementIfItIsntNull() {
            var cell = BuildCell();

            var improvement = BuildImprovementTemplate(false);

            MockImprovementEstimator.Setup(
                estimator => estimator.GetExpectedImprovementForCell(cell, It.IsAny<IResourceNode>())
            ).Returns(improvement);

            MockFreshWaterCanon.Setup(canon => canon.HasAccessToFreshWater(cell)).Returns(true);

            MockImprovementYieldLogic.Setup(
                logic => logic.GetYieldOfImprovementTemplate(
                    improvement, It.IsAny<IResourceNode>(), AvailableResources,
                    AvailableTechs, true
                )
            ).Returns(new YieldSummary(food: 1, production: 20));

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 20, science: 1),
                yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
        }

        [Test]
        public void GetYieldEstimateForCell_IgnoresYieldFromExpectedImprovementIfItIsNull() {
            var cell = BuildCell();

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(science: 1f), yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
            
            MockImprovementYieldLogic.Verify(
                logic => logic.GetYieldOfImprovementTemplate(
                    It.IsAny<IImprovementTemplate>(), It.IsAny<IResourceNode>(),
                    It.IsAny<IEnumerable<IResourceDefinition>>(),
                    It.IsAny<IEnumerable<ITechDefinition>>(), It.IsAny<bool>()
                ),
                Times.Never
            );
        }

        [Test]
        public void GetYieldEstimateForCell_AddsYieldFromBuildings() {
            var cell = BuildCell();

            MockYieldFromBuildingsLogic.Setup(
                logic => logic.GetBonusCellYieldFromBuildings(cell, AvailableBuildings)
            ).Returns(new YieldSummary(food: 1, production: 20));

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 20, science: 1),
                yieldEstimator.GetYieldEstimateForCell(cell, AvailableTechs)
            );
        }

        [Test]
        public void GetYieldEstimateForResource_ReturnsBaseYieldPlusImprovedYield() {
            var resource = BuildResourceDefinition(
                bonusYieldBase: new YieldSummary(food: 1, production: 20),
                bonusYieldWhenImproved: new YieldSummary(production: 22, gold: 300)
            );

            var yieldEstimator = Container.Resolve<YieldEstimator>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 42, gold: 300),
                yieldEstimator.GetYieldEstimateForResource(resource)
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IImprovementTemplate BuildImprovementTemplate(bool clearsVegetationWhenBuilt) {
            var mockImprovement = new Mock<IImprovementTemplate>();

            mockImprovement.Setup(template => template.ClearsVegetationWhenBuilt).Returns(clearsVegetationWhenBuilt);

            return mockImprovement.Object;
        }

        private IResourceNode BuildResourceNode(IHexCell location) {
            var newNode = new Mock<IResourceNode>().Object;

            MockNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        private IResourceDefinition BuildResourceDefinition(
            YieldSummary bonusYieldBase, YieldSummary bonusYieldWhenImproved
        ) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.BonusYieldBase)        .Returns(bonusYieldBase);
            mockResource.Setup(resource => resource.BonusYieldWhenImproved).Returns(bonusYieldWhenImproved);

            return mockResource.Object;
        }

        #endregion

        #endregion

    }

}
