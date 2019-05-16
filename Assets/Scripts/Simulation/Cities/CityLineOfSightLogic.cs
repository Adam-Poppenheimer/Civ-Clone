using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    public class CityLineOfSightLogic : ICityLineOfSightLogic {

        #region instance fields and properties

        private IHexGrid                                 Grid;
        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CityLineOfSightLogic(
            IHexGrid grid, IPossessionRelationship<ICity, IHexCell> cellPossessionCanon
        ){
            Grid                = grid;
            CellPossessionCanon = cellPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICityVisibilityLogic

        public IEnumerable<IHexCell> GetCellsVisibleToCity(ICity city) {
            var retval = new HashSet<IHexCell>();

            foreach(var cellInCityBorders in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                retval.Add(cellInCityBorders);
                foreach(var neighbor in Grid.GetNeighbors(cellInCityBorders)) {
                    retval.Add(neighbor);
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
