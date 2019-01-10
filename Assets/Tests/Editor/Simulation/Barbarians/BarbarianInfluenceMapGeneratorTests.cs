using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianInfluenceMapGeneratorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                                      MockGrid;
        private Mock<IUnitFactory>                                  MockUnitFactory;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IUnitStrengthEstimator>                        MockUnitStrengthEstimator;
        private Mock<IAIConfig>                                     MockAIConfig;

        int nextIndex;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            nextIndex = 0;

            MockGrid                  = new Mock<IHexGrid>();
            MockUnitFactory           = new Mock<IUnitFactory>();
            MockUnitPossessionCanon   = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockUnitPositionCanon     = new Mock<IUnitPositionCanon>();
            MockUnitStrengthEstimator = new Mock<IUnitStrengthEstimator>();
            MockAIConfig              = new Mock<IAIConfig>();

            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                 .Object);
            Container.Bind<IUnitFactory>                                 ().FromInstance(MockUnitFactory          .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon  .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon    .Object);
            Container.Bind<IUnitStrengthEstimator>                       ().FromInstance(MockUnitStrengthEstimator.Object);
            Container.Bind<IAIConfig>                                    ().FromInstance(MockAIConfig             .Object);

            Container.Bind<BarbarianInfluenceMapGenerator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GenerateMaps_EachValueForEachCellZeroedOut() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(new List<IUnit>());

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(3, influenceMaps.AllyPresence .Length, "AllyPresence has an unexpected length");
            Assert.AreEqual(3, influenceMaps.EnemyPresence.Length, "EnemyPresence has an unexpected length");

            for(int i = 0; i < 3; i++) {
                Assert.AreEqual(0f, influenceMaps.AllyPresence [i], string.Format("AllyPresence [{0}] has an unexpected value", i));
                Assert.AreEqual(0f, influenceMaps.EnemyPresence[i], string.Format("EnemyPresence[{0}] has an unexpected value", i));
            }
        }

        [Test]
        public void GenerateMaps_BarbarianUnitsApplyStrengthAsAllyPresence_ToLocation() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(true)), 25f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(25f, influenceMaps.AllyPresence[0]);
        }

        [Test]
        public void GenerateMaps_BarbarianUnitsApplyStrengthAsAllyPresence_ToNearbyCells_ReducedByDistance() {
            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 1)).Returns(new List<IHexCell>() { cells[1] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 2)).Returns(new List<IHexCell>() { cells[2] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 3)).Returns(new List<IHexCell>() { cells[3] });

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(true)), 25f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(25f / 2f, influenceMaps.AllyPresence[1], "Cell one away from unit's location has an unexpected AllyPresence");
            Assert.AreEqual(25f / 4f, influenceMaps.AllyPresence[2], "Cell two away from unit's location has an unexpected AllyPresence");
            Assert.AreEqual(25f / 8f, influenceMaps.AllyPresence[3], "Cell three away from unit's location has an unexpected AllyPresence");
        }

        [Test]
        public void GenerateMaps_SpilloverInfluenceFromBarbarianUnits_LimitedToMaxInfluenceRadius() {
            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(1);

            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 1)).Returns(new List<IHexCell>() { cells[1] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 2)).Returns(new List<IHexCell>() { cells[2] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 3)).Returns(new List<IHexCell>() { cells[3] });

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(true)), 25f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(25f / 2f, influenceMaps.AllyPresence[1], "Cell one away from unit's location has an unexpected AllyPresence");
            Assert.AreEqual(0f,       influenceMaps.AllyPresence[2], "Cell two away from unit's location has an unexpected AllyPresence");
            Assert.AreEqual(0f,       influenceMaps.AllyPresence[3], "Cell three away from unit's location has an unexpected AllyPresence");
        }

        [Test]
        public void GenerateMaps_NonBarbarianUnitsApplyStrengthAsEnemyPresence_ToLocation() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(false)), 25f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(25f, influenceMaps.EnemyPresence[0]);
        }

        [Test]
        public void GenerateMaps_NonBarbarianUnitsApplyStrengthAsEnemyPresence_ToNearbyCells_ReducedByDistance() {
            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 1)).Returns(new List<IHexCell>() { cells[1] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 2)).Returns(new List<IHexCell>() { cells[2] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 3)).Returns(new List<IHexCell>() { cells[3] });

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(false)), 25f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(25f / 2f, influenceMaps.EnemyPresence[1], "Cell one away from unit's location has an unexpected EnemyPresence");
            Assert.AreEqual(25f / 4f, influenceMaps.EnemyPresence[2], "Cell two away from unit's location has an unexpected EnemyPresence");
            Assert.AreEqual(25f / 8f, influenceMaps.EnemyPresence[3], "Cell three away from unit's location has an unexpected EnemyPresence");
        }

        [Test]
        public void GenerateMaps_SpilloverInfluenceFromNonBarbarianUnits_LimitedToMaxInfluenceRadius() {
            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(1);

            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell(),
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 1)).Returns(new List<IHexCell>() { cells[1] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 2)).Returns(new List<IHexCell>() { cells[2] });
            MockGrid.Setup(grid => grid.GetCellsInRing(cells[0], 3)).Returns(new List<IHexCell>() { cells[3] });

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(false)), 25f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(25f / 2f, influenceMaps.EnemyPresence[1], "Cell one away from unit's location has an unexpected EnemyPresence");
            Assert.AreEqual(0f,       influenceMaps.EnemyPresence[2], "Cell two away from unit's location has an unexpected EnemyPresence");
            Assert.AreEqual(0f,       influenceMaps.EnemyPresence[3], "Cell three away from unit's location has an unexpected EnemyPresence");
        }

        [Test]
        public void GenerateMaps_MultipleSourcesOfAllyPresenceStack() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(true)), 25f),
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(true)), 43f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(68f, influenceMaps.AllyPresence[0]);
        }

        [Test]
        public void GenerateMaps_MultipleSourcesOfEnemyPresenceStack() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var units = new List<IUnit>() {
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(false)), 25f),
                BuildUnit(cells[0], BuildCiv(BuildCivTemplate(false)), 43f)
            };

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(units);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            Assert.AreEqual(68f, influenceMaps.EnemyPresence[0]);
        }

        [Test]
        public void ClearMaps_AllMapsSetToNull() {
            var cells = new List<IHexCell>() { BuildCell(), BuildCell(), BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(new List<IUnit>().AsReadOnly());

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var influenceMaps = mapGenerator.GenerateMaps();

            mapGenerator.ClearMaps();

            Assert.IsNull(influenceMaps.AllyPresence,  "AllyPresence not set to null");
            Assert.IsNull(influenceMaps.EnemyPresence, "EnemyPresence not set to null");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Index).Returns(nextIndex++);

            return mockCell.Object;
        }

        private ICivilizationTemplate BuildCivTemplate(bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(ICivilizationTemplate template) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.Template).Returns(template);
            
            return mockCiv.Object;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner, float strength) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitStrength(newUnit)).Returns(strength);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
