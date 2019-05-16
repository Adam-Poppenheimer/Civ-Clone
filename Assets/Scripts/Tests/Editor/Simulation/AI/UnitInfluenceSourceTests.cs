using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.AI;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.AI {

    public class UnitInfluenceSourceTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitFactory>                                  MockUnitFactory;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IUnitStrengthEstimator>                        MockUnitStrengthEstimator;
        private Mock<IInfluenceMapApplier>                          MockInfluenceMapApplier;
        private Mock<IAIConfig>                                     MockAIConfig;
        private Mock<IWarCanon>                                     MockWarCanon;

        private List<IUnit> AllUnits = new List<IUnit>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllUnits.Clear();

            MockUnitFactory           = new Mock<IUnitFactory>();
            MockUnitPossessionCanon   = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockUnitPositionCanon     = new Mock<IUnitPositionCanon>();
            MockUnitStrengthEstimator = new Mock<IUnitStrengthEstimator>();
            MockInfluenceMapApplier   = new Mock<IInfluenceMapApplier>();
            MockAIConfig              = new Mock<IAIConfig>();
            MockWarCanon              = new Mock<IWarCanon>();

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(AllUnits);

            Container.Bind<IUnitFactory>                                 ().FromInstance(MockUnitFactory          .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon  .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon    .Object);
            Container.Bind<IUnitStrengthEstimator>                       ().FromInstance(MockUnitStrengthEstimator.Object);
            Container.Bind<IInfluenceMapApplier>                         ().FromInstance(MockInfluenceMapApplier  .Object);
            Container.Bind<IAIConfig>                                    ().FromInstance(MockAIConfig             .Object);
            Container.Bind<IWarCanon>                                    ().FromInstance(MockWarCanon             .Object);

            Container.Bind<UnitInfluenceSource>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ApplyToMaps_AppliesStrengthOfDomesticUnits_ToAllyPresence() {
            var domesticCiv = BuildCiv();
            var foreignCiv = BuildCiv();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();
            var cellFour  = BuildCell();

            BuildUnit(cellOne,   domesticCiv, 10f);
            BuildUnit(cellTwo,   foreignCiv,  20f);
            BuildUnit(cellThree, domesticCiv, 30f);
            BuildUnit(cellFour,  foreignCiv,  40f);

            InfluenceRolloff rolloff = (a, b) => 0f;
            InfluenceApplier applier = (a, b) => 0;

            MockInfluenceMapApplier.Setup(mapApplier => mapApplier.PowerOfTwoRolloff).Returns(rolloff);
            MockInfluenceMapApplier.Setup(mapApplier => mapApplier.ApplySum)         .Returns(applier);

            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 1f }, EnemyPresence = new float[] { 1f, 2f }
            };

            var influenceSource = Container.Resolve<UnitInfluenceSource>();

            influenceSource.ApplyToMaps(maps, domesticCiv);

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(10f, maps.AllyPresence, cellOne, 3, rolloff, applier),
                Times.Once, "Influence not applied on CellOne as expected"
            );

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(20f, maps.AllyPresence, cellTwo, 3, rolloff, applier),
                Times.Never, "Influence unexpectedly applied on CellTwo"
            );

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(30f, maps.AllyPresence, cellThree, 3, rolloff, applier),
                Times.Once, "Influence not applied on CellThree as expected"
            );

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(40f, maps.AllyPresence, cellFour, 3, rolloff, applier),
                Times.Never, "Influence unexpectedly applied on CellFour"
            );
        }

        [Test]
        public void ApplyToMaps_AppliesStrengthOfForeignUnitsAtWarWithCiv_ToEnemyPresence() {
            var domesticCiv = BuildCiv();
            var foreignCivOne = BuildCiv();
            var foreignCivTwo = BuildCiv();

            MockWarCanon.Setup(canon => canon.AreAtWar(foreignCivOne, domesticCiv  )).Returns(true);
            MockWarCanon.Setup(canon => canon.AreAtWar(domesticCiv,   foreignCivOne)).Returns(true);
            MockWarCanon.Setup(canon => canon.AreAtWar(foreignCivTwo, domesticCiv  )).Returns(true);
            MockWarCanon.Setup(canon => canon.AreAtWar(domesticCiv,   foreignCivTwo)).Returns(true);

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();
            var cellFour  = BuildCell();

            BuildUnit(cellOne,   domesticCiv,    10f);
            BuildUnit(cellTwo,   foreignCivOne,  20f);
            BuildUnit(cellThree, domesticCiv,    30f);
            BuildUnit(cellFour,  foreignCivTwo,  40f);

            InfluenceRolloff rolloff = (a, b) => 0f;
            InfluenceApplier applier = (a, b) => 0;

            MockInfluenceMapApplier.Setup(mapApplier => mapApplier.PowerOfTwoRolloff).Returns(rolloff);
            MockInfluenceMapApplier.Setup(mapApplier => mapApplier.ApplySum)         .Returns(applier);

            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 1f }, EnemyPresence = new float[] { 1f, 2f }
            };

            var influenceSource = Container.Resolve<UnitInfluenceSource>();

            influenceSource.ApplyToMaps(maps, domesticCiv);

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(10f, maps.EnemyPresence, cellOne, 3, rolloff, applier),
                Times.Never, "Influence unexpectedly applied on CellOne"
            );

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(20f, maps.EnemyPresence, cellTwo, 3, rolloff, applier),
                Times.Once, "Influence not applied on CellTwo as expected"
            );

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(30f, maps.EnemyPresence, cellThree, 3, rolloff, applier),
                Times.Never, "Influence unexpectedly applied on CellThree"
            );

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(40f, maps.EnemyPresence, cellFour, 3, rolloff, applier),
                Times.Once, "Influence not applied on CellFour as expected"
            );
        }

        [Test]
        public void ApplyToMaps_DoesNothingWithForeignUnitsNotAtWarWithCiv() {
            var domesticCiv = BuildCiv();
            var foreignCivOne = BuildCiv();
            var foreignCivTwo = BuildCiv();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();
            var cellFour  = BuildCell();

            BuildUnit(cellOne,   foreignCivOne, 10f);
            BuildUnit(cellTwo,   foreignCivTwo, 20f);
            BuildUnit(cellThree, foreignCivOne, 30f);
            BuildUnit(cellFour,  foreignCivTwo, 40f);

            InfluenceRolloff rolloff = (a, b) => 0f;
            InfluenceApplier applier = (a, b) => 0;

            MockInfluenceMapApplier.Setup(mapApplier => mapApplier.PowerOfTwoRolloff).Returns(rolloff);
            MockInfluenceMapApplier.Setup(mapApplier => mapApplier.ApplySum)         .Returns(applier);

            MockAIConfig.Setup(config => config.UnitMaxInfluenceRadius).Returns(3);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 1f }, EnemyPresence = new float[] { 1f, 2f }
            };

            var influenceSource = Container.Resolve<UnitInfluenceSource>();

            influenceSource.ApplyToMaps(maps, domesticCiv);

            MockInfluenceMapApplier.Verify(
                mapApplier => mapApplier.ApplyInfluenceToMap(
                    It.IsAny<float>(), It.IsAny<float[]>(), It.IsAny<IHexCell>(), It.IsAny<int>(),
                    It.IsAny<InfluenceRolloff>(), It.IsAny<InfluenceApplier>()
                ), Times.Never
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner, float strength) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);
            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitStrength(newUnit)).Returns(strength);

            AllUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
