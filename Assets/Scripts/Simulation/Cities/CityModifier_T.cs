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

        private ExtractionData DataForExtraction;




        private ISocialPolicyCanon                            SocialPolicyCanon;
        private ICapitalCityCanon                             CapitalCityCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;

        #endregion

        #region constructors

        public CityModifier(ExtractionData dataForExtraction) {
            DataForExtraction = dataForExtraction;
        }


        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ISocialPolicyCanon socialPolicyCanon, ICapitalCityCanon capitalCityCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ) {
            SocialPolicyCanon       = socialPolicyCanon;
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
                IEnumerable<T> capitalValues = SocialPolicyCanon
                    .GetPolicyBonusesForCiv(cityOwner)
                    .Select(bonuses => DataForExtraction.PolicyCapitalBonusesExtractor(bonuses));

                if(capitalValues.Any()) {
                    retval = DataForExtraction.Aggregator(
                        retval, capitalValues.Aggregate(DataForExtraction.Aggregator)
                    );
                }
            }

            IEnumerable<T> cityValues = SocialPolicyCanon
                    .GetPolicyBonusesForCiv(cityOwner)
                    .Select(bonuses => DataForExtraction.PolicyCityBonusesExtractor(bonuses));

            if(cityValues.Any()) {
                retval = DataForExtraction.Aggregator(
                    retval, cityValues.Aggregate(DataForExtraction.Aggregator)
                );
            }

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
