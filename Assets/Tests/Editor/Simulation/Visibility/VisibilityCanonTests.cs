using System;

using NUnit.Framework;
using Moq;
using Zenject;
using UniRx;

using Assets.Simulation.Visibility;
using Assets.Simulation.Core;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.Visibility {

    public class VisibilityCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IGameCore>  MockGameCore;
        private Mock<ITechCanon> MockTechCanon;

        private VisibilitySignals VisibilitySignals;
        private HexCellSignals    CellSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGameCore      = new Mock<IGameCore>();
            MockTechCanon     = new Mock<ITechCanon>();

            VisibilitySignals = new VisibilitySignals();
            CellSignals       = new HexCellSignals();

            Container.Bind<IGameCore>        ().FromInstance(MockGameCore .Object);
            Container.Bind<ITechCanon>       ().FromInstance(MockTechCanon.Object);

            Container.Bind<VisibilitySignals>().FromInstance(VisibilitySignals);
            Container.Bind<HexCellSignals>   ().FromInstance(CellSignals);

            Container.Bind<VisibilityCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CellVisibilityModeChanged_CellVisibilityModeChangedSignalFired() {
            var canon = Container.Resolve<VisibilityCanon>();

            VisibilitySignals.CellVisibilityModeChangedSignal.Subscribe(unit => Assert.Pass());

            canon.CellVisibilityMode = CellVisibilityMode.HideAll;

            Assert.Fail();
        }

        [Test]
        public void ResourceVisibilityModeChanged_ResourceVisibilityModeChangedSignalFired() {
            var canon = Container.Resolve<VisibilityCanon>();

            VisibilitySignals.ResourceVisibilityModeChangedSignal.Subscribe(unit => Assert.Pass());

            canon.ResourceVisibilityMode = ResourceVisibilityMode.HideAll;

            Assert.Fail();
        }

        [Test]
        public void GetCellVisibility_DefaultsToZero() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            Assert.AreEqual(0, canon.GetVisibilityOfCellToCiv(cell, civ));
        }

        [Test]
        public void IncreaseCellVisibilityToCiv_VisibilityIncreasedByOne() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cell, civ);

            Assert.AreEqual(1, canon.GetVisibilityOfCellToCiv(cell, civ));
        }

        [Test]
        public void IncreaseCellVisibilityToCiv_AndVisibilityWasZero_CellVisibilityRefreshed() {
            Mock<IHexCell> mockCell;

            var cellToTest = BuildCell(out mockCell);
            var civToTest  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.Verify(cell => cell.RefreshVisibility(), Times.Once);
        }

        [Test]
        public void IncreaseCellVisibilityToCiv_AndVisibilityWasGreaterThanZero_CellVisibilityNotRefreshed() {
            Mock<IHexCell> mockCell;

            var cellToTest = BuildCell(out mockCell);
            var civToTest  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.ResetCalls();

            canon.IncreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.Verify(cell => cell.RefreshVisibility(), Times.Never);
        }

        [Test]
        public void DecreaseCellVisibilityToCiv_VisibilityDecreasedByOne() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.DecreaseCellVisibilityToCiv(cell, civ);

            Assert.AreEqual(-1, canon.GetVisibilityOfCellToCiv(cell, civ));
        }

        [Test]
        public void DecreaseCellVisibilityToCiv_AndVisibilityWasZero_CellVisibilityNotRefreshed() {
            Mock<IHexCell> mockCell;

            var cellToTest = BuildCell(out mockCell);
            var civToTest  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.DecreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.Verify(cell => cell.RefreshVisibility(), Times.Never);
        }

        [Test]
        public void DecreaseCellVisibilityToCiv_AndVisibilityWasOne_CellVisibilityRefreshed() {
            Mock<IHexCell> mockCell;

            var cellToTest = BuildCell(out mockCell);
            var civToTest  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.ResetCalls();

            canon.DecreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.Verify(cell => cell.RefreshVisibility(), Times.Once);
        }

        [Test]
        public void DecreaseCellVisibilityToCiv_AndVisibilityWasGreaterThanOne_CellVisibilityNotRefreshed() {
            Mock<IHexCell> mockCell;

            var cellToTest = BuildCell(out mockCell);
            var civToTest  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cellToTest, civToTest);
            canon.IncreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.ResetCalls();

            canon.DecreaseCellVisibilityToCiv(cellToTest, civToTest);

            mockCell.Verify(cell => cell.RefreshVisibility(), Times.Never);
        }

        [Test]
        public void IsCellVisibleToCiv_FalseIfCellVisibilityZero() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            Assert.IsFalse(canon.IsCellVisibleToCiv(cell, civ));
        }

        [Test]
        public void IsCellVisibleToCiv_TrueIfCellVisibilityGreaterThanZero() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cell, civ);

            Assert.IsTrue(canon.IsCellVisibleToCiv(cell, civ));
        }

        [Test]
        public void IsCellVisible_AndVisibilityModeActiveCiv_ReturnsVisibilityStatusForActiveCiv() {
            var cellOne = BuildCell();
            var cellTwo = BuildCell();

            var activeCiv = BuildCiv();
            var otherCiv  = BuildCiv();

            MockGameCore.Setup(core => core.ActiveCiv).Returns(activeCiv);

            var canon = Container.Resolve<VisibilityCanon>();

            canon.CellVisibilityMode = CellVisibilityMode.ActiveCiv;

            canon.IncreaseCellVisibilityToCiv(cellOne, activeCiv);
            canon.IncreaseCellVisibilityToCiv(cellTwo, otherCiv);

            Assert.IsTrue (canon.IsCellVisible(cellOne), "CellOne is incorrectly invisible");
            Assert.IsFalse(canon.IsCellVisible(cellTwo), "CellTwo is incorrectly visible");
        }

        [Test]
        public void IsCellVisible_AndVisibilityModeActiveCiv_ReturnsFalseIfActiveCivNull() {
            var cell = BuildCell();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.CellVisibilityMode = CellVisibilityMode.ActiveCiv;

            Assert.IsFalse(canon.IsCellVisible(cell));
        }

        [Test]
        public void IsCellVisible_AndVisibilityModeRevealAll_ReturnsTrue() {
            var cell = BuildCell();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.CellVisibilityMode = CellVisibilityMode.RevealAll;

            Assert.IsTrue(canon.IsCellVisible(cell));
        }

        [Test]
        public void IsCellVisible_AndVisibilityModeHideAll_ReturnsFalse() {
            var cell = BuildCell();
            var civ  = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cell, civ);

            canon.CellVisibilityMode = CellVisibilityMode.HideAll;

            Assert.IsFalse(canon.IsCellVisible(cell));
        }

        [Test]
        public void ClearCellVisibility_AllVisibilityDataReset() {
            var cellOne = BuildCell();
            var cellTwo = BuildCell();

            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.IncreaseCellVisibilityToCiv(cellOne, civOne);
            canon.IncreaseCellVisibilityToCiv(cellOne, civTwo);
            canon.IncreaseCellVisibilityToCiv(cellTwo, civOne);
            canon.IncreaseCellVisibilityToCiv(cellTwo, civTwo);

            canon.ClearCellVisibility();

            Assert.AreEqual(0, canon.GetVisibilityOfCellToCiv(cellOne, civOne), "CellOne's visibility to CivOne not cleared");
            Assert.AreEqual(0, canon.GetVisibilityOfCellToCiv(cellOne, civTwo), "CellOne's visibility to CivTwo not cleared");
            Assert.AreEqual(0, canon.GetVisibilityOfCellToCiv(cellTwo, civOne), "CellTwo's visibility to CivOne not cleared");
            Assert.AreEqual(0, canon.GetVisibilityOfCellToCiv(cellTwo, civTwo), "CellTwo's visibility to CivTwo not cleared");
        }




        [Test]
        public void IsResourceVisible_AndResourceVisibilityActiveCiv_ReturnsResourceDiscoveryByActiveCiv() {
            var resourceOne = BuildResource();
            var resourceTwo = BuildResource();

            var activeCiv = BuildCiv();
            var otherCiv  = BuildCiv();

            MockTechCanon.Setup(canon => canon.IsResourceDiscoveredByCiv(resourceOne, activeCiv)).Returns(true);
            MockTechCanon.Setup(canon => canon.IsResourceDiscoveredByCiv(resourceTwo, otherCiv)) .Returns(true);

            MockGameCore.Setup(core => core.ActiveCiv).Returns(activeCiv);

            var visibilityCanon = Container.Resolve<VisibilityCanon>();

            visibilityCanon.ResourceVisibilityMode = ResourceVisibilityMode.ActiveCiv;

            Assert.IsTrue (visibilityCanon.IsResourceVisible(resourceOne), "ResourceOne is incorrectly invisible");
            Assert.IsFalse(visibilityCanon.IsResourceVisible(resourceTwo), "ResourceTwo is incorrectly visible");
        }

        [Test]
        public void IsResourceVisible_AndResourceVisibilityActiveCiv_ReturnsFalseIfActiveCivNull() {
            var resource = BuildResource();

            var visibilityCanon = Container.Resolve<VisibilityCanon>();

            Assert.IsFalse(visibilityCanon.IsResourceVisible(resource));
        }

        [Test]
        public void IsResourceVisible_AndResourceVisibilityRevealAll_ReturnsTrue() {
            var resource = BuildResource();

            var canon = Container.Resolve<VisibilityCanon>();

            canon.ResourceVisibilityMode = ResourceVisibilityMode.RevealAll;

            Assert.IsTrue(canon.IsResourceVisible(resource));
        }

        [Test]
        public void IsResourceVisible_AndResourceVisibilityHideAll_ReturnsFalse() {
            var resource = BuildResource();
            var civ      = BuildCiv();

            MockGameCore.Setup(core => core.ActiveCiv).Returns(civ);

            MockTechCanon.Setup(techCanon => techCanon.IsResourceDiscoveredByCiv(resource, civ)).Returns(true);

            var visibilityCanon = Container.Resolve<VisibilityCanon>();

            visibilityCanon.ResourceVisibilityMode = ResourceVisibilityMode.HideAll;

            Assert.IsFalse(visibilityCanon.IsResourceVisible(resource));
        }

        [Test]
        public void MissingSignalTests() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IHexCell BuildCell(out Mock<IHexCell> mock) {
            mock = new Mock<IHexCell>();

            return mock.Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IResourceDefinition BuildResource() {
            return new Mock<IResourceDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
