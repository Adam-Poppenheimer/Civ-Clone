using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.Improvements {

    public class ImprovementSignals {

        #region instance fields and properties

        public ISubject<IImprovement> ImprovementBeingDestroyedSignal { get; private set; }

        #endregion

        #region constructors

        public ImprovementSignals() {
            ImprovementBeingDestroyedSignal = new Subject<IImprovement>();
        }

        #endregion

    }

}
