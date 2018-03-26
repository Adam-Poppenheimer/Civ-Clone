using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IDiplomacyCore {

        #region methods

        IEnumerable<IDiplomaticProposal> GetProposalsMadeToCiv(ICivilization civ);

        IEnumerable<IDiplomaticProposal> GetProposalsMadeByCiv(ICivilization civ);

        bool TryAcceptProposal(IDiplomaticProposal proposal);

        void RejectProposal(IDiplomaticProposal proposal);

        void SendProposal(IDiplomaticProposal proposal);

        #endregion

    }

}
