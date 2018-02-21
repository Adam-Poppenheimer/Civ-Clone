using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

using UnityCustomUtilities.Extensions;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Cities {

    public class CityCombatFacadeUnit : IUnit {

        #region instance fields and properties

        #region from IUnit

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return _emptyList; }
        }
        private List<IAbilityDefinition> _emptyList = new List<IAbilityDefinition>();

        public int AttackRange {
            get { return Config.CityAttackRange; }
        }

        public int CombatStrength {
            get { return CombatLogic.GetCombatStrengthOfCity(UnderlyingCity); }
        }

        public int CurrentMovement { get; set; }

        public List<IHexCell> CurrentPath { get; set; }

        public GameObject gameObject {
            get { return UnderlyingCity == null ? null : UnderlyingCity.gameObject; }
        }

        public int Health {
            get { return _health; }
            set { _health = value.Clamp(0, MaxHealth); }
        }
        private int _health;

        public int MaxHealth {
            get { return CombatLogic.GetMaxHealthOfCity(UnderlyingCity); }
        }

        public bool IsAquatic {
            get { return false; }
        }

        public int MaxMovement {
            get { return 0; }
        }

        public string Name {
            get { return string.Format("City at {0}", UnderlyingCity.Location.Coordinates); }
        }

        public int RangedAttackStrength {
            get { return CombatLogic.GetRangedAttackStrengthOfCity(UnderlyingCity); }
        }

        public UnitType Type {
            get { return UnitType.City; }
        }

        public int VisionRange {
            get { return Config.VisionRange; }
        }

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return new List<ISpecialtyResourceDefinition>(); }
        }

        public IUnitTemplate Template {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

        public ICity UnderlyingCity { get; private set; }

        private ICityConfig Config;

        private ICityCombatLogic CombatLogic;

        private UnitSignals Signals;

        #endregion

        #region constructors

        [Inject]
        public CityCombatFacadeUnit(
            ICity underlyingCity, ICityConfig config, ICityCombatLogic combatLogic,
            IUnitPositionCanon unitPositionCanon, UnitSignals signals
        ) {
            UnderlyingCity = underlyingCity;
            Config         = config;
            CombatLogic    = combatLogic;
            Signals        = signals;

            Health = MaxHealth;

            unitPositionCanon.ChangeOwnerOfPossession(this, underlyingCity.Location);

            CurrentMovement = 1;
        }

        #endregion

        #region instance methods

        #region Unity messages

        private void OnDestroy() {
            Signals.UnitBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #region from IUnit

        public void PerformMovement() {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
