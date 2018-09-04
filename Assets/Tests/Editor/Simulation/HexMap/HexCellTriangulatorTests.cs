﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.HexMap {

    public class HexCellTriangulatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                  MockGrid;
        private Mock<IRiverTriangulator>        MockRiverTriangulator;
        private Mock<IHexGridMeshBuilder>       MockMeshBuilder;
        private Mock<ICultureTriangulator>      MockCultureTriangulator;
        private Mock<IBasicTerrainTriangulator> MockBasicTerrainTriangulator;
        private Mock<IWaterTriangulator>        MockWaterTriangulator;
        private Mock<IRoadTriangulator>         MockRoadTriangulator;
        private Mock<IMarshTriangulator>        MockMarshTriangulator;
        private Mock<IFloodPlainsTriangulator>  MockFloodPlainsTriangulator;
        private Mock<IOasisTriangulator>        MockOasisTriangulator;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                     = new Mock<IHexGrid>();
            MockRiverTriangulator        = new Mock<IRiverTriangulator>();
            MockMeshBuilder              = new Mock<IHexGridMeshBuilder>();
            MockCultureTriangulator      = new Mock<ICultureTriangulator>();
            MockBasicTerrainTriangulator = new Mock<IBasicTerrainTriangulator>();
            MockWaterTriangulator        = new Mock<IWaterTriangulator>();
            MockRoadTriangulator         = new Mock<IRoadTriangulator>();
            MockMarshTriangulator        = new Mock<IMarshTriangulator>();
            MockFloodPlainsTriangulator  = new Mock<IFloodPlainsTriangulator>();
            MockOasisTriangulator        = new Mock<IOasisTriangulator>();

            Container.Bind<IHexGrid>                 ().FromInstance(MockGrid                    .Object);
            Container.Bind<IRiverTriangulator>       ().FromInstance(MockRiverTriangulator       .Object);
            Container.Bind<IHexGridMeshBuilder>      ().FromInstance(MockMeshBuilder             .Object);
            Container.Bind<ICultureTriangulator>     ().FromInstance(MockCultureTriangulator     .Object);
            Container.Bind<IBasicTerrainTriangulator>().FromInstance(MockBasicTerrainTriangulator.Object);
            Container.Bind<IWaterTriangulator>       ().FromInstance(MockWaterTriangulator       .Object);
            Container.Bind<IRoadTriangulator>        ().FromInstance(MockRoadTriangulator        .Object);
            Container.Bind<IMarshTriangulator>       ().FromInstance(MockMarshTriangulator       .Object);
            Container.Bind<IFloodPlainsTriangulator> ().FromInstance(MockFloodPlainsTriangulator .Object);
            Container.Bind<IOasisTriangulator>       ().FromInstance(MockOasisTriangulator       .Object);

            Container.Bind<HexCellTriangulator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void TriangulateCell_TriangulatesTerrainCenterInEachDirection() {
            var cellToTest = BuildCell(new Vector3(1f, 2f, 3f));

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainCenter(northEastData),
                Times.Once, "TriangulateTerrainCenter not called on northEastData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainCenter(eastData),
                Times.Once, "TriangulateTerrainCenter not called on eastData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainCenter(southEastData),
                Times.Once, "TriangulateTerrainCenter not called on southEastData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainCenter(southWestData),
                Times.Once, "TriangulateTerrainCenter not called on southWestData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainCenter(westData),
                Times.Once, "TriangulateTerrainCenter not called on westData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainCenter(northWestData),
                Times.Once, "TriangulateTerrainCenter not called on northWestData as expected"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesRiverInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(northEastData)).Returns(true);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(eastData))     .Returns(false);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(southEastData)).Returns(true);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(southWestData)).Returns(false);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(westData))     .Returns(true);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockRiverTriangulator.Verify(
                triangulator => triangulator.TriangulateRiver(northEastData),
                Times.Once, "Did not call TriangulateRiver on northEastData as expected"
            );

            MockRiverTriangulator.Verify(
                triangulator => triangulator.TriangulateRiver(southEastData),
                Times.Once, "Did not call TriangulateRiver on southEastData as expected"
            );

            MockRiverTriangulator.Verify(
                triangulator => triangulator.TriangulateRiver(westData),
                Times.Once, "Did not call TriangulateRiver on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateRiverInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(northEastData)).Returns(true);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(eastData))     .Returns(false);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(southEastData)).Returns(true);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(southWestData)).Returns(false);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(westData))     .Returns(true);
            MockRiverTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRiver(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockRiverTriangulator.Verify(
                triangulator => triangulator.TriangulateRiver(eastData),
                Times.Never, "Unexpectedly called TriangulateRiver on eastData"
            );

            MockRiverTriangulator.Verify(
                triangulator => triangulator.TriangulateRiver(southWestData),
                Times.Never, "Unexpectedly called TriangulateRiver on southWestData"
            );

            MockRiverTriangulator.Verify(
                triangulator => triangulator.TriangulateRiver(northWestData),
                Times.Never, "Unexpectedly called TriangulateRiver on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesTerrainInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(northEastData)).Returns(true);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(eastData))     .Returns(false);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(southEastData)).Returns(true);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(southWestData)).Returns(false);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(westData))     .Returns(true);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainEdge(northEastData),
                Times.Once, "Did not call TriangulateTerrainEdge on northEastData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainEdge(southEastData),
                Times.Once, "Did not call TriangulateTerrainEdge on southEastData as expected"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainEdge(westData),
                Times.Once, "Did not call TriangulateTerrainEdge on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateTerrainInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(northEastData)).Returns(true);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(eastData))     .Returns(false);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(southEastData)).Returns(true);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(southWestData)).Returns(false);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(westData))     .Returns(true);
            MockBasicTerrainTriangulator.Setup(triangulator => triangulator.ShouldTriangulateTerrainEdge(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainEdge(eastData),
                Times.Never, "Unexpectedly called TriangulateTerrainEdge on eastData"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainEdge(southWestData),
                Times.Never, "Unexpectedly called TriangulateTerrainEdge on southWestData"
            );

            MockBasicTerrainTriangulator.Verify(
                triangulator => triangulator.TriangulateTerrainEdge(northWestData),
                Times.Never, "Unexpectedly called TriangulateTerrainEdge on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesWaterInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(northEastData)).Returns(true);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(eastData))     .Returns(false);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(southEastData)).Returns(true);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(southWestData)).Returns(false);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(westData))     .Returns(true);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockWaterTriangulator.Verify(
                triangulator => triangulator.TriangulateWater(northEastData),
                Times.Once, "Did not call TriangulateWater on northEastData as expected"
            );

            MockWaterTriangulator.Verify(
                triangulator => triangulator.TriangulateWater(southEastData),
                Times.Once, "Did not call TriangulateWater on southEastData as expected"
            );

            MockWaterTriangulator.Verify(
                triangulator => triangulator.TriangulateWater(westData),
                Times.Once, "Did not call TriangulateWater on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateWaterInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(northEastData)).Returns(true);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(eastData))     .Returns(false);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(southEastData)).Returns(true);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(southWestData)).Returns(false);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(westData))     .Returns(true);
            MockWaterTriangulator.Setup(triangulator => triangulator.ShouldTriangulateWater(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockWaterTriangulator.Verify(
                triangulator => triangulator.TriangulateWater(eastData),
                Times.Never, "Unexpectedly called TriangulateWater on eastData"
            );

            MockWaterTriangulator.Verify(
                triangulator => triangulator.TriangulateWater(southWestData),
                Times.Never, "Unexpectedly called TriangulateWater on southWestData"
            );

            MockWaterTriangulator.Verify(
                triangulator => triangulator.TriangulateWater(northWestData),
                Times.Never, "Unexpectedly called TriangulateWater on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesCultureInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(northEastData)).Returns(true);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(eastData))     .Returns(false);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(southEastData)).Returns(true);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(southWestData)).Returns(false);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(westData))     .Returns(true);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockCultureTriangulator.Verify(
                triangulator => triangulator.TriangulateCulture(northEastData),
                Times.Once, "Did not call TriangulateCulture on northEastData as expected"
            );

            MockCultureTriangulator.Verify(
                triangulator => triangulator.TriangulateCulture(southEastData),
                Times.Once, "Did not call TriangulateCulture on southEastData as expected"
            );

            MockCultureTriangulator.Verify(
                triangulator => triangulator.TriangulateCulture(westData),
                Times.Once, "Did not call TriangulateCulture on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateCultureInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(northEastData)).Returns(true);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(eastData))     .Returns(false);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(southEastData)).Returns(true);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(southWestData)).Returns(false);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(westData))     .Returns(true);
            MockCultureTriangulator.Setup(triangulator => triangulator.ShouldTriangulateCulture(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockCultureTriangulator.Verify(
                triangulator => triangulator.TriangulateCulture(eastData),
                Times.Never, "Unexpectedly called TriangulateCulture on eastData"
            );

            MockCultureTriangulator.Verify(
                triangulator => triangulator.TriangulateCulture(southWestData),
                Times.Never, "Unexpectedly called TriangulateCulture on southWestData"
            );

            MockCultureTriangulator.Verify(
                triangulator => triangulator.TriangulateCulture(northWestData),
                Times.Never, "Unexpectedly called TriangulateCulture on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesRoadsInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(northEastData)).Returns(true);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(eastData))     .Returns(false);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(southEastData)).Returns(true);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(southWestData)).Returns(false);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(westData))     .Returns(true);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockRoadTriangulator.Verify(
                triangulator => triangulator.TriangulateRoads(northEastData),
                Times.Once, "Did not call TriangulateRoads on northEastData as expected"
            );

            MockRoadTriangulator.Verify(
                triangulator => triangulator.TriangulateRoads(southEastData),
                Times.Once, "Did not call TriangulateRoads on southEastData as expected"
            );

            MockRoadTriangulator.Verify(
                triangulator => triangulator.TriangulateRoads(westData),
                Times.Once, "Did not call TriangulateRoads on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateRoadsInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(northEastData)).Returns(true);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(eastData))     .Returns(false);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(southEastData)).Returns(true);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(southWestData)).Returns(false);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(westData))     .Returns(true);
            MockRoadTriangulator.Setup(triangulator => triangulator.ShouldTriangulateRoads(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockRoadTriangulator.Verify(
                triangulator => triangulator.TriangulateRoads(eastData),
                Times.Never, "Unexpectedly called TriangulateRoads on eastData"
            );

            MockRoadTriangulator.Verify(
                triangulator => triangulator.TriangulateRoads(southWestData),
                Times.Never, "Unexpectedly called TriangulateRoads on southWestData"
            );

            MockRoadTriangulator.Verify(
                triangulator => triangulator.TriangulateRoads(northWestData),
                Times.Never, "Unexpectedly called TriangulateRoads on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesMarshCenterInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(northEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(eastData))     .Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(southEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(southWestData)).Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(westData))     .Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCenter(northEastData),
                Times.Once, "Did not call TriangulateMarshCenter on northEastData as expected"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCenter(southEastData),
                Times.Once, "Did not call TriangulateMarshCenter on southEastData as expected"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCenter(westData),
                Times.Once, "Did not call TriangulateMarshCenter on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesntTriangulateMarshCenterInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(northEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(eastData))     .Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(southEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(southWestData)).Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(westData))     .Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCenter(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCenter(eastData),
                Times.Never, "Unexpectedly called TriangulateRoads on eastData"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCenter(southWestData),
                Times.Never, "Unexpectedly called TriangulateRoads on southWestData"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCenter(northWestData),
                Times.Never, "Unexpectedly called TriangulateRoads on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesMarshEdgeInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(northEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(eastData))     .Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(southEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(southWestData)).Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(westData))     .Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshEdge(northEastData),
                Times.Once, "Did not call TriangulateMarshEdge on northEastData as expected"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshEdge(southEastData),
                Times.Once, "Did not call TriangulateMarshEdge on southEastData as expected"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshEdge(westData),
                Times.Once, "Did not call TriangulateMarshEdge on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesntTriangulateMarshEdgeInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(northEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(eastData))     .Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(southEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(southWestData)).Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(westData))     .Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshEdge(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshEdge(eastData),
                Times.Never, "Unexpectedly called TriangulateMarshEdge on eastData"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshEdge(southWestData),
                Times.Never, "Unexpectedly called TriangulateMarshEdge on southWestData"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshEdge(northWestData),
                Times.Never, "Unexpectedly called TriangulateMarshEdge on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesMarshCornerInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(northEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(eastData))     .Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(southEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(southWestData)).Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(westData))     .Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCorner(northEastData),
                Times.Once, "Did not call TriangulateMarshCorner on northEastData as expected"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCorner(southEastData),
                Times.Once, "Did not call TriangulateMarshCorner on southEastData as expected"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCorner(westData),
                Times.Once, "Did not call TriangulateMarshCorner on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesntTriangulateMarshCornerInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(northEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(eastData))     .Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(southEastData)).Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(southWestData)).Returns(false);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(westData))     .Returns(true);
            MockMarshTriangulator.Setup(triangulator => triangulator.ShouldTriangulateMarshCorner(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCorner(eastData),
                Times.Never, "Unexpectedly called TriangulateMarshCorner on eastData"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCorner(southWestData),
                Times.Never, "Unexpectedly called TriangulateMarshCorner on southWestData"
            );

            MockMarshTriangulator.Verify(
                triangulator => triangulator.TriangulateMarshCorner(northWestData),
                Times.Never, "Unexpectedly called TriangulateMarshCorner on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesFloodPlainEdgeInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(northEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(eastData))     .Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(southEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(southWestData)).Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(westData))     .Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainEdge(northEastData),
                Times.Once, "Did not call TriangulateFloodPlainEdge on northEastData as expected"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainEdge(southEastData),
                Times.Once, "Did not call TriangulateFloodPlainEdge on southEastData as expected"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainEdge(westData),
                Times.Once, "Did not call TriangulateFloodPlainEdge on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateFloodPlainEdgeInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(northEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(eastData))     .Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(southEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(southWestData)).Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(westData))     .Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainEdge(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainEdge(eastData),
                Times.Never, "Unexpectedly called TriangulateFloodPlainEdge on eastData"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainEdge(southWestData),
                Times.Never, "Unexpectedly called TriangulateFloodPlainEdge on southWestData"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainEdge(northWestData),
                Times.Never, "Unexpectedly called TriangulateFloodPlainEdge on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesFloodPlainCornerInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(northEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(eastData))     .Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(southEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(southWestData)).Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(westData))     .Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainCorner(northEastData),
                Times.Once, "Did not call TriangulateFloodPlainCorner on northEastData as expected"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainCorner(southEastData),
                Times.Once, "Did not call TriangulateFloodPlainCorner on southEastData as expected"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainCorner(westData),
                Times.Once, "Did not call TriangulateFloodPlainCorner on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesNotTriangulateFloodPlainCornerInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(northEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(eastData))     .Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(southEastData)).Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(southWestData)).Returns(false);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(westData))     .Returns(true);
            MockFloodPlainsTriangulator.Setup(triangulator => triangulator.ShouldTriangulateFloodPlainCorner(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainCorner(eastData),
                Times.Never, "Unexpectedly called TriangulateFloodPlainCorner on eastData"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainCorner(southWestData),
                Times.Never, "Unexpectedly called TriangulateFloodPlainCorner on southWestData"
            );

            MockFloodPlainsTriangulator.Verify(
                triangulator => triangulator.TriangulateFloodPlainCorner(northWestData),
                Times.Never, "Unexpectedly called TriangulateFloodPlainCorner on northWestData"
            );
        }

        [Test]
        public void TriangulateCell_TriangulatesOasesInDirectionsItShould() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(northEastData)).Returns(true);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(eastData))     .Returns(false);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(southEastData)).Returns(true);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(southWestData)).Returns(false);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(westData))     .Returns(true);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockOasisTriangulator.Verify(
                triangulator => triangulator.TriangulateOasis(northEastData),
                Times.Once, "Did not call TriangulateOasis on northEastData as expected"
            );

            MockOasisTriangulator.Verify(
                triangulator => triangulator.TriangulateOasis(southEastData),
                Times.Once, "Did not call TriangulateOasis on southEastData as expected"
            );

            MockOasisTriangulator.Verify(
                triangulator => triangulator.TriangulateOasis(westData),
                Times.Once, "Did not call TriangulateOasis on westData as expected"
            );
        }

        [Test]
        public void TriangulateCell_DoesntTriangulateOasesInDirectionsItShouldnt() {
            var cellToTest = BuildCell(Vector3.zero);

            var northEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NE);
            var eastData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.E);
            var southEastData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SE);
            var southWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.SW);
            var westData      = BuildTriangulationDataInDirection(cellToTest, HexDirection.W);
            var northWestData = BuildTriangulationDataInDirection(cellToTest, HexDirection.NW);

            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(northEastData)).Returns(true);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(eastData))     .Returns(false);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(southEastData)).Returns(true);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(southWestData)).Returns(false);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(westData))     .Returns(true);
            MockOasisTriangulator.Setup(triangulator => triangulator.ShouldTriangulateOasis(northWestData)).Returns(false);

            var cellTriangulator = Container.Resolve<HexCellTriangulator>();

            cellTriangulator.TriangulateCell(cellToTest);

            MockOasisTriangulator.Verify(
                triangulator => triangulator.TriangulateOasis(eastData),
                Times.Never, "Unexpectedly called TriangulateOasis on eastData"
            );

            MockOasisTriangulator.Verify(
                triangulator => triangulator.TriangulateOasis(southWestData),
                Times.Never, "Unexpectedly called TriangulateOasis on southWestData"
            );

            MockOasisTriangulator.Verify(
                triangulator => triangulator.TriangulateOasis(northWestData),
                Times.Never, "Unexpectedly called TriangulateOasis on northWestData"
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector3 localPosition) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.LocalPosition).Returns(localPosition);

            return mockCell.Object;
        }

        private CellTriangulationData BuildTriangulationDataInDirection(
            IHexCell cell, HexDirection direction
        ){
            var newData = new CellTriangulationData(cell, null, null, null, direction, null, null);

            MockMeshBuilder.Setup(
                builder => builder.GetTriangulationData(cell, null, null, direction)
            ).Returns(newData);

            return newData;
        }

        #endregion

        #endregion

    }

}
