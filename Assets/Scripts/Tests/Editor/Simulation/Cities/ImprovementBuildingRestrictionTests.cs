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
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    public class ImprovementBuildingRestrictionTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IHexCell>> MockCellPossessionCanon;
        private Mock<IImprovementLocationCanon>                MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            Container.Bind<IPossessionRelationship<ICity, IHexCell>>().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IImprovementLocationCanon>               ().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<ImprovementBuildingRestriction>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsTemplateValidForCity_TrueIfAllPrerequisiteImprovementsPresent() {
            var improvementTemplateOne = BuildImprovementTemplate();
            var improvementTemplateTwo = BuildImprovementTemplate();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            BuildImprovement(improvementTemplateOne, cellOne);
            BuildImprovement(improvementTemplateOne, cellTwo);
            BuildImprovement(improvementTemplateTwo, cellThree);

            var city = BuildCity(cellOne, cellTwo, cellThree);

            var civ = BuildCiv();

            var buildingTemplate = BuildBuildingTemplate(improvementTemplateOne, improvementTemplateTwo);

            var restriction = Container.Resolve<ImprovementBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(buildingTemplate, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_FalseIfAnyPrerequisiteImprovementNotPresent() {
            var improvementTemplateOne = BuildImprovementTemplate();
            var improvementTemplateTwo = BuildImprovementTemplate();

            var cellOne = BuildCell();
            var cellTwo = BuildCell();

            BuildImprovement(improvementTemplateOne, cellOne);
            BuildImprovement(improvementTemplateOne, cellTwo);

            var city = BuildCity(cellOne, cellTwo);

            var civ = BuildCiv();

            var buildingTemplate = BuildBuildingTemplate(improvementTemplateOne, improvementTemplateTwo);

            var restriction = Container.Resolve<ImprovementBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(buildingTemplate, city, civ));
        }

        #endregion

        #region utilities

        private IImprovementTemplate BuildImprovementTemplate() {
            return new Mock<IImprovementTemplate>().Object;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IImprovement BuildImprovement(IImprovementTemplate template, IHexCell location) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template).Returns(template);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                        .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        private ICity BuildCity(params IHexCell[] cells) {
            var newCity = new Mock<ICity>().Object;

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(cells);

            return newCity;
        }

        private IBuildingTemplate BuildBuildingTemplate(params IImprovementTemplate[] improvementPrereqs) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.PrerequisiteImprovementsNearCity).Returns(improvementPrereqs);

            return mockTemplate.Object;
        }
        
        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
