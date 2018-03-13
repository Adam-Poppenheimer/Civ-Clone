using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Combat {

    public class CombatResponder {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CombatResponder(UnitSignals signals, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            signals.MeleeCombatWithUnitSignal.Subscribe(OnMeleeCombatWithUnit);
            signals.MeleeCombatWithCitySignal.Subscribe(OnMeleeCombatWithCity);

            signals.RangedCombatWithUnitSignal.Subscribe(OnRangedCombatWithUnit);
            signals.RangedCombatWithCitySignal.Subscribe(OnRangedCombatWithCity);

            UnitPositionCanon   = unitPositionCanon;
            CityPossessionCanon = cityPossessionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
        }

        #endregion

        #region instance methods

        private void OnMeleeCombatWithUnit(UnitUnitCombatData combatData) {
            var attacker = combatData.Attacker;
            var defender = combatData.Defender;

            if(attacker.Health <= 0) {
                GameObject.Destroy(attacker.gameObject);
            }

            if(defender.Health <= 0) {
                var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

                GameObject.DestroyImmediate(defender.gameObject);

                if(attacker.Health > 0) {
                    UnitPositionCanon.ChangeOwnerOfPossession(attacker, defenderLocation);
                }
            }
        }

        private void OnMeleeCombatWithCity(UnitCityCombatData combatData) {
            var attacker = combatData.Attacker;
            var city = combatData.City;

            if(attacker.Health <= 0) {
                GameObject.Destroy(attacker.gameObject);
            }

            if(city.CombatFacade.Health <= 0) {
                var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

                var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

                foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cityLocation)) {
                    if(unit.Type != UnitType.City) {
                        GameObject.DestroyImmediate(unit.gameObject);
                    }
                }

                UnitPossessionCanon.ChangeOwnerOfPossession(city.CombatFacade, attackerOwner);

                CityPossessionCanon.ChangeOwnerOfPossession(city, attackerOwner);
            }
        }

        private void OnRangedCombatWithUnit(UnitUnitCombatData combatData) {
            var defender = combatData.Defender;

            if(defender.Health <= 0) {
                GameObject.Destroy(defender.gameObject);
            }
        }

        private void OnRangedCombatWithCity(UnitCityCombatData combatData) {

        }

        #endregion

    }

}
