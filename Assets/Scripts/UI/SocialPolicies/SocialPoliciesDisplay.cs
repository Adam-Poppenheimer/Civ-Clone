using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

using Assets.UI.Civilizations;

namespace Assets.UI.SocialPolicies {

    public class SocialPoliciesDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        public bool IgnoreCost { get; set; }

        [SerializeField] private SocialPolicyTreeRecord PolicyTreeRecordPrefab = null;
        [SerializeField] private RectTransform          PolicyTreeTopRow       = null;
        [SerializeField] private RectTransform          PolicyTreeBottomRow    = null;

        [SerializeField] private RectTransform CostSection = null;

        [SerializeField] private Text CultureStockpileField     = null;
        [SerializeField] private Text CultureForNextPolicyField = null;
        [SerializeField] private Text TurnsUntilNextPolicyField = null;

        private List<SocialPolicyTreeRecord> PolicyTreeRecords;

        private IDisposable CivUnlockedPolicyTreeSubscription;
        private IDisposable CivLockedPolicyTreeSubscription;

        private IDisposable CivUnlockedPolicySubscription;
        private IDisposable CivLockedPolicySubscription;




        private ISocialPolicyCostLogic             PolicyCostLogic;
        private ICivilizationYieldLogic            YieldLogic;
        private IEnumerable<IPolicyTreeDefinition> AvailablePolicyTrees;
        private CivilizationSignals                CivSignals;
        private DiContainer                        Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ISocialPolicyCostLogic policyCostLogic, ICivilizationYieldLogic yieldLogic,
            [Inject(Id = "Available Policy Trees")] IEnumerable<IPolicyTreeDefinition> availablePolicyTrees,
            CivilizationSignals civSignals, DiContainer container
        ) {
            PolicyCostLogic      = policyCostLogic;
            YieldLogic           = yieldLogic;
            AvailablePolicyTrees = availablePolicyTrees;
            CivSignals           = civSignals;
            Container            = container;
        }

        #region Unity messages

        private void OnEnable() {
            CivUnlockedPolicyTreeSubscription = CivSignals.CivUnlockedPolicyTree.Subscribe(data => Refresh());
            CivLockedPolicyTreeSubscription   = CivSignals.CivLockedPolicyTree  .Subscribe(data => Refresh());
            CivUnlockedPolicySubscription     = CivSignals.CivUnlockedPolicy    .Subscribe(data => Refresh());
            CivLockedPolicySubscription       = CivSignals.CivLockedPolicy      .Subscribe(data => Refresh());
        }

        private void OnDisable() {
            CivUnlockedPolicyTreeSubscription.Dispose();
            CivLockedPolicyTreeSubscription  .Dispose();
            CivUnlockedPolicySubscription    .Dispose();
            CivLockedPolicySubscription      .Dispose();
        }

        #endregion

        #region from CivilizationDisplayBase

        public override void Refresh() {
            if(PolicyTreeRecords == null) {
                SetUpPolicyTrees();
            }

            if(ObjectToDisplay == null) {
                return;
            }

            foreach(var treeRecord in PolicyTreeRecords) {
                treeRecord.SelectedCiv = ObjectToDisplay;
                treeRecord.IgnoreCost  = IgnoreCost;
                treeRecord.Refresh();
            }

            CostSection.gameObject.SetActive(!IgnoreCost);

            if(!IgnoreCost) {
                int   stockpile         = ObjectToDisplay.CultureStockpile;
                int   costForNext       = PolicyCostLogic.GetCostOfNextPolicyForCiv(ObjectToDisplay);
                float cultureProduction = YieldLogic.GetYieldOfCivilization(ObjectToDisplay)[YieldType.Culture];

                CultureStockpileField    .text = stockpile  .ToString();
                CultureForNextPolicyField.text = costForNext.ToString();

                if(stockpile >= costForNext) {
                    TurnsUntilNextPolicyField.text = "(0 turns)";
                }else if(cultureProduction == 0) {
                    TurnsUntilNextPolicyField.text = "(-- turns)";
                }else {
                    TurnsUntilNextPolicyField.text = string.Format(
                        "({0} turns)", Math.Max(0, Mathf.CeilToInt((costForNext - stockpile) / cultureProduction))
                    );
                }
            }
        }

        #endregion

        private void SetUpPolicyTrees() {
            PolicyTreeRecords = new List<SocialPolicyTreeRecord>();

            foreach(var tree in AvailablePolicyTrees) {
                var treeRecord = Instantiate(PolicyTreeRecordPrefab);

                Container.InjectGameObject(treeRecord.gameObject);

                treeRecord.TreeToRecord = tree;

                treeRecord.transform.SetParent(tree.Row == 0 ? PolicyTreeTopRow : PolicyTreeBottomRow, false);

                treeRecord.gameObject.SetActive(true);

                PolicyTreeRecords.Add(treeRecord);
            }
        }

        #endregion

    }

}
