using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    public class CityModifier<T> : ICityModifier<T> {

        #region internal types

        public class ExtractionData {

            public Func<ISocialPolicyBonusesData, T> PolicyCapitalBonusesExtractor;
            public Func<ISocialPolicyBonusesData, T> PolicyCityBonusesExtractor;

            public Func<IBuildingTemplate, T> BuildingLocalBonusesExtractor;
            public Func<IBuildingTemplate, T> BuildingGlobalBonusesExtractor;

            public Func<T, T, T> Aggregator;

            public T UnitaryValue;

        }

        #endregion

        #region instance fields and properties

        private ISocialPolicyBonusLogic                       SocialPolicyBonusLogic;
        private ICapitalCityCanon                             CapitalCityCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;

        private ExtractionData DataForExtraction;

        #endregion

        #region constructors

        public CityModifier(ExtractionData dataForExtraction) {
            DataForExtraction = dataForExtraction;
        }


        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ISocialPolicyBonusLogic socialPolicyBonusLogic, ICapitalCityCanon capitalCityCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ) {
            SocialPolicyBonusLogic  = socialPolicyBonusLogic;
            CapitalCityCanon        = capitalCityCanon;
            CityPossessionCanon     = cityPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #region from ICityModifierLogic<T>

        public T GetValueForCity(ICity city) {
            T retval = default(T);

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var ownerCapital = CapitalCityCanon.GetCapitalOfCiv(cityOwner);

            if(city == ownerCapital) {
                T policyCapitalMod = SocialPolicyBonusLogic.ExtractBonusFromCiv(
                    cityOwner, DataForExtraction.PolicyCapitalBonusesExtractor, DataForExtraction.Aggregator
                );

                retval = DataForExtraction.Aggregator(retval, policyCapitalMod);
            }

            T policyCityMod = SocialPolicyBonusLogic.ExtractBonusFromCiv(
                cityOwner, DataForExtraction.PolicyCityBonusesExtractor, DataForExtraction.Aggregator
            );

            retval = DataForExtraction.Aggregator(retval, policyCityMod);

            if(DataForExtraction.BuildingLocalBonusesExtractor != null) {
                foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                    retval = DataForExtraction.Aggregator(
                        retval, DataForExtraction.BuildingLocalBonusesExtractor(building.Template)
                    );
                }
            }

            if(DataForExtraction.BuildingGlobalBonusesExtractor != null) {   
                foreach(var building in GetGlobalBuildings(cityOwner)) {
                    retval = DataForExtraction.Aggregator(
                        retval, DataForExtraction.BuildingGlobalBonusesExtractor(building.Template)
                    );
                }
            }

            return DataForExtraction.Aggregator(DataForExtraction.UnitaryValue, retval);
        }

        #endregion

        private IEnumerable<IBuilding> GetGlobalBuildings(ICivilization civ) {
            var retval = new List<IBuilding>();

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                retval.AddRange(BuildingPossessionCanon.GetPossessionsOfOwner(city));
            }

            return retval;
        }

        #endregion
        
    }

}
