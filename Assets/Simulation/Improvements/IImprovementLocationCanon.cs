using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementLocationCanon : IPossessionRelationship<IHexCell, IImprovement> {

        #region methods

        bool CanPlaceImprovementOfTemplateAtLocation(IImprovementTemplate template, IHexCell location);

        #endregion

    }

}
