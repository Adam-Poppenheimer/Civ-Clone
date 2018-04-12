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

        private IUnitPositionCanon                            UnitPositionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICityFactory                                  CityFactory;

        #endregion

        #region constructors

        [Inject]
        public CombatResponder(UnitSignals signals, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            ICityFactory cityFactory
        ){
            signals.MeleeCombatWithUnitSignal.Subscribe (OnMeleeCombatWithUnit);
            signals.RangedCombatWithUnitSignal.Subscribe(OnRangedCombatWithUnit);

            UnitPositionCanon   = unitPositionCanon;
            CityPossessionCanon = cityPossessionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
            CityFactory         = cityFactory;
        }

        #endregion

        #region instance methods

        private void OnMeleeCombatWithUnit(UnitCombatResults combatData) {
            var attacker = combatData.Attacker;
            var defender = combatData.Defender;

            if(attacker.Hitpoints <= 0) {
                GameObject.Destroy(attacker.gameObject);
            }

            if(defender.Hitpoints <= 0) {
                if(defender.Type == UnitType.City) {
                    HandleCityCapture(attacker, defender);
                }else {
                    HandleDefenderDeath(attacker, defender);
                }                
            }
        }

        private void HandleCityCapture(IUnit attacker, IUnit cityFacade) {
            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

            var cityCaptured = CityFactory.AllCities.Where(city => city.CombatFacade == cityFacade).FirstOrDefault();

            var cityLocation = CityLocationCanon.GetOwnerOfPossession(cityCaptured);

            foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cityLocation)) {
                if(unit.Type != UnitType.City) {
                    GameObject.DestroyImmediate(unit.gameObject);
                }
            }

            UnitPossessionCanon.ChangeOwnerOfPossession(cityFacade, attackerOwner);

            CityPossessionCanon.ChangeOwnerOfPossession(cityCaptured, attackerOwner);
        }

        private void HandleDefenderDeath(IUnit attacker, IUnit defender) {
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            GameObject.DestroyImmediate(defender.gameObject);

            if(attacker.Hitpoints > 0) {
                UnitPositionCanon.ChangeOwnerOfPossession(attacker, defenderLocation);
            }
        }

        private void OnRangedCombatWithUnit(UnitCombatResults combatData) {
            var defender = combatData.Defender;

            if(defender.Hitpoints <= 0) {
                GameObject.Destroy(defender.gameObject);
            }
        }

        #endregion

    }

}
