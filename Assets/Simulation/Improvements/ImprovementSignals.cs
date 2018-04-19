using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementSignals {

        #region instance fields and properties

        public ISubject<IImprovement>                  ImprovementConstructedSignal              { get; private set; }
        public ISubject<IImprovement>                  ImprovementBeingPillagedSignal            { get; private set; }
        public ISubject<IImprovement>                  ImprovementBeingDestroyedSignal           { get; private set; }
        public ISubject<Tuple<IImprovement, IHexCell>> ImprovementBeingRemovedFromLocationSignal { get; private set; }

        #endregion

        #region constructors

        public ImprovementSignals(){
            ImprovementConstructedSignal              = new Subject<IImprovement>();
            ImprovementBeingPillagedSignal            = new Subject<IImprovement>();
            ImprovementBeingDestroyedSignal           = new Subject<IImprovement>();
            ImprovementBeingRemovedFromLocationSignal = new Subject<Tuple<IImprovement, IHexCell>>();
        }

        #endregion

    }

}
