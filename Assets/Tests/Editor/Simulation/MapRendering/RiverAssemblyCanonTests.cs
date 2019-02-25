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

    public class RiverAssemblyCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IRiverSectionCanon> MockSectionCanon;
        private Mock<IRiverBuilder>      MockRiverBuilder;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockSectionCanon = new Mock<IRiverSectionCanon>();
            MockRiverBuilder = new Mock<IRiverBuilder>();

            Container.Bind<IRiverSectionCanon>().FromInstance(MockSectionCanon.Object);
            Container.Bind<IRiverBuilder>     ().FromInstance(MockRiverBuilder.Object);

            Container.Bind<RiverAssemblyCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void Rivers_DefaultsToAnEmptySet() {
            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            CollectionAssert.IsEmpty(assemblyCanon.Rivers);
        }

        [Test]
        public void UnassignedSections_DefaultsToAnEmptySet() {
            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            CollectionAssert.IsEmpty(assemblyCanon.UnassignedSections);
        }

        [Test]
        public void RefreshRivers_RefreshesRiverSections() {
            MockSectionCanon.Setup(canon => canon.Sections).Returns(new List<RiverSection>().AsReadOnly());

            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            assemblyCanon.RefreshRivers();

            MockSectionCanon.Verify(canon => canon.RefreshRiverSections(), Times.Once);
        }

        [Test]
        public void RefreshRivers_RefreshesUnassignedSections() {
            var sections = new List<RiverSection>() {
                new RiverSection(), new RiverSection(), new RiverSection()
            };

            MockSectionCanon.Setup(canon => canon.Sections).Returns(sections.AsReadOnly());

            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            assemblyCanon.RefreshRivers();

            CollectionAssert.AreEquivalent(sections, assemblyCanon.UnassignedSections);
        }

        [Test]
        public void RefreshRivers_BuildsRiversFromUnassignedEndpoints_UntilNoUnassignedSectionsExist() {
            var sections = new List<RiverSection>() {
                new RiverSection() { HasPreviousEndpoint = true },
                new RiverSection(),
                new RiverSection() { HasNextEndpoint = true },
                new RiverSection(),
                new RiverSection()
            };

            MockSectionCanon.Setup(canon => canon.Sections).Returns(sections.AsReadOnly());

            MockRiverBuilder.Setup(
                builder => builder.BuildRiverFromSection(sections[0], It.IsAny<HashSet<RiverSection>>())

            ).Callback<RiverSection, HashSet<RiverSection>>((startingPoint, unassignedSections) => {
                unassignedSections.Remove(sections[0]);
                unassignedSections.Remove(sections[1]);

            }).Returns(
                new List<RiverSection>() { sections[0], sections[1] }
            );

            MockRiverBuilder.Setup(
                builder => builder.BuildRiverFromSection(sections[2], It.IsAny<HashSet<RiverSection>>())

            ).Callback<RiverSection, HashSet<RiverSection>>((startingPoint, unassignedSections) => {
                unassignedSections.Remove(sections[2]);
                unassignedSections.Remove(sections[3]);
                unassignedSections.Remove(sections[4]);

            }).Returns(
                new List<RiverSection>() { sections[2], sections[3], sections[4] }
            );

            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            assemblyCanon.RefreshRivers();

            Assert.AreEqual(2, assemblyCanon.Rivers.Count, "Unexpected number of rivers created");

            CollectionAssert.AreEqual(
                new List<RiverSection>() { sections[0], sections[1] }, assemblyCanon.Rivers[0],
                "Rivers[0] has an unexpected list of sections"
            );

            CollectionAssert.AreEqual(
                new List<RiverSection>() { sections[2], sections[3], sections[4] }, assemblyCanon.Rivers[1],
                "Rivers[1] has an unexpected list of sections"
            );
        }

        [Test]
        public void RefreshRivers_BuildsRiversFromUnassignedEndpoints_UntilNoUnassignedEndpointsExist() {
            var sections = new List<RiverSection>() {
                new RiverSection() { HasPreviousEndpoint = true },
                new RiverSection(),
                new RiverSection(),
                new RiverSection(),
                new RiverSection()
            };

            MockSectionCanon.Setup(canon => canon.Sections).Returns(sections.AsReadOnly());

            MockRiverBuilder.Setup(
                builder => builder.BuildRiverFromSection(sections[0], It.IsAny<HashSet<RiverSection>>())

            ).Callback<RiverSection, HashSet<RiverSection>>((startingPoint, unassignedSections) => {
                unassignedSections.Remove(sections[0]);
                unassignedSections.Remove(sections[1]);

            }).Returns(
                new List<RiverSection>() { sections[0], sections[1] }
            );

            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            assemblyCanon.RefreshRivers();

            Assert.AreEqual(1, assemblyCanon.Rivers.Count, "Unexpected number of rivers created");

            CollectionAssert.AreEqual(
                new List<RiverSection>() { sections[0], sections[1] }, assemblyCanon.Rivers[0],
                "Rivers[0] has an unexpected list of sections"
            );

            CollectionAssert.AreEquivalent(
                new List<RiverSection>() { sections[2], sections[3], sections[4] }, assemblyCanon.UnassignedSections,
                "Unexpected set of unassigned sections"
            );
        }

        [Test]
        public void RefreshRivers_ClearsAllPreviousRivers_AndUnassignedSections() {
            var sections = new List<RiverSection>() {
                new RiverSection() { HasPreviousEndpoint = true },
                new RiverSection(),
                new RiverSection(),
                new RiverSection(),
                new RiverSection()
            };

            MockSectionCanon.Setup(canon => canon.Sections).Returns(sections.AsReadOnly());

            MockRiverBuilder.Setup(
                builder => builder.BuildRiverFromSection(sections[0], It.IsAny<HashSet<RiverSection>>())

            ).Callback<RiverSection, HashSet<RiverSection>>((startingPoint, unassignedSections) => {
                unassignedSections.Remove(sections[0]);
                unassignedSections.Remove(sections[1]);

            }).Returns(
                new List<RiverSection>() { sections[0], sections[1] }
            );

            var assemblyCanon = Container.Resolve<RiverAssemblyCanon>();

            assemblyCanon.RefreshRivers();

            sections.Clear();

            sections.Add(new RiverSection());
            sections.Add(new RiverSection());

            assemblyCanon.RefreshRivers();

            CollectionAssert.IsEmpty(assemblyCanon.Rivers, "Rivers not cleared as expected");
            CollectionAssert.AreEquivalent(sections, assemblyCanon.UnassignedSections, "Unexpected set of unassigned sections");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
