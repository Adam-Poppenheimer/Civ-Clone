using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianPillageBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>         MockUnitPositionCanon;
        private Mock<IHexGrid>                   MockGrid;
        private Mock<IBarbarianBrainWeightLogic> MockWeightLogic;
        private Mock<IBarbarianUtilityLogic>     MockUtilityLogic;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockGrid              = new Mock<IHexGrid>();
            MockWeightLogic       = new Mock<IBarbarianBrainWeightLogic>();
            MockUtilityLogic      = new Mock<IBarbarianUtilityLogic>();

            Container.Bind<IUnitPositionCanon>        ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IHexGrid>                  ().FromInstance(MockGrid             .Object);
            Container.Bind<IBarbarianBrainWeightLogic>().FromInstance(MockWeightLogic      .Object);
            Container.Bind<IBarbarianUtilityLogic>    ().FromInstance(MockUtilityLogic     .Object);

            Container.Bind<IAbilityExecuter>().FromInstance(new Mock<IAbilityExecuter>().Object);
            Container.Bind<IHexPathfinder>  ().FromInstance(new Mock<IHexPathfinder  >().Object);

            Container.Bind<BarbarianPillageBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_ReturnsMaxUtilityFromNearbyCells_AccordingToPillageUtilityFunction() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.4f);

            var cellDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { BuildCell(), 3f }, { BuildCell(), 2f },
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(cellDict.Keys.ToList());

            var maps = new InfluenceMaps();

            MockUtilityLogic.Setup(logic => logic.GetPillageUtilityFunction(unit, maps)).Returns(cell => cellDict[cell]);

            var pillageBrain = Container.Resolve<BarbarianPillageBrain>();

            Assert.AreEqual(3f, pillageBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetCommandsForUnit_AndSomeValidCellExists_ReturnsAMoveAndAPillageCommand() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3f);

            var maps = new InfluenceMaps();

            var nearbyCells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };            

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            MockWeightLogic.Setup(tools => tools.GetPillageWeightFunction(unit, maps)).Returns(cell => cell.Index);

            var pillageBrain = Container.Resolve<BarbarianPillageBrain>();

            var commands = pillageBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(2, commands.Count, "Unexpected number of commands");

            Assert.IsTrue(commands[0] is MoveUnitCommand,    "commands[0] is not of type MoveUnitCommand");
            Assert.IsTrue(commands[1] is PillageUnitCommand, "commands[1] is not of type PillageUnitCommand");
        }

        [Test]
        public void GetCommandsForUnit_AndSomeValidCellExists_MoveCommandGivenCorrectUnitAndLocation() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3f);

            var maps = new InfluenceMaps();

            var nearbyCells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };            

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            MockWeightLogic.Setup(tools => tools.GetPillageWeightFunction(unit, maps)).Returns(cell => cell.Index);

            var pillageBrain = Container.Resolve<BarbarianPillageBrain>();

            var commands = pillageBrain.GetCommandsForUnit(unit, maps);

            var moveCommand = commands[0] as MoveUnitCommand;

            Assert.AreEqual(unit,           moveCommand.UnitToMove,      "moveCommand has an unexpected UnitToMove");
            Assert.AreEqual(nearbyCells[2], moveCommand.DesiredLocation, "moveCommand has an unexpected DesiredLocation");
        }

        [Test]
        public void GetCommandsForUnit_AndSomeValidCellExists_PillageCommandGivenCorrectPillager() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3f);

            var maps = new InfluenceMaps();

            var nearbyCells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };            

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            MockWeightLogic.Setup(tools => tools.GetPillageWeightFunction(unit, maps)).Returns(cell => cell.Index);

            var pillageBrain = Container.Resolve<BarbarianPillageBrain>();

            var commands = pillageBrain.GetCommandsForUnit(unit, maps);

            var pillageCommand = commands[1] as PillageUnitCommand;

            Assert.AreEqual(unit, pillageCommand.Pillager, "pillageCommand has an unexpected Pillager");
        }

        [Test]
        public void GetCommandsForUnit_AndNoValidCellExists_ReturnsEmptyList() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3f);

            var maps = new InfluenceMaps();            

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(new List<IHexCell>());

            var pillageBrain = Container.Resolve<BarbarianPillageBrain>();

            var commands = pillageBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(0, commands.Count);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Index).Returns(AllCells.Count);

            var newCell = mockCell.Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IUnit BuildUnit(IHexCell location, float maxMovement) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.MaxMovement).Returns(maxMovement);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
