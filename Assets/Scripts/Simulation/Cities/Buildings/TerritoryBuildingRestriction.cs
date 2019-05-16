using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    public class TerritoryBuildingRestriction : IBuildingRestriction {

        #region instance fields and properties

        private IHexGrid                                 Grid;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IRiverCanon                              RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public TerritoryBuildingRestriction(
            IHexGrid grid, IPossessionRelationship<IHexCell, ICity> cityLocationCanon, IRiverCanon riverCanon
        ) {
            Grid              = grid;
            CityLocationCanon = cityLocationCanon;
            RiverCanon        = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingRestriction

        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city, ICivilization cityOwner) {
            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            if( template.RequiresAdjacentRiver && !RiverCanon.HasRiver(cityLocation)){
                return false;
            }

            if( template.RequiresCoastalCity && 
                !Grid.GetNeighbors(cityLocation).Any(neighbor => neighbor.Terrain.IsWater())
            ){
                return false;
            }

            return true;
        }

        #endregion

        #endregion

    }

}
