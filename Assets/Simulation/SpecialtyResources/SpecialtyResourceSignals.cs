using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources {

    public class SpecialtyResourceSignals {

        #region instance fields and properties

        public ISubject<IResourceNode>                  ResourceNodeBeingDestroyedSignal      { get; private set; }
        public ISubject<Tuple<IResourceNode, IHexCell>> ResourceNodeRemovedFromLocationSignal { get; private set; }

        #endregion

        #region constructors

        public SpecialtyResourceSignals() {
            ResourceNodeBeingDestroyedSignal           = new Subject<IResourceNode>();
            ResourceNodeRemovedFromLocationSignal = new Subject<Tuple<IResourceNode, IHexCell>>();
        }

        #endregion

    }

}
