using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexFeatureManager : IHexFeatureManager {

        #region instance fields and properties

        private DictionaryOfLists<IHexCell, Vector3> FeaturePositionsForCell =
            new DictionaryOfLists<IHexCell, Vector3>();




        private INoiseGenerator   NoiseGenerator;
        private IHexGrid          Grid;
        private FeaturePlacerBase CityFeaturePlacer;
        private FeaturePlacerBase ResourceFeaturePlacer;
        private FeaturePlacerBase ImprovementFeaturePlacer;
        private FeaturePlacerBase TreeFeaturePlacer;
        private Transform         FeatureContainer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            INoiseGenerator noiseGenerator, IHexGrid grid,
            [Inject(Id = "City Feature Placer")]        FeaturePlacerBase cityFeaturePlacer,
            [Inject(Id = "Resource Feature Placer")]    FeaturePlacerBase resourceFeaturePlacer,
            [Inject(Id = "Improvement Feature Placer")] FeaturePlacerBase improvementFeaturePlacer,
            [Inject(Id = "Tree Feature Placer")]        FeaturePlacerBase treeFeaturePlacer,
            [Inject(Id = "Feature Container")] Transform featureContainer
        ){
            NoiseGenerator           = noiseGenerator;
            Grid                     = grid;
            CityFeaturePlacer        = cityFeaturePlacer;
            ResourceFeaturePlacer    = resourceFeaturePlacer;
            ImprovementFeaturePlacer = improvementFeaturePlacer;
            TreeFeaturePlacer        = treeFeaturePlacer;
            FeatureContainer         = featureContainer;
        }

        public void Clear() {
            for(int i = FeatureContainer.childCount - 1; i >= 0; --i) {
                GameObject.Destroy(FeatureContainer.GetChild(i).gameObject);
            }
        }

        public void Apply() {
            foreach(var cellWithFeatures in FeaturePositionsForCell.Keys) {
                ApplyFeaturesToCell(cellWithFeatures, FeaturePositionsForCell[cellWithFeatures]);
            }

            FeaturePositionsForCell.Clear();
        }



        public void AddFeatureLocationsForCell(IHexCell cell) {
            var listOfLocations = new List<Vector3>();

            AddCenterFeaturePoints(cell, listOfLocations);

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                AddDirectionalFeaturePoints(cell, direction, listOfLocations);
            }

            FeaturePositionsForCell[cell] = listOfLocations;
        }

        private void AddCenterFeaturePoints(IHexCell cell, List<Vector3> pointList) {
            pointList.Add(Grid.PerformIntersectionWithTerrainSurface(cell.LocalPosition));
        }

        //This method divides the sextant of the cell (excluding the edge regions between cells
        //in the current direction into four triangles. For each of these triangles, it then
        //adds a single location somewhere between each of that triangle's vertices and that
        //triangle's midpoint.
        //Triangles one and two are abutting the outer edge of the cell. Triangle
        //three is right in the middle of the sextant, and triangle four is closest
        //to the edge, with one vertex right in the middle of the cell.
        private void AddDirectionalFeaturePoints(
            IHexCell cell, HexDirection direction, List<Vector3> pointList
        ) {
            var center = cell.LocalPosition;
            var cornerOne = HexMetrics.GetFirstOuterSolidCorner (direction) + center;
            var cornerTwo = HexMetrics.GetSecondOuterSolidCorner(direction) + center;

            var edgeMidpoint = (cornerOne + cornerTwo) / 2f;

            var leftMidpoint  = (center + cornerOne) / 2f;
            var rightMidpoint = (center + cornerTwo) / 2f;

            AddFeaturePointsFromTriangle(cornerOne,    edgeMidpoint, leftMidpoint,  pointList);
            AddFeaturePointsFromTriangle(edgeMidpoint, cornerTwo,    rightMidpoint, pointList);
            AddFeaturePointsFromTriangle(leftMidpoint, edgeMidpoint, rightMidpoint, pointList);
            AddFeaturePointsFromTriangle(center,       leftMidpoint, rightMidpoint, pointList);
        }

        private void AddFeaturePointsFromTriangle(
            Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree, List<Vector3> pointList
        ) {
            var triangleMidpoint = (vertexOne + vertexTwo + vertexThree) / 3f;

            Vector3 terrainPoint;

            if(Grid.TryPerformIntersectionWithTerrainSurface(Vector3.Lerp(vertexOne, triangleMidpoint, 0.5f), out terrainPoint)) {
                pointList.Add(terrainPoint);
            }

            if(Grid.TryPerformIntersectionWithTerrainSurface(Vector3.Lerp(vertexTwo, triangleMidpoint, 0.5f), out terrainPoint)) {
                pointList.Add(terrainPoint);
            }

            if(Grid.TryPerformIntersectionWithTerrainSurface(Vector3.Lerp(vertexThree, triangleMidpoint, 0.5f), out terrainPoint)) {
                pointList.Add(terrainPoint);
            }
        }

        private void ApplyFeaturesToCell(IHexCell cell, List<Vector3> locations) {
            for(int i = 0; i < locations.Count; i++) {
                var location = locations[i];

                var locationHash = NoiseGenerator.SampleHashGrid(location);

                if(CityFeaturePlacer.TryPlaceFeatureAtLocation(cell, location, i, locationHash)) {
                    continue;

                }else if(ImprovementFeaturePlacer.TryPlaceFeatureAtLocation(cell, location, i, locationHash)) {
                    continue;

                }else if(ResourceFeaturePlacer.TryPlaceFeatureAtLocation(cell, location, i, locationHash)) {
                    continue;

                }else {
                    TreeFeaturePlacer.TryPlaceFeatureAtLocation(cell, location, i, locationHash);
                }
            }
        }

        #endregion

    }

}
