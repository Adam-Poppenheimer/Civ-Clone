﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities {

    public class CityCombatFacadeUnit : IUnit {

        #region instance fields and properties

        #region from IUnit

        public IEnumerable<IUnitAbilityDefinition> Abilities {
            get { return _emptyList; }
        }
        private List<IUnitAbilityDefinition> _emptyList = new List<IUnitAbilityDefinition>();

        public int AttackRange {
            get { return Config.CityAttackRange; }
        }

        public int CombatStrength {
            get { return CombatLogic.GetCombatStrengthOfCity(UnderlyingCity); }
        }

        public int CurrentMovement { get; set; }

        public List<IHexCell> CurrentPath { get; set; }

        public GameObject gameObject {
            get { return UnderlyingCity.transform.gameObject; }
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

        #endregion


        public ICity UnderlyingCity { get; private set; }

        private ICityConfig Config;

        private ICityCombatLogic CombatLogic;

        #endregion

        #region constructors

        [Inject]
        public CityCombatFacadeUnit(
            ICity underlyingCity, ICityConfig config, ICityCombatLogic combatLogic,
            IUnitPositionCanon unitPositionCanon
        ) {
            UnderlyingCity = underlyingCity;
            Config         = config;
            CombatLogic    = combatLogic;

            Health = MaxHealth;

            unitPositionCanon.ChangeOwnerOfPossession(this, underlyingCity.Location);

            CurrentMovement = 1;
        }

        #endregion

        #region instance methods

        #region from IUnit



        #endregion

        #endregion

        public void PerformMovement() {
            throw new NotImplementedException();
        }
    }

}
