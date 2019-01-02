using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Moq;
using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Visibility;    

namespace Assets.Tests.Simulation.Visibility {

    public class ExplorationCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IGameCore> MockGameCore;

        private VisibilitySignals VisibilitySignals;
        private HexCellSignals    CellSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGameCore = new Mock<IGameCore>();

            VisibilitySignals = new VisibilitySignals();
            CellSignals       = new HexCellSignals();

            Container.Bind<IGameCore>        ().FromInstance(MockGameCore.Object);

            Container.Bind<VisibilitySignals>().FromInstance(VisibilitySignals);
            Container.Bind<HexCellSignals>   ().FromInstance(CellSignals);

            Container.Bind<ExplorationCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsCellExploredByCiv_DefaultsToFalse() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<ExplorationCanon>();

            Assert.IsFalse(canon.IsCellExploredByCiv(cell, civ));
        }

        [Test]
        public void SetCellAsExploredByCiv_IsCellExploredByCivBecomesTrue() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<ExplorationCanon>();

            canon.SetCellAsExploredByCiv(cell, civ);

            Assert.IsTrue(canon.IsCellExploredByCiv(cell, civ));
        }

        [Test]
        public void SetCellAsUnexploredByCiv_IsCellExploredByCivBecomesFalse() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<ExplorationCanon>();

            canon.SetCellAsExploredByCiv  (cell, civ);
            canon.SetCellAsUnexploredByCiv(cell, civ);

            Assert.IsFalse(canon.IsCellExploredByCiv(cell, civ));
        }

        [Test]
        public void IsCellExplored_AndExplorationModeAllCellsExplored_ReturnsTrue() {
            var cell = BuildCell();

            var canon = Container.Resolve<ExplorationCanon>();

            canon.ExplorationMode = CellExplorationMode.AllCellsExplored;

            Assert.IsTrue(canon.IsCellExplored(cell));
        }

        [Test]
        public void IsCellExplored_AndExplorationModeActiveCiv_ReturnsExplorationStatusForActiveCiv() {
            var cell      = BuildCell();
            var activeCiv = BuildCiv();
            var otherCiv  = BuildCiv();

            MockGameCore.Setup(core => core.ActiveCiv).Returns(activeCiv);

            var canon = Container.Resolve<ExplorationCanon>();

            canon.SetCellAsExploredByCiv(cell, otherCiv);

            canon.ExplorationMode = CellExplorationMode.ActiveCiv;

            Assert.IsFalse(canon.IsCellExplored(cell));
        }

        [Test]
        public void IsCellExplored_AndExplorationModeActiveCiv_ReturnsFalseIfActiveCivNull() {
            var cell = BuildCell();

            var canon = Container.Resolve<ExplorationCanon>();

            canon.ExplorationMode = CellExplorationMode.ActiveCiv;

            Assert.IsFalse(canon.IsCellExplored(cell));
        }

        [Test]
        public void Clear_AllCellsRevertToDefaultExploration() {
            var cellOne = BuildCell();
            var cellTwo = BuildCell();

            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var canon = Container.Resolve<ExplorationCanon>();

            canon.SetCellAsExploredByCiv(cellOne, civOne);
            canon.SetCellAsExploredByCiv(cellOne, civTwo);
            canon.SetCellAsExploredByCiv(cellTwo, civOne);
            canon.SetCellAsExploredByCiv(cellTwo, civTwo);

            canon.Clear();

            Assert.IsFalse(canon.IsCellExploredByCiv(cellOne, civOne));
            Assert.IsFalse(canon.IsCellExploredByCiv(cellOne, civTwo));
            Assert.IsFalse(canon.IsCellExploredByCiv(cellTwo, civOne));
            Assert.IsFalse(canon.IsCellExploredByCiv(cellTwo, civTwo));
        }

        [Test]
        public void ExplorationModeChanged_CellExplorationModeChangedSignalFired() {
            var canon = Container.Resolve<ExplorationCanon>();

            VisibilitySignals.CellExplorationModeChangedSignal.Subscribe(
                unit => Assert.Pass()
            );

            canon.ExplorationMode = CellExplorationMode.AllCellsExplored;
            Assert.Fail();
        }

        [Test]
        public void MapBeingClearedSignalFired_ExplorationDataCleared() {
            var cellOne = BuildCell();
            var cellTwo = BuildCell();

            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var canon = Container.Resolve<ExplorationCanon>();

            canon.SetCellAsExploredByCiv(cellOne, civOne);
            canon.SetCellAsExploredByCiv(cellOne, civTwo);
            canon.SetCellAsExploredByCiv(cellTwo, civOne);
            canon.SetCellAsExploredByCiv(cellTwo, civTwo);

            CellSignals.MapBeingClearedSignal.OnNext(new Unit());

            Assert.IsFalse(canon.IsCellExploredByCiv(cellOne, civOne));
            Assert.IsFalse(canon.IsCellExploredByCiv(cellOne, civTwo));
            Assert.IsFalse(canon.IsCellExploredByCiv(cellTwo, civOne));
            Assert.IsFalse(canon.IsCellExploredByCiv(cellTwo, civTwo));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }
}
