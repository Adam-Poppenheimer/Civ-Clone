using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public class CivilizationTerritoryLogic : ICivilizationTerritoryLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IHexCell>      CityTerritoryCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CivilizationTerritoryLogic(
            IPossessionRelationship<ICity, IHexCell> cityTerritoryCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            CityTerritoryCanon = cityTerritoryCanon;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICivilizationTerritoryLogic

        public IEnumerable<IHexCell> GetCellsClaimedByCiv(ICivilization civ) {
            var retval = new List<IHexCell>();

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                retval.AddRange(CityTerritoryCanon.GetPossessionsOfOwner(city));
            }

            return retval;
        }

        public ICivilization GetCivClaimingCell(IHexCell cell) {
            var cityOwningCell = CityTerritoryCanon.GetOwnerOfPossession(cell);

            if(cityOwningCell != null) {
                return CityPossessionCanon.GetOwnerOfPossession(cityOwningCell);
            }else {
                return null;
            }
        }

        #endregion

        #endregion
        
    }

}
