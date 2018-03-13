using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;


namespace Assets.Simulation.Cities {

    public class CityCombatantTemplate : IUnitTemplate {

        #region instance fields and properties

        #region from IUnitTemplate

        public string name {
            get { return UnderlyingCity.gameObject.name; }
        }

        public string Description {
            get { return ""; }
        }

        public Sprite Image {
            get { return CityConfig.CombatantImage; }
        }

        public Sprite Icon {
            get { return CityConfig.CombatantIcon; }
        }

        public GameObject Prefab {
            get { return CityConfig.CombatantPrefab; }
        }

        public int ProductionCost {
            get { return 0; }
        }

        public int MaxMovement {
            get { return 1; }
        }

        public UnitType Type {
            get { return UnitType.City; }
        }

        public bool IsAquatic {
            get { return false; }
        }

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return _emptyAbilities; }
        }
        private static List<IAbilityDefinition> _emptyAbilities = new List<IAbilityDefinition>();

        public int AttackRange {
            get { return CityConfig.CityAttackRange; }
        }

        public int CombatStrength {
            get { return CombatLogic.GetRangedAttackStrengthOfCity(UnderlyingCity); }
        }

        public int RangedAttackStrength {
            get { return CombatLogic.GetRangedAttackStrengthOfCity(UnderlyingCity); }
        }

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return _emptyResources; }
        }
        private static List<ISpecialtyResourceDefinition> _emptyResources = new List<ISpecialtyResourceDefinition>();

        public bool BenefitsFromDefensiveTerrain {
            get { return false; }
        }

        public bool IgnoresTerrainCosts {
            get { return false; }
        }

        public bool HasRoughTerrainPenalty {
            get { return false; }
        }

        public bool IsImmobile {
            get { return true; }
        }

        #endregion



        private ICity UnderlyingCity;

        private ICityConfig CityConfig;

        private ICityCombatLogic CombatLogic;

        private UnitSignals Signals;

        #endregion

        #region constructors

        public CityCombatantTemplate(
            ICity underlyingCity, ICityConfig cityConfig, ICityCombatLogic combatLogic
        ){
            UnderlyingCity = underlyingCity;
            CityConfig     = cityConfig;
            CombatLogic    = combatLogic;
        }

        #endregion

    }

}
