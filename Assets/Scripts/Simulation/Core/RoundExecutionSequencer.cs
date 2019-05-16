using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.Core {

    public class RoundExecutionSequencer {

        #region instance fields and properties

        private List<IRoundExecuter> RoundExecuters;
        private IDiplomacyCore       DiplomacyCore;

        #endregion

        #region constructors

        [Inject]
        public RoundExecutionSequencer(
            List<IRoundExecuter> roundExecuters, IDiplomacyCore diplomacyCore, CoreSignals coreSignals
        ) {
            RoundExecuters = roundExecuters;
            DiplomacyCore  = diplomacyCore;

            coreSignals.StartingRound.Subscribe(OnStartingRound);
            coreSignals.EndingRound  .Subscribe(OnEndingRound);
        }

        #endregion

        #region instance methods

        private void OnStartingRound(int round) {
            foreach(var executer in RoundExecuters) {
                executer.PerformStartOfRoundActions();
            }
        }

        private void OnEndingRound(int round) {
            foreach(var executer in RoundExecuters) {
                executer.PerformEndOfRoundActions();
            }

            DiplomacyCore.UpdateOngoingDeals();
        }

        #endregion

    }

}
