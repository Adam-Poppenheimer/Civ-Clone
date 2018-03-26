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

    public class ProposalDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown ReceiverDropdown;

        [SerializeField] private RectTransform AvailableOffersContainer;
        [SerializeField] private RectTransform OffersInProposalContainer;

        [SerializeField] private RectTransform AvailableDemandsContainer;
        [SerializeField] private RectTransform DemandsInProposalContainer;

        [SerializeField] private DiplomaticExchangeRecord ExchangeRecordPrefab;

        private ICivilization Receiver;

        private Dictionary<int, ICivilization> CivOfDropdownIndex = new Dictionary<int, ICivilization>();

        private IDiplomaticProposal ActiveProposal;

        private Dictionary<IDiplomaticExchange, DiplomaticExchangeRecord> RecordOfExchange =
            new Dictionary<IDiplomaticExchange, DiplomaticExchangeRecord>();



        private IGameCore GameCore;

        private IDiplomacyCore DiplomacyCore;

        private IExchangeBuilder ExchangeBuilder;

        private ICivilizationFactory CivilizationFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IGameCore gameCore, IDiplomacyCore diplomacyCore,
            IExchangeBuilder exchangeBuilder, ICivilizationFactory civilizationFactory
        ){
            GameCore            = gameCore;
            DiplomacyCore       = diplomacyCore;
            ExchangeBuilder     = exchangeBuilder;
            CivilizationFactory = civilizationFactory;
        }

        #region Unity messages

        private void OnEnable() {
            ConfigureDropdown();
            ChangeReceiver(0);
            Refresh();
        }

        private void OnDisable() {
            ClearAll();
        }

        #endregion

        public void Refresh() {
            ClearAll();

            if(Receiver == null) {
                return;
            }

            var sender = GameCore.ActiveCivilization;

            var exchangeSummary = ExchangeBuilder.BuildAllValidExchangesBetween(sender, Receiver);

            ActiveProposal = new DiplomaticProposal(sender, Receiver);

            foreach(var offer in exchangeSummary.AllPossibleOffersFromSender) {
                BuildExchangeRecord(offer, AvailableOffersContainer, AddToOffers);
            }

            foreach(var demand in exchangeSummary.AllPossibleDemandsOfReceiver) {
                BuildExchangeRecord(demand, AvailableDemandsContainer, AddToDemands);
            }
        }

        public void ChangeReceiver(int civIndex) {
            CivOfDropdownIndex.TryGetValue(civIndex, out Receiver);
        }

        public void SendProposal() {
            DiplomacyCore.SendProposal(ActiveProposal);
        }

        private void ConfigureDropdown() {
            ReceiverDropdown.ClearOptions();
            CivOfDropdownIndex.Clear();

            var dropdownOptions = new List<Dropdown.OptionData>();

            foreach(var civilization in CivilizationFactory.AllCivilizations.Where(civ => civ != GameCore.ActiveCivilization)) {
                CivOfDropdownIndex[dropdownOptions.Count] = civilization;

                dropdownOptions.Add(new Dropdown.OptionData(civilization.Name));
            }

            ReceiverDropdown.AddOptions(dropdownOptions);
            ReceiverDropdown.value = 0;
        }

        public void ClearOptions() {
            for(int i = AvailableOffersContainer.childCount - 1; i >= 0; i--) {
                Destroy(AvailableOffersContainer.GetChild(i).gameObject);
            }

            for(int i = AvailableDemandsContainer.childCount - 1; i >= 0; i--) {
                Destroy(AvailableDemandsContainer.GetChild(i).gameObject);
            }

            RecordOfExchange.Clear();
        }

        public void ClearDeal() {
            ActiveProposal = null;

            for(int i = OffersInProposalContainer.childCount - 1; i >= 0; i--) {
                OffersInProposalContainer.GetChild(i).SetParent(AvailableOffersContainer, false);
            }

            for(int i = DemandsInProposalContainer.childCount - 1; i >= 0; i--) {
                DemandsInProposalContainer.GetChild(i).SetParent(AvailableDemandsContainer, false);
            }
        }

        private void ClearAll() {
            ClearDeal();
            ClearOptions();
        }

        private void BuildExchangeRecord(
            IDiplomaticExchange exchange, RectTransform container,
            Action<IDiplomaticExchange> clickCallback
        ){
            var newRecord = Instantiate(ExchangeRecordPrefab);

            newRecord.ExchangeToRecord = exchange;
            newRecord.ClickCallback    = clickCallback;

            newRecord.Refresh();

            newRecord.gameObject.SetActive(true);
            newRecord.transform.SetParent(container, false);

            RecordOfExchange[exchange] = newRecord;
        }

        private void AddToOffers(IDiplomaticExchange exchange) {
            if(ActiveProposal.CanAddAsOffer(exchange)) {
                ActiveProposal.AddAsOffer(exchange);

                var recordOfExchange = RecordOfExchange[exchange];

                recordOfExchange.ClickCallback = RemoveFromOffers;
                recordOfExchange.ResetInput();

                recordOfExchange.transform.SetParent(OffersInProposalContainer, false);
            }
        }

        private void RemoveFromOffers(IDiplomaticExchange exchange) {
            ActiveProposal.RemoveFromOffers(exchange);

            var recordOfExchange = RecordOfExchange[exchange];

            recordOfExchange.ClickCallback = AddToOffers;
            recordOfExchange.ResetInput();

            recordOfExchange.transform.SetParent(AvailableOffersContainer, false);
        }

        private void AddToDemands(IDiplomaticExchange exchange) {
            if(ActiveProposal.CanAddAsDemand(exchange)) {
                ActiveProposal.AddAsDemand(exchange);

                var recordOfExchange = RecordOfExchange[exchange];

                recordOfExchange.ClickCallback = RemoveFromDemands;
                recordOfExchange.ResetInput();

                recordOfExchange.transform.SetParent(DemandsInProposalContainer, false);
            }
        }

        private void RemoveFromDemands(IDiplomaticExchange exchange) {
            ActiveProposal.RemoveFromDemands(exchange);

            var recordOfExchange = RecordOfExchange[exchange];

            recordOfExchange.ClickCallback = AddToDemands;
            recordOfExchange.ResetInput();

            recordOfExchange.transform.SetParent(AvailableDemandsContainer, false);
        }

        #endregion

    }

}
