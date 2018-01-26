using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementValidityLogic : IImprovementValidityLogic {

        #region instance fields and properties

        private ICityFactory CityFactory;

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public ImprovementValidityLogic(ICityFactory cityFactory, IHexGrid grid) {
            CityFactory = cityFactory;
            Grid        = grid;
        }

        #endregion

        #region instance methods

        #region from IImprovementValidityLogic

        public bool IsTemplateValidForCell(IImprovementTemplate template, IHexCell cell) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            if(CityFactory.AllCities.Where(city => city.Location == cell).Count() != 0) {
                return false;
            }

            if(template.RequiresAdjacentUpwardCliff) {

                bool hasAdjacentUpwardCliff = false;
                foreach(var neighbor in Grid.GetNeighbors(cell)) {
                    if( cell.Elevation < neighbor.Elevation &&
                        HexMetrics.GetEdgeType(cell.Elevation, neighbor.Elevation) == HexEdgeType.Cliff
                    ){
                        hasAdjacentUpwardCliff = true;
                        break;
                    }
                }
                
                if(!hasAdjacentUpwardCliff) {
                    return false;
                }
            }

            return template.ValidTerrains.Contains(cell.Terrain)
                && template.ValidFeatures.Contains(cell.Feature);
        }

        #endregion

        #endregion

    }

}
