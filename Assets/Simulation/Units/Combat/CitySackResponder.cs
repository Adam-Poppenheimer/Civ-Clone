using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Combat {

    public class CitySackResponder : IPostCombatResponder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ICityFactory                                  CityFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CitySackResponder(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon, ICityFactory cityFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            CityFactory         = cityFactory;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IPostCombatResponder

        public void RespondToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            if(!ShouldCombatResultInCitySack(attacker, defender, combatInfo)) {
                return;
            }

            var citySacked = CityFactory.AllCities.Where(city => city.CombatFacade == defender).FirstOrDefault();
            var cityOwner  = CityPossessionCanon.GetOwnerOfPossession(citySacked);

            float totalPopOfCityOwner = CityPossessionCanon.GetPossessionsOfOwner(cityOwner).Sum(city => city.Population);

            float sackProportion = citySacked.Population / totalPopOfCityOwner;

            int goldStolen = Math.Max(0, Mathf.RoundToInt(cityOwner.GoldStockpile * sackProportion));

            cityOwner.GoldStockpile -= goldStolen;
        }

        #endregion

        private bool ShouldCombatResultInCitySack(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

            return attackerOwner.Template.IsBarbaric 
                && defender.CurrentHitpoints <= 0
                && defender.Type == UnitType.City
                && combatInfo.CombatType == CombatType.Melee;
        }

        #endregion
        
    }

}
