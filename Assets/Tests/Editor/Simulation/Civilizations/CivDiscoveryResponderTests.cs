using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Visibility;

namespace Assets.Tests.Simulation.Civilizations {

    public class CivDiscoveryResponderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<ICivDiscoveryCanon>                            MockCivDiscoveryCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private VisibilitySignals                                   VisibilitySignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockCivDiscoveryCanon   = new Mock<ICivDiscoveryCanon>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            VisibilitySignals       = new VisibilitySignals();

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<ICivDiscoveryCanon>                           ().FromInstance(MockCivDiscoveryCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<VisibilitySignals>                            ().FromInstance(VisibilitySignals);

            Container.Bind<CivDiscoveryResponder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void OnCellBecameExploredByCiv_AndCellHasUnits_SetsUnitOwnersAsDiscovered() {
            var domesticCiv   = BuildCiv();
            var foreignCivOne = BuildCiv();
            var foreignCivTwo = BuildCiv();

            var cell = BuildCell(new List<IUnit>() {
                BuildUnit(foreignCivOne), BuildUnit(foreignCivTwo)
            });

            MockCivDiscoveryCanon.Setup(canon => canon.CanEstablishDiscoveryBetweenCivs(domesticCiv, foreignCivOne)).Returns(true);
            MockCivDiscoveryCanon.Setup(canon => canon.CanEstablishDiscoveryBetweenCivs(domesticCiv, foreignCivTwo)).Returns(true);

            Container.Resolve<CivDiscoveryResponder>();

            VisibilitySignals.CellBecameExploredByCiv.OnNext(new UniRx.Tuple<IHexCell, ICivilization>(cell, domesticCiv));

            MockCivDiscoveryCanon.Verify(
                canon => canon.EstablishDiscoveryBetweenCivs(domesticCiv, foreignCivOne),
                Times.Once, "Failed to establish discovery between domesticCiv and foreignCivOne"
            );

            MockCivDiscoveryCanon.Verify(
                canon => canon.EstablishDiscoveryBetweenCivs(domesticCiv, foreignCivTwo),
                Times.Once, "Failed to establish discovery between domesticCiv and foreignCivTwo"
            );
        }

        [Test]
        public void OnCellBecameExploredByCiv_AndCellHasUnits_DoesNotSetUnitOwnersAsDiscoveredWhenDiscoveryIsInvalid() {
            var domesticCiv   = BuildCiv();
            var foreignCivOne = BuildCiv();
            var foreignCivTwo = BuildCiv();

            var cell = BuildCell(new List<IUnit>() {
                BuildUnit(foreignCivOne), BuildUnit(foreignCivTwo)
            });

            Container.Resolve<CivDiscoveryResponder>();

            VisibilitySignals.CellBecameExploredByCiv.OnNext(new UniRx.Tuple<IHexCell, ICivilization>(cell, domesticCiv));

            MockCivDiscoveryCanon.Verify(
                canon => canon.EstablishDiscoveryBetweenCivs(domesticCiv, foreignCivOne),
                Times.Never, "Incorrectly established discovery between domesticCiv and foreignCivOne"
            );

            MockCivDiscoveryCanon.Verify(
                canon => canon.EstablishDiscoveryBetweenCivs(domesticCiv, foreignCivTwo),
                Times.Never, "Incorrectly established discovery between domesticCiv and foreignCivTwo"
            );
        }



        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IHexCell BuildCell(IEnumerable<IUnit> units) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(units);

            return newCell;
        }

        #endregion

        #endregion

    }
}
