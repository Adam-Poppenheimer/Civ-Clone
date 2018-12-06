using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class FoundCityAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private ICityValidityLogic                            CityValidityLogic;
        private IUnitPositionCanon                            UnitPositionCanon;
        private ICityFactory                                  CityFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitOwnershipCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public FoundCityAbilityHandler(ICityValidityLogic cityValidityLogic,
            IUnitPositionCanon unitPositionCanon, ICityFactory cityFactory,
            IPossessionRelationship<ICivilization, IUnit> unitOwnershipCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            CityValidityLogic   = cityValidityLogic;
            UnitPositionCanon   = unitPositionCanon;
            CityFactory         = cityFactory;
            UnitOwnershipCanon  = unitOwnershipCanon;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(command.Type == AbilityCommandType.FoundCity) {

                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                return CityValidityLogic.IsCellValidForCity(unitLocation);
            }else {
                return false;
            }
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitOwner = UnitOwnershipCanon.GetOwnerOfPossession(unit);
                var citiesOfOwner = CityPossessionCanon.GetPossessionsOfOwner(unitOwner);

                var cityName = unitOwner.Template.GetNextName(citiesOfOwner);

                CityFactory.Create(UnitPositionCanon.GetOwnerOfPossession(unit), unitOwner, cityName);
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
