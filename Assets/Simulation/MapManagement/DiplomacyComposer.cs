using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapManagement {

    public class DiplomacyComposer : IDiplomacyComposer {

        #region instance fields and properties

        private IDiplomacyCore              DiplomacyCore;
        private ICivilizationFactory        CivFactory;
        private IWarCanon                   WarCanon;
        private IDiplomaticProposalComposer ProposalComposer;
        private IOngoingDealComposer        OngoingDealComposer;

        #endregion

        #region constructors

        [Inject]
        public DiplomacyComposer(
            IDiplomacyCore diplomacyCore, ICivilizationFactory civFactory,
            IWarCanon warCanon, IDiplomaticProposalComposer proposalComposer,
            IOngoingDealComposer ongoingDealComposer
        ) {
            DiplomacyCore       = diplomacyCore;
            CivFactory          = civFactory;
            WarCanon            = warCanon;
            ProposalComposer    = proposalComposer;
            OngoingDealComposer = ongoingDealComposer;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            WarCanon.Clear();
            DiplomacyCore.ClearProposals();
            DiplomacyCore.ClearOngoingDeals();
        }

        public void ComposeDiplomacy(SerializableMapData mapData) {
            var diplomacyData = new SerializableDiplomacyData();

            foreach(var war in WarCanon.GetAllActiveWars()) {
                diplomacyData.ActiveWars.Add(
                    new UniRx.Tuple<string, string>(war.Attacker.Template.Name, war.Defender.Template.Name)
                );
            }

            foreach(var fromCiv in CivFactory.AllCivilizations) {
                foreach(var proposalFrom in DiplomacyCore.GetProposalsSentFromCiv(fromCiv)) {
                    diplomacyData.ActiveProposals.Add(ProposalComposer.ComposeProposal(proposalFrom));
                }

                foreach(var ongoingDealFrom in DiplomacyCore.GetOngoingDealsSentFromCiv(fromCiv)) {
                    diplomacyData.ActiveOngoingDeals.Add(OngoingDealComposer.ComposeOngoingDeal(ongoingDealFrom));
                }
            }

            mapData.DiplomacyData = diplomacyData;
        }

        public void DecomposeDiplomacy(SerializableMapData mapData) {
            var diplomacyData = mapData.DiplomacyData;

            if(diplomacyData == null) {
                return;
            }

            var allCivs = CivFactory.AllCivilizations;

            foreach(var warData in diplomacyData.ActiveWars) {
                var attacker = allCivs.Where(civ => civ.Template.Name.Equals(warData.Item1)).FirstOrDefault();

                if(attacker == null) {
                    throw new InvalidOperationException("Could not find a civ with name " + warData.Item1);
                }

                var defender = allCivs.Where(civ => civ.Template.Name.Equals(warData.Item2)).FirstOrDefault();

                if(defender == null) {
                    throw new InvalidOperationException("Could not find a civ with name " + warData.Item2);
                }

                if(!WarCanon.CanDeclareWar(attacker, defender)) {
                    throw new InvalidOperationException(string.Format(
                        "Cannot declare the specified war between {0} and {1}",
                        attacker.Template.Name, defender.Template.Name
                    ));
                }

                WarCanon.DeclareWar(attacker, defender);
            }

            foreach(var proposalData in diplomacyData.ActiveProposals) {
                var proposal = ProposalComposer.DecomposeProposal(proposalData);

                DiplomacyCore.SendProposal(proposal);
            }

            foreach(var ongoingDealData in diplomacyData.ActiveOngoingDeals) {
                var ongoingDeal = OngoingDealComposer.DecomposeOngoingDeal(ongoingDealData);

                DiplomacyCore.SubscribeOngoingDeal(ongoingDeal);
            }
        }

        #endregion

    }

}
