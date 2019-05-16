using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.UI.Units {

    public class UnitPromotionsDisplay : UnitDisplayBase {

        #region instance fields and properties

        [SerializeField] private PromotionDisplay ExistingPromotionDisplayPrefab;
        [SerializeField] private PromotionDisplay AvailablePromotionDisplayPrefab;

        [SerializeField] private RectTransform ChosenPromotionsContainer;
        [SerializeField] private RectTransform AvailablePromotionsContainer;

        private List<PromotionDisplay> InstantiatedPromotionDisplays = 
            new List<PromotionDisplay>();

        private IDisposable UnitGainedPromotionSubscription;




        private IUnitExperienceLogic UnitExperienceLogic;
        private IUnitPromotionLogic  UnitPromotionLogic;
        private UnitSignals          UnitSignals;
        private DiContainer          Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IUnitExperienceLogic experienceLogic, IUnitPromotionLogic unitPromotionLogic,
            UnitSignals unitSignals, DiContainer container
        ){
            UnitExperienceLogic = experienceLogic;
            UnitPromotionLogic  = unitPromotionLogic;
            UnitSignals         = unitSignals;
            Container           = container;
        }

        #region from UnitDisplayBase

        protected override void DoOnEnable() {
            UnitGainedPromotionSubscription = UnitSignals.GainedPromotion.Subscribe(OnUnitGainedPromotion);
        }

        protected override void DoOnDisable() {
            UnitGainedPromotionSubscription.Dispose();
        }

        public override void Refresh() {
            Clear();

            if(ObjectToDisplay == null) {
                return;
            }

            foreach(var chosenPromotion in UnitPromotionLogic.GetPromotionsForUnit(ObjectToDisplay)) {
                BuildRecordForChosenPromotion(chosenPromotion);
            }

            if(ObjectToDisplay.Experience >= UnitExperienceLogic.GetExperienceForNextLevelOnUnit(ObjectToDisplay)) {
                foreach(var availablePromotion in ObjectToDisplay.PromotionTree.GetAvailablePromotions()) {
                    BuildRecordForAvailablePromotion(availablePromotion);
                }
            }
        }

        #endregion

        private void BuildRecordForChosenPromotion(IPromotion promotion) {
            var newRecord = Instantiate(ExistingPromotionDisplayPrefab);

            Container.InjectGameObject(newRecord.gameObject);

            newRecord.transform.SetParent(ChosenPromotionsContainer, false);

            newRecord.PromotionToDisplay = promotion;
            newRecord.AcceptsInput       = false;
            newRecord.InputAction        = null;

            newRecord.gameObject.SetActive(true);

            InstantiatedPromotionDisplays.Add(newRecord);
        }

        private void BuildRecordForAvailablePromotion(IPromotion promotion) {
            var newRecord = Instantiate(AvailablePromotionDisplayPrefab);

            Container.InjectGameObject(newRecord.gameObject);

            newRecord.transform.SetParent(AvailablePromotionsContainer, false);

            newRecord.PromotionToDisplay = promotion;
            newRecord.AcceptsInput       = true;
            newRecord.InputAction        = OnAvailablePromotionClicked;

            newRecord.gameObject.SetActive(true);

            InstantiatedPromotionDisplays.Add(newRecord);
        }

        private void OnAvailablePromotionClicked(IPromotion promotion) {
            if(ObjectToDisplay.PromotionTree.CanChoosePromotion(promotion)) {

                ObjectToDisplay.Experience -= UnitExperienceLogic.GetExperienceForNextLevelOnUnit(ObjectToDisplay);
                ObjectToDisplay.Level++;

                ObjectToDisplay.PromotionTree.ChoosePromotion(promotion);
            }
        }

        private void Clear() {
            for(int i = InstantiatedPromotionDisplays.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedPromotionDisplays[i].gameObject);
            }

            InstantiatedPromotionDisplays.Clear();
        }

        private void OnUnitGainedPromotion(IUnit unit) {
            if(unit == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
