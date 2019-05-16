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

        public ISubject<IImprovement>                  Constructed         { get; private set; }
        public ISubject<IImprovement>                  Pillaged            { get; private set; }
        public ISubject<IImprovement>                  BeingDestroyed      { get; private set; }
        public ISubject<Tuple<IImprovement, IHexCell>> AddedToLocation     { get; private set; }
        public ISubject<Tuple<IImprovement, IHexCell>> RemovedFromLocation { get; private set; }

        #endregion

        #region constructors

        public ImprovementSignals(){
            Constructed         = new Subject<IImprovement>();
            Pillaged            = new Subject<IImprovement>();
            BeingDestroyed      = new Subject<IImprovement>();
            AddedToLocation     = new Subject<Tuple<IImprovement, IHexCell>>();
            RemovedFromLocation = new Subject<Tuple<IImprovement, IHexCell>>();
        }

        #endregion

    }

}
