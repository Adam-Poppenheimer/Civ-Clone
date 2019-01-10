using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Barbarians;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianUnitSpawnerTests : ZenjectUnitTestFixture {

        #region instance fields and properties
               
        private Mock<ICivilizationFactory>          MockCivFactory;
        private Mock<IBarbarianConfig>              MockBarbarianConfig;
        private Mock<IUnitFactory>                  MockUnitFactory;
        private Mock<IRandomizer>                   MockRandomizer;
        private Mock<IBarbarianSpawningTools>       MockSpawningTools;
        private Mock<IBarbarianAvailableUnitsLogic> MockAvailableUnitsLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCivFactory          = new Mock<ICivilizationFactory>();
            MockBarbarianConfig     = new Mock<IBarbarianConfig>();
            MockUnitFactory         = new Mock<IUnitFactory>();
            MockRandomizer          = new Mock<IRandomizer>();
            MockSpawningTools       = new Mock<IBarbarianSpawningTools>();
            MockAvailableUnitsLogic = new Mock<IBarbarianAvailableUnitsLogic>();

            Container.Bind<ICivilizationFactory>         ().FromInstance(MockCivFactory         .Object);
            Container.Bind<IBarbarianConfig>             ().FromInstance(MockBarbarianConfig    .Object);
            Container.Bind<IUnitFactory>                 ().FromInstance(MockUnitFactory        .Object);
            Container.Bind<IRandomizer>                  ().FromInstance(MockRandomizer         .Object);
            Container.Bind<IBarbarianSpawningTools>      ().FromInstance(MockSpawningTools      .Object);
            Container.Bind<IBarbarianAvailableUnitsLogic>().FromInstance(MockAvailableUnitsLogic.Object);

            Container.Bind<BarbarianUnitSpawner>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void TrySpawnUnit_WaterSpawningSelected_AndValidWaterSpawn_CreatesUnitOfTemplateAtLocation() {
            var encampment = BuildEncampment();

            var locationOfUnit  = BuildCell();
            var templateToBuild = BuildUnitTemplate();

            var spawnInfo = new UnitSpawnInfo() {
                IsValidSpawn = true, LocationOfUnit = locationOfUnit, TemplateToBuild = templateToBuild
            };

            Func<IHexCell, IEnumerable<IUnitTemplate>> selector = cell => new List<IUnitTemplate>();

            MockAvailableUnitsLogic.Setup(logic => logic.NavalTemplateSelector).Returns(selector);

            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, selector)).Returns(spawnInfo);

            var barbarianCiv = BuildCiv();

            MockCivFactory.Setup(factory => factory.BarbarianCiv).Returns(barbarianCiv);

            MockRandomizer.Setup(randomizer => randomizer.GetRandom01()).Returns(-1f);

            var unitSpawner = Container.Resolve<BarbarianUnitSpawner>();

            Assert.IsTrue(unitSpawner.TrySpawnUnit(encampment), "TrySpawnUnit did not return true as expected");

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(locationOfUnit, templateToBuild, barbarianCiv),
                Times.Once, "UnitFactory.BuildNot not called as expected"
            );
        }

        [Test]
        public void TrySpawnUnit_WaterSpawningSelected_AndInvalidWaterSpawn_CreatesLandSpawnIfLandSpawnValid() {
            var encampment = BuildEncampment();

            var landLocation  = BuildCell();
            var waterLocation = BuildCell();

            var landTemplate  = BuildUnitTemplate();
            var waterTemplate = BuildUnitTemplate();

            var landSpawnInfo  = new UnitSpawnInfo() { IsValidSpawn = true,  LocationOfUnit = landLocation,  TemplateToBuild = landTemplate };
            var WaterSpawnInfo = new UnitSpawnInfo() { IsValidSpawn = false, LocationOfUnit = waterLocation, TemplateToBuild = waterTemplate };

            Func<IHexCell, IEnumerable<IUnitTemplate>> landSelector  = cell => new List<IUnitTemplate>();
            Func<IHexCell, IEnumerable<IUnitTemplate>> navalSelector = cell => new List<IUnitTemplate>();

            MockAvailableUnitsLogic.Setup(logic => logic.LandTemplateSelector) .Returns(landSelector);
            MockAvailableUnitsLogic.Setup(logic => logic.NavalTemplateSelector).Returns(navalSelector);

            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, landSelector )).Returns(landSpawnInfo);
            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, navalSelector)).Returns(WaterSpawnInfo);

            var barbarianCiv = BuildCiv();

            MockCivFactory.Setup(factory => factory.BarbarianCiv).Returns(barbarianCiv);

            MockRandomizer.Setup(randomizer => randomizer.GetRandom01()).Returns(-1f);

            var unitSpawner = Container.Resolve<BarbarianUnitSpawner>();

            Assert.IsTrue(unitSpawner.TrySpawnUnit(encampment), "TrySpawnUnit did not return true as expected");

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(landLocation, landTemplate, barbarianCiv), Times.Once,
                "Land template was not created as expected"
            );

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(waterLocation, waterTemplate, barbarianCiv), Times.Never,
                "Water template was unexpectedly created"
            );
        }

        [Test]
        public void TrySpawnUnit_WaterSpawningSelected_AndInvalidWaterSpawn_DoesNothingIfLandSpawnInvalid() {
            var encampment = BuildEncampment();

            var landLocation  = BuildCell();
            var waterLocation = BuildCell();

            var landTemplate  = BuildUnitTemplate();
            var waterTemplate = BuildUnitTemplate();

            var landSpawnInfo  = new UnitSpawnInfo() { IsValidSpawn = false, LocationOfUnit = landLocation,  TemplateToBuild = landTemplate };
            var WaterSpawnInfo = new UnitSpawnInfo() { IsValidSpawn = false, LocationOfUnit = waterLocation, TemplateToBuild = waterTemplate };

            Func<IHexCell, IEnumerable<IUnitTemplate>> landSelector  = cell => new List<IUnitTemplate>();
            Func<IHexCell, IEnumerable<IUnitTemplate>> navalSelector = cell => new List<IUnitTemplate>();

            MockAvailableUnitsLogic.Setup(logic => logic.LandTemplateSelector) .Returns(landSelector);
            MockAvailableUnitsLogic.Setup(logic => logic.NavalTemplateSelector).Returns(navalSelector);

            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, landSelector )).Returns(landSpawnInfo);
            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, navalSelector)).Returns(WaterSpawnInfo);

            var barbarianCiv = BuildCiv();

            MockCivFactory.Setup(factory => factory.BarbarianCiv).Returns(barbarianCiv);

            MockRandomizer.Setup(randomizer => randomizer.GetRandom01()).Returns(-1f);

            var unitSpawner = Container.Resolve<BarbarianUnitSpawner>();

            Assert.IsFalse(unitSpawner.TrySpawnUnit(encampment), "TrySpawnUnit did not return false as expected");

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(It.IsAny<IHexCell>(), It.IsAny<IUnitTemplate>(), It.IsAny<ICivilization>()),
                Times.Never, "UnitFactory.BuildUnit unexpectedly called"
            );
        }

        [Test]
        public void TrySpawnUnit_LandSpawningSelected_AndValidLandSpawn_CreatesUnitOfTemplateAtLocation() {
            var encampment = BuildEncampment();

            var landLocation  = BuildCell();
            var waterLocation = BuildCell();

            var landTemplate  = BuildUnitTemplate();
            var waterTemplate = BuildUnitTemplate();

            var landSpawnInfo  = new UnitSpawnInfo() { IsValidSpawn = true, LocationOfUnit = landLocation,  TemplateToBuild = landTemplate };
            var WaterSpawnInfo = new UnitSpawnInfo() { IsValidSpawn = true, LocationOfUnit = waterLocation, TemplateToBuild = waterTemplate };

            Func<IHexCell, IEnumerable<IUnitTemplate>> landSelector  = cell => new List<IUnitTemplate>();
            Func<IHexCell, IEnumerable<IUnitTemplate>> navalSelector = cell => new List<IUnitTemplate>();

            MockAvailableUnitsLogic.Setup(logic => logic.LandTemplateSelector) .Returns(landSelector);
            MockAvailableUnitsLogic.Setup(logic => logic.NavalTemplateSelector).Returns(navalSelector);

            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, landSelector )).Returns(landSpawnInfo);
            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, navalSelector)).Returns(WaterSpawnInfo);

            var barbarianCiv = BuildCiv();

            MockCivFactory.Setup(factory => factory.BarbarianCiv).Returns(barbarianCiv);

            MockRandomizer.Setup(randomizer => randomizer.GetRandom01()).Returns(1f);

            var unitSpawner = Container.Resolve<BarbarianUnitSpawner>();

            Assert.IsTrue(unitSpawner.TrySpawnUnit(encampment), "TrySpawnUnit did not return true as expected");

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(landLocation, landTemplate, barbarianCiv),
                Times.Once, "UnitFactory.BuildUnit not called as expected"
            );
        }

        [Test]
        public void TrySpawnUnit_LandSpawningSelected_AndInvalidLandSpawn_DoesNothingAndReturnsFalse() {
            var encampment = BuildEncampment();

            var landLocation  = BuildCell();
            var waterLocation = BuildCell();

            var landTemplate  = BuildUnitTemplate();
            var waterTemplate = BuildUnitTemplate();

            var landSpawnInfo  = new UnitSpawnInfo() { IsValidSpawn = false, LocationOfUnit = landLocation,  TemplateToBuild = landTemplate };
            var WaterSpawnInfo = new UnitSpawnInfo() { IsValidSpawn = true,  LocationOfUnit = waterLocation, TemplateToBuild = waterTemplate };

            Func<IHexCell, IEnumerable<IUnitTemplate>> landSelector  = cell => new List<IUnitTemplate>();
            Func<IHexCell, IEnumerable<IUnitTemplate>> navalSelector = cell => new List<IUnitTemplate>();

            MockAvailableUnitsLogic.Setup(logic => logic.LandTemplateSelector) .Returns(landSelector);
            MockAvailableUnitsLogic.Setup(logic => logic.NavalTemplateSelector).Returns(navalSelector);

            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, landSelector )).Returns(landSpawnInfo);
            MockSpawningTools.Setup(tools => tools.TryGetValidSpawn(encampment, navalSelector)).Returns(WaterSpawnInfo);

            var barbarianCiv = BuildCiv();

            MockCivFactory.Setup(factory => factory.BarbarianCiv).Returns(barbarianCiv);

            MockRandomizer.Setup(randomizer => randomizer.GetRandom01()).Returns(1f);

            var unitSpawner = Container.Resolve<BarbarianUnitSpawner>();

            Assert.IsFalse(unitSpawner.TrySpawnUnit(encampment), "TrySpawnUnit did not return false as expected");

            MockUnitFactory.Verify(
                factory => factory.BuildUnit(It.IsAny<IHexCell>(), It.IsAny<IUnitTemplate>(), It.IsAny<ICivilization>()),
                Times.Never, "UnitFactory.BuildUnit was unexpectedly called"
            );
        }

        #endregion

        #region utilities

        private IEncampment BuildEncampment() {
            return new Mock<IEncampment>().Object;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnitTemplate BuildUnitTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
