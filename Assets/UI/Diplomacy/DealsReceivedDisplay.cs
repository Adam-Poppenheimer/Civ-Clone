using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;

namespace Assets.UI.Diplomacy {

    public class DealsReceivedDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private DiplomaticProposalRecord ProposalRecordPrefab;
        [SerializeField] private RectTransform            ProposalsReceivedContainer;

        [SerializeField] private DiplomaticExchangeRecord ExchangeRecordPrefab;
        [SerializeField] private RectTransform            OfferedExchangesContainer;
        [SerializeField] private RectTransform            DemandedExchangesContainer;

        [SerializeField] private Button AcceptDealButton;
        [SerializeField] private Button RejectDealButton;

        private IDiplomaticProposal SelectedProposal { get; set; }




        private IGameCore      GameCore;
        private IDiplomacyCore DiplomacyCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IGameCore gameCore, IDiplomacyCore diplomacyCore) {
            GameCore      = gameCore;
            DiplomacyCore = diplomacyCore;
        }

        #region Unity messages

        private void OnEnable() {
            Refresh();
        }

        private void OnDisable() {
            SelectedProposal = null;
        }

        #endregion

        public void Refresh() {
            ClearAll();

            var receivedProposals = DiplomacyCore.GetProposalsMadeToCiv(GameCore.ActiveCivilization).ToList();

            foreach(var proposal in receivedProposals) {
                if(!proposal.CanPerformProposal()) {
                    DiplomacyCore.RejectProposal(proposal);
                }else {
                    BuildProposalRecord(proposal);
                }
            }

            if(SelectedProposal != null) {
                foreach(var offer in SelectedProposal.OfferedBySender) {
                    BuildExchangeRecord(offer, OfferedExchangesContainer);
                }
                
                foreach(var demand in SelectedProposal.DemandedOfReceiver) {
                    BuildExchangeRecord(demand, DemandedExchangesContainer);
                }
            }

            AcceptDealButton.gameObject.SetActive(SelectedProposal != null);
            RejectDealButton.gameObject.SetActive(SelectedProposal != null);
        }

        private void BuildProposalRecord(IDiplomaticProposal proposal) {
            var newRecord = Instantiate(ProposalRecordPrefab);

            newRecord.ProposalToSummarize = proposal;
            newRecord.ClickCallback = SetNewSelectedProposal;

            newRecord.transform.SetParent(ProposalsReceivedContainer, false);
            newRecord.gameObject.SetActive(true);
        }

        private void BuildExchangeRecord(IDiplomaticExchange exchange, RectTransform container) {
            var newRecord = Instantiate(ExchangeRecordPrefab);

            newRecord.ExchangeToRecord = exchange;
            newRecord.IsSelectable = false;

            newRecord.transform.SetParent(container, false);
            newRecord.gameObject.SetActive(true);

            newRecord.Refresh();
        }

        public void ClearAll() {
            ClearProposalSummary();
            ClearProposals();
        }

        public void ClearProposals() {
            for(int i = ProposalsReceivedContainer.childCount - 1; i >= 0; i--) {
                Destroy(ProposalsReceivedContainer.GetChild(i).gameObject);
            }
        }

        public void ClearProposalSummary() {
            for(int i = OfferedExchangesContainer.childCount - 1; i >= 0; i--) {
                Destroy(OfferedExchangesContainer.GetChild(i).gameObject);
            }

            for(int i = DemandedExchangesContainer.childCount - 1; i >= 0; i--) {
                Destroy(DemandedExchangesContainer.GetChild(i).gameObject);
            }
        }

        public void AcceptDeal() {
            if(SelectedProposal != null) {
                DiplomacyCore.TryAcceptProposal(SelectedProposal);
                SelectedProposal = null;
                Refresh();
            }
        }

        public void RejectDeal() {
            if(SelectedProposal != null) {
                DiplomacyCore.RejectProposal(SelectedProposal);
                SelectedProposal = null;
                Refresh();
            }
        }

        private void SetNewSelectedProposal(IDiplomaticProposal proposal) {
            SelectedProposal = proposal;
            Refresh();
        }

        #endregion

    }

}
