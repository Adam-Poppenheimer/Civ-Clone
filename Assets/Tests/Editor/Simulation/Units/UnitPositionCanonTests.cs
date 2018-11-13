using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitPositionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexMapSimulationConfig>                       MockHexSimulationConfig;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private HexCellSignals CellSignals;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockHexSimulationConfig = new Mock<IHexMapSimulationConfig>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            CellSignals = new HexCellSignals();

            Container.Bind<IHexMapSimulationConfig>                      ().FromInstance(MockHexSimulationConfig.Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<HexCellSignals>().FromInstance(CellSignals);

            Container.Bind<UnitPositionCanon>().AsSingle();

            Container.Bind<UnitSignals>().AsSingle();
        }

        private void SetConfigData(UnitPositionCanonTestData.ConfigData configData) {
            MockHexSimulationConfig.Setup(config => config.GetBaseMoveCostOfTerrain   (It.IsAny<CellTerrain>()))   .Returns(configData.TerrainMoveCost);
            MockHexSimulationConfig.Setup(config => config.GetBaseMoveCostOfShape     (It.IsAny<CellShape>()))     .Returns(configData.ShapeMoveCost);
            MockHexSimulationConfig.Setup(config => config.GetBaseMoveCostOfVegetation(It.IsAny<CellVegetation>())).Returns(configData.VegetationMoveCost);
            MockHexSimulationConfig.Setup(config => config.GetBaseMoveCostOfFeature   (It.IsAny<CellFeature>()))   .Returns(configData.FeatureMoveCost);

            MockHexSimulationConfig.Setup(config => config.CityMoveCost).Returns(configData.CityMoveCost);

            MockHexSimulationConfig.Setup(config => config.RoadMoveCostMultiplier).Returns(configData.RoadMoveCostMultipler);
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource(sourceType: typeof(UnitPositionCanonTestData), sourceName: "CanPlaceUnitAtLocationAttackingTestCases")]
        [TestCaseSource(sourceType: typeof(UnitPositionCanonTestData), sourceName: "CanPlaceUnitAtLocationNormalTestCases")]        
        public bool CanPlaceUnitAtLocationTests(UnitPositionCanonTestData.UnitAtLocationData testData) {
            var domesticCiv = BuildCivilization("Domestic Civ");
            var foreignCiv  = BuildCivilization("Foreign Civ");

            var unit = BuildUnit(testData.Unit, domesticCiv, foreignCiv);

            IHexCell location = null;

            SetConfigData(testData.Config);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            if(testData.Location != null) {
                location = BuildCell(testData.Location, domesticCiv, foreignCiv, positionCanon);
            }

            return positionCanon.CanPlaceUnitAtLocation(unit, location, testData.IsMeleeAttacking);
        }

        [Test]
        [TestCaseSource(sourceType: typeof(UnitPositionCanonTestData), sourceName: "CanPlaceUnitTemplateAtLocationTestCases")]
        public bool CanPlaceUnitTemplateAtLocationTests(UnitPositionCanonTestData.TemplateAtLocationData testData) {
            var domesticCiv = BuildCivilization("Domestic Civ");
            var foreignCiv  = BuildCivilization("Foreign Civ");

            var template = BuildUnitTemplate(testData.Template);

            IHexCell location = null;

            SetConfigData(testData.Config);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            if(testData.Location != null) {
                location = BuildCell(testData.Location, domesticCiv, foreignCiv, positionCanon);
            }

            return positionCanon.CanPlaceUnitTemplateAtLocation(template, location, domesticCiv);
        }

        [Test]
        [TestCaseSource(sourceType: typeof(UnitPositionCanonTestData), sourceName: "GetTraversalCostForUnitTestCases")]
        public float GetTraversalCostForUnitTests(UnitPositionCanonTestData.TraversalCostData testData) {
            SetConfigData(testData.Config);

            var domesticCiv = BuildCivilization("Domestic Civ");
            var foreignCiv  = BuildCivilization("Foreign Civ");

            var unit = BuildUnit(testData.Unit, domesticCiv, foreignCiv);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            var currentCell = BuildCell(testData.CurrentCell, domesticCiv, foreignCiv, positionCanon);
            var nextCell    = BuildCell(testData.NextCell,    domesticCiv, foreignCiv, positionCanon);

            return positionCanon.GetTraversalCostForUnit(unit, currentCell, nextCell, testData.IsMeleeAttacking);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.Name).Returns(name);

            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

            return mockCiv.Object;
        }

        private IUnit BuildUnit(UnitPositionCanonTestData.UnitData unitData, ICivilization domesticCiv, ICivilization foreignCiv) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type)           .Returns(unitData.Type);
            mockUnit.Setup(unit => unit.MaxMovement)    .Returns(unitData.MaxMovement);
            mockUnit.Setup(unit => unit.MovementSummary).Returns(unitData.MovementSummary);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit))
                                   .Returns(unitData.BelongsToDomesticCiv ? domesticCiv : foreignCiv);

            return newUnit;
        }

        private IUnitTemplate BuildUnitTemplate(UnitPositionCanonTestData.UnitTemplateData templateData) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.Type)           .Returns(templateData.type);
            mockTemplate.Setup(template => template.MovementSummary).Returns(templateData.MovementSummary);

            return mockTemplate.Object;
        }

        //Does not add units to the cells that it creates
        private IHexCell BuildCell(
            UnitPositionCanonTestData.HexCellData cellData,
            ICivilization domesticCiv, ICivilization foreignCiv,
            UnitPositionCanon positionCanonToTest
        ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(cellData.Shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(cellData.Vegetation);
            mockCell.Setup(cell => cell.Feature)   .Returns(cellData.Feature);
            mockCell.Setup(cell => cell.HasRoads)  .Returns(cellData.HasRoads);

            var newCell = mockCell.Object;

            if(cellData.City != null) {
                BuildCity(cellData.City, newCell, domesticCiv, foreignCiv);
            }

            var unitsAtLocation = cellData.Units.Select(unitData => BuildUnit(unitData, domesticCiv, foreignCiv));

            foreach(var unitAlreadyAt in unitsAtLocation) {
                if(!positionCanonToTest.CanChangeOwnerOfPossession(unitAlreadyAt, newCell)) {
                    throw new InvalidOperationException("Could not place a unit on the cell that should've been placeable");
                }else {
                    positionCanonToTest.ChangeOwnerOfPossession(unitAlreadyAt, newCell);
                }
            }

            return newCell;
        }

        private ICity BuildCity(
            UnitPositionCanonTestData.CityData cityData, IHexCell location,
            ICivilization domesticCiv, ICivilization foreignCiv
        ) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<ICity>() { newCity });

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity))
                                   .Returns(cityData.BelongsToDomesticCiv ? domesticCiv : foreignCiv);

            return newCity;
        }

        #endregion

        #endregion

    }

}
