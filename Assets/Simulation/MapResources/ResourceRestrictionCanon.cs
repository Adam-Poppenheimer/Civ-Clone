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
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
