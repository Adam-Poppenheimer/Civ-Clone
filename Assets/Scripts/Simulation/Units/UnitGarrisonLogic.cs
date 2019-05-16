using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public class UnitGarrisonLogic : IUnitGarrisonLogic {

        #region instance fields and properties
        
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IUnitPositionCanon                       UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitGarrisonLogic(
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IUnitPositionCanon unitPositionCanon
        ) {
            CityLocationCanon = cityLocationCanon;
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitGarrisonLogic

        public bool IsUnitGarrisoned(IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var citiesAt = CityLocationCanon.GetPossessionsOfOwner(unitLocation);

            return citiesAt.Any();
        }

        public bool IsCityGarrisoned(ICity city) {
            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            var militaryUnitsAt = UnitPositionCanon
                .GetPossessionsOfOwner(cityLocation)
                .Where(unit => unit.Type != UnitType.Civilian && unit.Type != UnitType.City);

            return militaryUnitsAt.Any();
        }

        #endregion

        #endregion
        
    }

}
