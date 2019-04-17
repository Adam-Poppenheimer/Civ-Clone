using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Units.Combat {

    public class CityConquestResponder : IPostCombatResponder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICityFactory                                  CityFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private CitySignals                                   CitySignals;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IBuildingFactory                              BuildingFactory;

        #endregion

        #region constructors

        [Inject]
        public CityConquestResponder(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity>      cityLocationCanon,
            ICityFactory                                  cityFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IUnitPositionCanon                            unitPositionCanon,
            CitySignals                                   citySignals,
            IPossessionRelationship<ICity, IBuilding>     buildingPossessionCanon,
            IBuildingFactory                              buildingFactory
        ) {
            UnitPossessionCanon     = unitPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            CityFactory             = cityFactory;
            CityPossessionCanon     = cityPossessionCanon;
            UnitPositionCanon       = unitPositionCanon;
            CitySignals             = citySignals;
            BuildingPossessionCanon = buildingPossessionCanon;
            BuildingFactory         = buildingFactory;
        }

        #endregion

        #region instance methods

        #region from IPostCombatResponder

        public void RespondToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            if(!ShouldCombatResultInCityCapture(attacker, defender, combatInfo)) {
                return;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

            var cityCaptured = CityFactory.AllCities.Where(city => city.CombatFacade == defender).FirstOrDefault();

            var cityLocation = CityLocationCanon  .GetOwnerOfPossession(cityCaptured);
            var cityOwner    = CityPossessionCanon.GetOwnerOfPossession(cityCaptured);

            foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cityLocation).ToArray()) {
                if(unit.Type != UnitType.City) {
                    UnitPositionCanon.ChangeOwnerOfPossession(unit, null);
                    unit.Destroy();
                }
            }

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(cityCaptured).ToArray()){
                if(building.Template.Type == BuildingType.NationalWonder) {
                    BuildingFactory.DestroyBuilding(building);
                }
            }

            CityPossessionCanon.ChangeOwnerOfPossession(cityCaptured, attackerOwner);

            CitySignals.CityCaptured.OnNext(new CityCaptureData() {
                City = cityCaptured, OldOwner = cityOwner, NewOwner = attackerOwner
            });

            attacker.CurrentPath = new List<IHexCell>() { cityLocation };
            attacker.PerformMovement(true);
        }

        #endregion

        private bool ShouldCombatResultInCityCapture(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

            return !attackerOwner.Template.IsBarbaric 
                && defender.CurrentHitpoints <= 0
                && defender.Type == UnitType.City
                && combatInfo.CombatType == CombatType.Melee;
        }

        #endregion

    }

}
