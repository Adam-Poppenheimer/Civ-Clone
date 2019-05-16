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

        public ISubject<int> StartingRound { get; private set; }
        public ISubject<int> EndingRound   { get; private set; }

        public ISubject<int> RoundBegan { get; private set; }
        public ISubject<int> RoundEnded { get; private set; }

        public ISubject<IPlayer> TurnBegan { get; private set; }
        public ISubject<IPlayer> TurnEnded { get; private set; }

        public ISubject<IPlayer> ActivePlayerChanged { get; private set; }

        #endregion

        #region constructors

        public CoreSignals() {
            StartingRound = new Subject<int>();
            EndingRound   = new Subject<int>();

            RoundBegan = new Subject<int>();
            RoundEnded = new Subject<int>();

            TurnBegan = new Subject<IPlayer>();
            TurnEnded = new Subject<IPlayer>();

            ActivePlayerChanged = new Subject<IPlayer>();
        }

        #endregion

    }

}
