using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class DiplomacyCore : IDiplomacyCore {

        #region instance fields and properties

        private List<IDiplomaticProposal> ActiveProposals = new List<IDiplomaticProposal>();

        #endregion

        #region constructors

        public DiplomacyCore() {

        }

        #endregion

        #region instance methods

        #region from IDiplomacyCore

        public IEnumerable<IDiplomaticProposal> GetProposalsMadeByCiv(ICivilization civ) {
            return ActiveProposals.Where(proposal => proposal.Sender == civ);
        }

        public IEnumerable<IDiplomaticProposal> GetProposalsMadeToCiv(ICivilization civ) {
            return ActiveProposals.Where(proposal => proposal.Receiver == civ);
        }

         public bool TryAcceptProposal(IDiplomaticProposal proposal) {
            if(proposal == null) {
                throw new ArgumentNullException("proposal");
            }
            if(proposal.CanPerformProposal()) {
                proposal.PerformProposal();
                ActiveProposals.Remove(proposal);
                return true;
            }else {
                return false;
            }
        }

        public void RejectProposal(IDiplomaticProposal proposal) {
            if(proposal == null) {
                throw new ArgumentNullException("proposal");
            }
            ActiveProposals.Remove(proposal);
        }

        public void SendProposal(IDiplomaticProposal proposal) {
            if(proposal == null) {
                throw new ArgumentNullException("proposal");
            }
            ActiveProposals.Add(proposal);
        }

        #endregion

        #endregion
        
    }

}
