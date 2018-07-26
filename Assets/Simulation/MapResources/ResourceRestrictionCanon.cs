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
            return (resource.ValidOnGrassland    && cell.Terrain    == CellTerrain.Grassland)
                || (resource.ValidOnPlains       && cell.Terrain    == CellTerrain.Plains)
                || (resource.ValidOnDesert       && cell.Terrain    == CellTerrain.Desert)
                || (resource.ValidOnTundra       && cell.Terrain    == CellTerrain.Tundra)
                || (resource.ValidOnSnow         && cell.Terrain    == CellTerrain.Snow)
                || (resource.ValidOnShallowWater && cell.Terrain    == CellTerrain.ShallowWater)

                || (resource.ValidOnHills        && cell.Shape      == CellShape.Hills)

                || (resource.ValidOnForest       && cell.Vegetation == CellVegetation.Forest)
                || (resource.ValidOnJungle       && cell.Vegetation == CellVegetation.Jungle)
                || (resource.ValidOnMarsh        && cell.Vegetation == CellVegetation.Marsh);
        }

        #endregion

        #endregion
        
    }

}
