using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Assets.Simulation.Diplomacy;

namespace Assets.UI.Diplomacy {

    public class DiplomaticExchangeRecord : MonoBehaviour {

        #region instance fields and properties

        public IDiplomaticExchange ExchangeToRecord { get; set; }

        public Action<IDiplomaticExchange> ClickCallback { get; set; }

        public bool IsSelectable {
            get { return SelectionButton.interactable; }
            set { SelectionButton.interactable = value; }
        }

        [SerializeField] private TextMeshProUGUI SummaryField    = null;
        [SerializeField] private Button          SelectionButton = null;
        [SerializeField] private InputField      DataInputField  = null;

        #endregion

        #region instance methods

        #region Unity messages

        private void Awake() {
            DataInputField.onEndEdit.AddListener(delegate(string input) {
                if(ExchangeToRecord != null && ExchangeToRecord.RequiresIntegerInput) {
                    ExchangeToRecord.IntegerInput = int.Parse(input);
                }
            });
        }

        #endregion

        public void Refresh() {
            if(ExchangeToRecord == null) {
                return;
            }

            SummaryField.text = ExchangeToRecord.GetSummary();

            if(ExchangeToRecord.RequiresIntegerInput) {
                DataInputField.gameObject.SetActive(true);
                DataInputField.text = ExchangeToRecord.IntegerInput.ToString();
                DataInputField.interactable = IsSelectable;
            }else {
                DataInputField.gameObject.SetActive(false);
            }

            if(SelectionButton != null) {
                SelectionButton.onClick.RemoveAllListeners();
                if(IsSelectable) {
                    SelectionButton.onClick.AddListener(() => ClickCallback(ExchangeToRecord));
                }
            }
        }

        public void ResetInput() {
            DataInputField.text = "";
        }

        #endregion

    }

}
