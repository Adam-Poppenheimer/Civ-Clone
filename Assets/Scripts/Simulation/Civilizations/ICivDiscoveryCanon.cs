using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.Civilizations {

    public interface ICivDiscoveryCanon {

        #region methods

        IEnumerable<ICivilization> GetCivsDiscoveredByCiv(ICivilization civ);

        bool HaveCivsDiscoveredEachOther(ICivilization civOne, ICivilization civTwo);

        bool CanEstablishDiscoveryBetweenCivs(ICivilization civOne, ICivilization civTwo);
        void EstablishDiscoveryBetweenCivs   (ICivilization civOne, ICivilization civTwo);

        bool CanRevokeDiscoveryBetweenCivs(ICivilization civOne, ICivilization civTwo);
        void RevokeDiscoveryBetweenCivs   (ICivilization civOne, ICivilization civTwo);

        List<Tuple<ICivilization, ICivilization>> GetDiscoveryPairs();

        void Clear();

        #endregion

    }

}
