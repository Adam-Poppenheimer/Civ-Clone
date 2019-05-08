using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public class ResourceSignals {

        #region instance fields and properties

        public ISubject<IResourceNode>                        NodeBeingDestroyed      { get; private set; }
        public ISubject<UniRx.Tuple<IResourceNode, IHexCell>> NodeAddedToLocation     { get; private set; }
        public ISubject<UniRx.Tuple<IResourceNode, IHexCell>> NodeRemovedFromLocation { get; private set; }

        #endregion

        #region constructors

        public ResourceSignals() {
            NodeBeingDestroyed      = new Subject<IResourceNode>();
            NodeAddedToLocation     = new Subject<UniRx.Tuple<IResourceNode, IHexCell>>();
            NodeRemovedFromLocation = new Subject<UniRx.Tuple<IResourceNode, IHexCell>>();
        }

        #endregion

    }

}
