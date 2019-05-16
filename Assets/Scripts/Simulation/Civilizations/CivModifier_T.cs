using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Civilizations {

    public class CivModifier<T> : ICivModifier<T> {

        #region internal types

        public class ExtractionData {

            public Func<ISocialPolicyBonusesData, T> PolicyExtractor;            
            public Func<IBuildingTemplate,        T> GlobalBuildingExtractor;

            public Func<T, T, T> Aggregator;

            public T UnitaryValue;

        }

        #endregion

        #region instance fields and properties

        private ExtractionData DataForExtraction;



        
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ISocialPolicyCanon                            SocialPolicyCanon;

        #endregion

        #region constructors

        public CivModifier(ExtractionData dataForExtraction) {
            DataForExtraction = dataForExtraction;
        }

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ISocialPolicyCanon socialPolicyCanon
        ) {
            CityPossessionCanon     = cityPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
            SocialPolicyCanon       = socialPolicyCanon;
        }

        #region from ICivModifier<T>

        public T GetValueForCiv(ICivilization civ) {
            var retval = DataForExtraction.UnitaryValue;

            var policyBonuses = SocialPolicyCanon.GetPolicyBonusesForCiv(civ);

            var policyValues = policyBonuses.Select(bonuses => DataForExtraction.PolicyExtractor(bonuses));

            if(policyValues.Any()) {
                retval = DataForExtraction.Aggregator(retval, policyValues.Aggregate(DataForExtraction.Aggregator));
            }

            if(DataForExtraction.GlobalBuildingExtractor != null) {
                var citiesOf = CityPossessionCanon.GetPossessionsOfOwner(civ);

                var buildingsOf = citiesOf.SelectMany(
                    city => BuildingPossessionCanon.GetPossessionsOfOwner(city)
                ).ToList();

                foreach(var building in buildingsOf) {
                    retval = DataForExtraction.Aggregator(
                        retval, DataForExtraction.GlobalBuildingExtractor(building.Template)
                    );
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
