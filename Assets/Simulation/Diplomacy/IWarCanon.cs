using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IWarCanon {

        #region methods

        bool AreAtWar(ICivilization civOne, ICivilization civTwo);

        bool AreAtPeace(ICivilization civOne, ICivilization civTwo);

        IEnumerable<ICivilization> GetCivsAtWarWithCiv(ICivilization civ);

        IEnumerable<ICivilization> GetCivsAtPeaceWithCiv(ICivilization civ);

        bool CanDeclareWar(ICivilization attacker, ICivilization defender);

        void DeclareWar(ICivilization attacker, ICivilization defender);

        bool CanEstablishPeace(ICivilization civOne, ICivilization civTwo);

        void EstablishPeace(ICivilization civOne, ICivilization civTwo);

        #endregion

    }

}
