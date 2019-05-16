using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class RiverBuilderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IRiverBuilderUtilities> MockRiverBuilderUtilities;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRiverBuilderUtilities = new Mock<IRiverBuilderUtilities>();

            Container.Bind<IRiverBuilderUtilities>().FromInstance(MockRiverBuilderUtilities.Object);

            Container.Bind<RiverBuilder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void BuildRiverFromSection_AndSectionHasBothEndpoints_ReturnsListWithStartingSection() {
            var startingSection = new RiverSection() { HasPreviousEndpoint = true, HasNextEndpoint = true };

            var unassignedRiverSections = new HashSet<RiverSection>();

            var riverBuilder = Container.Resolve<RiverBuilder>();

            CollectionAssert.AreEqual(
                new List<RiverSection>() { startingSection },
                riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections)
            );
        }

        [Test]
        public void BuildRiverFromSection_PullsAndAddsActiveSections_UntilAPreviousEndpointIsFound() {
            var startingSection = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>();

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection() { HasPreviousEndpoint = true };
            var sectionFour  = new RiverSection() { HasNextEndpoint     = true };

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionThree,    sectionTwo,      unassignedRiverSections)).Returns(sectionFour);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            CollectionAssert.AreEqual(
                new List<RiverSection>() { startingSection, sectionOne, sectionTwo, sectionThree },
                riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections)
            );
        }

        [Test]
        public void BuildRiverFromSection_PullsAndAddsActiveSections_UntilANextEndpointIsFound() {
            var startingSection = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>();

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection() { HasNextEndpoint = true };
            var sectionFour  = new RiverSection() { HasNextEndpoint = true };

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionThree,    sectionTwo,      unassignedRiverSections)).Returns(sectionFour);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            CollectionAssert.AreEqual(
                new List<RiverSection>() { startingSection, sectionOne, sectionTwo, sectionThree },
                riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections)
            );
        }

        [Test]
        public void BuildRiverFromSection_PullsAndAddsActiveSections_UntilActiveSectionBecomesNull() {
            var startingSection = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>();

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection();
            var sectionFour  = new RiverSection();

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionThree,    sectionTwo,      unassignedRiverSections)).Returns(sectionFour);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            CollectionAssert.AreEqual(
                new List<RiverSection>() { startingSection, sectionOne, sectionTwo, sectionThree, sectionFour },
                riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections)
            );
        }

        [Test]
        public void BuildRiverFromSection_RemovesAllAddedSectionsFromUnassignedRiverSections() {
            var startingSection = new RiverSection();

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection();
            var sectionFour  = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>() {
                sectionOne, sectionTwo, sectionThree, sectionFour, new RiverSection(), new RiverSection()
            };

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionThree,    sectionTwo,      unassignedRiverSections)).Returns(sectionFour);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections);

            Assert.AreEqual(2, unassignedRiverSections.Count, "Unexpected number of unassigned sections left");

            Assert.IsFalse(unassignedRiverSections.Contains(startingSection), "UnassignedRiverSections unexpectedly contains startingSection");
            Assert.IsFalse(unassignedRiverSections.Contains(sectionOne),      "UnassignedRiverSections unexpectedly contains sectionOne");
            Assert.IsFalse(unassignedRiverSections.Contains(sectionTwo),      "UnassignedRiverSections unexpectedly contains sectionTwo");
            Assert.IsFalse(unassignedRiverSections.Contains(sectionThree),    "UnassignedRiverSections unexpectedly contains sectionThree");
            Assert.IsFalse(unassignedRiverSections.Contains(sectionFour),     "UnassignedRiverSections unexpectedly contains sectionFour");
        }

        [Test]
        public void BuildRiverFromSection_RemovesSingleSectionRiverFromUnassignedRiverSections() {
            var startingSection = new RiverSection() { HasPreviousEndpoint = true, HasNextEndpoint = true };

            var unassignedRiverSections = new HashSet<RiverSection>() { startingSection, new RiverSection(), new RiverSection() };

            var riverBuilder = Container.Resolve<RiverBuilder>();

            riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections);

            Assert.AreEqual(2, unassignedRiverSections.Count, "Unexpected number of unassigned sections left");

            Assert.IsFalse(unassignedRiverSections.Contains(startingSection), "UnassignedRiverSections unexpectedly contains startingSection");
        }

        [Test]
        public void BuildRiverFromSection_StartingSectionHasPreviousEndpoint_AndCCWFlow_ReversesRiver() {
            var startingSection = new RiverSection() { HasPreviousEndpoint = true, FlowFromOne = RiverFlow.Counterclockwise };

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>();

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            CollectionAssert.AreEqual(
                new List<RiverSection>() { sectionThree, sectionTwo, sectionOne, startingSection },
                riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections)
            );
        }

        [Test]
        public void BuildRiverFromSection_StartingSectionHasNextEndpoint_AndCWFlow_ReversesRiver() {
            var startingSection = new RiverSection() { HasNextEndpoint = true, FlowFromOne = RiverFlow.Clockwise };

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>();

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            CollectionAssert.AreEqual(
                new List<RiverSection>() { sectionThree, sectionTwo, sectionOne, startingSection },
                riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections)
            );
        }

        [Test]
        public void BuildRiverFromSection_SetsCurveStatusOfAllSectionsInRiver() {
            var startingSection = new RiverSection();

            var sectionOne   = new RiverSection();
            var sectionTwo   = new RiverSection();
            var sectionThree = new RiverSection();

            var unassignedRiverSections = new HashSet<RiverSection>();

            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(startingSection, null,            unassignedRiverSections)).Returns(sectionOne);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionOne,      startingSection, unassignedRiverSections)).Returns(sectionTwo);
            MockRiverBuilderUtilities.Setup(utilities => utilities.GetNextActiveSectionForRiver(sectionTwo,      sectionOne,      unassignedRiverSections)).Returns(sectionThree);

            var riverBuilder = Container.Resolve<RiverBuilder>();

            var river = riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections);

            MockRiverBuilderUtilities.Verify(utilities => utilities.SetCurveStatusOfSection(startingSection, river), Times.Once, "Curve status of startingSection not set");
            MockRiverBuilderUtilities.Verify(utilities => utilities.SetCurveStatusOfSection(sectionOne,      river), Times.Once, "Curve status of sectionOne not set");
            MockRiverBuilderUtilities.Verify(utilities => utilities.SetCurveStatusOfSection(sectionTwo,      river), Times.Once, "Curve status of sectionTwo not set");
            MockRiverBuilderUtilities.Verify(utilities => utilities.SetCurveStatusOfSection(sectionThree,    river), Times.Once, "Curve status of sectionThree not set");
        }

        [Test]
        public void BuildRiverFromSection_SetsCurveStatusOfSingleSectionRiver() {
            var startingSection = new RiverSection() { HasPreviousEndpoint = true, HasNextEndpoint = true };

            var unassignedRiverSections = new HashSet<RiverSection>();

            var riverBuilder = Container.Resolve<RiverBuilder>();

            var river = riverBuilder.BuildRiverFromSection(startingSection, unassignedRiverSections);

            MockRiverBuilderUtilities.Verify(utilities => utilities.SetCurveStatusOfSection(startingSection, river));
        }

        [Test]
        public void BuildRiverFromSection_AndStartingSectionNull_ThrowsArgumentNullException() {
            var unassignedRiverSections = new HashSet<RiverSection>();

            var riverBuilder = Container.Resolve<RiverBuilder>();

            Assert.Throws<ArgumentNullException>(() => riverBuilder.BuildRiverFromSection(null, unassignedRiverSections));
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
