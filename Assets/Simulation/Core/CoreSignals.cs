using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Players;

namespace Assets.Simulation.Core {

    public class CoreSignals {

        #region instance fields and properties

        public ISubject<int> RoundBeganSignal { get; private set; }
        public ISubject<int> RoundEndedSignal { get; private set; }

        public ISubject<IPlayer> TurnBeganSignal { get; private set; }
        public ISubject<IPlayer> TurnEndedSignal { get; private set; }

        public ISubject<IPlayer> ActivePlayerChangedSignal { get; private set; }

        #endregion

        #region constructors

        public CoreSignals() {
            RoundBeganSignal = new Subject<int>();
            RoundEndedSignal = new Subject<int>();

            TurnBeganSignal = new Subject<IPlayer>();
            TurnEndedSignal = new Subject<IPlayer>();

            ActivePlayerChangedSignal = new Subject<IPlayer>();
        }

        #endregion

    }

}
