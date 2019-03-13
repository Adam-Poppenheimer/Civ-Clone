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
        public void ApplyLandBesideRiverWeights_AndTakeFromRightTrue_OnlyRightWeightSetToOne() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData();

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideRiverWeights(point, orientationData, true);

            Assert.AreEqual(0f, orientationData.CenterWeight,    "Unexpected CenterWeight");
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
        }

        [Test]
        public void ApplyLandBesideRiverWeights_AndTakeFromRightFalse_OnlyCenterWeightSetToOne() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData();

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideRiverWeights(point, orientationData, false);

            Assert.AreEqual(1f, orientationData.CenterWeight,    "Unexpected CenterWeight");
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndRightIsNull_OnlyCenterWeightSetToOne() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData();

            var weightLogic = Container.Resolve<PointOrientationWeightLogic>();

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(1f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");            
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
        }

        [Test]
        public void ApplyLandBesideLandWeights_AndPointInPreviousCornerTriangle_WeightsSetToBarycentricCoords() {
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(1.1f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(2.2f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(3.3f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f,   orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(1.1f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(2.2f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(3.3f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f,   orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(1f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(1f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(0.75f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f,    orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0.25f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f,    orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f,    orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(0.5f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0.5f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f,   orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(0.25f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f,    orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0.75f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f,    orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f,    orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(0f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            weightLogic.ApplyLandBesideLandWeights(point, orientationData);

            Assert.AreEqual(0f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            Assert.AreEqual(1f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            Assert.AreEqual(0.5f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f,   orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0.5f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            Assert.AreEqual(0f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(1f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            Assert.AreEqual(0f,   orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f,   orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(0.5f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f,   orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0.5f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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

            Assert.AreEqual(0f, orientationData.CenterWeight,    "Unexpected CenterWeight");            
            Assert.AreEqual(0f, orientationData.LeftWeight,      "Unexpected LeftWeight");
            Assert.AreEqual(1f, orientationData.RightWeight,     "Unexpected RightWeight");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "Unexpected NextRightWeight");
            Assert.AreEqual(0f, orientationData.RiverWeight,     "Unexpected RiverWeight");
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
