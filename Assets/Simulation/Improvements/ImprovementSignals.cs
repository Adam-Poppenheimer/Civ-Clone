using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementSignals {

        #region instance fields and properties

        public ISubject<IImprovement>                  ImprovementConstructedSignal         { get; private set; }
        public ISubject<IImprovement>                  ImprovementPillagedSignal            { get; private set; }
        public ISubject<IImprovement>                  ImprovementBeingDestroyedSignal      { get; private set; }
        public ISubject<Tuple<IImprovement, IHexCell>> ImprovementRemovedFromLocationSignal { get; private set; }

        #endregion

        #region constructors

        public ImprovementSignals(){
            ImprovementConstructedSignal              = new Subject<IImprovement>();
            ImprovementPillagedSignal            = new Subject<IImprovement>();
            ImprovementBeingDestroyedSignal           = new Subject<IImprovement>();
            ImprovementRemovedFromLocationSignal = new Subject<Tuple<IImprovement, IHexCell>>();
        }

        #endregion

    }

}
