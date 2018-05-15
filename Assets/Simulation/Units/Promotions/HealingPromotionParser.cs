using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Promotions {

    public class HealingPromotionParser : IHealingPromotionParser {

        #region instance fields and properties

        private IUnitPositionCanon                            UnitPositionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ICivilizationTerritoryLogic                   CivTerritoryLogic;

        #endregion

        #region constructors

        [Inject]
        public HealingPromotionParser(
            IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ICivilizationTerritoryLogic civTerritoryLogic
        ){
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            CivTerritoryLogic   = civTerritoryLogic;
        }

        #endregion

        #region instance methods

        #region from IHealingPromotionParser

        public void ParsePromotionForHealingInfo(IPromotion promotion, IUnit unit, HealingInfo info) {
            if(!promotion.RequiresForeignTerritory || IsUnitInForeignTerritory(unit)) {

                info.BonusHealingToSelf     += promotion.BonusHealingToSelf;
                info.BonusHealingToAdjacent += promotion.BonusHealingToAdjacent;

                info.HealsEveryTurn |= promotion.HealsEveryTurn;

                info.AlternateNavalBaseHealing = Math.Max(
                    info.AlternateNavalBaseHealing, promotion.AlternativeNavalBaseHealing
                );
            }
        }

        #endregion

        private bool IsUnitInForeignTerritory(IUnit unit) {
            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            if(unitPosition == null) {
                return false;
            }

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            if(unitOwner == null) {
                return false;
            }

            return unitOwner != null
                && unitOwner != CivTerritoryLogic.GetCivClaimingCell(unitPosition);
        }

        #endregion
        
    }

}
