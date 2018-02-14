using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class TileResourceLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct GetYieldOfCellTestData {

            public TerrainType Terrain;
            public TerrainFeature Feature;

            public ResourceNodeData ResourceNodeOnCell;

            public ImprovementData ImprovementOnCell;

        }

        public class ResourceNodeData {

            public ResourceSummary BonusYield;

        }

        public class ImprovementData {

            public ResourceSummary BonusYield;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable<TestCaseData> TestCases {
            get {
                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/no feature/no resource").Returns(new ResourceSummary(1f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Plains,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = null
                }).SetName("Plains/no feature/no resource").Returns(new ResourceSummary(1.1f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Desert,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = null
                }).SetName("Desert/no feature/no resource").Returns(new ResourceSummary(1.2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Rock,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = null
                }).SetName("Rock/no feature/no resource").Returns(new ResourceSummary(1.3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Snow,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = null
                }).SetName("Snow/no feature/no resource").Returns(new ResourceSummary(1.4f));




                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Plains,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = null
                }).SetName("Plains/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Desert,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = null
                }).SetName("Desert/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Rock,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = null
                }).SetName("Rock/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Snow,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = null
                }).SetName("Snow/Forest/no resource").Returns(new ResourceSummary(2f));



                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(0f, 2f)
                    }
                }).SetName("Grassland/no feature/Has a resource").Returns(new ResourceSummary(1f, 2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(0f, 2f)
                    }
                }).SetName("Grassland/Forest/Has a resource").Returns(new ResourceSummary(2f, 2f));




                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    ImprovementOnCell = new ImprovementData() {
                        BonusYield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/no feature/Has an improvement").Returns(new ResourceSummary(1f, 0f, 3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    ImprovementOnCell = new ImprovementData() {
                        BonusYield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/Forest/Has an improvement").Returns(new ResourceSummary(2f, 0f, 3f));


                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(0f, 2f)
                    },
                    ImprovementOnCell = new ImprovementData() {
                        BonusYield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/no feature/Has a resource and an improvement").Returns(new ResourceSummary(1f, 2f, 3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(0f, 2f)
                    },
                    ImprovementOnCell = new ImprovementData() {
                        BonusYield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/Forest/Has a resource and an improvement").Returns(new ResourceSummary(2f, 2f, 3f));
            }
        }

        private static List<ResourceSummary> TerrainYields = new List<ResourceSummary>() {
            new ResourceSummary(1f),
            new ResourceSummary(1.1f),
            new ResourceSummary(1.2f),
            new ResourceSummary(1.3f),
            new ResourceSummary(1.4f),
        };

        private static List<ResourceSummary> FeatureYields = new List<ResourceSummary>() {
            new ResourceSummary(0f),
            new ResourceSummary(2f)
        };

        #endregion

        #region instance fields and properties

        private Mock<IHexGridConfig> MockConfig;

        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;

        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                   = new Mock<IHexGridConfig>();
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            MockConfig.Setup(config => config.TerrainYields).Returns(TerrainYields.AsReadOnly());
            MockConfig.Setup(config => config.FeatureYields).Returns(FeatureYields.AsReadOnly());

            Container.Bind<IHexGridConfig>                                  ().FromInstance(MockConfig                  .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<TileResourceLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetYieldOfCell should return the yield of the cell's terrain " +
            "if it has no feature, and the yield of the feature if it does. If the cell has a " +
            "ResourceNode, it should also add that resource's bonus yield to the result returned.")]
        [TestCaseSource("TestCases")]
        public ResourceSummary GetYieldOfCellTests(GetYieldOfCellTestData data) {
            var cell = BuildCell(data.Terrain, data.Feature);

            if(data.ResourceNodeOnCell != null) {
                BuildResourceNode(cell, data.ResourceNodeOnCell.BonusYield);
            }

            if(data.ImprovementOnCell != null) {
                BuildImprovement(cell, data.ImprovementOnCell.BonusYield);
            }

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            return resourceLogic.GetYieldOfCell(cell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(TerrainType terrain, TerrainFeature feature) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain = terrain;
            newCell.Feature = feature;

            return newCell;
        }

        private IResourceNode BuildResourceNode(IHexCell location, ResourceSummary bonusYield) {
            var mockNode = new Mock<IResourceNode>();

            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Setup(resource => resource.BonusYield).Returns(bonusYield);

            mockNode.Setup(node => node.Resource).Returns(mockDefinition.Object);

            var newNode = mockNode.Object;

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        private IImprovement BuildImprovement(IHexCell location, ResourceSummary bonusYield) {
            var mockImprovement = new Mock<IImprovement>();

            var mockTemplate = new Mock<IImprovementTemplate>();
            mockTemplate.Setup(template => template.BonusYield).Returns(bonusYield);

            mockImprovement.Setup(improvement => improvement.Template).Returns(mockTemplate.Object);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });
            
            return newImprovement;
        }

        #endregion

        #endregion

    }

}
