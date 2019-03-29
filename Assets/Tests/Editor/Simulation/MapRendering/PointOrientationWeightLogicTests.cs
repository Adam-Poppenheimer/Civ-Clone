using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;
using Assets.Util;

namespace Assets.Tests.Simulation.MapRendering {

    public class PointOrientationWeightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>      MockRenderConfig;
        private Mock<IGeometry2D>           MockGeometry2D;
        private Mock<ICellEdgeContourCanon> MockCellEdgeContourCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig         = new Mock<IMapRenderConfig>();
            MockGeometry2D           = new Mock<IGeometry2D>();
            MockCellEdgeContourCanon = new Mock<ICellEdgeContourCanon>();

            Container.Bind<IMapRenderConfig>     ().FromInstance(MockRenderConfig        .Object);
            Container.Bind<IGeometry2D>          ().FromInstance(MockGeometry2D          .Object);
            Container.Bind<ICellEdgeContourCanon>().FromInstance(MockCellEdgeContourCanon.Object);

            Container.Bind<PointOrientationWeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ApplyLandBesideRiverWeights_OfCellWeights_OnlyCenterWeightSetToOne() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData();

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideRiverWeights(point, orientationData);

            Assert.AreEqual(1f, orientationData.CenterWeight,      "Unexpected CenterWeight");
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
        }

        [Test]
        public void ApplyLandBesideRiverWeights_AndPointCloseToContour_RiverAlphaWeightSetToOne() {
            var center = BuildCell(new Vector2(1f, 1f));

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(contour);

            var point = new Vector2(1f, 2f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(2f, 2f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Center = center, Sextant = HexDirection.E
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideRiverWeights(point, orientationData);

            Assert.AreEqual(1f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideRiverWeights_AndPointNotCloseToContour_RiverAlphaWeightSetToZero() {
            var center = BuildCell(new Vector2(1f, 1f));

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(contour);

            var point = new Vector2(1f, 2f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(5f, 5f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Center = center, Sextant = HexDirection.E
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideRiverWeights(point, orientationData);

            Assert.AreEqual(0f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndRightIsNull_OnlyCenterWeightSetToOne() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData();

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, true, true);

            Assert.AreEqual(1f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");            
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
            Assert.AreEqual(0f, orientationData.RiverAlphaWeight,  "Unexpected RiverAlphaWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointInPreviousCornerTriangle_CellWeightsSetToBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = BuildCell(new Vector2(1f, 1f)),
                Left = BuildCell(new Vector2(2f, 2f)), Right = BuildCell(new Vector2(3f, 3f))
            };

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ(HexDirection.E )).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ(HexDirection.SW)).Returns(new Vector2(20f, 20f));
            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ(HexDirection.NW)).Returns(new Vector2(30f, 30f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(point, new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f))
            ).Returns(true);

            float expectedCenterWeight = 1.1f, expectedLeftWeight = 2.2f, expectedRightWeight = 3.3f;

            MockGeometry2D.Setup(geometry => geometry.GetBarycentric2D(
                point, new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f),
                out expectedCenterWeight, out expectedLeftWeight, out expectedRightWeight
            ));

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(1.1f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(2.2f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(3.3f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f,   orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointInNextCornerTriangle_WeightsSetToBarycentriCoords() {
            var point = new Vector2(-1f, -1f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = BuildCell(new Vector2(1f, 1f)),
                Right = BuildCell(new Vector2(2f, 2f)), NextRight = BuildCell(new Vector2(3f, 3f))
            };

            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E )).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.SW)).Returns(new Vector2(20f, 20f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.NW)).Returns(new Vector2(30f, 30f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(point, new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f))
            ).Returns(true);

            float expectedCenterWeight = 1.1f, expectedRightWeight = 2.2f, expectedNextRightWeight = 3.3f;

            MockGeometry2D.Setup(geometry => geometry.GetBarycentric2D(
                point, new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f),
                out expectedCenterWeight, out expectedRightWeight, out expectedNextRightWeight
            ));

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(1.1f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(2.2f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(3.3f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f,   orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointInSolidSextant_OnlyCenterWeightSetToOne() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(7f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(1f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointOnCenterSolidEdge_OnlyCenterHasWeight() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(10f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(1f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointBetweenCenterSolidAndOuterBoundary_BlendsCenterAndRight() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(12.5f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0.75f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f,    orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0.25f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f,    orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f,    orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointOnOuterBoundary_EvenMixOfCenterAndRight() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(15f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0.5f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0.5f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f,   orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointBetweenOuterBoundaryAndRightSolid_BlendsCenterAndRight() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(17.5f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0.25f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f,    orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0.75f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f,    orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f,    orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointOnRightSolidEdge_OnlyRightHasWeight() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(20f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointInRightSolidSextant_OnlyRightHasWeight() {
            var center = BuildCell(new Vector2(5f,  1f));
            var right  = BuildCell(new Vector2(25f, 1f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(5f,  0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.W)).Returns(new Vector2(-5f, 0f));

            var point = new Vector2(22f, 4.5f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_HasPreviousRiver_AndPointNearPreviousContour_SetsRiverAlphaWeightToOne() {
            var center = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(contour);

            var point = new Vector2(1f, 1f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(2f, 2f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, true, false);

            Assert.AreEqual(1f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideLandWeights_HasPreviousRiver_AndPointNotNearPreviousContour_SetsRiverAlphaWeightToZero() {
            var center = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(contour);

            var point = new Vector2(1f, 1f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(5f, 5f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, true, false);

            Assert.AreEqual(0f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideLandWeights_NoPreviousRiver_AndPointNearPreviousContour_SetsRiverAlphaWeightToZero() {
            var center = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(contour);

            var point = new Vector2(1f, 1f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(2f, 2f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideLandWeights_HasNextRiver_AndPointNearNextContour_SetsRiverAlphaWeightToOne() {
            var center = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.SE)).Returns(contour);

            var point = new Vector2(1f, 1f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(2f, 2f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, true);

            Assert.AreEqual(1f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideLandWeights_HasNextRiver_AndPointNotNearNextContour_SetsRiverAlphaWeightToZero() {
            var center = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.SE)).Returns(contour);

            var point = new Vector2(1f, 1f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(5f, 5f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, true);

            Assert.AreEqual(0f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyLandBesideLandWeights_NoNextRiver_AndPointNearNextContour_SetsRiverAlphaWeightToZero() {
            var center = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var contour = new List<Vector2>() { new Vector2(1f, 11f), new Vector2(2f, 22f), new Vector2(3f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.SE)).Returns(contour);

            var point = new Vector2(1f, 1f);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, contour)).Returns(new Vector2(2f, 2f));

            MockRenderConfig.Setup(config => config.RiverBankWidth).Returns(2f);

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData, false, false);

            Assert.AreEqual(0f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void ApplyRiverWeights_AndPointOnCenterRightContour_OnlyCenterHasWeight() {
            var point = new Vector2(5f, 18f);

            var center = BuildCell(new Vector2(1f, 1f));
            var right  = BuildCell(new Vector2(2f, 2f));

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var centerRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W)).Returns(rightCenterContour);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, centerRightContour)).Returns(new Vector2(5f,  1f));
            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, rightCenterContour)).Returns(new Vector2(15f, 1f));            

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyRiverWeights(point, orientationData);

            Assert.AreEqual(1f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyRiverWeights_AndPointBetweenCenterRightContourAndContourMidpoint_BlendsBetweenCenterAndRiverWeight() {
            var point = new Vector2(7.5f, 18f);

            var center = BuildCell(new Vector2(1f, 1f));
            var right  = BuildCell(new Vector2(2f, 2f));

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var centerRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W)).Returns(rightCenterContour);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, centerRightContour)).Returns(new Vector2(5f,  1f));
            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, rightCenterContour)).Returns(new Vector2(15f, 1f));            

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyRiverWeights(point, orientationData);

            Assert.AreEqual(0.5f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f,   orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0.5f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyRiverWeights_AndPointOnContourMidpoint_OnlyRiverHasWeight() {
            var point = new Vector2(10f, 18f);

            var center = BuildCell(new Vector2(1f, 1f));
            var right  = BuildCell(new Vector2(2f, 2f));

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var centerRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W)).Returns(rightCenterContour);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, centerRightContour)).Returns(new Vector2(5f,  1f));
            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, rightCenterContour)).Returns(new Vector2(15f, 1f));            

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyRiverWeights(point, orientationData);

            Assert.AreEqual(0f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(1f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyRiverWeights_AndPointBetweenContourMidpointAndRightCenterContour_BlendsBetweenRiverAndRightWeight() {
            var point = new Vector2(12.5f, 18f);

            var center = BuildCell(new Vector2(1f, 1f));
            var right  = BuildCell(new Vector2(2f, 2f));

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var centerRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W)).Returns(rightCenterContour);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, centerRightContour)).Returns(new Vector2(5f,  1f));
            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, rightCenterContour)).Returns(new Vector2(15f, 1f));            

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyRiverWeights(point, orientationData);

            Assert.AreEqual(0f,   orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(0.5f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0.5f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyRiverWeights_AndPointOnRightCenterContour_OnlyRightHasWeight() {
            var point = new Vector2(15f, 18f);

            var center = BuildCell(new Vector2(1f, 1f));
            var right  = BuildCell(new Vector2(2f, 2f));

            var orientationData = new PointOrientationData() {
                Sextant = HexDirection.E, Center = center, Right = right
            };

            var centerRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W)).Returns(rightCenterContour);

            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, centerRightContour)).Returns(new Vector2(5f,  1f));
            MockCellEdgeContourCanon.Setup(canon => canon.GetClosestPointOnContour(point, rightCenterContour)).Returns(new Vector2(15f, 1f));            

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyRiverWeights(point, orientationData);

            Assert.AreEqual(0f, orientationData.CenterWeight,      "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverHeightWeight, "Unexpected RiverHeightWeight");
        }

        [Test]
        public void ApplyRiverWeights_RiverAlphaWeightsSetToOne() {
            var orientationData = new PointOrientationData();

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyRiverWeights(Vector2.zero, orientationData);

            Assert.AreEqual(1f, orientationData.RiverAlphaWeight);
        }

        [Test]
        public void GetRiverCornerWeights_PointInBottomCenterCenterLeftTriangle_ReturnsCorrectWeightsFromBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            Vector2 centerLeftMidpoint = (centerRightContour.First() + leftCenterContour.First()) / 2f;
            Vector2 riverMidpoint      = (centerRightContour.First() + leftCenterContour.First() + rightCenterContour.Last()) / 3f;

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    point, centerRightContour.First(), centerLeftMidpoint, riverMidpoint
                )
            ).Returns(true);

            float coordA = 1f, coordB = 2f, coordC = 3f;

            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    point, centerRightContour.First(), centerLeftMidpoint, riverMidpoint,
                    out coordA, out coordB, out coordC
                )
            );

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(1f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(0f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(0f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(3f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_PointInCenterLeftLeftTriangle_ReturnsCorrectWeightsFromBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            Vector2 centerLeftMidpoint = (centerRightContour.First() + leftCenterContour.First()) / 2f;
            Vector2 riverMidpoint      = (centerRightContour.First() + leftCenterContour.First() + rightCenterContour.Last()) / 3f;

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    point, centerLeftMidpoint, leftCenterContour.First(), riverMidpoint
                )
            ).Returns(true);

            float coordA = 1f, coordB = 2f, coordC = 3f;

            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    point, centerLeftMidpoint, leftCenterContour.First(), riverMidpoint,
                    out coordA, out coordB, out coordC
                )
            );

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(0f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(2f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(0f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(3f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_PointInCenterCenterRightTriangle_ReturnsCorrectWeightsFromBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            Vector2 centerRightMidpoint = (centerRightContour.First() + rightCenterContour.Last()) / 2f;
            Vector2 riverMidpoint       = (centerRightContour.First() + leftCenterContour.First() + rightCenterContour.Last()) / 3f;

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    point, centerRightContour.First(), riverMidpoint, centerRightMidpoint
                )
            ).Returns(true);

            float coordA = 1f, coordB = 2f, coordC = 3f;

            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    point, centerRightContour.First(), riverMidpoint, centerRightMidpoint,
                    out coordA, out coordB, out coordC
                )
            );

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(1f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(0f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(0f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(3f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_PointInCenterRightRightTriangle_ReturnsCorrectWeightsFromBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            Vector2 centerRightMidpoint = (centerRightContour.First() + rightCenterContour.Last()) / 2f;
            Vector2 riverMidpoint       = (centerRightContour.First() + leftCenterContour.First() + rightCenterContour.Last()) / 3f;

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    point, centerRightMidpoint, riverMidpoint, rightCenterContour.Last()
                )
            ).Returns(true);

            float coordA = 1f, coordB = 2f, coordC = 3f;

            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    point, centerRightMidpoint, riverMidpoint, rightCenterContour.Last(),
                    out coordA, out coordB, out coordC
                )
            );

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(0f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(0f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(3f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(2f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_PointInLeftLeftRightTriangle_ReturnsCorrectWeightsFromBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            Vector2 leftRightMidpoint = (leftCenterContour.First() + rightCenterContour.Last()) / 2f;
            Vector2 riverMidpoint     = (centerRightContour.First() + leftCenterContour.First() + rightCenterContour.Last()) / 3f;

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    point, riverMidpoint, leftCenterContour.First(), leftRightMidpoint
                )
            ).Returns(true);

            float coordA = 1f, coordB = 2f, coordC = 3f;

            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    point, riverMidpoint, leftCenterContour.First(), leftRightMidpoint,
                    out coordA, out coordB, out coordC
                )
            );

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(0f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(2f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(0f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(3f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_PointInLeftRightRightTriangle_ReturnsCorrectWeightsFromBarycentricCoords() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            Vector2 leftRightMidpoint = (leftCenterContour.First() + rightCenterContour.Last()) / 2f;
            Vector2 riverMidpoint     = (centerRightContour.First() + leftCenterContour.First() + rightCenterContour.Last()) / 3f;

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    point, rightCenterContour.Last(), riverMidpoint, leftRightMidpoint
                )
            ).Returns(true);

            float coordA = 1f, coordB = 2f, coordC = 3f;

            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    point, rightCenterContour.Last(), riverMidpoint, leftRightMidpoint,
                    out coordA, out coordB, out coordC
                )
            );

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(0f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(0f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(1f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(3f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_PointInNoTriangle_ReturnsZeroForAllWeights() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(0f, centerWeight,      "Unexpected centerWeight");
            Assert.AreEqual(0f, leftWeight,        "Unexpected leftWeight");
            Assert.AreEqual(0f, rightWeight,       "Unexpected rightWeight");
            Assert.AreEqual(0f, riverHeightWeight, "Unexpected riverHeightWeight");
        }

        [Test]
        public void GetRiverCornerWeights_RiverAlphaWeightSetToOne() {
            var point = new Vector2(-1f, -1f);

            var center = BuildCell(Vector2.zero);
            var left   = BuildCell(Vector2.zero);
            var right  = BuildCell(Vector2.zero);

            var centerRightContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(11f, 11f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(2f, 2f), new Vector2(22f, 22f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(3f, 3f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            float centerWeight, leftWeight, rightWeight, riverHeightWeight, riverAlphaWeight;

            weightLogic.GetRiverCornerWeights(
                point, center, left, right, HexDirection.E,
                out centerWeight, out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
            );

            Assert.AreEqual(1f, riverAlphaWeight);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector2 absolutePositionXZ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.AbsolutePositionXZ).Returns(absolutePositionXZ);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
