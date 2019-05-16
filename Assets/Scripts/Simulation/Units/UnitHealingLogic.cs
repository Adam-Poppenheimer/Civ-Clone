using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Units {

    public class UnitHealingLogic : IUnitHealingLogic {

        #region instance fields and properties

        private IUnitConfig                                   UnitConfig;
        private IPromotionParser                              PromotionParser;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICivilizationTerritoryLogic                   CivTerritoryLogic;
        private IHexGrid                                      Grid;

        #endregion

        #region constructors

        [Inject]
        public UnitHealingLogic(
            IUnitConfig unitConfig, IPromotionParser promotionParser,
            IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            ICivilizationTerritoryLogic civTerritoryLogic, IHexGrid grid
        ){
            UnitConfig          = unitConfig;
            PromotionParser     = promotionParser;
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
            CivTerritoryLogic   = civTerritoryLogic;
            Grid                = grid;
        }

        #endregion

        #region instance methods

        public void PerformHealingOnUnit(IUnit unit) {
            switch(unit.Type) {
                case UnitType.City:        HealCityFacade(unit); break;
                case UnitType.NavalMelee:
                case UnitType.NavalRanged: HealNavalUnit (unit); break;
                default:                   HealLandUnit  (unit); break;
            }
        }

        private void HealCityFacade(IUnit facade) {
            facade.CurrentHitpoints += Mathf.RoundToInt(UnitConfig.CityRepairPercentPerTurn * facade.MaxHitpoints);
        }

        private void HealNavalUnit(IUnit navalUnit) {
            var healingInfo = PromotionParser.GetHealingInfo(navalUnit);

            if(!healingInfo.HealsEveryTurn && navalUnit.CurrentMovement < navalUnit.MaxMovement) {
                return;
            }

            int healingThisTurn;

            if(IsUnitInCity(navalUnit)) {
                healingThisTurn = UnitConfig.GarrisonedNavalHealingPerTurn;
            }else if(IsUnitInFriendlyTerritory(navalUnit)) {
                healingThisTurn = UnitConfig.FriendlyNavalHealingPerTurn;
            }else {
                healingThisTurn = Math.Max(UnitConfig.ForeignNavalHealingPerTurn, healingInfo.AlternateNavalBaseHealing);
            }

            healingThisTurn += healingInfo.BonusHealingToSelf + GetHealingFromNeighborsOf(navalUnit);

            navalUnit.CurrentHitpoints += healingThisTurn;
        }

        private void HealLandUnit(IUnit landUnit) {
            var healingInfo = PromotionParser.GetHealingInfo(landUnit);

            if(!healingInfo.HealsEveryTurn && landUnit.CurrentMovement < landUnit.MaxMovement) {
                return;
            }

            int healingThisTurn;

            if(IsUnitInCity(landUnit)) {
                healingThisTurn = UnitConfig.GarrisonedLandHealingPerTurn;
            }else if(IsUnitInFriendlyTerritory(landUnit)) {
                healingThisTurn = UnitConfig.FriendlyLandHealingPerTurn;
            }else {
                healingThisTurn = UnitConfig.ForeignLandHealingPerTurn;
            }

            healingThisTurn += healingInfo.BonusHealingToSelf + GetHealingFromNeighborsOf(landUnit);

            landUnit.CurrentHitpoints += healingThisTurn;
        }

        private bool IsUnitInCity(IUnit unit) {
            var unitLocation   = UnitPositionCanon.GetOwnerOfPossession(unit);
            var cityAtLocation = CityLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

            return cityAtLocation != null;
        }

        private bool IsUnitInFriendlyTerritory(IUnit unit) {
            var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);
            var unitLocation = UnitPositionCanon  .GetOwnerOfPossession(unit);

            return unitLocation != null && unitOwner == CivTerritoryLogic.GetCivClaimingCell(unitLocation);
        }

        private int GetHealingFromNeighborsOf(IUnit unit) {
            int retval = 0;

            foreach(var neighboringUnit in GetFriendlyUnitsNeighboringUnit(unit)) {
                var neighborHealingInfo = PromotionParser.GetHealingInfo(neighboringUnit);

                retval += neighborHealingInfo.BonusHealingToAdjacent;
            }

            return retval;
        }

        private IEnumerable<IUnit> GetFriendlyUnitsNeighboringUnit(IUnit unit) {
            var retval = new List<IUnit>();

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);
            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            if(unitLocation != null) {
                foreach(var neighboringCell in Grid.GetNeighbors(unitLocation)) {
                    foreach(var neighboringUnit in UnitPositionCanon.GetPossessionsOfOwner(neighboringCell)) {
                        if(unitOwner == UnitPossessionCanon.GetOwnerOfPossession(neighboringUnit)) {
                            retval.Add(neighboringUnit);
                        }
                    }
                }
            }

            return retval;
        }

        #endregion

    }

}
