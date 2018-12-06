using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Civilizations {

    public class CapitalCityCanon : ICapitalCityCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, ICity> CapitalDictionary = 
            new Dictionary<ICivilization, ICity>();




        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ICityConfig                                   CityConfig;
        private IBuildingFactory                              BuildingFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CapitalCityCanon(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ICityConfig cityConfig, IBuildingFactory buildingFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            BuildingPossessionCanon = buildingPossessionCanon;
            CityConfig              = cityConfig;
            BuildingFactory         = buildingFactory;
            CityPossessionCanon     = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICapitalCityCanon

        public ICity GetCapitalOfCiv(ICivilization civ) {
            ICity retval;
            CapitalDictionary.TryGetValue(civ, out retval);
            return retval;
        }

        public bool CanSetCapitalOfCiv(ICivilization civ, ICity city) {
            if(civ == null) {
                throw new ArgumentNullException("civ");
            }

            return city == null || (GetCapitalOfCiv(civ) != city && CityPossessionCanon.GetOwnerOfPossession(city) == civ);
        }

        public void SetCapitalOfCiv(ICivilization civ, ICity newCapital) {
            if(civ == null) {
                throw new ArgumentNullException("civ");
            }

            if(!CanSetCapitalOfCiv(civ, newCapital)) {
                throw new InvalidOperationException("CanSetCapitalOfCiv must return true on the given arguments");
            }

            ICity oldCapital;
            CapitalDictionary.TryGetValue(civ, out oldCapital);

            if(oldCapital != null) {
                foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(oldCapital).ToArray()) {
                    if(CityConfig.CapitalTemplates.Contains(building.Template)) {
                        BuildingFactory.DestroyBuilding(building);
                    }
                }
            }

            CapitalDictionary[civ] = newCapital;

            if(newCapital != null) {
                var templatesInCapital = BuildingPossessionCanon.GetPossessionsOfOwner(newCapital).Select(building => building.Template);

                foreach(var template in CityConfig.CapitalTemplates.Except(templatesInCapital)) {
                    BuildingFactory.BuildBuilding(template, newCapital);
                }
            }
        }

        #endregion

        #endregion
        
    }

}
