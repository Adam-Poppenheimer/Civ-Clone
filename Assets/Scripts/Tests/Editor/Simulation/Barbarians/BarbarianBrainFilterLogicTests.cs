using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianBrainFilterLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;
        private Mock<ICombatExecuter>    MockCombatExecuter;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockCombatExecuter    = new Mock<ICombatExecuter>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<ICombatExecuter>   ().FromInstance(MockCombatExecuter   .Object);

            Container.Bind<BarbarianBrainFilterLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsTrueIfCellHasCiviliansTheCaptorCanAttack() {
            var unit = BuildUnit(UnitType.Melee);

            var cell = BuildCell(
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian)
            );

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, It.IsAny<IUnit>()))
                              .Returns(true);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetCaptureCivilianFilter(unit);

            Assert.IsTrue(returnedDelegate(cell));
        }

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsFalseIfCivilianCannotBeAttacked() {
            var unit = BuildUnit(UnitType.Melee);

            var unitOne   = BuildUnit(UnitType.Civilian);
            var unitTwo   = BuildUnit(UnitType.Civilian);
            var unitThree = BuildUnit(UnitType.Civilian);

            var cell = BuildCell(unitOne, unitTwo, unitThree);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, unitOne  )).Returns(true);
            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, unitTwo  )).Returns(false);
            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, unitThree)).Returns(true);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetCaptureCivilianFilter(unit);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsFalseIfNonCivilianUnitAtLocation() {
            var unit = BuildUnit(UnitType.Melee);

            var cell = BuildCell(
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Melee)
            );

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, It.IsAny<IUnit>()))
                              .Returns(true);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetCaptureCivilianFilter(unit);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsFalseIfNoUnitsAtLocation() {
            var unit = BuildUnit(UnitType.Melee);

            var cell = BuildCell();

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, It.IsAny<IUnit>()))
                              .Returns(true);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetCaptureCivilianFilter(unit);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetMeleeAttackFilter_ReturnedDelegate_ReturnsTrueIfSomeOccupantValidForMeleeAttack() {
            var attacker = BuildUnit(UnitType.Melee);

            var unitOne   = BuildUnit(UnitType.Melee);
            var unitTwo   = BuildUnit(UnitType.Melee);
            var unitThree = BuildUnit(UnitType.Melee);

            var cell = BuildCell(unitOne, unitTwo, unitThree);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, unitOne  )).Returns(false);
            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, unitTwo  )).Returns(false);
            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, unitThree)).Returns(true);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetMeleeAttackFilter(attacker);

            Assert.IsTrue(returnedDelegate(cell));
        }

        [Test]
        public void GetMeleeAttackFilter_ReturnedDelegate_ReturnsFalseIfNoOccupantValidForMeleeAttack() {
            var attacker = BuildUnit(UnitType.Melee);

            var unitOne   = BuildUnit(UnitType.Melee);
            var unitTwo   = BuildUnit(UnitType.Melee);
            var unitThree = BuildUnit(UnitType.Melee);

            var cell = BuildCell(unitOne, unitTwo, unitThree);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetMeleeAttackFilter(attacker);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetMeleeAttackFilter_ReturnedDelegate_ReturnsFalseIfNoOccupants() {
            var attacker = BuildUnit(UnitType.Melee);

            var cell = BuildCell();

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetMeleeAttackFilter(attacker);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetRangedAttackFilter_ReturnedDelegate_ReturnsTrueIfSomeOccupantValidForRangedAttack() {
            var attacker = BuildUnit(UnitType.Melee);

            var unitOne   = BuildUnit(UnitType.Melee);
            var unitTwo   = BuildUnit(UnitType.Melee);
            var unitThree = BuildUnit(UnitType.Melee);

            var cell = BuildCell(unitOne, unitTwo, unitThree);

            MockCombatExecuter.Setup(executer => executer.CanPerformRangedAttack(attacker, unitOne  )).Returns(false);
            MockCombatExecuter.Setup(executer => executer.CanPerformRangedAttack(attacker, unitTwo  )).Returns(false);
            MockCombatExecuter.Setup(executer => executer.CanPerformRangedAttack(attacker, unitThree)).Returns(true);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetRangedAttackFilter(attacker);

            Assert.IsTrue(returnedDelegate(cell));
        }

        [Test]
        public void GetRangedAttackFilter_ReturnedDelegate_ReturnsFalseIfNoOccupantValidForRangedAttack() {
            var attacker = BuildUnit(UnitType.Melee);

            var unitOne   = BuildUnit(UnitType.Melee);
            var unitTwo   = BuildUnit(UnitType.Melee);
            var unitThree = BuildUnit(UnitType.Melee);

            var cell = BuildCell(unitOne, unitTwo, unitThree);

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetRangedAttackFilter(attacker);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetRangedAttackFilter_ReturnedDelegate_ReturnsFalseIfNoOccupants() {
            var attacker = BuildUnit(UnitType.Melee);

            var cell = BuildCell();

            var filterLogic = Container.Resolve<BarbarianBrainFilterLogic>();

            var returnedDelegate = filterLogic.GetRangedAttackFilter(attacker);

            Assert.IsFalse(returnedDelegate(cell));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            return mockUnit.Object;
        }

        private IHexCell BuildCell(params IUnit[] units) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(units);

            return newCell;
        }

        #endregion

        #endregion

    }

}
