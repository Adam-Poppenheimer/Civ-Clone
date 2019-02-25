using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class RiverSectionCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class AreSectionFlowsCongruousTestData {

            public RiverSectionTestData ThisSection;
            public RiverSectionTestData OtherSection;

        }

        public class RiverSectionTestData {

            public HexDirection DirectionFromOne;

            public RiverFlow FlowFromOne;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable AreSectionFlowsCongruousTestCases {
            get {
                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(NE, Clockwise) and (NE, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(NE, Counterclockwise) and (NE, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(NE, Clockwise) and (NE, Counterclockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(NE, Counterclockwise) and (NE, Counterclockwise)").Returns(false);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(NE, Clockwise) and (E, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(NE, Counterclockwise) and (E, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(NE, Clockwise) and (E, Counterclockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(NE, Counterclockwise) and (E, Counterclockwise)").Returns(true);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(NE, Clockwise) and (SE, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(NE, Counterclockwise) and (SE, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(NE, Clockwise) and (SE, Counterclockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(NE, Counterclockwise) and (SE, Counterclockwise)").Returns(false);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(E, Clockwise) and (NE, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(E, Counterclockwise) and (NE, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(E, Clockwise) and (NE, Counterclockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(E, Counterclockwise) and (NE, Counterclockwise)").Returns(true);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(E, Clockwise) and (E, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(E, Counterclockwise) and (E, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(E, Clockwise) and (E, Counterclockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(E, Counterclockwise) and (E, Counterclockwise)").Returns(false);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(E, Clockwise) and (SE, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(E, Counterclockwise) and (SE, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(E, Clockwise) and (SE, Counterclockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(E, Counterclockwise) and (SE, Counterclockwise)").Returns(true);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(SE, Clockwise) and (NE, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(SE, Counterclockwise) and (NE, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(SE, Clockwise) and (NE, Counterclockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.NE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(SE, Counterclockwise) and (NE, Counterclockwise)").Returns(false);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(SE, Clockwise) and (E, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(SE, Counterclockwise) and (E, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(SE, Clockwise) and (E, Counterclockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.E,  FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(SE, Counterclockwise) and (E, Counterclockwise)").Returns(true);


                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(SE, Clockwise) and (SE, Clockwise)").Returns(false);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise }
                }).SetName("(SE, Counterclockwise) and (SE, Clockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Clockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(SE, Clockwise) and (SE, Counterclockwise)").Returns(true);

                yield return new TestCaseData(new AreSectionFlowsCongruousTestData() {
                    ThisSection  = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise },
                    OtherSection = new RiverSectionTestData() { DirectionFromOne = HexDirection.SE, FlowFromOne = RiverFlow.Counterclockwise }
                }).SetName("(SE, Counterclockwise) and (SE, Counterclockwise)").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGrid>         MockGrid;
        private Mock<IMapRenderConfig> MockRenderConfig;
        private Mock<IRiverCanon>      MockRiverCanon;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockGrid         = new Mock<IHexGrid>();
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockRiverCanon   = new Mock<IRiverCanon>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IHexGrid>        ().FromInstance(MockGrid        .Object);
            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);
            Container.Bind<IRiverCanon>     ().FromInstance(MockRiverCanon  .Object);

            Container.Bind<RiverSectionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void Sections_DefaultsToEmptySet() {
            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            CollectionAssert.IsEmpty(sectionCanon.Sections);
        }

        #region RefreshRiverSections

        [Test]
        public void RefreshRiverSections_CreatesOneRiverSection_ForAllEdgesWithRivers() {
            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>());

            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise }
            });

            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.SE, RiverFlow.Counterclockwise }
            });

            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Counterclockwise },
                { HexDirection.E,  RiverFlow.Counterclockwise },
                { HexDirection.W,  RiverFlow.Clockwise }
            });

            var neighbor = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(It.IsAny<IHexCell>(), It.IsAny<HexDirection>())).Returns(neighbor);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.AreEqual(5, sectionCanon.Sections.Count);
        }

        [Test]
        public void RefreshRiverSections_SectionsHaveAppropriateAdjacentCells() {
            var cellOne   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise } });
            var cellTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Clockwise } });
            var cellThree = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });

            var neighborOne   = BuildCell();
            var neighborTwo   = BuildCell();
            var neighborThree = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(cellOne,   HexDirection.NE)).Returns(neighborOne);
            MockGrid.Setup(grid => grid.GetNeighbor(cellTwo,   HexDirection.E )).Returns(neighborTwo);
            MockGrid.Setup(grid => grid.GetNeighbor(cellThree, HexDirection.SE)).Returns(neighborThree);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.AreEqual(cellOne,     sectionCanon.Sections[0].AdjacentCellOne, "Sections[0] has an unexpected AdjacentCellOne");
            Assert.AreEqual(neighborOne, sectionCanon.Sections[0].AdjacentCellTwo, "Sections[0] has an unexpected AdjacentCellTwo");

            Assert.AreEqual(cellTwo,     sectionCanon.Sections[1].AdjacentCellOne, "Sections[1] has an unexpected AdjacentCellOne");
            Assert.AreEqual(neighborTwo, sectionCanon.Sections[1].AdjacentCellTwo, "Sections[1] has an unexpected AdjacentCellTwo");

            Assert.AreEqual(cellThree,     sectionCanon.Sections[2].AdjacentCellOne, "Sections[2] has an unexpected AdjacentCellOne");
            Assert.AreEqual(neighborThree, sectionCanon.Sections[2].AdjacentCellTwo, "Sections[2] has an unexpected AdjacentCellTwo");
        }

        [Test]
        public void RefreshRiverSections_SectionsHaveAppropriateDirectionFromOne() {
            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise } });
            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Clockwise } });
            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.AreEqual(HexDirection.NE, sectionCanon.Sections[0].DirectionFromOne, "Sections[0] has an unexpected DirectionFromOne");
            Assert.AreEqual(HexDirection.E,  sectionCanon.Sections[1].DirectionFromOne, "Sections[1] has an unexpected DirectionFromOne");
            Assert.AreEqual(HexDirection.SE, sectionCanon.Sections[2].DirectionFromOne, "Sections[2] has an unexpected DirectionFromOne");
        }

        [Test]
        public void RefreshRiverSections_SectionsHaveAppropriateStartAndEndPoints() {
            BuildCell(new Vector3(1f, 2f, 3f), new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise } });
            BuildCell(new Vector3(4f, 5f, 6f), new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Clockwise } });
            BuildCell(new Vector3(7f, 8f, 9f), new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });

            MockRenderConfig.Setup(config => config.GetFirstCorner (HexDirection.NE)).Returns(new Vector3(1f,  1f,  1f));
            MockRenderConfig.Setup(config => config.GetSecondCorner(HexDirection.NE)).Returns(new Vector3(10f, 10f, 10f));

            MockRenderConfig.Setup(config => config.GetFirstCorner (HexDirection.E)).Returns(new Vector3(2f,  2f,  2f));
            MockRenderConfig.Setup(config => config.GetSecondCorner(HexDirection.E)).Returns(new Vector3(20f, 20f, 20f));

            MockRenderConfig.Setup(config => config.GetFirstCorner (HexDirection.SE)).Returns(new Vector3(3f,  3f,  3f));
            MockRenderConfig.Setup(config => config.GetSecondCorner(HexDirection.SE)).Returns(new Vector3(30f, 30f, 30f));

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.AreEqual(new Vector3(2f,  3f,  4f),  sectionCanon.Sections[0].Start, "Sections[0] has an unexpected Start");
            Assert.AreEqual(new Vector3(11f, 12f, 13f), sectionCanon.Sections[0].End,   "Sections[0] has an unexpected End");

            Assert.AreEqual(new Vector3(6f,  7f,  8f),  sectionCanon.Sections[1].Start, "Sections[1] has an unexpected Start");
            Assert.AreEqual(new Vector3(24f, 25f, 26f), sectionCanon.Sections[1].End,   "Sections[1] has an unexpected End");

            Assert.AreEqual(new Vector3(10f, 11f, 12f), sectionCanon.Sections[2].Start, "Sections[2] has an unexpected Start");
            Assert.AreEqual(new Vector3(37f, 38f, 39f), sectionCanon.Sections[2].End,   "Sections[2] has an unexpected End");
        }

        [Test]
        public void RefreshRiverSections_SectionsHaveAppropriateFlow() {
            BuildCell(new Vector3(1f, 2f, 3f), new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Counterclockwise } });
            BuildCell(new Vector3(4f, 5f, 6f), new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Counterclockwise } });
            BuildCell(new Vector3(7f, 8f, 9f), new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise        } });

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.AreEqual(RiverFlow.Counterclockwise, sectionCanon.Sections[0].FlowFromOne, "Sections[0] has an unexpected FlowFromOne");
            Assert.AreEqual(RiverFlow.Counterclockwise, sectionCanon.Sections[1].FlowFromOne, "Sections[1] has an unexpected FlowFromOne");
            Assert.AreEqual(RiverFlow.Clockwise,        sectionCanon.Sections[2].FlowFromOne, "Sections[2] has an unexpected FlowFromOne");


        }

        [Test]
        public void RefreshRiverSections_AssignsHasPreviousEndpointCorrectly() {
            var cellOne   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.NW, RiverFlow.Clockwise }
            });

            var cellTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise } });
            var cellThree = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise } });
                            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise } });

            var neighborOne   = BuildCell();
            var neighborTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.W,  RiverFlow.Clockwise } });
            var neighborThree = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NW, RiverFlow.Clockwise } });

            MockGrid.Setup(grid => grid.GetNeighbor(cellOne,   HexDirection.NE)).Returns(neighborOne);
            MockGrid.Setup(grid => grid.GetNeighbor(cellTwo,   HexDirection.NE)).Returns(neighborTwo);
            MockGrid.Setup(grid => grid.GetNeighbor(cellThree, HexDirection.NE)).Returns(neighborThree);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.IsFalse(sectionCanon.Sections[0].HasPreviousEndpoint, "Sections[0].HasPreviousEndpoint has an unexpected value");
            Assert.IsFalse(sectionCanon.Sections[1].HasPreviousEndpoint, "Sections[1].HasPreviousEndpoint has an unexpected value");
            Assert.IsTrue (sectionCanon.Sections[2].HasPreviousEndpoint, "Sections[2].HasPreviousEndpoint has an unexpected value");
            Assert.IsTrue (sectionCanon.Sections[3].HasPreviousEndpoint, "Sections[3].HasPreviousEndpoint has an unexpected value");
        }

        [Test]
        public void RefreshRiverSections_AssignsHasNextEndpointCorrectly() {
            var cellOne   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.SE, RiverFlow.Clockwise },
                { HexDirection.SW, RiverFlow.Clockwise }
            });

            var cellTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });
            var cellThree = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });
                            BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });

            var neighborOne   = BuildCell();
            var neighborTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.W,  RiverFlow.Clockwise } });
            var neighborThree = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SW, RiverFlow.Clockwise } });

            MockGrid.Setup(grid => grid.GetNeighbor(cellOne,   HexDirection.SE)).Returns(neighborOne);
            MockGrid.Setup(grid => grid.GetNeighbor(cellTwo,   HexDirection.SE)).Returns(neighborTwo);
            MockGrid.Setup(grid => grid.GetNeighbor(cellThree, HexDirection.SE)).Returns(neighborThree);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.IsFalse(sectionCanon.Sections[0].HasNextEndpoint, "Sections[0].HasPreviousEndpoint has an unexpected value");
            Assert.IsFalse(sectionCanon.Sections[1].HasNextEndpoint, "Sections[1].HasPreviousEndpoint has an unexpected value");
            Assert.IsTrue (sectionCanon.Sections[2].HasNextEndpoint, "Sections[2].HasPreviousEndpoint has an unexpected value");
            Assert.IsTrue (sectionCanon.Sections[3].HasNextEndpoint, "Sections[3].HasPreviousEndpoint has an unexpected value");
        }

        #endregion

        #region GetSectionBetweenCells

        [Test]
        public void GetSectionBetweenCells_ReturnsASectionAdjacentToBothCells() {
            var cellOne   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Clockwise } });
            var cellTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise } });
            var cellThree = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(cellOne, HexDirection.E )).Returns(cellTwo);
            MockGrid.Setup(grid => grid.GetNeighbor(cellTwo, HexDirection.SE)).Returns(cellThree);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            var oneTwoSection   = sectionCanon.GetSectionBetweenCells(cellOne, cellTwo);
            var twoThreeSection = sectionCanon.GetSectionBetweenCells(cellTwo, cellThree);

            Assert.IsTrue(oneTwoSection.AdjacentCellOne == cellOne || oneTwoSection.AdjacentCellTwo == cellOne, "OneTwoSection not adjacent to CellOne");
            Assert.IsTrue(oneTwoSection.AdjacentCellOne == cellTwo || oneTwoSection.AdjacentCellTwo == cellTwo, "OneTwoSection not adjacent to CellTwo");

            Assert.IsTrue(twoThreeSection.AdjacentCellOne == cellTwo   || twoThreeSection.AdjacentCellTwo == cellTwo,   "TwoThreeSection not adjacent to CellTwo");
            Assert.IsTrue(twoThreeSection.AdjacentCellOne == cellThree || twoThreeSection.AdjacentCellTwo == cellThree, "TwoThreeSection not adjacent to CellThree");
        }

        [Test]
        public void GetSectionBetweenCells_IgnoresArgumentOrder() {
            var cellOne   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Clockwise } });
            var cellTwo   = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>());

            MockGrid.Setup(grid => grid.GetNeighbor(cellOne, HexDirection.E)).Returns(cellTwo);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.AreEqual(sectionCanon.GetSectionBetweenCells(cellOne, cellTwo), sectionCanon.GetSectionBetweenCells(cellTwo, cellOne));
        }

        #endregion

        #region IsNeighborOf

        [Test]
        public void IsNeighborOf_FalseIfOtherSectionNull() {
            var center = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.E,  RiverFlow.Clockwise }
            });

            var left = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>());

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            Assert.IsFalse(sectionCanon.IsNeighborOf(sectionCanon.Sections[1], null));
        }

        [Test]
        public void IsNeighborOf_TrueIfOtherSectionIsSectionBetweenCenterAndLeft() {
            var center = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.E,  RiverFlow.Clockwise },
                { HexDirection.SE, RiverFlow.Clockwise },
            });

            var left      = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise        } });
            var right     = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Counterclockwise } });
            var nextRight = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise        } });

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockGrid.Setup(grid => grid.GetNeighbor(left,      HexDirection.SE)).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(nextRight, HexDirection.NE)).Returns(right);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            RiverSection centerLeftSection = sectionCanon.Sections[0];
            RiverSection sectionToTest = sectionCanon.Sections[1];

            Assert.IsTrue(sectionCanon.IsNeighborOf(sectionToTest, centerLeftSection));
        }

        [Test]
        public void IsNeighborOf_TrueIfOtherSectionIsSectionBetweenLeftAndRight() {
            var center = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.E,  RiverFlow.Clockwise },
                { HexDirection.SE, RiverFlow.Clockwise },
            });

            var left      = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise        } });
            var right     = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Counterclockwise } });
            var nextRight = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise        } });

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockGrid.Setup(grid => grid.GetNeighbor(left,      HexDirection.SE)).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(nextRight, HexDirection.NE)).Returns(right);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            RiverSection leftRightSection = sectionCanon.Sections[3];
            RiverSection sectionToTest = sectionCanon.Sections[1];

            Assert.IsTrue(sectionCanon.IsNeighborOf(sectionToTest, leftRightSection));
        }

        [Test]
        public void IsNeighborOf_TrueIfOtherSectionIsSectionBetweenCenterAndNextRight() {
            var center = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.E,  RiverFlow.Clockwise },
                { HexDirection.SE, RiverFlow.Clockwise },
            });

            var left      = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise        } });
            var right     = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Counterclockwise } });
            var nextRight = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise        } });

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockGrid.Setup(grid => grid.GetNeighbor(left,      HexDirection.SE)).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(nextRight, HexDirection.NE)).Returns(right);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            RiverSection centerNextRightSection = sectionCanon.Sections[2];
            RiverSection sectionToTest = sectionCanon.Sections[1];

            Assert.IsTrue(sectionCanon.IsNeighborOf(sectionToTest, centerNextRightSection));
        }

        [Test]
        public void IsNeighborOf_TrueIfOtherSectionIsSectionBetweenRightAndNextRight() {
            var center = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.E,  RiverFlow.Clockwise },
                { HexDirection.SE, RiverFlow.Clockwise },
            });

            var left      = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise        } });
            var right     = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Counterclockwise } });
            var nextRight = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise        } });

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockGrid.Setup(grid => grid.GetNeighbor(left,      HexDirection.SE)).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(nextRight, HexDirection.NE)).Returns(right);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            RiverSection rightNextRightSection = sectionCanon.Sections[5];
            RiverSection sectionToTest = sectionCanon.Sections[1];

            Assert.IsTrue(sectionCanon.IsNeighborOf(sectionToTest, rightNextRightSection));
        }

        [Test]
        public void IsNeighborOf_FalseIfOtherSectionNotOneOfTheCheckedSections() {
            var center = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() {
                { HexDirection.NE, RiverFlow.Clockwise },
                { HexDirection.E,  RiverFlow.Clockwise },
                { HexDirection.SE, RiverFlow.Clockwise },
            });

            var left      = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.SE, RiverFlow.Clockwise        } });
            var right     = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.E,  RiverFlow.Counterclockwise } });
            var nextRight = BuildCell(Vector3.zero, new Dictionary<HexDirection, RiverFlow>() { { HexDirection.NE, RiverFlow.Clockwise        } });

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockGrid.Setup(grid => grid.GetNeighbor(left,      HexDirection.SE)).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(nextRight, HexDirection.NE)).Returns(right);

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            sectionCanon.RefreshRiverSections();

            RiverSection unattachedSection = sectionCanon.Sections[4];
            RiverSection sectionToTest = sectionCanon.Sections[1];

            Assert.IsFalse(sectionCanon.IsNeighborOf(sectionToTest, unattachedSection));
        }

        #endregion

        #region AreSectionFlowsCongruous

        [Test]
        [TestCaseSource("AreSectionFlowsCongruousTestCases")]
        public bool AreSectionFlowsCongruousTests(AreSectionFlowsCongruousTestData testData) {
            var thisSection  = new RiverSection() {
                DirectionFromOne = testData.ThisSection.DirectionFromOne,
                FlowFromOne      = testData.ThisSection.FlowFromOne
            };

            var otherSection  = new RiverSection() {
                DirectionFromOne = testData.OtherSection.DirectionFromOne,
                FlowFromOne      = testData.OtherSection.FlowFromOne
            };

            var sectionCanon = Container.Resolve<RiverSectionCanon>();

            return sectionCanon.AreSectionFlowsCongruous(thisSection, otherSection);
        }

        #endregion

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector3 absolutePosition, Dictionary<HexDirection, RiverFlow> flowDict) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.AbsolutePosition).Returns(absolutePosition);

            var newCell = mockCell.Object;

            foreach(var riveredDirection in flowDict.Keys) {
                MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge   (newCell, riveredDirection)).Returns(true);
                MockRiverCanon.Setup(canon => canon.GetFlowOfRiverAtEdge(newCell, riveredDirection)).Returns(flowDict[riveredDirection]);
            }

            AllCells.Add(newCell);

            return newCell;
        }

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        #endregion

        #endregion

    }

}
