using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Production {

    [Serializable]
    public class ProductionCondition {

        #region internal types

        public enum TargetType {
            Project = 0,
        }

        public enum RestrictionType {
            MustBe    = 0,
            MustNotBe = 1,
        }

        public enum ProjectRestrictionCategory {
            Unit     = 0,
            Building = 1
        }

        public enum UnitRestrictionCategory {
            OfType = 0,
            OfName = 1,
        }

        public enum BuildingRestrictionCategory {
            OfType = 0,
            OfName = 1,
        }

        #endregion

        #region instance fields and properties

        public TargetType Target;
        public RestrictionType Restriction;

        public ProjectRestrictionCategory ProjectRestriction;

        public UnitRestrictionCategory UnitRestriction;
        public List<UnitType> UnitTypeArguments = new List<UnitType>();

        public BuildingRestrictionCategory BuildingRestriction;
        public List<BuildingType> BuildingTypeArguments = new List<BuildingType>();

        public List<string> NameArguments = new List<string>();

        #endregion

        #region instance methods

        public bool IsMetBy(IProductionProject project, ICity city) {
            switch(Target) {
                case TargetType.Project: return IsConditionMetByProject(project);
                default: return false;
            }
        }

        private bool IsConditionMetByProject(IProductionProject project) {
            if(ProjectRestriction == ProjectRestrictionCategory.Unit && project.UnitToConstruct != null) {
                return IsConditionMetByUnit(project.UnitToConstruct);

            }else if(ProjectRestriction == ProjectRestrictionCategory.Building && project.BuildingToConstruct != null) {
                return IsConditionMetByBuilding(project.BuildingToConstruct);

            }else {
                return false;
            }
        }

        private bool IsConditionMetByUnit(IUnitTemplate unit) {
            if(UnitRestriction == UnitRestrictionCategory.OfType) {
                bool hasArguedType = UnitTypeArguments.Contains(unit.Type);

                return Restriction == RestrictionType.MustBe ? hasArguedType : !hasArguedType;

            }else if(UnitRestriction == UnitRestrictionCategory.OfName) {
                bool isArguedName = NameArguments.Any(name => name.Equals(unit.name));

                return Restriction == RestrictionType.MustBe ? isArguedName : !isArguedName;

            } else {
                return false;
            }
        }

        private bool IsConditionMetByBuilding(IBuildingTemplate building) {
            if(BuildingRestriction == BuildingRestrictionCategory.OfType) {
                bool hasArguedType = BuildingTypeArguments.Contains(building.Type);

                return Restriction == RestrictionType.MustBe ? hasArguedType : !hasArguedType;

            }else if(BuildingRestriction == BuildingRestrictionCategory.OfName) {
                bool isArguedName = NameArguments.Any(name => name.Equals(building.name));

                return Restriction == RestrictionType.MustBe ? isArguedName : !isArguedName;

            } else {
                return false;
            }
        }

        #endregion

    }

}
