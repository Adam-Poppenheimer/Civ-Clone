using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class CapitalCityComposer : ICapitalCityComposer {

        #region instance fields and properties

        private ICivilizationFactory                     CivFactory;
        private ICapitalCityCanon                        CapitalCityCanon;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IHexGrid                                 Grid;

        #endregion

        #region constructors

        [Inject]
        public CapitalCityComposer(
            ICivilizationFactory civFactory, ICapitalCityCanon capitalCityCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon, IHexGrid grid
        ) {
            CivFactory        = civFactory;
            CapitalCityCanon  = capitalCityCanon;
            CityLocationCanon = cityLocationCanon;
            Grid              = grid;
        }

        #endregion

        #region instance methods

        #region from ICapitalCityComposer

        public void ComposeCapitalCities(SerializableMapData mapData) {
            for(int i = 0; i < CivFactory.AllCivilizations.Count; i++) {
                var civ = CivFactory.AllCivilizations[i];
                var civData = mapData.Civilizations[i];

                var civCapital = CapitalCityCanon.GetCapitalOfCiv(civ);

                if(civCapital != null) {
                    var capitalLocation = CityLocationCanon.GetOwnerOfPossession(civCapital);

                    civData.CapitalLocation = capitalLocation.Coordinates;
                }else {
                    civData.CapitalLocation = null;
                }
            }
        }

        public void DecomposeCapitalCities(SerializableMapData mapData) {
            for(int i = 0; i < mapData.Civilizations.Count; i++) {
                var civ = CivFactory.AllCivilizations[i];
                var civData = mapData.Civilizations[i];

                if(civData.CapitalLocation != null) {
                    var capitalLocation = Grid.GetCellAtCoordinates(civData.CapitalLocation.Value);

                    var capitalCity = CityLocationCanon.GetPossessionsOfOwner(capitalLocation).FirstOrDefault();

                    CapitalCityCanon.SetCapitalOfCiv(civ, capitalCity);
                }
            }
        }

        #endregion

        #endregion

    }

}
