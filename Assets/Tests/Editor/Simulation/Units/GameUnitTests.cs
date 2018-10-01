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
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class GameUnitTests : ZenjectUnitTestFixture {

        #region static fields and properties

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

        private Mock<IUnitConfig>               MockConfig;
        private Mock<IUnitPositionCanon>        MockPositionCanon;
        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;
        private Mock<IHexGrid>                  MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                   = new Mock<IUnitConfig>();
            MockPositionCanon            = new Mock<IUnitPositionCanon>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockGrid                     = new Mock<IHexGrid>();

            Container.Bind<IUnitConfig>              ().FromInstance(MockConfig                  .Object);
            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockPositionCanon           .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IHexGrid>                 ().FromInstance(MockGrid                    .Object);

            Container.Bind<IPromotionParser>().FromMock();

            Container.Bind<GameUnit>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<ISubject<IUnit>>().WithId("Unit Clicked Signal").To<Subject<IUnit>>().AsSingle();

            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When OnPointerClick is called, Unit should fire Unit Clicked Signal " +
            "with itself as an argument")]
        public void OnPointerClick_SignalFired() {
            var unitToTest = Container.Resolve<GameUnit>();

            Container.Resolve<UnitSignals>().ClickedSignal.Subscribe(delegate(IUnit clickedUnit) {
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

            Container.Resolve<UnitSignals>().BeginDragSignal.Subscribe(delegate(UniRx.Tuple<IUnit, PointerEventData> dataTuple) {
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

            Container.Resolve<UnitSignals>().DragSignal.Subscribe(delegate(UniRx.Tuple<IUnit, PointerEventData> dataTuple) {
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

            Container.Resolve<UnitSignals>().EndDragSignal.Subscribe(delegate(UniRx.Tuple<IUnit, PointerEventData> dataTuple) {
                    Assert.AreEqual(unitToTest, dataTuple.Item1, "Unit End Drag Signal was passed an incorrect Unit");
                    Assert.AreEqual(eventData,  dataTuple.Item2, "Unit End Drag Signal was passed an incorrect EventData");
                    Assert.Pass();
                });

            unitToTest.OnEndDrag(eventData);
            Assert.Fail("Unit End Drag Signal was never fired");
        }

        #endregion

        #region utilities

        private GameUnit BuildUnit(IHexCell location, int currentMovement) {
            var newUnit = Container.Resolve<GameUnit>();

            newUnit.CurrentMovement = currentMovement;

            MockPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IHexCell BuildCell(int movementCost) {
            var cellMock = new Mock<IHexCell>();
            cellMock.Name = string.Format("MapTile with cost {0}", movementCost);

            var newCell = cellMock.Object;

            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), cellMock.Object))
                .Returns(true);

            MockPositionCanon.Setup(logic => logic.GetTraversalCostForUnit(
                It.IsAny<IUnit>(), It.IsAny<IHexCell>(), newCell, It.IsAny<bool>()
            )).Returns(movementCost);

            return newCell;
        }

        #endregion

        #endregion

    }

}
