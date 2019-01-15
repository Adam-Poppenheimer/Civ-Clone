using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.AI;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.AI {

    public class ImprovementInfluenceSourceTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;
        private Mock<IHexGrid>                                         MockGrid;
        private Mock<ICivilizationTerritoryLogic>                      MockCivTerritoryLogic;
        private Mock<IAIConfig>                                        MockAIConfig;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodeLocationCanon;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockGrid                     = new Mock<IHexGrid>();
            MockCivTerritoryLogic        = new Mock<ICivilizationTerritoryLogic>();
            MockAIConfig                 = new Mock<IAIConfig>();
            MockNodeLocationCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IHexGrid>                                        ().FromInstance(MockGrid                    .Object);
            Container.Bind<ICivilizationTerritoryLogic>                     ().FromInstance(MockCivTerritoryLogic       .Object);
            Container.Bind<IAIConfig>                                       ().FromInstance(MockAIConfig                .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodeLocationCanon       .Object);

            Container.Bind<ImprovementInfluenceSource>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ApplyToMaps_AppliesRoadPillageValue_ToCellsWithRoads() {
            BuildCell(false, null, null);
            BuildCell(true,  null, null);
            BuildCell(true,  null, null);
            BuildCell(false, null, null);

            MockAIConfig.Setup(config => config.RoadPillageValue).Returns(10f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[4]
            };

            var targetCiv = BuildCiv(new List<IHexCell>());

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            influenceSource.ApplyToMaps(maps, targetCiv);

            CollectionAssert.AreEqual(
                new float[] { 0f, 10f, 10f, 0f }, maps.PillagingValue
            );
        }

        [Test]
        public void ApplyToMaps_AppliesNormalImprovementPillageValue_ToCellsWithNonExtractingImprovements() {
            var extractingTemplate    = BuildImprovementTemplate();
            var nonExtractingTemplate = BuildImprovementTemplate();

            var resourceOne = BuildResource(extractingTemplate);
            var resourceTwo = BuildResource(null);

            BuildCell(false, BuildImprovement(nonExtractingTemplate), null);
            BuildCell(false, BuildImprovement(nonExtractingTemplate), BuildNode(resourceOne));
            BuildCell(false, BuildImprovement(extractingTemplate),    null);
            BuildCell(false, BuildImprovement(extractingTemplate),    BuildNode(resourceOne));
            BuildCell(false, BuildImprovement(extractingTemplate),    BuildNode(resourceTwo));

            MockAIConfig.Setup(config => config.NormalImprovementPillageValue).Returns(10f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[5]
            };

            var targetCiv = BuildCiv(new List<IHexCell>());

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            influenceSource.ApplyToMaps(maps, targetCiv);

            CollectionAssert.AreEqual(
                new float[] { 10f, 10f, 10f, 0f, 10f }, maps.PillagingValue
            );
        }

        [Test]
        public void ApplyToMaps_AppliesExtractingImprovementPillageValue_ToCellsWithExtractingImprovements() {
            var extractingTemplate    = BuildImprovementTemplate();
            var nonExtractingTemplate = BuildImprovementTemplate();

            var resourceOne = BuildResource(extractingTemplate);
            var resourceTwo = BuildResource(null);

            BuildCell(false, BuildImprovement(nonExtractingTemplate), null);
            BuildCell(false, BuildImprovement(nonExtractingTemplate), BuildNode(resourceOne));
            BuildCell(false, BuildImprovement(extractingTemplate),    null);
            BuildCell(false, BuildImprovement(extractingTemplate),    BuildNode(resourceOne));
            BuildCell(false, BuildImprovement(extractingTemplate),    BuildNode(resourceTwo));

            MockAIConfig.Setup(config => config.ExtractingImprovementPillageValue).Returns(10f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[5]
            };

            var targetCiv = BuildCiv(new List<IHexCell>());

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            influenceSource.ApplyToMaps(maps, targetCiv);

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 0f, 10f, 0f }, maps.PillagingValue
            );
        }

        [Test]
        public void ApplyToMaps_IgnoresCellsClaimedByTargetCiv() {
            var cellsClaimedByTargetCiv = new List<IHexCell>() {
                BuildCell(true, null, null),
                BuildCell(true, null, null)
            };
            
            var cellsClaimedByForeignCiv = new List<IHexCell>() {
                BuildCell(true, null, null)
            };
            
            BuildCell(true, null, null);

            var targetCiv = BuildCiv(cellsClaimedByTargetCiv);
                            BuildCiv(cellsClaimedByForeignCiv);

            MockAIConfig.Setup(config => config.RoadPillageValue).Returns(10f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[4]
            };

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            influenceSource.ApplyToMaps(maps, targetCiv);

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 10f, 10f }, maps.PillagingValue
            );
        }

        [Test]
        public void ApplyToMaps_IgnoresPillagedImprovements() {
            var unpillagedImprovement = BuildImprovement(BuildImprovementTemplate(), false);
            var pillagedImprovement   = BuildImprovement(BuildImprovementTemplate(), true);

            BuildCell(false, unpillagedImprovement, null);
            BuildCell(false, pillagedImprovement,   null);
            BuildCell(true,  pillagedImprovement,   null);

            var targetCiv = BuildCiv(new List<IHexCell>());

            MockAIConfig.Setup(config => config.RoadPillageValue)             .Returns(10f);
            MockAIConfig.Setup(config => config.NormalImprovementPillageValue).Returns(20f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[3]
            };

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            influenceSource.ApplyToMaps(maps, targetCiv);

            CollectionAssert.AreEqual(
                new float[] { 20f, 0f, 10f }, maps.PillagingValue
            );
        }

        [Test]
        public void ApplyToMaps_AndMapsNull_ThrowsArgumentNullException() {
            var targetCiv = BuildCiv(new List<IHexCell>());

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            Assert.Throws<ArgumentNullException>(() => influenceSource.ApplyToMaps(null, targetCiv));
        }

        [Test]
        public void ApplyToMaps_AndTargetCivNull_ThrowsArgumentNullException() {
            var maps = new InfluenceMaps();

            var influenceSource = Container.Resolve<ImprovementInfluenceSource>();

            Assert.Throws<ArgumentNullException>(() => influenceSource.ApplyToMaps(maps, null));
        }

        #endregion

        #region utilities

        private IImprovementTemplate BuildImprovementTemplate() {
            return new Mock<IImprovementTemplate>().Object;
        }

        private IResourceDefinition BuildResource(IImprovementTemplate extractor) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.Extractor).Returns(extractor);

            return mockResource.Object;
        }

        private IImprovement BuildImprovement(IImprovementTemplate template, bool isPillaged = false) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template)  .Returns(template);
            mockImprovement.Setup(improvement => improvement.IsPillaged).Returns(isPillaged);

            return mockImprovement.Object;
        }

        private IResourceNode BuildNode(IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            return mockNode.Object;
        }

        private IHexCell BuildCell(bool hasRoads, IImprovement improvement, IResourceNode resourceNode) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.HasRoads).Returns(hasRoads);
            mockCell.Setup(cell => cell.Index)   .Returns(AllCells.Count);

            var newCell = mockCell.Object;

            var improvements  = improvement  != null ? new List<IImprovement> () { improvement  } : new List<IImprovement>();
            var resourceNodes = resourceNode != null ? new List<IResourceNode>() { resourceNode } : new List<IResourceNode>();

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(improvements);
            MockNodeLocationCanon       .Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(resourceNodes);

            AllCells.Add(newCell);

            return newCell;
        }

        private ICivilization BuildCiv(IEnumerable<IHexCell> cells) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var cell in cells) {
                MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(cell)).Returns(newCiv);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
