using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Barbarians;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianAvailableUnitsLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBarbarianConfig>     MockBarbarianConfig;        
        private Mock<IUnitPositionCanon>   MockUnitPositionCanon;
        private Mock<ICivilizationFactory> MockCivFactory;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBarbarianConfig   = new Mock<IBarbarianConfig>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockCivFactory        = new Mock<ICivilizationFactory>();

            Container.Bind<IBarbarianConfig>    ().FromInstance(MockBarbarianConfig  .Object);
            Container.Bind<IUnitPositionCanon>  ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<ICivilizationFactory>().FromInstance(MockCivFactory       .Object);

            Container.Bind<BarbarianAvailableUnitsLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetAvailableLandTemplates_AndCellIsLand_ReturnsUnitsToSpawn_ThatAreLandMilitary() {
            var unitsToSpawn = new List<IUnitTemplate>() {
                BuildUnitTemplate(UnitType.Archery),    BuildUnitTemplate(UnitType.Melee),
                BuildUnitTemplate(UnitType.NavalMelee), BuildUnitTemplate(UnitType.NavalRanged),
                BuildUnitTemplate(UnitType.Civilian),
            };

            var cell = BuildCell(CellTerrain.Grassland, unitsToSpawn);

            MockBarbarianConfig.Setup(config => config.UnitsToSpawn).Returns(unitsToSpawn);

            var availableUnitsLogic = Container.Resolve<BarbarianAvailableUnitsLogic>();

            CollectionAssert.AreEquivalent(
                unitsToSpawn.Where(template => template.Type.IsLandMilitary()),
                availableUnitsLogic.GetAvailableLandTemplates(cell)
            );
        }

        [Test]
        public void GetAvailableLandTemplates_RemovesTemplatesThatCantBePlacedOnCell() {
            var unitsToSpawn = new List<IUnitTemplate>() {
                BuildUnitTemplate(UnitType.Archery),    BuildUnitTemplate(UnitType.Melee),
                BuildUnitTemplate(UnitType.NavalMelee), BuildUnitTemplate(UnitType.NavalRanged),
                BuildUnitTemplate(UnitType.Civilian),
            };

            var cell = BuildCell(CellTerrain.Grassland, new List<IUnitTemplate>() { unitsToSpawn[1], unitsToSpawn[2] });

            MockBarbarianConfig.Setup(config => config.UnitsToSpawn).Returns(unitsToSpawn);

            var availableUnitsLogic = Container.Resolve<BarbarianAvailableUnitsLogic>();

            CollectionAssert.AreEquivalent(
                new List<IUnitTemplate>() { unitsToSpawn[1] },
                availableUnitsLogic.GetAvailableLandTemplates(cell)
            );
        }

        [Test]
        public void GetAvailableLandTemplates_AndCellIsWater_ReturnsEmptyList() {
            var unitsToSpawn = new List<IUnitTemplate>() {
                BuildUnitTemplate(UnitType.Archery),    BuildUnitTemplate(UnitType.Melee),
                BuildUnitTemplate(UnitType.NavalMelee), BuildUnitTemplate(UnitType.NavalRanged),
                BuildUnitTemplate(UnitType.Civilian),
            };

            var cell = BuildCell(CellTerrain.ShallowWater, unitsToSpawn);

            MockBarbarianConfig.Setup(config => config.UnitsToSpawn).Returns(unitsToSpawn);

            var availableUnitsLogic = Container.Resolve<BarbarianAvailableUnitsLogic>();

            CollectionAssert.IsEmpty(availableUnitsLogic.GetAvailableLandTemplates(cell));
        }

        [Test]
        public void GetAvailableWaterTemplates_AndCellIsLand_ReturnsEmptyList() {
            var unitsToSpawn = new List<IUnitTemplate>() {
                BuildUnitTemplate(UnitType.Archery),    BuildUnitTemplate(UnitType.Melee),
                BuildUnitTemplate(UnitType.NavalMelee), BuildUnitTemplate(UnitType.NavalRanged),
                BuildUnitTemplate(UnitType.Civilian),
            };

            var cell = BuildCell(CellTerrain.Grassland, unitsToSpawn);

            MockBarbarianConfig.Setup(config => config.UnitsToSpawn).Returns(unitsToSpawn);

            var availableUnitsLogic = Container.Resolve<BarbarianAvailableUnitsLogic>();

            CollectionAssert.IsEmpty(availableUnitsLogic.GetAvailableNavalTemplates(cell));
        }

        [Test]
        public void GetAvailableWaterTemplates_AndCellIsWater_ReturnsUnitsToSpawn_ThatAreWaterMilitary() {
            var unitsToSpawn = new List<IUnitTemplate>() {
                BuildUnitTemplate(UnitType.Archery),    BuildUnitTemplate(UnitType.Melee),
                BuildUnitTemplate(UnitType.NavalMelee), BuildUnitTemplate(UnitType.NavalRanged),
                BuildUnitTemplate(UnitType.Civilian),
            };

            var cell = BuildCell(CellTerrain.ShallowWater, unitsToSpawn);

            MockBarbarianConfig.Setup(config => config.UnitsToSpawn).Returns(unitsToSpawn);

            var availableUnitsLogic = Container.Resolve<BarbarianAvailableUnitsLogic>();

            CollectionAssert.AreEquivalent(
                unitsToSpawn.Where(template => template.Type.IsWaterMilitary()),
                availableUnitsLogic.GetAvailableNavalTemplates(cell)
            );
        }

        [Test]
        public void GetAvailableWaterTemplates_RemovesTemplatesThatCantBePlacedOnCell() {
            var unitsToSpawn = new List<IUnitTemplate>() {
                BuildUnitTemplate(UnitType.Archery),    BuildUnitTemplate(UnitType.Melee),
                BuildUnitTemplate(UnitType.NavalMelee), BuildUnitTemplate(UnitType.NavalRanged),
                BuildUnitTemplate(UnitType.Civilian),
            };

            var cell = BuildCell(CellTerrain.ShallowWater, new List<IUnitTemplate>() { unitsToSpawn[1], unitsToSpawn[2] });

            MockBarbarianConfig.Setup(config => config.UnitsToSpawn).Returns(unitsToSpawn);

            var availableUnitsLogic = Container.Resolve<BarbarianAvailableUnitsLogic>();

            CollectionAssert.AreEquivalent(
                new List<IUnitTemplate>() { unitsToSpawn[2] },
                availableUnitsLogic.GetAvailableNavalTemplates(cell)
            );
        }

        #endregion

        #region utilities

        private IUnitTemplate BuildUnitTemplate(UnitType type) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.Type).Returns(type);

            return mockTemplate.Object;
        }

        private IHexCell BuildCell(CellTerrain terrain, IEnumerable<IUnitTemplate> validTemplates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(terrain);

            var newCell = mockCell.Object;

            foreach(var template in validTemplates) {
                MockUnitPositionCanon.Setup(
                    canon => canon.CanPlaceUnitTemplateAtLocation(template, newCell, It.IsAny<ICivilization>())
                ).Returns(true);
            }

            return newCell;
        }

        #endregion

        #endregion

    }

}
