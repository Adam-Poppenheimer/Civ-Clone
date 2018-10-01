using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    [Serializable]
    public class CombatCondition {

        #region internal types

        public enum TargetType {
            Subject    = 0,
            Opponent   = 1,
            Location   = 2,
            CombatType = 3,
        }

        public enum RestrictionType {
            MustBe    = 0,
            MustNotBe = 1,
        }

        public enum UnitRestrictionCategory {
            OfType  = 0,
            Wounded = 1,
        }

        public enum LocationRestrictionCategory {
            OfTerrain    = 0,
            OfShape      = 1,
            OfVegetation = 2,
            RoughTerrain = 3,
        }

        #endregion

        #region instance fields and properties

        public TargetType      Target;
        public RestrictionType Restriction;

        public UnitRestrictionCategory UnitRestriction;
        public List<UnitType> UnitTypeArguments;

        public LocationRestrictionCategory LocationRestriction;
        public List<CellTerrain>    TerrainArguments;
        public List<CellShape>      ShapeArguments;
        public List<CellVegetation> VegetationArguments;

        public CombatType CombatTypeArgument;

        #endregion

        #region instance methods

        public bool IsConditionMet(
            IUnit subject, IUnit opponent, IHexCell location, CombatType combatType
        ) {
            switch(Target) {
                case TargetType.Subject:    return IsConditionMetByUnit      (subject);
                case TargetType.Opponent:   return IsConditionMetByUnit      (opponent);
                case TargetType.Location:   return IsConditionMetByCell      (location);
                case TargetType.CombatType: return IsConditionMetByCombatType(combatType);
                default: return false;
            }
        }

        private bool IsConditionMetByUnit(IUnit unit) {
            if(UnitRestriction == UnitRestrictionCategory.OfType) {
                var argumentsContain = UnitTypeArguments.Contains(unit.Type);

                return Restriction == RestrictionType.MustBe ? argumentsContain : !argumentsContain;

            }else if(UnitRestriction == UnitRestrictionCategory.Wounded) {
                return Restriction == RestrictionType.MustBe ? unit.IsWounded : !unit.IsWounded;

            }else {
                return false;
            }
        }

        private bool IsConditionMetByCell(IHexCell cell) {
            if(LocationRestriction == LocationRestrictionCategory.RoughTerrain) {
                return Restriction == RestrictionType.MustBe ? cell.IsRoughTerrain : !cell.IsRoughTerrain;

            }else if(LocationRestriction == LocationRestrictionCategory.OfTerrain) {
                var argumentsContain = TerrainArguments.Contains(cell.Terrain);

                return Restriction == RestrictionType.MustBe ? argumentsContain : !argumentsContain;

            }else if(LocationRestriction == LocationRestrictionCategory.OfShape) {
                var argumentsContain = ShapeArguments.Contains(cell.Shape);

                return Restriction == RestrictionType.MustBe ? argumentsContain : !argumentsContain;

            }else if(LocationRestriction == LocationRestrictionCategory.OfVegetation) {
                var argumentsContain = VegetationArguments.Contains(cell.Vegetation);

                return Restriction == RestrictionType.MustBe ? argumentsContain : !argumentsContain;

            }else {
                return false;
            }
        }

        private bool IsConditionMetByCombatType(CombatType combatType) {
            switch(Restriction) {
                case RestrictionType.MustBe:    return CombatTypeArgument == combatType;
                case RestrictionType.MustNotBe: return CombatTypeArgument != combatType;
                default: return false;
            }
        }

        #endregion

    }

}
