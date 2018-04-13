using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class CivilizationConnectionLogic : ICivilizationConnectionLogic {

        #region instance fields and properties

        private IHexGrid                                 HexGrid;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private ICapitalCityCanon                        CapitalCityCanon;
        private IConnectionPathCostLogic                 ConnectionPathCostLogic;

        #endregion

        #region constructors

        [Inject]
        public CivilizationConnectionLogic(
            IHexGrid hexGrid, IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            ICapitalCityCanon capitalCityCanon, IConnectionPathCostLogic connectionPathCostLogic
        ) {
            HexGrid                 = hexGrid;
            CityLocationCanon       = cityLocationCanon;
            CapitalCityCanon        = capitalCityCanon;
            ConnectionPathCostLogic = connectionPathCostLogic;
        }

        #endregion

        #region instance methods

        #region from ICivilizationConnectionLogic

        public bool AreCivilizationsConnected(ICivilization civOne, ICivilization civTwo) {
            var capitalOne = CapitalCityCanon.GetCapitalOfCiv(civOne);
            var capitalTwo = CapitalCityCanon.GetCapitalOfCiv(civTwo);

            var capitalOneLocation = CityLocationCanon.GetOwnerOfPossession(capitalOne);
            var capitalTwoLocation = CityLocationCanon.GetOwnerOfPossession(capitalTwo);

            return HexGrid.GetShortestPathBetween(
                capitalOneLocation, capitalTwoLocation, ConnectionPathCostLogic.BuildPathCostFunction(civOne, civTwo)
            ) != null;
        }

        #endregion

        #endregion

    }

}
