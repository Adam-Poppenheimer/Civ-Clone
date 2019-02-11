using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class FeatureLocationLogic : IFeatureLocationLogic {

        #region instance fields and properties

        private IHexGrid            Grid;
        private IMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public FeatureLocationLogic(IHexGrid grid, IMapRenderConfig renderConfig) {
            Grid         = grid;
            RenderConfig = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IFeatureLocationLogic

        //This method adds a single feature point right in the middle of the cell
        public IEnumerable<Vector3> GetCenterFeaturePoints(IHexCell cell) {
            return new Vector3[]{
                Grid.PerformIntersectionWithTerrainSurface(cell.GridRelativePosition)
            };
        }

        //This method divides the sextant of the cell (excluding the edge regions between cells)
        //in the current direction into four triangles. For each of these triangles, it then
        //adds a single location somewhere between each of that triangle's vertices and that
        //triangle's midpoint.
        //Triangles one and two are abutting the outer edge of the cell. Triangle
        //three is right in the middle of the sextant, and triangle four is closest
        //to the edge, with one vertex right in the middle of the cell.
        public IEnumerable<Vector3> GetDirectionalFeaturePoints(IHexCell cell, HexDirection direction) {
            var center = cell.GridRelativePosition;
            var cornerOne = RenderConfig.GetFirstSolidCorner (direction) + center;
            var cornerTwo = RenderConfig.GetSecondSolidCorner(direction) + center;

            var edgeMidpoint = (cornerOne + cornerTwo) / 2f;

            var leftMidpoint  = (center + cornerOne) / 2f;
            var rightMidpoint = (center + cornerTwo) / 2f;

            var retval = new List<Vector3>();

            AddFeaturePointsFromTriangle(cornerOne,    edgeMidpoint, leftMidpoint,  retval);
            AddFeaturePointsFromTriangle(edgeMidpoint, cornerTwo,    rightMidpoint, retval);
            AddFeaturePointsFromTriangle(leftMidpoint, edgeMidpoint, rightMidpoint, retval);
            AddFeaturePointsFromTriangle(center,       leftMidpoint, rightMidpoint, retval);

            return retval;
        }

        private void AddFeaturePointsFromTriangle(
            Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree, List<Vector3> currentLocations
        ) {
            var triangleMidpoint = (vertexOne + vertexTwo + vertexThree) / 3f;

            Vector3 terrainPoint;

            if(Grid.TryPerformIntersectionWithTerrainSurface(Vector3.Lerp(vertexOne, triangleMidpoint, 0.5f), out terrainPoint)) {
                currentLocations.Add(terrainPoint);
            }

            if(Grid.TryPerformIntersectionWithTerrainSurface(Vector3.Lerp(vertexTwo, triangleMidpoint, 0.5f), out terrainPoint)) {
                currentLocations.Add(terrainPoint);
            }

            if(Grid.TryPerformIntersectionWithTerrainSurface(Vector3.Lerp(vertexThree, triangleMidpoint, 0.5f), out terrainPoint)) {
                currentLocations.Add(terrainPoint);
            }
        }

        #endregion

        #endregion
        
    }

}
