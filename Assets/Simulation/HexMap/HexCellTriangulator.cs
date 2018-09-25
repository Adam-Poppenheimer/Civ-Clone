using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexCellTriangulator : IHexCellTriangulator {

        #region instance fields and properties

        private IHexGrid                  Grid;
        private IRiverTriangulator        RiverTriangulator;
        private IHexGridMeshBuilder       MeshBuilder;
        private ICultureTriangulator      CultureTriangulator;
        private IBasicTerrainTriangulator BasicTerrainTriangulator;
        private IWaterTriangulator        WaterTriangulator;
        private IRoadTriangulator         RoadTriangulator;
        private IMarshTriangulator        MarshTriangulator;
        private IFloodPlainsTriangulator  FloodPlainsTriangulator;
        private IOasisTriangulator        OasisTriangulator;
        private IFarmTriangulator         FarmTriangulator;

        #endregion

        #region constructors

        [Inject]
        public HexCellTriangulator(
            IHexGrid grid, IRiverTriangulator riverTriangulator,
            IHexGridMeshBuilder meshBuilder, ICultureTriangulator cultureTriangulator,
            IBasicTerrainTriangulator basicTerrainTriangulator,
            IWaterTriangulator waterTriangulator, IRoadTriangulator roadTriangulator,
            IMarshTriangulator marshTriangulator, IFloodPlainsTriangulator floodPlainsTriangulator,
            IOasisTriangulator oasisTriangulator, IFarmTriangulator farmTriangulator
        ) {
            Grid                     = grid;
            RiverTriangulator        = riverTriangulator;
            MeshBuilder              = meshBuilder;
            CultureTriangulator      = cultureTriangulator;
            BasicTerrainTriangulator = basicTerrainTriangulator;
            WaterTriangulator        = waterTriangulator;
            RoadTriangulator         = roadTriangulator;
            MarshTriangulator        = marshTriangulator;
            FloodPlainsTriangulator  = floodPlainsTriangulator;
            OasisTriangulator        = oasisTriangulator;
            FarmTriangulator         = farmTriangulator;
        }

        #endregion

        #region instance methods

        #region HexCellTriangulationLogic

        public void TriangulateCell(IHexCell cell) {
            for(HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; ++direction) {
                TriangulateInDirection(direction, cell);
            }
        }

        #endregion

        private void TriangulateInDirection(HexDirection direction, IHexCell cell) {
            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            var neighbor         = Grid.GetNeighbor(cell, direction);

            var thisData = MeshBuilder.GetTriangulationData(
                cell, previousNeighbor, neighbor, direction
            );

            BasicTerrainTriangulator.TriangulateTerrainCenter(thisData);

            if(RiverTriangulator.ShouldTriangulateRiver(thisData)) {
                RiverTriangulator.TriangulateRiver(thisData);
            }

            if(BasicTerrainTriangulator.ShouldTriangulateTerrainEdge(thisData)) {
                BasicTerrainTriangulator.TriangulateTerrainEdge(thisData);
            }

            if(WaterTriangulator.ShouldTriangulateWater(thisData)) {
                WaterTriangulator.TriangulateWater(thisData);
            }

            if(CultureTriangulator.ShouldTriangulateCulture(thisData)) {
                CultureTriangulator.TriangulateCulture(thisData);               
            }

            if(RoadTriangulator.ShouldTriangulateRoads(thisData)) {
                RoadTriangulator.TriangulateRoads(thisData);
            }

            if(MarshTriangulator.ShouldTriangulateMarshCenter(thisData)) {
                MarshTriangulator.TriangulateMarshCenter(thisData);
            }

            if(MarshTriangulator.ShouldTriangulateMarshEdge(thisData)) {
                MarshTriangulator.TriangulateMarshEdge(thisData);
            }

            if(MarshTriangulator.ShouldTriangulateMarshCorner(thisData)) {
                MarshTriangulator.TriangulateMarshCorner(thisData);
            }

            if(FloodPlainsTriangulator.ShouldTriangulateFloodPlainEdge(thisData)) {
                FloodPlainsTriangulator.TriangulateFloodPlainEdge(thisData);
            }

            if(FloodPlainsTriangulator.ShouldTriangulateFloodPlainCorner(thisData)) {
                FloodPlainsTriangulator.TriangulateFloodPlainCorner(thisData);
            }

            if(OasisTriangulator.ShouldTriangulateOasis(thisData)) {
                OasisTriangulator.TriangulateOasis(thisData);
            }

            if(FarmTriangulator.ShouldTriangulateFarmCenter(thisData)) {
                FarmTriangulator.TriangulateFarmCenter(thisData);
            }

            if(FarmTriangulator.ShouldTriangulateFarmEdge(thisData)) {
                FarmTriangulator.TriangulateFarmEdge(thisData);
            }

            if(FarmTriangulator.ShouldTriangulateFarmCorner(thisData)) {
                FarmTriangulator.TriangulateFarmCorner(thisData);
            }
        }

        #endregion

    }

}
