using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Visibility;
using Assets.Simulation.Barbarians;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianEncampmentSpawnerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IEncampmentFactory>               MockEncampmentFactory;
        private Mock<IHexGrid>                         MockGrid;
        private Mock<IWeightedRandomSampler<IHexCell>> MockCellSampler;
        private Mock<IBarbarianSpawningTools>          MockSpawningTools;
        private Mock<IBarbarianUnitSpawner>            MockUnitSpawner;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockEncampmentFactory = new Mock<IEncampmentFactory>();
            MockGrid              = new Mock<IHexGrid>();
            MockCellSampler       = new Mock<IWeightedRandomSampler<IHexCell>>();
            MockSpawningTools     = new Mock<IBarbarianSpawningTools>();
            MockUnitSpawner       = new Mock<IBarbarianUnitSpawner>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IEncampmentFactory>              ().FromInstance(MockEncampmentFactory.Object);
            Container.Bind<IHexGrid>                        ().FromInstance(MockGrid             .Object);
            Container.Bind<IWeightedRandomSampler<IHexCell>>().FromInstance(MockCellSampler      .Object);
            Container.Bind<IBarbarianSpawningTools>         ().FromInstance(MockSpawningTools    .Object);
            Container.Bind<IBarbarianUnitSpawner>           ().FromInstance(MockUnitSpawner      .Object);

            Container.Bind<BarbarianEncampmentSpawner>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void TrySpawnEncampment_BuildsEncampmentFromRandomlySampledCandidate() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };

            var maps = new InfluenceMaps();

            Func<IHexCell, int> weightFunction = cell => 1;

            MockSpawningTools.Setup(tools => tools.EncampmentValidityFilter)           .Returns(cell => true);
            MockSpawningTools.Setup(tools => tools.BuildEncampmentWeightFunction(maps)).Returns(weightFunction);

            MockCellSampler.Setup(
                sampler => sampler.SampleElementsFromSet(It.IsAny<IEnumerable<IHexCell>>(), 1, weightFunction)
            ).Returns(new List<IHexCell>() { cells[0] });

            var encampmentSpawner = Container.Resolve<BarbarianEncampmentSpawner>();

            encampmentSpawner.TrySpawnEncampment(maps);

            MockEncampmentFactory.Verify(factory => factory.CreateEncampment(cells[0]), Times.Once);
        }

        [Test]
        public void TrySpawnEncampment_TriesToSpawnAUnitOnNewlyBuiltEncampments() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };

            var maps = new InfluenceMaps();

            Func<IHexCell, int> weightFunction = cell => 1;

            MockSpawningTools.Setup(tools => tools.EncampmentValidityFilter)           .Returns(cell => true);
            MockSpawningTools.Setup(tools => tools.BuildEncampmentWeightFunction(maps)).Returns(weightFunction);

            MockCellSampler.Setup(
                sampler => sampler.SampleElementsFromSet(It.IsAny<IEnumerable<IHexCell>>(), 1, weightFunction)
            ).Returns(new List<IHexCell>() { cells[0] });

            var newEncampment = BuildEncampment();

            MockEncampmentFactory.Setup(factory => factory.CreateEncampment(cells[0])).Returns(newEncampment);

            var encampmentSpawner = Container.Resolve<BarbarianEncampmentSpawner>();

            encampmentSpawner.TrySpawnEncampment(maps);

            MockUnitSpawner.Verify(spawner => spawner.TrySpawnUnit(newEncampment));
        }

        [Test]
        public void TrySpawnEncampment_FiltersCellsThroughCellValidityFilterOfSpawningTools() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };

            var maps = new InfluenceMaps();

            Func<IHexCell, bool> validityFilter = cell => cell == cells[2];
            Func<IHexCell, int>  weightFunction = cell => 1;

            MockSpawningTools.Setup(tools => tools.EncampmentValidityFilter)           .Returns(validityFilter);
            MockSpawningTools.Setup(tools => tools.BuildEncampmentWeightFunction(maps)).Returns(weightFunction);

            MockCellSampler.Setup(
                sampler => sampler.SampleElementsFromSet(It.IsAny<IEnumerable<IHexCell>>(), 1, weightFunction)
            ).Returns(new List<IHexCell>());

            var encampmentSpawner = Container.Resolve<BarbarianEncampmentSpawner>();

            encampmentSpawner.TrySpawnEncampment(maps);

            MockCellSampler.Verify(
                sampler => sampler.SampleElementsFromSet(
                    It.Is<IEnumerable<IHexCell>>(set => new HashSet<IHexCell>(set).SetEquals(new List<IHexCell>() { cells[2] })),
                    1, weightFunction
                ), Times.Once
            );
        }

        [Test]
        public void TrySpawnEncampment_DoesNothingIfNoValidCells() {
            BuildCell();
            BuildCell();
            BuildCell();

            var maps = new InfluenceMaps();

            Func<IHexCell, bool> validityFilter = cell => false;
            Func<IHexCell, int>  weightFunction = cell => 1;

            MockSpawningTools.Setup(tools => tools.EncampmentValidityFilter)           .Returns(validityFilter);
            MockSpawningTools.Setup(tools => tools.BuildEncampmentWeightFunction(maps)).Returns(weightFunction);

            var encampmentSpawner = Container.Resolve<BarbarianEncampmentSpawner>();

            encampmentSpawner.TrySpawnEncampment(maps);

            MockEncampmentFactory.Verify(factory => factory.CreateEncampment(It.IsAny<IHexCell>()), Times.Never);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IEncampment BuildEncampment() {
            return new Mock<IEncampment>().Object;
        }

        #endregion

        #endregion

    }

}
