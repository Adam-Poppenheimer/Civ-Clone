using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public interface IEncampmentLocationCanon : IPossessionRelationship<IHexCell, IEncampment> {

        #region methods

        bool CanCellAcceptAnEncampment(IHexCell location);

        #endregion

    }

}
