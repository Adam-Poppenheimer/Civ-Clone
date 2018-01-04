using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable HealthSetTestCases {
            get {
                yield return new TestCaseData(100, 140);
                yield return new TestCaseData(100, 60);
                yield return new TestCaseData(100, -20);
            }
        }

        private static IEnumerable TotalMovementTestCases {
            get {
                yield return new TestCaseData(7, new List<int>() { 1, 1, 2, 3 }, 0)
                    .SetName("CurrentMovement and path costs are identical");

                yield return new TestCaseData(7, new List<int>() { 1, 1, 2 }, 3)
                    .SetName("CurrentMovement exceeds path costs");
            }
        }

        private static IEnumerable PartialMovementTestCases {
            get {
                yield return new TestCaseData(4, new List<int>() { 1, 1, 2, 3 }, 2)
                    .SetName("Goes full distance on even costs");

                yield return new TestCaseData(3, new List<int>() { 1, 1, 2, 3 }, 2)
                    .SetName("Moves into next tile if movement is insufficient but nonzero");
            }
        }

        #endregion

        #region instance fields and properties

        private static Mock<IUnitConfig> MockConfig;

        private static Mock<IUnitTerrainCostLogic> MockTerrainCostLogic;

        private static Mock<IUnitPositionCanon> MockPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig           = new Mock<IUnitConfig>();
            MockTerrainCostLogic = new Mock<IUnitTerrainCostLogic>();
            MockPositionCanon    = new Mock<IUnitPositionCanon>();

            Container.Bind<IUnitConfig>()          .FromInstance(MockConfig.Object);
            Container.Bind<IUnitTerrainCostLogic>().FromInstance(MockTerrainCostLogic.Object);
            Container.Bind<IUnitPositionCanon>()   .FromInstance(MockPositionCanon.Object);

            Container.Bind<GameUnit>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<ISubject<IUnit>>().WithId("Unit Clicked Signal").To<Subject<IUnit>>().AsSingle();

            Container.Bind<ISubject<UniRx.Tuple<IUnit, PointerEventData>>>()
                .WithId("Unit Begin Drag Signal")
                .To<Subject<UniRx.Tuple<IUnit, PointerEventData>>>()
                .AsSingle();

            Container.Bind<ISubject<UniRx.Tuple<IUnit, PointerEventData>>>()
                .WithId("Unit Drag Signal")
                .To<Subject<UniRx.Tuple<IUnit, PointerEventData>>>()
                .AsSingle();

            Container.Bind<ISubject<UniRx.Tuple<IUnit, PointerEventData>>>()
                .WithId("Unit End Drag Signal")
                .To<Subject<UniRx.Tuple<IUnit, PointerEventData>>>()
                .AsSingle();

            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Health is set, its value is always kept between 0 and " +
            "Config.MaxHealth inclusive")]
        [TestCaseSource("HealthSetTestCases")]
        public void HealthSet_StaysWithinProperBounds(int maxHealth, int newHealthValue) {
            MockConfig.Setup(config => config.MaxHealth).Returns(maxHealth);

            var unitToTest = Container.Resolve<GameUnit>();
            unitToTest.Health = newHealthValue;

            Assert.AreEqual(
                newHealthValue.Clamp(0, maxHealth),
                unitToTest.Health,
                "UnitToTest.Health has an unexpected value"
            );
        }

        [Test(Description = "When OnPointerClick is called, Unit should fire Unit Clicked Signal " +
            "with itself as an argument")]
        public void OnPointerClick_SignalFired() {
            var unitToTest = Container.Resolve<GameUnit>();

            Container.Resolve<UnitSignals>().UnitClickedSignal.Subscribe(delegate(IUnit clickedUnit) {
                Assert.AreEqual(unitToTest, clickedUnit, "Unit Clicked Signal received an incorrect clickedUnit");
                Assert.Pass();
            });

            unitToTest.OnPointerClick(new PointerEventData(EventSystem.current));
            Assert.Fail("Unit Clicked Signal was never fired");
        }

        [Test(Description = "When OnBeginDrag is called, Unit should fire Unit Begin Drag Signal " +
            "with itself and the argued PointerEventData as arguments")]
        public void OnBeginDrag_SignalFired() {
            var unitToTest = Container.Resolve<GameUnit>();
            var eventData = new PointerEventData(EventSystem.current);

            Container.Resolve<UnitSignals>().UnitBeginDragSignal.Subscribe(delegate(UniRx.Tuple<IUnit, PointerEventData> dataTuple) {
                    Assert.AreEqual(unitToTest, dataTuple.Item1, "Unit Begin Drag Signal was passed an incorrect Unit");
                    Assert.AreEqual(eventData,  dataTuple.Item2, "Unit Begin Drag Signal was passed an incorrect EventData");
                    Assert.Pass();
                });

            unitToTest.OnBeginDrag(eventData);
            Assert.Fail("Unit Begin Drag Signal was never fired");
        }

        [Test(Description = "When OnDrag is called, Unit should fire Unit Drag Signal " +
            "with itself and the argued PointerEventData as arguments")]
        public void OnDrag_SignalFired() {
            var unitToTest = Container.Resolve<GameUnit>();
            var eventData = new PointerEventData(EventSystem.current);

            Container.Resolve<UnitSignals>().UnitDragSignal.Subscribe(delegate(UniRx.Tuple<IUnit, PointerEventData> dataTuple) {
                    Assert.AreEqual(unitToTest, dataTuple.Item1, "Unit Drag Signal was passed an incorrect Unit");
                    Assert.AreEqual(eventData,  dataTuple.Item2, "Unit Drag Signal was passed an incorrect EventData");
                    Assert.Pass();
                });

            unitToTest.OnDrag(eventData);
            Assert.Fail("Unit Drag Signal was never fired");
        }

        [Test(Description = "When OnEndDrag is called, Unit should fire Unit End Drag Signal " +
            "with itself and the argued PointerEventData as arguments")]
        public void OnEndDrag_SignalFired() {
            var unitToTest = Container.Resolve<GameUnit>();
            var eventData = new PointerEventData(EventSystem.current);

            Container.Resolve<UnitSignals>().UnitEndDragSignal.Subscribe(delegate(UniRx.Tuple<IUnit, PointerEventData> dataTuple) {
                    Assert.AreEqual(unitToTest, dataTuple.Item1, "Unit End Drag Signal was passed an incorrect Unit");
                    Assert.AreEqual(eventData,  dataTuple.Item2, "Unit End Drag Signal was passed an incorrect EventData");
                    Assert.Pass();
                });

            unitToTest.OnEndDrag(eventData);
            Assert.Fail("Unit End Drag Signal was never fired");
        }

        [Test(Description = "When PerformMovement is called, it shouldn't throw an exception " +
            "when CurrentPath is null or empty list")]
        public void PerformMovement_DoesNotThrowOnNullOrEmptyPath() {
            var unit = BuildUnit(location: BuildTile(1), currentMovement: 2);
            unit.CurrentPath = null;

            Assert.DoesNotThrow(() => unit.PerformMovement(),
                "PerformMovement falsely threw an exception when CurrentPath was null");

            unit.CurrentPath = new List<IHexCell>();

            Assert.DoesNotThrow(() => unit.PerformMovement(),
                "PerformMovement falsely threw an exception when CurrentPath was empty");
        }

        [Test(Description = "When PerformMovement is called and Unit has enough movement to traverse " +
            "from its current location through all tiles in its path and reach the end, it should do so. " +
            "This should result in a single position change to the last location on the path and an empty " +
            "CurrentPath field")]
        [TestCaseSource("TotalMovementTestCases")]
        public void PerformMovement_ReachesEndIfSufficientMovement(int startingMovement, List<int> pathCosts, int endingMovement) {
            var startingPath = pathCosts.Select(cost => BuildTile(cost)).ToList();

            var unit = Container.Resolve<GameUnit>();

            unit.CurrentPath = new List<IHexCell>(startingPath);
            unit.CurrentMovement = startingMovement;

            unit.PerformMovement();

            CollectionAssert.IsEmpty(unit.CurrentPath, "CurrentPath is not empty");

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, startingPath.Last()),
                Times.Once, "Unit was not repositioned to the last element of its starting path");

            foreach(var tile in startingPath) {
                if(tile != startingPath.Last()) {
                    MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, tile),
                        Times.Never, "Unit was falsely repositioned to some intermediate tile on its starting path");
                }
            }

            Assert.AreEqual(endingMovement, unit.CurrentMovement);
        }

        [Test(Description = "When PerformMovement is called but Unit doesn't have enough movement to " +
            "go all the way through its CurrentPath, it should go as far as it can and stop. This should " +
            "only result in one location change")]
        [TestCaseSource("PartialMovementTestCases")]
        public void PerformMovement_MakesPartialProgress(int startingMovement, List<int> pathCosts, int expectedStopIndex) {
            var startingPath = pathCosts.Select(cost => BuildTile(cost)).ToList();

            var expectedStopTile = startingPath[expectedStopIndex];

            var expectedEndingPath = new List<IHexCell>(startingPath);
            expectedEndingPath.RemoveRange(0, expectedStopIndex + 1);

            var unit = Container.Resolve<GameUnit>();

            unit.CurrentPath = new List<IHexCell>(startingPath);
            unit.CurrentMovement = startingMovement;

            unit.PerformMovement();

            Assert.AreEqual(0, unit.CurrentMovement, "CurrentMovement is nonzero");

            CollectionAssert.AreEqual(expectedEndingPath, unit.CurrentPath, "CurrentPath has an unexpected value");

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, expectedStopTile), Times.Once,
                "Unit was not relocated to expectedStopTile");

            foreach(var tile in startingPath) {
                if(tile != expectedStopTile) {
                    MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, tile), Times.Never,
                        "Unit was falsely relocated to tiles that weren't expectedStopTile");
                }
            }
        }

        [Test(Description = "When PerformMovement is called and one of the tiles Unit expects to travel into or through " +
            "cannot accept them, Unit should go as far as it can and then abort, clearing its current path")]
        public void PerformMovement_AbortsWhenInterrupted() {
            var startingTile     = BuildTile(1);
            var interveningTile  = BuildTile(1);
            var destinationTile  = BuildTile(1);

            var path = new List<IHexCell>() { startingTile, interveningTile, destinationTile };

            var unit = Container.Resolve<GameUnit>();
            unit.CurrentPath = new List<IHexCell>(path);
            unit.CurrentMovement = 3;

            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(unit, interveningTile)).Returns(false);

            unit.PerformMovement();

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, interveningTile), Times.Never,
                "Unit falsely attempted to move onto an unoccupiable tile");

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, destinationTile), Times.Never,
                "Unit falsely attempted to move through an unoccupiable tile");

            Assert.That(unit.CurrentPath == null || unit.CurrentPath.Count == 0, "Unit failed to clear its path as expected");

            MockPositionCanon.ResetCalls();

            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(unit, interveningTile)).Returns(true);
            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(unit, destinationTile)).Returns(false);

            unit.CurrentPath = new List<IHexCell>(path);
            unit.CurrentMovement = 3;

            unit.PerformMovement();

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, interveningTile), Times.Once,
                "Unit failed to move as far along its path as it could");

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(unit, destinationTile), Times.Never,
                "Unit falsely attempted to move onto an unoccupiable tile");

            Assert.That(unit.CurrentPath == null || unit.CurrentPath.Count == 0, "Unit failed to clear its path as expected");
        }

        #endregion

        #region utilities

        private GameUnit BuildUnit(IHexCell location, int currentMovement) {
            var newUnit = Container.Resolve<GameUnit>();

            newUnit.CurrentMovement = currentMovement;

            MockPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IHexCell BuildTile(int movementCost) {
            var tileMock = new Mock<IHexCell>();
            tileMock.Name = string.Format("MapTile with cost {0}", movementCost);

            var newTile = tileMock.Object;

            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), tileMock.Object))
                .Returns(true);

            MockTerrainCostLogic.Setup(logic => logic.GetCostToMoveUnitIntoTile(It.IsAny<IUnit>(), newTile))
                .Returns(movementCost);

            return newTile;
        }

        #endregion

        #endregion

    }

}
