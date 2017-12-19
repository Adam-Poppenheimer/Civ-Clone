using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementLocationCanon : IPossessionRelationship<IMapTile, IImprovement> {

        #region methods

        bool CanPlaceImprovementOfTemplateAtLocation(IImprovementTemplate template, IMapTile location);

        #endregion

    }

}
