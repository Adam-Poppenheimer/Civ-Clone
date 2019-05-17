using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

using Zenject;
using UniRx;

using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.UI.SocialPolicies {

    public class SocialPolicyTreeRecord : MonoBehaviour {

        #region instance fields and properties

        public IPolicyTreeDefinition TreeToRecord { get; set; }

        public bool IgnoreCost { get; set; }

        public ICivilization SelectedCiv { get; set; }


        [SerializeField] private SocialPolicyRecord PolicyRecordPrefab    = null;
        [SerializeField] private RectTransform      PolicyRecordContainer = null;

        [SerializeField] private Text   NameField          = null;
        [SerializeField] private Image  Background         = null;
        [SerializeField] private Button UnlockPolicyButton = null;

        [SerializeField] private Color CompletedColor   = Color.clear;
        [SerializeField] private Color UnlockedColor    = Color.clear;
        [SerializeField] private Color AvailableColor   = Color.clear;
        [SerializeField] private Color UnavailableColor = Color.clear;

        [SerializeField] private UILineRenderer PrerequisiteLines = null;


        private List<SocialPolicyRecord> PolicyRecords;




        private ISocialPolicyCanon     PolicyCanon;
        private ISocialPolicyCostLogic PolicyCostLogic;
        private DiContainer            Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ISocialPolicyCanon policyCanon, ISocialPolicyCostLogic policyCostLogic,
            DiContainer container
        ){
            PolicyCanon     = policyCanon;
            PolicyCostLogic = policyCostLogic;
            Container       = container;
        }

        public void Refresh() {
            if(PolicyRecords == null) {
                SetUpPolicies();
            }

            if(TreeToRecord == null) {
                return;
            }

            NameField.text = TreeToRecord.name;

            var unlockedTrees  = PolicyCanon.GetTreesUnlockedFor (SelectedCiv);
            var availableTrees = PolicyCanon.GetTreesAvailableFor(SelectedCiv);

            if(PolicyCanon.IsTreeCompletedByCiv(TreeToRecord, SelectedCiv)) {
                Background.color = CompletedColor;
                UnlockPolicyButton.gameObject.SetActive(false);

            }else if(unlockedTrees.Contains(TreeToRecord)) {
                Background.color = UnlockedColor;
                UnlockPolicyButton.gameObject.SetActive(false);

            }else if(availableTrees.Contains(TreeToRecord)) {
                Background.color = AvailableColor;
                UnlockPolicyButton.gameObject.SetActive(true);

            }else {
                Background.color = UnavailableColor;
                UnlockPolicyButton.gameObject.SetActive(false);
            }

            var unlockedPolicies  = PolicyCanon.GetPoliciesUnlockedFor (SelectedCiv);
            var availablePolicies = PolicyCanon.GetPoliciesAvailableFor(SelectedCiv);

            foreach(var policyRecord in PolicyRecords) {
                var policy = policyRecord.PolicyToRecord;

                if(unlockedPolicies.Contains(policy)) {
                    policyRecord.Status = SocialPolicyRecord.StatusType.Unlocked;

                }else if(availablePolicies.Contains(policy)) {
                    policyRecord.Status = SocialPolicyRecord.StatusType.Available;

                }else {
                    policyRecord.Status = SocialPolicyRecord.StatusType.Unavailable;
                }

                policyRecord.Refresh();
            }
        }

        public void TryUnlockPolicyTree() {
            int policyCost = PolicyCostLogic.GetCostOfNextPolicyForCiv(SelectedCiv);

            if( (IgnoreCost || SelectedCiv.CultureStockpile >= policyCost) &&
                PolicyCanon.GetTreesAvailableFor(SelectedCiv).Contains(TreeToRecord)
            ){
                SelectedCiv.CultureStockpile -= IgnoreCost ? 0 : policyCost;
                PolicyCanon.SetTreeAsUnlockedForCiv(TreeToRecord, SelectedCiv);
            }
        }

        private void SetUpPolicies() {
            PolicyRecords = new List<SocialPolicyRecord>();

            var containerRect = PolicyRecordContainer.rect;

            foreach(var policy in TreeToRecord.Policies) {
                var newPolicyRecord = Container.InstantiatePrefabForComponent<SocialPolicyRecord>(PolicyRecordPrefab);

                newPolicyRecord.name = string.Format("{0} Record", policy.name);
                newPolicyRecord.gameObject.SetActive(true);

                newPolicyRecord.transform.SetParent(PolicyRecordContainer, false);

                newPolicyRecord.transform.localPosition = new Vector3(
                    policy.TreeNormalizedX * containerRect.width,
                    policy.TreeNormalizedY * containerRect.height
                );

                newPolicyRecord.PolicyToRecord = policy;
                newPolicyRecord.SelectionButton.onClick.AddListener(() => OnPolicyRecordClicked(newPolicyRecord));
                newPolicyRecord.Refresh();

                PolicyRecords.Add(newPolicyRecord);
            }

            StartCoroutine(DrawPrerequisiteLinesCoroutine());
        }

        private void OnPolicyRecordClicked(SocialPolicyRecord record) {
            int policyCost = PolicyCostLogic.GetCostOfNextPolicyForCiv(SelectedCiv);

            if( (IgnoreCost || SelectedCiv.CultureStockpile >= policyCost) &&
                PolicyCanon.GetPoliciesAvailableFor(SelectedCiv).Contains(record.PolicyToRecord)
            ) {
                SelectedCiv.CultureStockpile -= IgnoreCost ? 0 : policyCost;
                PolicyCanon.SetPolicyAsUnlockedForCiv(record.PolicyToRecord, SelectedCiv);
            }
        }

        private IEnumerator DrawPrerequisiteLinesCoroutine() {
            yield return new WaitForEndOfFrame();

            var lineList = new List<Vector2>();

            foreach(var recordOfCurrent in PolicyRecords) {
                var currentCellTransform = recordOfCurrent.GetComponent<RectTransform>();

                var currentConnectionPoint = currentCellTransform.localPosition
                    + new Vector3(0f, currentCellTransform.rect.height / 2f, 0f);

                foreach(var prerequisite in recordOfCurrent.PolicyToRecord.Prerequisites) {
                    var recordOfPrereq = PolicyRecords.Where(record => record.PolicyToRecord == prerequisite)
                                                      .FirstOrDefault();

                    if(recordOfPrereq == null) {
                        continue;
                    }

                    var prerequisiteCellTransform = recordOfPrereq.GetComponent<RectTransform>();
                    var prerequisiteConnectionPoint = prerequisiteCellTransform.localPosition
                        - new Vector3(0f, prerequisiteCellTransform.rect.height / 2f, 0f);

                    lineList.Add(prerequisiteConnectionPoint);
                    lineList.Add(currentConnectionPoint);
                }
            }

            PrerequisiteLines.Points = lineList.ToArray();
        }

        #endregion

    }

}
