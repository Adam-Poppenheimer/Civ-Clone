using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementValidityLogic : IImprovementValidityLogic {

        #region instance fields and properties

        private ICityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public ImprovementValidityLogic(ICityFactory cityFactory) {
            CityFactory = cityFactory;
        }

        #endregion

        #region instance methods

        #region from IImprovementValidityLogic

        public bool IsTemplateValidForTile(IImprovementTemplate template, IMapTile tile) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            if(CityFactory.AllCities.Where(city => city.Location == tile).Count() != 0) {
                return false;
            }

            return template.ValidTerrains.Contains(tile.Terrain)
                && template.ValidShapes  .Contains(tile.Shape  )
                && template.ValidFeatures.Contains(tile.Feature);
        }

        #endregion

        #endregion

    }

}
