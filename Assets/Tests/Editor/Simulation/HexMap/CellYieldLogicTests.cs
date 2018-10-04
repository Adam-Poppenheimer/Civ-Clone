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
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class CellYieldLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>        MockBuildingPossessionCanon;
        private Mock<ITechCanon>                                       MockTechCanon;
        private Mock<IFreshWaterLogic>                                 MockFreshWaterCanon;

        private Mock<IInherentCellYieldLogic>                          MockInherentCellYieldLogic;
        private Mock<IResourceNodeYieldLogic>                          MockResourceNodeYieldLogic;
        private Mock<IImprovementYieldLogic>                           MockImprovementYieldLogic;
        private Mock<ICellYieldFromBuildingsLogic>                     MockBuildingYieldLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingPossessionCanon  = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockTechCanon                = new Mock<ITechCanon>();
            MockFreshWaterCanon          = new Mock<IFreshWaterLogic>();

            MockInherentCellYieldLogic   = new Mock<IInherentCellYieldLogic>();
            MockResourceNodeYieldLogic   = new Mock<IResourceNodeYieldLogic>();
            MockImprovementYieldLogic    = new Mock<IImprovementYieldLogic>();
            MockBuildingYieldLogic       = new Mock<ICellYieldFromBuildingsLogic>();

            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>       ().FromInstance(MockBuildingPossessionCanon .Object);
            Container.Bind<ITechCanon>                                      ().FromInstance(MockTechCanon               .Object);
            Container.Bind<IFreshWaterLogic>                                ().FromInstance(MockFreshWaterCanon         .Object);

            Container.Bind<IInherentCellYieldLogic>                         ().FromInstance(MockInherentCellYieldLogic  .Object);
            Container.Bind<IResourceNodeYieldLogic>                         ().FromInstance(MockResourceNodeYieldLogic  .Object);
            Container.Bind<IImprovementYieldLogic>                          ().FromInstance(MockImprovementYieldLogic   .Object);
            Container.Bind<ICellYieldFromBuildingsLogic>                    ().FromInstance(MockBuildingYieldLogic      .Object);

            Container.Bind<CellYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetYieldOfCell_AlwaysCallsIntoInherentYield() {
            var cell = BuildCell();

            var cellYield = new YieldSummary(food: 1, production: 2, gold: 3);

            MockInherentCellYieldLogic.Setup(logic => logic.GetInherentCellYield(cell, false)).Returns(cellYield);

            var yieldLogic = Container.Resolve<CellYieldLogic>();

            Assert.AreEqual(cellYield, yieldLogic.GetYieldOfCell(cell, null));

            MockInherentCellYieldLogic.Verify(logic => logic.GetInherentCellYield(cell, false), Times.Once);
        }

        [Test]
        public void GetYieldOfCell_CallsIntoBuildingYieldIfCellBelongsToCity() {
            var cell = BuildCell();

            var buildings = new List<IBuilding>() {
                BuildBuilding(), BuildBuilding(), BuildBuilding()
            };

            var buildingTemplates = buildings.Select(building => building.Template);

            BuildCity(cell, buildings);

            var buildingYield = new YieldSummary(food: 1, production: 2, gold: 3);

            MockBuildingYieldLogic.Setup(
                logic => logic.GetBonusCellYieldFromBuildings(cell, It.IsAny<IEnumerable<IBuildingTemplate>>())
            ).Returns(buildingYield);

            var yieldLogic = Container.Resolve<CellYieldLogic>();

            Assert.AreEqual(buildingYield, yieldLogic.GetYieldOfCell(cell, null));

            MockBuildingYieldLogic.Verify(
                logic => logic.GetBonusCellYieldFromBuildings(
                    cell,
                    It.Is<IEnumerable<IBuildingTemplate>>(
                        templates => templates.SequenceEqual(buildingTemplates)
                    )
                ),
                Times.Once
            );
        }

        [Test]
        public void GetYieldOfCell_IgnoresBuildingYieldIfCellDoesNotBelongToCity() {
            var cell = BuildCell();

            var yieldLogic = Container.Resolve<CellYieldLogic>();

            Assert.AreEqual(YieldSummary.Empty, yieldLogic.GetYieldOfCell(cell, null));
            
            MockBuildingYieldLogic.Verify(
                logic => logic.GetBonusCellYieldFromBuildings(
                    It.IsAny<IHexCell>(), It.IsAny<IEnumerable<IBuildingTemplate>>()
                ),
                Times.Never
            );
        }

        [Test]
        public void GetYieldOfCell_CallsIntoNodeYieldIfCellHasNode() {
            var cell = BuildCell();

            var node = BuildResourceNode(cell);

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                 .Returns(new List<IResourceNode>() { node });

            var visibleResources = new List<IResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition(), BuildResourceDefinition(),
            };

            var civ = BuildCivilization(visibleResources, new List<ITechDefinition>());

            var improvement = BuildImprovement(cell);

            var nodeYield = new YieldSummary(food: 5, production: 4, gold: 3);

            MockResourceNodeYieldLogic.Setup(
                logic => logic.GetYieldFromNode(node, It.IsAny<IEnumerable<IResourceDefinition>>(), improvement)
            ).Returns(nodeYield);

            var yieldLogic = Container.Resolve<CellYieldLogic>();

            Assert.AreEqual(nodeYield, yieldLogic.GetYieldOfCell(cell, civ));

            MockResourceNodeYieldLogic.Verify(
                logic => logic.GetYieldFromNode(
                    node,
                    It.Is<IEnumerable<IResourceDefinition>>(resources => resources.SequenceEqual(visibleResources)),
                    improvement
                ),
                Times.Once
            );
        }

        [Test]
        public void GetYieldOfCell_IgnoresNodeYieldIfCellDoesNotHaveNode() {
            var cell = BuildCell();

            var civ = BuildCivilization(new List<IResourceDefinition>(), new List<ITechDefinition>());

            var yieldLogic = Container.Resolve<CellYieldLogic>();

            yieldLogic.GetYieldOfCell(cell, civ);

            MockResourceNodeYieldLogic.Verify(
                logic => logic.GetYieldFromNode(
                    It.IsAny<IResourceNode>(), It.IsAny<IEnumerable<IResourceDefinition>>(),
                    It.IsAny<IImprovement>()
                ),
                Times.Never
            );
        }

        [Test]
        public void GetYieldOfCell_CallsIntoImprovementYieldIfCellHasImprovement() {
            var cell = BuildCell(true);

            var visibleResources = new List<IResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition()
            };

            var discoveredTechs = new List<ITechDefinition>() {
                BuildTechDefinition(), BuildTechDefinition(),
            };

            var civ = BuildCivilization(visibleResources, discoveredTechs);

            var node = BuildResourceNode(cell);

            var improvement = BuildImprovement(cell);

            var improvementYield = new YieldSummary(food: 5, production: 4, gold: 3);

            MockImprovementYieldLogic.Setup(
                logic => logic.GetYieldOfImprovement(
                    improvement, node, It.IsAny<IEnumerable<IResourceDefinition>>(),
                    It.IsAny<IEnumerable<ITechDefinition>>(), It.IsAny<bool>()
                )
            ).Returns(improvementYield);


            var yieldLogic = Container.Resolve<CellYieldLogic>();

            Assert.AreEqual(improvementYield, yieldLogic.GetYieldOfCell(cell, civ));

            MockImprovementYieldLogic.Verify(
                logic => logic.GetYieldOfImprovement(
                    improvement, node,
                    It.Is<IEnumerable<IResourceDefinition>>(
                        resources => resources.SequenceEqual(visibleResources)
                    ),
                    It.Is<IEnumerable<ITechDefinition>>(
                        techs => techs.SequenceEqual(discoveredTechs)
                    ),
                    true
                ),
                Times.Once
            );
        }

        [Test]
        public void GetYieldOfCell_IgnoresImprovementYieldIfCellDoesNotHaveImprovement() {
            var cell = BuildCell();

            var civ = BuildCivilization(new List<IResourceDefinition>(), new List<ITechDefinition>());

            var yieldLogic = Container.Resolve<CellYieldLogic>();

            yieldLogic.GetYieldOfCell(cell, civ);

            MockImprovementYieldLogic.Verify(
                logic => logic.GetYieldOfImprovement(
                    It.IsAny<IImprovement>(), It.IsAny<IResourceNode>(),
                    It.IsAny<IEnumerable<IResourceDefinition>>(),
                    It.IsAny<IEnumerable<ITechDefinition>>(), It.IsAny<bool>()
                ),
                Times.Never
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(bool hasFreshWater = false) {
            var newCell = new Mock<IHexCell>().Object;

            MockFreshWaterCanon.Setup(canon => canon.HasAccessToFreshWater(newCell)).Returns(hasFreshWater);

            return newCell;
        }

        private IBuilding BuildBuilding() {
            var newTemplate = new Mock<IBuildingTemplate>().Object;

            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(newTemplate);

            return mockBuilding.Object;
        }

        private ICity BuildCity(IHexCell location, IEnumerable<IBuilding> buildings) {
            var newCity = new Mock<ICity>().Object;

            MockCellPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(location)).Returns(newCity);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private IResourceNode BuildResourceNode(IHexCell location) {
            var newNode = new Mock<IResourceNode>().Object;

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        private ICivilization BuildCivilization(
            IEnumerable<IResourceDefinition> visibleResources, IEnumerable<ITechDefinition> discoveredTechs 
        ) {
            var newCiv = new Mock<ICivilization>().Object;

            MockTechCanon.Setup(canon => canon.GetResourcesVisibleToCiv(newCiv)).Returns(visibleResources);
            MockTechCanon.Setup(canon => canon.GetTechsDiscoveredByCiv (newCiv)).Returns(discoveredTechs);

            return newCiv;
        }

        private IResourceDefinition BuildResourceDefinition() {
            return new Mock<IResourceDefinition>().Object;
        }

        private IImprovement BuildImprovement(IHexCell cell) {
            var newImprovement = new Mock<IImprovement>().Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                        .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        private ITechDefinition BuildTechDefinition() {
            return new Mock<ITechDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
