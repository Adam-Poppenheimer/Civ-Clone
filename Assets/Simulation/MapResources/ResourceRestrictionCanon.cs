using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public class ResourceRestrictionCanon : IResourceRestrictionCanon {

        #region instance fields and properties



        #endregion

        #region constructors

        [Inject]
        public ResourceRestrictionCanon() {

        }

        #endregion

        #region instance methods

        #region from IResourceRestrictionCanon

        public bool IsResourceValidOnCell(IResourceDefinition resource, IHexCell cell) {
            return GetPlacementWeightOnCell(resource, cell) > 0;
        }

        public int GetPlacementWeightOnCell(IResourceDefinition resource, IHexCell cell) {
            if(cell.Shape == CellShape.Mountains) {
                return 0;
            }

            int weight = 0;

            weight += (cell.Terrain == CellTerrain.Grassland    ? 1 : 0) * resource.GrasslandWeight;
            weight += (cell.Terrain == CellTerrain.Plains       ? 1 : 0) * resource.PlainsWeight;
            weight += (cell.Terrain == CellTerrain.Desert       ? 1 : 0) * resource.DesertWeight;
            weight += (cell.Terrain == CellTerrain.Tundra       ? 1 : 0) * resource.TundraWeight;
            weight += (cell.Terrain == CellTerrain.Snow         ? 1 : 0) * resource.SnowWeight;
            weight += (cell.Terrain == CellTerrain.ShallowWater ? 1 : 0) * resource.ShallowWaterWeight;

            weight += (cell.Shape == CellShape.Hills ? 1 : 0) * resource.HillWeight;

            weight += (cell.Vegetation == CellVegetation.Forest ? 1 : 0) * resource.ForestWeight;
            weight += (cell.Vegetation == CellVegetation.Jungle ? 1 : 0) * resource.JungleWeight;
            weight += (cell.Vegetation == CellVegetation.Marsh  ? 1 : 0) * resource.MarshWeight;

            return Math.Max(0, weight);
        }

        #endregion

        #endregion
        
    }

}
