using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianBePrisonersBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                 MockGrid;
        private Mock<IHexPathfinder>           MockHexPathfinder;
        private Mock<IUnitPositionCanon>       MockUnitPositionCanon;
        private Mock<IEncampmentLocationCanon> MockEncampmentLocationCanon;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockGrid                    = new Mock<IHexGrid>();
            MockHexPathfinder           = new Mock<IHexPathfinder>();
            MockUnitPositionCanon       = new Mock<IUnitPositionCanon>();
            MockEncampmentLocationCanon = new Mock<IEncampmentLocationCanon>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IHexGrid>                ().FromInstance(MockGrid                   .Object);
            Container.Bind<IHexPathfinder>          ().FromInstance(MockHexPathfinder          .Object);
            Container.Bind<IUnitPositionCanon>      ().FromInstance(MockUnitPositionCanon      .Object);
            Container.Bind<IEncampmentLocationCanon>().FromInstance(MockEncampmentLocationCanon.Object);

            Container.Bind<BarbarianBePrisonersBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_ReturnsOneIfUnitIsCivilian() {
            var unit = BuildUnit(UnitType.Civilian, BuildCell());

            var brain = Container.Resolve<BarbarianBePrisonersBrain>();

            Assert.AreEqual(1f, brain.GetUtilityForUnit(unit, new BarbarianInfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_ReturnsZeroIfUnitNotCivilian() {
            var unit = BuildUnit(UnitType.Archery, BuildCell());

            var brain = Container.Resolve<BarbarianBePrisonersBrain>();

            Assert.AreEqual(0f, brain.GetUtilityForUnit(unit, new BarbarianInfluenceMaps()));
        }

        [Test]
        public void GetCommandsForUnit_AndMapHasEncampments_ReturnsOneMoveCommandForNearestCellWithAnEncampment() {
            var unitPosition = BuildCell();
            var unit = BuildUnit(UnitType.Civilian, unitPosition);

            var otherCellOne   = BuildCell();
            var otherCellTwo   = BuildCell();
            var otherCellThree = BuildCell();
            var otherCellFour  = BuildCell();

            BuildEncampment(otherCellOne);
            BuildEncampment(otherCellTwo);
            BuildEncampment(otherCellFour);

            MockHexPathfinder.Setup(pathfinder => pathfinder.GetCostToAllCells(
                unitPosition, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                It.IsAny<ReadOnlyCollection<IHexCell>>()

            )).Returns(new Dictionary<IHexCell, float>() {
                { otherCellOne,   10f },
                { otherCellTwo,   6f  },
                { otherCellThree, 4f  },
                { otherCellFour,  8f  },
            });

            var brain = Container.Resolve<BarbarianBePrisonersBrain>();

            var commandList = brain.GetCommandsForUnit(unit, new BarbarianInfluenceMaps());

            Assert.AreEqual(1, commandList.Count, "CommandList has an unexpected number of elements");

            var moveCommand = commandList[0] as MoveUnitCommand;

            Assert.IsNotNull(moveCommand, "Command is not of type MoveUnitCommand");

            Assert.AreEqual(unit,         moveCommand.UnitToMove,      "moveCommand has an unexpected UnitToMove value");
            Assert.AreEqual(otherCellTwo, moveCommand.DesiredLocation, "moveCommand has an unexpected DesiredLocation value");
        }

        [Test]
        public void GetCommandsForUnit_AndMapHasNoEncampments_ReturnsEmptyList() {
            var unitPosition = BuildCell();
            var unit = BuildUnit(UnitType.Civilian, unitPosition);

            var otherCellOne = BuildCell();

            MockHexPathfinder.Setup(pathfinder => pathfinder.GetCostToAllCells(
                unitPosition, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                It.IsAny<ReadOnlyCollection<IHexCell>>()

            )).Returns(new Dictionary<IHexCell, float>() {
                { otherCellOne,   10f },
            });

            var brain = Container.Resolve<BarbarianBePrisonersBrain>();

            CollectionAssert.IsEmpty(brain.GetCommandsForUnit(unit, new BarbarianInfluenceMaps()));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IUnit BuildUnit(UnitType type, IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IEncampment BuildEncampment(IHexCell location) {
            var newEncampment = new Mock<IEncampment>().Object;

            MockEncampmentLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                       .Returns(new List<IEncampment>() { newEncampment });

            return newEncampment;
        }

        #endregion

        #endregion

    }

}
