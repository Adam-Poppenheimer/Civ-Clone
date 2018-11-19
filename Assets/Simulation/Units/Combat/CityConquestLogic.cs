using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Combat {

    public class CityConquestLogic : IPostCombatResponder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICityFactory                                  CityFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private CitySignals                                   CitySignals;

        #endregion

        #region constructors

        [Inject]
        public CityConquestLogic(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity>      cityLocationCanon,
            ICityFactory                                  cityFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IUnitPositionCanon                            unitPositionCanon,
            CitySignals                                   citySignals
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
            CityFactory         = cityFactory;
            CityPossessionCanon = cityPossessionCanon;
            UnitPositionCanon   = unitPositionCanon;
            CitySignals         = citySignals;
        }

        #endregion

        #region instance methods

        #region from ICityConquestLogic

        public void RespondToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            if(!ShouldCombatResultInCityCapture(attacker, defender, combatInfo)) {
                return;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

            var cityCaptured = CityFactory.AllCities.Where(city => city.CombatFacade == defender).FirstOrDefault();

            var cityLocation = CityLocationCanon  .GetOwnerOfPossession(cityCaptured);
            var cityOwner    = CityPossessionCanon.GetOwnerOfPossession(cityCaptured);

            foreach(var unit in new List<IUnit>(UnitPositionCanon.GetPossessionsOfOwner(cityLocation))) {
                if(unit.Type != UnitType.City) {
                    UnitPositionCanon.ChangeOwnerOfPossession(unit, null);
                    unit.Destroy();
                }
            }

            CityPossessionCanon.ChangeOwnerOfPossession(cityCaptured, attackerOwner);

            CitySignals.CityCapturedSignal.OnNext(new CityCaptureData() {
                City = cityCaptured, OldOwner = cityOwner, NewOwner = attackerOwner
            });

            attacker.CurrentPath = new List<IHexCell>() { cityLocation };
            attacker.PerformMovement(true);
        }

        #endregion

        private bool ShouldCombatResultInCityCapture(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            return defender.CurrentHitpoints <= 0 && defender.Type == UnitType.City && combatInfo.CombatType == CombatType.Melee;
        }

        #endregion

    }

}
