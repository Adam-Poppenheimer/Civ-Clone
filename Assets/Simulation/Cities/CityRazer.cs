using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    public class CityRazer : ICityRazer {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private ICellModificationLogic                   CellModLogic;

        #endregion

        #region constructors

        [Inject]
        public CityRazer(
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon, ICellModificationLogic cellModLogic
        ) {
            CityLocationCanon = cityLocationCanon;
            CellModLogic      = cellModLogic;
        }

        #endregion

        #region instance methods

        #region from ICityRazer

        public void RazeCity(ICity city) {
            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            city.Destroy();

            if(cityLocation.Feature == CellFeature.None && CellModLogic.CanChangeFeatureOfCell(cityLocation, CellFeature.CityRuins)) {
                CellModLogic.ChangeFeatureOfCell(cityLocation, CellFeature.CityRuins);
            }
        }

        #endregion

        #endregion
        
    }

}
