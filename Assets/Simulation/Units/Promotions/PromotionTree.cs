using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units.Promotions {

    public class PromotionTree : IPromotionTree {

        #region instance fields and properties

        private List<IPromotion> ChosenPromotions = new List<IPromotion>();

        private IEnumerable<IPromotionPrerequisiteData> PrerequisiteData;
        private IUnit                                   ParentUnit;
        private UnitSignals                             Signals;

        #endregion

        #region constructors

        public PromotionTree(IPromotionTreeData treeData, IUnit parentUnit, UnitSignals signals) {
            PrerequisiteData = treeData.PrerequisiteData;
            ParentUnit       = parentUnit;
            Signals          = signals;
        }

        #endregion

        #region instance methods

        #region from IPromotionTree

        public bool ContainsPromotion(IPromotion promotion) {
            return PrerequisiteData.Exists(data => data.Promotion == promotion);
        }

        public IEnumerable<IPromotion> GetChosenPromotions() {
            return ChosenPromotions;
        }

        public IEnumerable<IPromotion> GetAvailablePromotions() {
            var retval = new List<IPromotion>();

            foreach(var prereqData in PrerequisiteData) {
                if(prereqData.Prerequisites.Count() == 0) {
                    retval.Add(prereqData.Promotion);

                }else if(prereqData.Prerequisites.Where(prereq => ChosenPromotions.Contains(prereq)).Count() > 0) {
                    retval.Add(prereqData.Promotion);
                }
            }

            return retval.Except(ChosenPromotions);
        }

        public bool CanChoosePromotion(IPromotion promotion) {
            return GetAvailablePromotions().Contains(promotion);
        }

        public void ChoosePromotion(IPromotion promotion) {
            if(!CanChoosePromotion(promotion)) {
                throw new InvalidOperationException("CanChoosePromotion must return true on the given argument");
            }

            ChosenPromotions.Add(promotion);

            Signals.UnitGainedPromotionSignal.OnNext(ParentUnit);
        }

        #endregion

        #endregion

    }

}
