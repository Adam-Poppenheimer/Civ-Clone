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

    public class RiverBuilderUtilitiesTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>           MockGrid;
        private Mock<IRiverSectionCanon> MockSectionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid = new Mock<IHexGrid>();
            MockSectionCanon = new Mock<IRiverSectionCanon>();

            Container.Bind<IHexGrid>          ().FromInstance(MockGrid        .Object);
            Container.Bind<IRiverSectionCanon>().FromInstance(MockSectionCanon.Object);

            Container.Bind<RiverBuilderUtilities>().AsSingle();
        }

        #endregion

        #region tests

        #region GetNextActiveSectionForRiver

        [Test]
        public void GetNextActiveSectionForRiver_AndNoValidSections_ReturnsNull() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>();

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndValidCenterLeftSectionExists_ReturnsCenterLeftSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerLeftSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { centerLeftSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left))                      .Returns(centerLeftSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerLeftSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerLeftSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.AreEqual(centerLeftSection, utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndCenterLeftSectionNotUnassigned_IgnoresCenterLeftSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerLeftSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>();

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left))                      .Returns(centerLeftSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerLeftSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerLeftSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndCenterLeftSectionNeighborOfLastSection_IgnoresCenterLeftSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerLeftSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { centerLeftSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left))                      .Returns(centerLeftSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerLeftSection, lastSection))              .Returns(true);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerLeftSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndCenterLeftSectionNotCongruousWithActiveSection_IgnoresCenterLeftSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerLeftSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { centerLeftSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left))                      .Returns(centerLeftSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerLeftSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerLeftSection)).Returns(false);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }


        [Test]
        public void GetNextActiveSectionForRiver_AndValidLeftRightSectionExists_ReturnsLeftRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var leftRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { leftRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(left, right))                      .Returns(leftRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(leftRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, leftRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.AreEqual(leftRightSection, utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndLeftRightSectionNotUnassigned_IgnoresLeftRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var leftRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>();

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(left, right))                      .Returns(leftRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(leftRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, leftRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndLeftRightSectionNeighborOfLastSection_IgnoresLeftRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var leftRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { leftRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(left, right))                      .Returns(leftRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(leftRightSection, lastSection))              .Returns(true);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, leftRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndLeftRightSectionNotCongruousWithActiveSection_IgnoresLeftRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var leftRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { leftRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(left, right))                      .Returns(leftRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(leftRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, leftRightSection)).Returns(false);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }


        [Test]
        public void GetNextActiveSectionForRiver_AndValidCenterNextRightSectionExists_ReturnsCenterNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { centerNextRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, nextRight))                      .Returns(centerNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerNextRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerNextRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.AreEqual(centerNextRightSection, utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndCenterNextRightSectionNotUnassigned_IgnoresCenterNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>();

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, nextRight))                      .Returns(centerNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerNextRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerNextRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndCenterNextRightSectionNeighborOfLastSection_IgnoresCenterNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { centerNextRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, nextRight))                      .Returns(centerNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerNextRightSection, lastSection))              .Returns(true);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerNextRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndCenterNextRightSectionNotCongruousWithActiveSection_IgnoresCenterNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var centerNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { centerNextRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, nextRight))                      .Returns(centerNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(centerNextRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, centerNextRightSection)).Returns(false);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }


        [Test]
        public void GetNextActiveSectionForRiver_AndValidRightNextRightSectionExists_ReturnsRightNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var rightNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { rightNextRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(right, nextRight))                      .Returns(rightNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(rightNextRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, rightNextRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.AreEqual(rightNextRightSection, utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndRightNextRightSectionNotUnassigned_IgnoresRightNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var rightNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>();

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(right, nextRight))                      .Returns(rightNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(rightNextRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, rightNextRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndRightNextRightSectionNeighborOfLastSection_IgnoresRightNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var rightNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { rightNextRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(right, nextRight))                      .Returns(rightNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(rightNextRightSection, lastSection))              .Returns(true);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, rightNextRightSection)).Returns(true);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        [Test]
        public void GetNextActiveSectionForRiver_AndRightNextRightSectionNotCongruousWithActiveSection_IgnoresRightNextRightSection() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var activeSection = new RiverSection() { AdjacentCellOne = center, AdjacentCellTwo = right, DirectionFromOne = HexDirection.E };
            var lastSection   = new RiverSection();
            
            var rightNextRightSection = new RiverSection();

            var unassignedSections = new HashSet<RiverSection>() { rightNextRightSection };

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(right, nextRight))                      .Returns(rightNextRightSection);
            MockSectionCanon.Setup(canon => canon.IsNeighborOf(rightNextRightSection, lastSection))              .Returns(false);
            MockSectionCanon.Setup(canon => canon.AreSectionFlowsCongruous(activeSection, rightNextRightSection)).Returns(false);

            var utilities = Container.Resolve<RiverBuilderUtilities>();

            Assert.IsNull(utilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedSections));
        }

        #endregion

        #region SetCurveStatusOFSection

        [Test]
        public void SetCurveStatusOfSection_AndNoCenterLeftSection_PreviousOnInternalCurveSetToFalse() {
            var center = BuildCell();
            var left   = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            var sectionToTest = new RiverSection() { AdjacentCellOne = center, DirectionFromOne = HexDirection.E };

            var river = new List<RiverSection>() { new RiverSection(), new RiverSection() };
            
            var utilities = Container.Resolve<RiverBuilderUtilities>();

            utilities.SetCurveStatusOfSection(sectionToTest, river);

            Assert.IsFalse(sectionToTest.PreviousOnInternalCurve);
        }

        [Test]
        public void SetCurveStatusOfSection_AndRiverDoesntContainCenterLeftSection_PreviousOnInternalCurveSetToFalse() {
            var center = BuildCell();
            var left   = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left)).Returns(new RiverSection());

            var sectionToTest = new RiverSection() { AdjacentCellOne = center, DirectionFromOne = HexDirection.E };

            var river = new List<RiverSection>() { new RiverSection(), new RiverSection() };
            
            var utilities = Container.Resolve<RiverBuilderUtilities>();

            utilities.SetCurveStatusOfSection(sectionToTest, river);

            Assert.IsFalse(sectionToTest.PreviousOnInternalCurve);
        }

        [Test]
        public void SetCurveStatusOfSection_AndRiverContainsCenterLeftSection_PreviousOnInternalCurveSetToTrue() {
            var center = BuildCell();
            var left   = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            var centerLeftSection = new RiverSection();

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left)).Returns(centerLeftSection);

            var sectionToTest = new RiverSection() { AdjacentCellOne = center, DirectionFromOne = HexDirection.E };

            var river = new List<RiverSection>() { centerLeftSection, new RiverSection() };
            
            var utilities = Container.Resolve<RiverBuilderUtilities>();

            utilities.SetCurveStatusOfSection(sectionToTest, river);

            Assert.IsTrue(sectionToTest.PreviousOnInternalCurve);
        }


        [Test]
        public void SetCurveStatusOfSection_AndNoCenterNextRightSection_NextOnInternalCurveSetToFalse() {
            var center    = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var sectionToTest = new RiverSection() { AdjacentCellOne = center, DirectionFromOne = HexDirection.E };

            var river = new List<RiverSection>() { new RiverSection(), new RiverSection() };
            
            var utilities = Container.Resolve<RiverBuilderUtilities>();

            utilities.SetCurveStatusOfSection(sectionToTest, river);

            Assert.IsFalse(sectionToTest.NextOnInternalCurve);
        }

        [Test]
        public void SetCurveStatusOfSection_AndRiverDoesntContainCenterNextRightSection_NextOnInternalCurveSetToFalse() {
            var center = BuildCell();
            var nextRight   = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, nextRight)).Returns(new RiverSection());

            var sectionToTest = new RiverSection() { AdjacentCellOne = center, DirectionFromOne = HexDirection.E };

            var river = new List<RiverSection>() { new RiverSection(), new RiverSection() };
            
            var utilities = Container.Resolve<RiverBuilderUtilities>();

            utilities.SetCurveStatusOfSection(sectionToTest, river);

            Assert.IsFalse(sectionToTest.NextOnInternalCurve);
        }

        [Test]
        public void SetCurveStatusOfSection_AndRiverContainsCenterNextRightSection_NextOnInternalCurveSetToTrue() {
            var center    = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var centerNextRightSection = new RiverSection();

            MockSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, nextRight)).Returns(centerNextRightSection);

            var sectionToTest = new RiverSection() { AdjacentCellOne = center, DirectionFromOne = HexDirection.E };

            var river = new List<RiverSection>() { centerNextRightSection, new RiverSection() };
            
            var utilities = Container.Resolve<RiverBuilderUtilities>();

            utilities.SetCurveStatusOfSection(sectionToTest, river);

            Assert.IsTrue(sectionToTest.NextOnInternalCurve);
        }

        #endregion

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
