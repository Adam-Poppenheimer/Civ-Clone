using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public interface IResourceRestrictionCanon {

        #region methods

        bool IsResourceValidOnCell(IResourceDefinition resource, IHexCell cell);

        #endregion

    }

}
