using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public class ResourceRestrictionCanon : IResourceRestrictionCanon {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceRestrictionCanon(
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            NodeLocationCanon = nodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IResourceRestrictionCanon

        public bool IsResourceValidOnCell(IResourceDefinition resource, IHexCell cell) {
            return GetPlacementWeightOnCell(resource, cell) > 0;
        }

        public int GetPlacementWeightOnCell(IResourceDefinition resource, IHexCell cell) {
            if(NodeLocationCanon.GetPossessionsOfOwner(cell).Any()) {
                return 0;
            }

            int weight = resource.GetWeightFromTerrain   (cell.Terrain   ) +
                         resource.GetWeightFromShape     (cell.Shape     ) +
                         resource.GetWeightFromVegetation(cell.Vegetation);

            return Math.Max(0, weight);
        }

        #endregion

        #endregion
        
    }

}
