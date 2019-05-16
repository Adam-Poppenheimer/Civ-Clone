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

    public class BarbarianAttackBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitVisibilityLogic>       MockUnitVisibilityLogic;
        private Mock<IUnitPositionCanon>         MockUnitPositionCanon;
        private Mock<IBarbarianUtilityLogic>     MockUtilityLogic;
        private Mock<IBarbarianBrainFilterLogic> MockFilterLogic;
        private Mock<IHexGrid>                   MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitVisibilityLogic = new Mock<IUnitVisibilityLogic>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockUtilityLogic        = new Mock<IBarbarianUtilityLogic>();
            MockFilterLogic         = new Mock<IBarbarianBrainFilterLogic>();
            MockGrid                = new Mock<IHexGrid>();

            Container.Bind<IUnitVisibilityLogic>      ().FromInstance(MockUnitVisibilityLogic.Object);
            Container.Bind<IUnitPositionCanon>        ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IBarbarianUtilityLogic>    ().FromInstance(MockUtilityLogic       .Object);
            Container.Bind<IBarbarianBrainFilterLogic>().FromInstance(MockFilterLogic        .Object);
            Container.Bind<IHexGrid>                  ().FromInstance(MockGrid               .Object);

            Container.Bind<ICombatExecuter>      ().FromInstance(new Mock<ICombatExecuter>      ().Object);
            Container.Bind<IUnitAttackOrderLogic>().FromInstance(new Mock<IUnitAttackOrderLogic>().Object);

            Container.Bind<BarbarianAttackBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_ReturnsZeroIfUnitCantAttack() {
            var unit = BuildUnit(canAttack: false);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            Assert.AreEqual(0f, attackBrain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitHasRangedAttackStrength_ReturnsMaxUtilityOfCellsWithinAttackRange() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true, attackRange: 3, rangedAttackStrength: 1);

            var maps = new InfluenceMaps();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { BuildCell(), 3f }, { BuildCell(), 2f },
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(cellsByUtility.Keys.ToList());

            MockFilterLogic .Setup(logic => logic.GetRangedAttackFilter   (unit))      .Returns(cell => true);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            Assert.AreEqual(3f, attackBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitHasRangedAttackStrength_ExcludesCellsThatDontPassRangedAttackFilter() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true, attackRange: 3, rangedAttackStrength: 1);

            var maps = new InfluenceMaps();

            IHexCell cellOne = BuildCell(), cellTwo = BuildCell(), cellThree = BuildCell();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { cellOne, 1f }, { cellTwo, 3f }, { cellThree, 2f },
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(cellsByUtility.Keys.ToList());

            MockFilterLogic .Setup(logic => logic.GetRangedAttackFilter   (unit))      .Returns(cell => cell != cellTwo);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            Assert.AreEqual(2f, attackBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitHasNoRangedAttackStrength_ReturnsMaxUtilityOfCellsVisibleToUnit() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true);

            var maps = new InfluenceMaps();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { BuildCell(), 3f }, { BuildCell(), 2f },
            };

            MockUnitVisibilityLogic.Setup(logic => logic.GetCellsVisibleToUnit(unit)).Returns(cellsByUtility.Keys);

            MockFilterLogic .Setup(logic => logic.GetMeleeAttackFilter    (unit))      .Returns(cell => true);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            Assert.AreEqual(3f, attackBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitHasNoRangedAttackStrength_ExcludesCellsThatDontPassMeleeAttackFilter() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true);

            var maps = new InfluenceMaps();

            IHexCell cellOne = BuildCell(), cellTwo = BuildCell(), cellThree = BuildCell();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { cellOne, 1f }, { cellTwo, 3f }, { cellThree, 2f },
            };

            MockUnitVisibilityLogic.Setup(logic => logic.GetCellsVisibleToUnit(unit)).Returns(cellsByUtility.Keys);

            MockFilterLogic .Setup(logic => logic.GetMeleeAttackFilter    (unit))      .Returns(cell => cell != cellTwo);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            Assert.AreEqual(2f, attackBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_AndNoValidAttackCandidates_ReturnsZero() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true);

            var maps = new InfluenceMaps();

            MockUnitVisibilityLogic.Setup(logic => logic.GetCellsVisibleToUnit(unit)).Returns(new List<IHexCell>());

            MockFilterLogic .Setup(logic => logic.GetMeleeAttackFilter    (unit))      .Returns(cell => true);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => 10f);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            Assert.AreEqual(0f, attackBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetCommandsForUnit_AndUnitHasRangedAttackStrength_ReturnsASingleAttackCommand_ConfiguredCorrectly() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true, attackRange: 3, rangedAttackStrength: 1);

            var maps = new InfluenceMaps();

            IHexCell cellTwo = BuildCell();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { cellTwo, 3f }, { BuildCell(), 2f },
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(cellsByUtility.Keys.ToList());

            MockFilterLogic .Setup(logic => logic.GetRangedAttackFilter   (unit))      .Returns(cell => true);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            var commands = attackBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(1, commands.Count, "Commands has an unexpected number of elements");

            var attackCommand = commands[0] as AttackUnitCommand;

            Assert.IsNotNull(attackCommand, "Command is not of type AttackUnitCommand");
            
            Assert.AreEqual(unit,              attackCommand.Attacker,         "Command has an unexpected Attacker value");
            Assert.AreEqual(cellTwo,           attackCommand.LocationToAttack, "Command has an unexpected LocationToAttack value");
            Assert.AreEqual(CombatType.Ranged, attackCommand.CombatType,       "Command has an unexpected CombatType value");
        }

        [Test]
        public void GetCommandsForUnit_AndUnitHasRangedAttackStrength_ReturnsEmptyListIfNoCandidatePassesFilter() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true, attackRange: 3, rangedAttackStrength: 1);

            var maps = new InfluenceMaps();

            IHexCell cellTwo = BuildCell();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { cellTwo, 3f }, { BuildCell(), 2f },
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(cellsByUtility.Keys.ToList());

            MockFilterLogic .Setup(logic => logic.GetRangedAttackFilter   (unit))      .Returns(cell => false);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            var commands = attackBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(0, commands.Count);
        }

        [Test]
        public void GetCommandsForUnit_AndUnitHasNoRangedAttackStrength_ReturnsASingleAttackCommand_ConfiguredCorrectly() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true);

            var maps = new InfluenceMaps();

            IHexCell cellTwo = BuildCell();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { cellTwo, 3f }, { BuildCell(), 2f },
            };

            MockUnitVisibilityLogic.Setup(logic => logic.GetCellsVisibleToUnit(unit)).Returns(cellsByUtility.Keys);

            MockFilterLogic .Setup(logic => logic.GetMeleeAttackFilter    (unit))      .Returns(cell => true);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            var commands = attackBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(1, commands.Count, "Commands has an unexpected number of elements");

            var attackCommand = commands[0] as AttackUnitCommand;

            Assert.IsNotNull(attackCommand, "Command is not of type AttackUnitCommand");
            
            Assert.AreEqual(unit,             attackCommand.Attacker,         "Command has an unexpected Attacker value");
            Assert.AreEqual(cellTwo,          attackCommand.LocationToAttack, "Command has an unexpected LocationToAttack value");
            Assert.AreEqual(CombatType.Melee, attackCommand.CombatType,       "Command has an unexpected CombatType value");
        }

        [Test]
        public void GetCommandsForUnit_AndUnitHasNoRangedAttackStrength_ReturnsEmptyListIfNoCandidatePassesFilter() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(location: unitLocation, canAttack: true);

            var maps = new InfluenceMaps();

            IHexCell cellTwo = BuildCell();

            var cellsByUtility = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { cellTwo, 3f }, { BuildCell(), 2f },
            };

            MockUnitVisibilityLogic.Setup(logic => logic.GetCellsVisibleToUnit(unit)).Returns(cellsByUtility.Keys);

            MockFilterLogic .Setup(logic => logic.GetMeleeAttackFilter    (unit))      .Returns(cell => false);
            MockUtilityLogic.Setup(logic => logic.GetAttackUtilityFunction(unit, maps)).Returns(cell => cellsByUtility[cell]);

            var attackBrain = Container.Resolve<BarbarianAttackBrain>();

            var commands = attackBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(0, commands.Count);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnit BuildUnit(
            IHexCell location = null, bool canAttack = false, int attackRange = 0,
            int rangedAttackStrength = 0
        ) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CanAttack)           .Returns(canAttack);
            mockUnit.Setup(unit => unit.AttackRange)         .Returns(attackRange);
            mockUnit.Setup(unit => unit.RangedAttackStrength).Returns(rangedAttackStrength);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
