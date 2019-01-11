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
        private Mock<IInfluenceMapApplier>                          MockInfluenceMapApplier;

        private List<IHexCell> AllCells = new List<IHexCell>();
        private List<IUnit>    AllUnits = new List<IUnit>();

        int nextIndex;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();
            AllUnits.Clear();

            nextIndex = 0;

            MockGrid                    = new Mock<IHexGrid>();
            MockUnitFactory             = new Mock<IUnitFactory>();
            MockUnitPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockUnitPositionCanon       = new Mock<IUnitPositionCanon>();
            MockUnitStrengthEstimator   = new Mock<IUnitStrengthEstimator>();
            MockAIConfig                = new Mock<IAIConfig>();
            MockInfluenceMapApplier     = new Mock<IInfluenceMapApplier>();

            MockGrid             .Setup(factory => factory.Cells)         .Returns(AllCells.AsReadOnly());
            MockUnitFactory      .Setup(factory => factory.AllUnits)      .Returns(AllUnits);

            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                   .Object);
            Container.Bind<IUnitFactory>                                 ().FromInstance(MockUnitFactory            .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon    .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon      .Object);
            Container.Bind<IUnitStrengthEstimator>                       ().FromInstance(MockUnitStrengthEstimator  .Object);
            Container.Bind<IAIConfig>                                    ().FromInstance(MockAIConfig               .Object);
            Container.Bind<IInfluenceMapApplier>                         ().FromInstance(MockInfluenceMapApplier    .Object);

            Container.Bind<BarbarianInfluenceMapGenerator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GenerateMaps_EachValueForEachCellZeroedOut() {
            BuildCell();
            BuildCell();
            BuildCell();

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
        public void GenerateMaps_AppliesEachBarbaricUnitToAllyPresenceMapCorrectly() {
            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            var barbarianCiv    = BuildCiv(BuildCivTemplate(true));
            var nonBarbarianCiv = BuildCiv(BuildCivTemplate(false));

            BuildUnit(cellOne,   nonBarbarianCiv, 10f);
            BuildUnit(cellTwo,   barbarianCiv,    20f);
            BuildUnit(cellThree, barbarianCiv,    30f);

            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            InfluenceRolloff rolloffFunction = (a, b) => 0f;
            InfluenceApplier applierFunction = (a, b) => 0f;

            MockInfluenceMapApplier.Setup(influenceApplier => influenceApplier.PowerOfTwoRolloff).Returns(rolloffFunction);
            MockInfluenceMapApplier.Setup(influenceApplier => influenceApplier.ApplySum)         .Returns(applierFunction);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var maps = mapGenerator.GenerateMaps();

            //Moq apparently has a problem with arrays that have the same
            //collection of elements in them, so in order to get Moq to
            //distinguish between different array instances we have to
            //manually make them different
            maps.AllyPresence[0] = 1f;
            maps.EnemyPresence[0] = 2f;

            MockInfluenceMapApplier.Verify(
                influenceApplier => influenceApplier.ApplyInfluenceToMap(
                    10f, maps.AllyPresence, cellOne, 3, rolloffFunction, applierFunction
                ), Times.Never, "ApplyInfluenceToMap unexpectedly called on cellOne"
            );

            MockInfluenceMapApplier.Verify(
                influenceApplier => influenceApplier.ApplyInfluenceToMap(
                    20f, maps.AllyPresence, cellTwo, 3, rolloffFunction, applierFunction
                ), Times.Once, "ApplyInfluenceToMap not called on cellTwo as expected"
            );

            MockInfluenceMapApplier.Verify(
                influenceApplier => influenceApplier.ApplyInfluenceToMap(
                    30f, maps.AllyPresence, cellThree, 3, rolloffFunction, applierFunction
                ), Times.Once, "ApplyInfluenceToMap not called on cellThree as expected"
            );
        }

        [Test]
        public void GenerateMaps_AppliesEachNonBarbaricUnitToEnemyPresenceMapCorrectly() {
            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            var barbarianCiv    = BuildCiv(BuildCivTemplate(true));
            var nonBarbarianCiv = BuildCiv(BuildCivTemplate(false));

            BuildUnit(cellOne,   nonBarbarianCiv, 10f);
            BuildUnit(cellTwo,   barbarianCiv,    20f);
            BuildUnit(cellThree, barbarianCiv,    30f);

            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            InfluenceRolloff rolloffFunction = (a, b) => 0f;
            InfluenceApplier applierFunction = (a, b) => 0f;

            MockInfluenceMapApplier.Setup(influenceApplier => influenceApplier.PowerOfTwoRolloff).Returns(rolloffFunction);
            MockInfluenceMapApplier.Setup(influenceApplier => influenceApplier.ApplySum)         .Returns(applierFunction);

            var mapGenerator = Container.Resolve<BarbarianInfluenceMapGenerator>();

            var maps = mapGenerator.GenerateMaps();

            //Moq apparently has a problem with arrays that have the same
            //collection of elements in them, so in order to get Moq to
            //distinguish between different array instances we have to
            //manually make them different
            maps.AllyPresence[0] = 1f;
            maps.EnemyPresence[0] = 2f;

            MockInfluenceMapApplier.Verify(
                influenceApplier => influenceApplier.ApplyInfluenceToMap(
                    10f, maps.EnemyPresence, cellOne, 3, rolloffFunction, applierFunction
                ), Times.Once, "ApplyInfluenceToMap not called on cellOne as expected"
            );

            MockInfluenceMapApplier.Verify(
                influenceApplier => influenceApplier.ApplyInfluenceToMap(
                    20f, maps.EnemyPresence, cellTwo, 3, rolloffFunction, applierFunction
                ), Times.Never, "ApplyInfluenceToMap unexpectedly called on cellTwo"
            );

            MockInfluenceMapApplier.Verify(
                influenceApplier => influenceApplier.ApplyInfluenceToMap(
                    30f, maps.EnemyPresence, cellThree, 3, rolloffFunction, applierFunction
                ), Times.Never, "ApplyInfluenceToMap unexpectedly called on cellThree"
            );
        }

        [Test]
        public void ClearMaps_AllMapsSetToNull() {
            BuildCell();
            BuildCell();
            BuildCell();

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

            mockCell.Name = "Cell " + nextIndex.ToString();
            mockCell.Setup(cell => cell.Index).Returns(nextIndex++);

            var newCell = mockCell.Object;

            AllCells.Add(newCell);

            return newCell;
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

            AllUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
