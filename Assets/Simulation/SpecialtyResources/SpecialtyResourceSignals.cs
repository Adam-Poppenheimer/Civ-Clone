using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.SpecialtyResources {

    public class SpecialtyResourceSignals {

        #region instance fields and properties

        public ISubject<IResourceNode> ResourceNodeBeingDestroyedSignal { get; private set; }

        #endregion

        #region constructors

        public SpecialtyResourceSignals() {
            ResourceNodeBeingDestroyedSignal = new Subject<IResourceNode>();
        }

        #endregion

    }

}
