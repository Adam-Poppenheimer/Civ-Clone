﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units.Promotions {

    public class PromotionTree : IPromotionTree {

        #region instance fields and properties

        #region from IPromotionTree

        public IPromotionTreeTemplate Template { get; private set; }

        #endregion

        private List<IPromotion> ChosenPromotions;

        #endregion

        #region events

        public event EventHandler<EventArgs> NewPromotionChosen;

        #endregion

        #region constructors

        public PromotionTree(IPromotionTreeTemplate template) {
            Template = template;

            ChosenPromotions = new List<IPromotion>();
        }

        public PromotionTree(IPromotionTreeTemplate template, IEnumerable<IPromotion> chosenPromotions) {
            Template = template;

            ChosenPromotions = new List<IPromotion>(chosenPromotions);
        }

        #endregion

        #region instance methods

        #region from IPromotionTree

        public bool ContainsPromotion(IPromotion promotion) {
            return Template.PrerequisiteData.Exists(data => data.Promotion == promotion);
        }

        public IEnumerable<IPromotion> GetChosenPromotions() {
            return ChosenPromotions;
        }

        public IEnumerable<IPromotion> GetAvailablePromotions() {
            var retval = new List<IPromotion>();

            foreach(var prereqData in Template.PrerequisiteData) {
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

            if(NewPromotionChosen != null) {
                NewPromotionChosen(this, EventArgs.Empty);
            }
        }

        #endregion

        #endregion

    }

}