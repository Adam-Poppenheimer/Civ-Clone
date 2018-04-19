using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Diplomacy {

    [Serializable]
    public class OngoingDealEventArgs : EventArgs {

        #region instance fields and properties

        public readonly IOngoingDeal Deal;

        #endregion

        #region constructors

        public OngoingDealEventArgs(IOngoingDeal deal) {
            Deal = deal;
        }

        #endregion

    }

}
