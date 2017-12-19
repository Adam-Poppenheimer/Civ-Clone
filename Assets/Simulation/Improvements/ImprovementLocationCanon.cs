using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementLocationCanon : PossessionRelationship<IMapTile, IImprovement>, IImprovementLocationCanon {

        #region instance methods

        #region from PossessionRelationship<IMapTile, IImprovement>

        protected override bool IsPossessionValid(IImprovement possession, IMapTile owner) {
            return GetPossessionsOfOwner(owner).Count() == 0;
        }

        #endregion

        #region from IImprovementLocationCanon

        public bool CanPlaceImprovementOfTemplateAtLocation(IImprovementTemplate template, IMapTile location) {
            return GetPossessionsOfOwner(location).Count() == 0;
        }

        #endregion

        #endregion

    }

}
