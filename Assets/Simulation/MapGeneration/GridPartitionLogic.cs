using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Simulation.MapGeneration {

    public class GridPartitionLogic : IGridPartitionLogic {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public GridPartitionLogic(IMapRenderConfig renderConfig) {
            RenderConfig = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IGridPartitionLogic

        //Creates something akin to a Voronoi diagram of the cells,
        //using a process similiar to Lloyd's algorithm to make the
        //shapes more regular
        public GridPartition GetPartitionOfGrid(IHexGrid grid, IMapTemplate template) {
            float xMin = 0f, zMin = 0f;
            float xMax = (grid.CellCountX + grid.CellCountZ * 0.5f - grid.CellCountZ / 2) * RenderConfig.InnerRadius * 2f;
            float zMax = grid.CellCountZ * RenderConfig.OuterRadius * 1.5f;

            var randomPoints = new List<Vector3>();

            var regionOfPoint = new Dictionary<Vector3, MapSection>();

            for(int i = 0; i < template.VoronoiPointCount; i++) {
                var randomPoint = new Vector3(
                    UnityEngine.Random.Range(xMin, xMax),
                    0f,
                    UnityEngine.Random.Range(zMin, zMax)
                );

                regionOfPoint[randomPoint] = new MapSection(grid);

                randomPoints.Add(randomPoint);
            }

            int iterationsLeft = template.VoronoiPartitionIterations;
            while(iterationsLeft > 0) {
                foreach(var cell in grid.Cells) {
                    Vector3 nearestPoint = Vector3.zero;
                    float shorestDistance = float.MaxValue;

                    foreach(var voronoiPoint in regionOfPoint.Keys) {
                        float distanceTo = Vector3.Distance(cell.GridRelativePosition, voronoiPoint);

                        if(distanceTo < shorestDistance) {
                            nearestPoint = voronoiPoint;
                            shorestDistance = distanceTo;
                        }
                    }

                    if(regionOfPoint.ContainsKey(nearestPoint)) {
                        regionOfPoint[nearestPoint].AddCell(cell);
                    }                
                }

                if(--iterationsLeft > 0) {
                    randomPoints.Clear();

                    var newRegionOfPoints = new Dictionary<Vector3, MapSection>();
                    foreach(var region in regionOfPoint.Values) {
                        if(region.Cells.Count > 0) {
                            randomPoints.Add(region.Centroid);

                            newRegionOfPoints[region.Centroid] = new MapSection(grid);
                        }
                    }

                    regionOfPoint = newRegionOfPoints;
                }
            }

            return new GridPartition(regionOfPoint.Values, grid);
        }

        #endregion

        #endregion

    }

}
