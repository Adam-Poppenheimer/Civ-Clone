using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianCaptureCivilianBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                   MockGrid;
        private Mock<IUnitPositionCanon>         MockUnitPositionCanon;
        private Mock<IHexPathfinder>             MockHexPathfinder;
        private Mock<IBarbarianConfig>           MockBarbarianConfig;
        private Mock<IBarbarianBrainFilterLogic> MockFilterLogic;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockGrid              = new Mock<IHexGrid>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockHexPathfinder     = new Mock<IHexPathfinder>();
            MockBarbarianConfig   = new Mock<IBarbarianConfig>();
            MockFilterLogic       = new Mock<IBarbarianBrainFilterLogic>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IHexGrid>                  ().FromInstance(MockGrid             .Object);
            Container.Bind<IUnitPositionCanon>        ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IHexPathfinder>            ().FromInstance(MockHexPathfinder    .Object);
            Container.Bind<IBarbarianConfig>          ().FromInstance(MockBarbarianConfig  .Object);
            Container.Bind<IBarbarianBrainFilterLogic>().FromInstance(MockFilterLogic      .Object);

            Container.Bind<ICombatExecuter>      ().FromInstance(new Mock<ICombatExecuter>      ().Object);
            Container.Bind<IUnitAttackOrderLogic>().FromInstance(new Mock<IUnitAttackOrderLogic>().Object);

            Container.Bind<BarbarianCaptureCivilianBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_AndSomeReachableCellSatisfiesCivilianCaptureFilter_ReturnsCivilianCaptureUtility() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.5f);

            var reachableCellDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 100f },
                { BuildCell(), 200f },
                { BuildCell(), 300f },
            };
            
            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(
                    unitLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(), AllCells
                )
            ).Returns(reachableCellDict);

            MockFilterLogic.Setup(tools => tools.GetCaptureCivilianFilter(unit)).Returns(cell => reachableCellDict[cell] > 250f);

            MockBarbarianConfig.Setup(config => config.CaptureCivilianUtility).Returns(0.8f);

            var captureBrain = Container.Resolve<BarbarianCaptureCivilianBrain>();

            Assert.AreEqual(0.8f, captureBrain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_AndNoReachableCellSatisfiesCivilianCaptureFilter_ReturnsZero() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.5f);

            var reachableCellDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 100f },
                { BuildCell(), 200f },
                { BuildCell(), 300f },
            };
            
            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(
                    unitLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(), AllCells
                )
            ).Returns(reachableCellDict);

            MockFilterLogic.Setup(tools => tools.GetCaptureCivilianFilter(unit)).Returns(cell => false);

            MockBarbarianConfig.Setup(config => config.CaptureCivilianUtility).Returns(0.8f);

            var captureBrain = Container.Resolve<BarbarianCaptureCivilianBrain>();

            Assert.AreEqual(0f, captureBrain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_AndNoReachableCells_ReturnsZero() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.5f);

            var reachableCellDict = new Dictionary<IHexCell, float>();
            
            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(
                    unitLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(), AllCells
                )
            ).Returns(reachableCellDict);

            MockFilterLogic.Setup(tools => tools.GetCaptureCivilianFilter(unit)).Returns(cell => true);

            MockBarbarianConfig.Setup(config => config.CaptureCivilianUtility).Returns(0.8f);

            var captureBrain = Container.Resolve<BarbarianCaptureCivilianBrain>();

            Assert.AreEqual(0f, captureBrain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetCommandsForUnit_AndSomeValidTarget_ReturnsSingleAttackUnitCommand() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.5f);

            var reachableCellDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 100f },
                { BuildCell(), 200f },
                { BuildCell(), 300f },
            };
            
            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(
                    unitLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(), AllCells
                )
            ).Returns(reachableCellDict);

            MockFilterLogic.Setup(tools => tools.GetCaptureCivilianFilter(unit)).Returns(cell => reachableCellDict[cell] > 250f);

            MockBarbarianConfig.Setup(config => config.CaptureCivilianUtility).Returns(0.8f);

            var captureBrain = Container.Resolve<BarbarianCaptureCivilianBrain>();

            var commands = captureBrain.GetCommandsForUnit(unit, new InfluenceMaps());

            Assert.AreEqual(1, commands.Count, "Unexpected number of commands");

            Assert.IsTrue(commands[0] is AttackUnitCommand, "Command is not of type AttackUnitCommand");
        }

        [Test]
        public void GetCommandsForUnit_AndSomeValidTarget_AttackUnitCommandFieldsSetCorrectly() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.5f);

            var reachableCellDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 100f },
                { BuildCell(), 200f },
                { BuildCell(), 300f },
            };
            
            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(
                    unitLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(), AllCells
                )
            ).Returns(reachableCellDict);

            MockFilterLogic.Setup(tools => tools.GetCaptureCivilianFilter(unit)).Returns(cell => reachableCellDict[cell] > 250f);

            MockBarbarianConfig.Setup(config => config.CaptureCivilianUtility).Returns(0.8f);

            var captureBrain = Container.Resolve<BarbarianCaptureCivilianBrain>();

            var commands = captureBrain.GetCommandsForUnit(unit, new InfluenceMaps());

            var attackCommand = commands[0] as AttackUnitCommand;

            Assert.AreEqual(unit,                          attackCommand.Attacker,         "AttackCommand has an unexpected Attacker");
            Assert.AreEqual(reachableCellDict.Keys.Last(), attackCommand.LocationToAttack, "AttackCommand has an unexpected LocationToAttack");
            Assert.AreEqual(CombatType.Melee,              attackCommand.CombatType,       "AttackCommand has an unexpected CombatType");
        }

        [Test]
        public void GetCommandsForUnit_AndNoValidTarget_ReturnsEmptyList() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(unitLocation, 3.5f);

            var reachableCellDict = new Dictionary<IHexCell, float>();
            
            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(
                    unitLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(), AllCells
                )
            ).Returns(reachableCellDict);

            MockFilterLogic.Setup(tools => tools.GetCaptureCivilianFilter(unit)).Returns(cell => true);

            MockBarbarianConfig.Setup(config => config.CaptureCivilianUtility).Returns(0.8f);

            var captureBrain = Container.Resolve<BarbarianCaptureCivilianBrain>();

            Assert.AreEqual(0, captureBrain.GetCommandsForUnit(unit, new InfluenceMaps()).Count);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IUnit BuildUnit(IHexCell location, float currentMovement) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CurrentMovement).Returns(currentMovement);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
