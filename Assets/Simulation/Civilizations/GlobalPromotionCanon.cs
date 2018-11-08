using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class GlobalPromotionCanon : IGlobalPromotionCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICivilization, IPromotion> PromotionsOfCiv =
            new DictionaryOfLists<ICivilization, IPromotion>();




        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public GlobalPromotionCanon(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            CivilizationSignals civSignals
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            
            civSignals.CivGainedUnitSignal.Subscribe(OnCivGainedUnit);
            civSignals.CivLostUnitSignal  .Subscribe(OnCivLostUnit);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICivilization, IPromotion>

        public IEnumerable<IPromotion> GetGlobalPromotionsOfCiv(ICivilization civ) {
            return PromotionsOfCiv[civ];
        }

        public void AddGlobalPromotionToCiv(IPromotion promotion, ICivilization civ) {
            PromotionsOfCiv.AddElementToList(civ, promotion);

            foreach(var unit in UnitPossessionCanon.GetPossessionsOfOwner(civ)) {
                unit.PromotionTree.AppendPromotion(promotion);
            }
        }

        public void RemoveGlobalPromotionFromCiv(IPromotion promotion, ICivilization civ) {
            if(PromotionsOfCiv[civ].Remove(promotion)) {
                foreach(var unit in UnitPossessionCanon.GetPossessionsOfOwner(civ)) {
                    unit.PromotionTree.RemoveAppendedPromotion(promotion);
                }
            }
        }

        #endregion

        private void OnCivGainedUnit(UniRx.Tuple<ICivilization, IUnit> data) {
            var newOwner = data.Item1;
            var unit     = data.Item2;

            if(newOwner != null) {
                foreach(var promotion in GetGlobalPromotionsOfCiv(newOwner)) {
                    unit.PromotionTree.AppendPromotion(promotion);
                }
            }
        }

        private void OnCivLostUnit(UniRx.Tuple<ICivilization, IUnit> data) {
            var oldOwner = data.Item1;
            var unit     = data.Item2;

            if(oldOwner != null) {
                foreach(var promotion in GetGlobalPromotionsOfCiv(oldOwner)) {
                    unit.PromotionTree.RemoveAppendedPromotion(promotion);
                }
            }
        }

        #endregion

    }

}
