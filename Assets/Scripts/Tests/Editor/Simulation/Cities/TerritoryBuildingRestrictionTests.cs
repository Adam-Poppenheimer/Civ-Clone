using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    public class TerritoryBuildingRestrictionTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                                 MockGrid;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<IRiverCanon>                              MockRiverCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid              = new Mock<IHexGrid>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockRiverCanon        = new Mock<IRiverCanon>();

            Container.Bind<IHexGrid>                                ().FromInstance(MockGrid             .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IRiverCanon>                             ().FromInstance(MockRiverCanon       .Object);

            Container.Bind<TerritoryBuildingRestriction>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsTemplateValidForCity_AndTemplateRequiresAdjacentRiver_TrueIfCityLocationHasRiver() {
            var template = BuildTemplate(requiresAdjacentRiver: true, requiresCoastalCity: false);

            var location = BuildCell(hasRiver: true, isWater: false);

            var city = BuildCity(location);            
            var civ  = BuildCiv();

            var restriction = Container.Resolve<TerritoryBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_AndTemplateRequiresAdjacentRiver_FalseIfCityLocationDoesntHaveRiver() {
            var template = BuildTemplate(requiresAdjacentRiver: true, requiresCoastalCity: false);

            var location = BuildCell(hasRiver: false, isWater: false);

            var city = BuildCity(location);            
            var civ  = BuildCiv();

            var restriction = Container.Resolve<TerritoryBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_AndTemplateRequiresAdjacentRiver_FalseIfOnlyAdjacentCellHasRiver() {
            var template = BuildTemplate(requiresAdjacentRiver: true, requiresCoastalCity: false);

            var location = BuildCell(false, false, BuildCell(true, true));

            var city = BuildCity(location);            
            var civ  = BuildCiv();

            var restriction = Container.Resolve<TerritoryBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_AndTemplateRequiresCoastalCity_TrueIfSomeAdjacentCellWater() {
            var template = BuildTemplate(requiresAdjacentRiver: false, requiresCoastalCity: true);

            var neighborOne = BuildCell(hasRiver: false, isWater: true);
            var neighborTwo = BuildCell(hasRiver: false, isWater: false);

            var location = BuildCell(false, false, neighborOne, neighborTwo);

            var city = BuildCity(location);            
            var civ  = BuildCiv();

            var restriction = Container.Resolve<TerritoryBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_AndTemplateRequiresCoastalCity_FalseIfNoAdjacentCellWater() {
            var template = BuildTemplate(requiresAdjacentRiver: false, requiresCoastalCity: true);

            var neighborOne = BuildCell(hasRiver: false, isWater: false);
            var neighborTwo = BuildCell(hasRiver: false, isWater: false);

            var location = BuildCell(false, true, neighborOne, neighborTwo);

            var city = BuildCity(location);            
            var civ  = BuildCiv();

            var restriction = Container.Resolve<TerritoryBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(template, city, civ));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(bool hasRiver, bool isWater, params IHexCell[] neighbors) {
            var newCell = BuildCell(hasRiver, isWater);

            MockGrid.Setup(grid => grid.GetNeighbors(newCell)).Returns(neighbors.ToList());

            return newCell;
        }

        private IHexCell BuildCell(bool hasRiver, bool isWater) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(isWater ? CellTerrain.ShallowWater : CellTerrain.Grassland);

            var newCell = mockCell.Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(hasRiver);

            return newCell;
        }

        private IBuildingTemplate BuildTemplate(bool requiresAdjacentRiver, bool requiresCoastalCity) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.RequiresAdjacentRiver).Returns(requiresAdjacentRiver);
            mockTemplate.Setup(template => template.RequiresCoastalCity)  .Returns(requiresCoastalCity);

            return mockTemplate.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }
 
        #endregion

        #endregion

    }

}
