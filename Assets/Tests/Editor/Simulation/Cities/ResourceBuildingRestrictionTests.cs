using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    public class ResourceBuildingRestrictionTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IFreeResourcesLogic>                              MockFreeResourcesLogic;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodeLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockFreeResourcesLogic  = new Mock<IFreeResourcesLogic>();
            MockCellPossessionCanon = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockNodeLocationCanon   = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();

            Container.Bind<IFreeResourcesLogic>                             ().FromInstance(MockFreeResourcesLogic .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodeLocationCanon  .Object);

            Container.Bind<ResourceBuildingRestriction>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsTemplateValidForCity_TrueIfCivHasFreeCopiesOfConsumedResources() {
            var resource = BuildResource();

            var template = BuildTemplate(
                new List<IResourceDefinition>() { resource },
                new List<IResourceDefinition>()
            );

            var city = BuildCity();

            var civ = BuildCiv(resource);

            var restriction = Container.Resolve<ResourceBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_FalseIfCivDoesntHaveFreeCopiesOfAnyConsumedResource() {
            var resourceOne = BuildResource();
            var resourceTwo = BuildResource();

            var template = BuildTemplate(
                new List<IResourceDefinition>() { resourceOne, resourceTwo },
                new List<IResourceDefinition>()
            );

            var city = BuildCity();

            var civ = BuildCiv(resourceOne);

            var restriction = Container.Resolve<ResourceBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_TrueIfSomeOwnedCellHasAPrerequisiteResource() {
            var resourceOne = BuildResource();
            var resourceTwo = BuildResource();

            var template = BuildTemplate(                
                new List<IResourceDefinition>(),
                new List<IResourceDefinition>() { resourceOne, resourceTwo }
            );

            var cellOne   = BuildCell(BuildResourceNode(resourceOne));
            var cellTwo   = BuildCell(null);
            var cellThree = BuildCell(null);

            var city = BuildCity(cellOne, cellTwo, cellThree);

            var civ = BuildCiv();

            var restriction = Container.Resolve<ResourceBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(template, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_FalseIfNoOwnedCellHasAPrerequisiteResource() {
            var resource = BuildResource();

            var template = BuildTemplate(
                new List<IResourceDefinition>(),
                new List<IResourceDefinition>() { resource }
            );

            var city = BuildCity();

            var civ = BuildCiv(resource);

            var restriction = Container.Resolve<ResourceBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(template, city, civ));
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(
            List<IResourceDefinition> resourcesConsumed,
            List<IResourceDefinition> prerequisiteResourcesNearCity
        ) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.ResourcesConsumed)            .Returns(resourcesConsumed);
            mockTemplate.Setup(template => template.PrerequisiteResourcesNearCity).Returns(prerequisiteResourcesNearCity);

            return mockTemplate.Object;
        }

        private IResourceDefinition BuildResource() {
            return new Mock<IResourceDefinition>().Object;
        }

        private IResourceNode BuildResourceNode(IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            return mockNode.Object;
        }

        private IHexCell BuildCell(IResourceNode node) {
            var newCell = new Mock<IHexCell>().Object;

            MockNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell))
                                 .Returns(new List<IResourceNode>() { node });

            return newCell;
        }

        private ICity BuildCity(params IHexCell[] cells) {
            var newCity = new Mock<ICity>().Object;

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(cells);

            return newCity;
        }

        private ICivilization BuildCiv(params IResourceDefinition[] freeResources) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var resource in freeResources) {
                MockFreeResourcesLogic.Setup(logic => logic.GetFreeCopiesOfResourceForCiv(resource, newCiv))
                                      .Returns(1);
            }
            
            return newCiv;
        }

        #endregion

        #endregion

    }

}
