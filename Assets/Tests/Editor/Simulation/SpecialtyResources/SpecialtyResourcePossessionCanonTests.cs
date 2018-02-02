using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.SpecialtyResources {

    [TestFixture]
    public class SpecialtyResourcePossessionCanonTests : ZenjectUnitTestFixture {

        #region internal types



        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>>    MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;

        private List<ISpecialtyResourceDefinition> AllResourceDefinitions = new List<ISpecialtyResourceDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllResourceDefinitions.Clear();

            MockCityPossessionCanon      = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);

            Container
                .Bind<IEnumerable<ISpecialtyResourceDefinition>>()
                .WithId("All Speciality Resources")
                .FromInstance(AllResourceDefinitions);

            Container.Bind<SpecialtyResourcePossessionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetCopiesOfResourceBelongingToCiv should check every cell belonging to " +
            "every city that the argued civ possesses, adding the copies of any ResourceNode whose " +
            "type is the same as the argued resource definition to the value returned.")]
        public void GetCopiesOfResourceBelongingToCiv_ChecksAllCellsOfAllCities() {
            var resource = BuildResourceDefinition();

            var cellOne   = BuildCell(BuildNode(resource, 1));
            var cellTwo   = BuildCell(BuildNode(resource, 2));
            var cellThree = BuildCell(BuildNode(resource, 4));

            var cityOne = BuildCity(new List<IHexCell>() { cellOne, cellTwo });
            var cityTwo = BuildCity(new List<IHexCell>() { cellThree });

            var civilization = BuildCivilization(new List<ICity>() { cityOne, cityTwo });

            var resourceCanon = Container.Resolve<SpecialtyResourcePossessionCanon>();

            Assert.AreEqual(7, resourceCanon.GetCopiesOfResourceBelongingToCiv(resource, civilization),
                "GetCopiesOfResourceBelongingToCiv returned an unexpected value");
        }

        [Test(Description = "GetFullResourceSummaryForCiv should return a dictionary containing " +
            "the values returned by GetCopiesOfResourceBelongingToCiv. Every resource definition " +
            "in the same should be represented")]
        public void GetFullResourceSummaryForCiv_EntriesForAllDefinitions() {
            var resourceOne   = BuildResourceDefinition();
            var resourceTwo   = BuildResourceDefinition();
            var resourceThree = BuildResourceDefinition();
            var resourceFour  = BuildResourceDefinition();

            var cellOne   = BuildCell(BuildNode(resourceOne,   1));
            var cellTwo   = BuildCell(BuildNode(resourceTwo,   2));
            var cellThree = BuildCell(BuildNode(resourceThree, 4));

            var cityOne = BuildCity(new List<IHexCell>() { cellOne, cellTwo });
            var cityTwo = BuildCity(new List<IHexCell>() { cellThree });

            var civilization = BuildCivilization(new List<ICity>() { cityOne, cityTwo });

            var resourceCanon = Container.Resolve<SpecialtyResourcePossessionCanon>();

            var summaryDict = resourceCanon.GetFullResourceSummaryForCiv(civilization);

            Assert.AreEqual(1, summaryDict[resourceOne],   "Returned an incorrect count for resourceOne");
            Assert.AreEqual(2, summaryDict[resourceTwo],   "Returned an incorrect count for resourceTwo");
            Assert.AreEqual(4, summaryDict[resourceThree], "Returned an incorrect count for resourceThree");
            Assert.AreEqual(0, summaryDict[resourceFour],  "Returned an incorrect count for resourceFour");
        }

        [Test(Description = "GetCopiesOfResourceBelongingToCiv should check the cells it surveys " +
            "for the necessary extractor improvement. If the argued resource definition has an extractor " +
            "and the cell doesn't possess an improvement of that type, its resource node's copies shouldn't " +
            "be added to the total.")]
        public void GetCopiesOfResourceBelongingToCiv_ChecksForExtractors() {
            var improvementTemplate = BuildImprovementTemplate();

            var resource = BuildResourceDefinition(improvementTemplate);

            var cellOne   = BuildCell(BuildNode(resource, 1));
            var cellTwo   = BuildCell(BuildNode(resource, 1));
            var cellThree = BuildCell(BuildNode(resource, 1));

            BuildImprovement(cellOne, improvementTemplate);
            BuildImprovement(cellThree, improvementTemplate);

            var cityOne = BuildCity(new List<IHexCell>() { cellOne, cellTwo });
            var cityTwo = BuildCity(new List<IHexCell>() { cellThree });

            var civilization = BuildCivilization(new List<ICity>() { cityOne, cityTwo });

            var resourceCanon = Container.Resolve<SpecialtyResourcePossessionCanon>();

            Assert.AreEqual(2, resourceCanon.GetCopiesOfResourceBelongingToCiv(resource, civilization));
        }

        #endregion

        #region utilities

        private IImprovementTemplate BuildImprovementTemplate() {
            return new Mock<IImprovementTemplate>().Object;
        }

        private IImprovement BuildImprovement(IHexCell location, IImprovementTemplate template) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template).Returns(template);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        private ICivilization BuildCivilization(List<ICity> cities) {
            var newCivilization = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCivilization)).Returns(cities);

            return newCivilization;
        }

        private IHexCell BuildCell(IResourceNode node) {
            var newCell = new Mock<IHexCell>().Object;

            if(node != null) {
                MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell))
                    .Returns(new List<IResourceNode>() { node });
            }

            return newCell;
        }

        private ICity BuildCity(List<IHexCell> cells) {
            var newCity = new Mock<ICity>().Object;

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(cells);

            return newCity;
        }

        private IResourceNode BuildNode(ISpecialtyResourceDefinition resource, int copies) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);
            mockNode.Setup(node => node.Copies).Returns(copies);

            return mockNode.Object;
        }

        private ISpecialtyResourceDefinition BuildResourceDefinition(IImprovementTemplate extractor = null) {
            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Setup(definition => definition.Extractor).Returns(extractor);

            var newDefinition = mockDefinition.Object;

            AllResourceDefinitions.Add(newDefinition);

            return newDefinition;
        }

        #endregion

        #endregion

    }

}
