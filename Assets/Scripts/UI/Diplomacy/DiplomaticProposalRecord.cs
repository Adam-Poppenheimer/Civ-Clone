using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.Diplomacy;

namespace Assets.UI.Diplomacy {

    public class DiplomaticProposalRecord : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text   SummaryField    = null;
        [SerializeField] private Button SelectionButton = null;

        public IDiplomaticProposal         ProposalToSummarize { get; set; }
        public Action<IDiplomaticProposal> ClickCallback       { get; set; }

        #endregion

        #region instance methods

        private void OnEnable() {
            if(SummaryField != null && ProposalToSummarize != null) {
                SummaryField.text = String.Format("From {0}", ProposalToSummarize.Sender.Template.Name);
            }

            SelectionButton.onClick.RemoveAllListeners();

            SelectionButton.onClick.AddListener(() => ClickCallback(ProposalToSummarize));
        }

        #endregion

    }

}
