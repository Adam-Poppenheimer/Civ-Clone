using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public class CoreSignals {

        #region instance fields and properties

        public ISubject<int> RoundBeganSignal { get; private set; }
        public ISubject<int> RoundEndedSignal { get; private set; }

        public ISubject<ICivilization> TurnBeganSignal { get; private set; }
        public ISubject<ICivilization> TurnEndedSignal { get; private set; }

        public ISubject<ICivilization> ActiveCivChangedSignal { get; private set; }

        #endregion

        #region constructors

        public CoreSignals() {
            RoundBeganSignal = new Subject<int>();
            RoundEndedSignal = new Subject<int>();

            TurnBeganSignal = new Subject<ICivilization>();
            TurnEndedSignal = new Subject<ICivilization>();

            ActiveCivChangedSignal = new Subject<ICivilization>();
        }

        #endregion

    }

}
