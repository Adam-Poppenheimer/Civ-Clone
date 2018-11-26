using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Modifiers {

    public class CityModifier<T> : ICityModifier<T> {

        #region internal types

        public class ExtractionData {

            public Func<ISocialPolicyBonusesData, T> CapitalBonusesExtractor;
            public Func<ISocialPolicyBonusesData, T> CityBonusesExtractor;

            public Func<T, T, T> Aggregator;

            public T UnitaryValue;

        }

        #endregion

        #region instance fields and properties

        private ISocialPolicyBonusLogic                       SocialPolicyBonusLogic;
        private ICapitalCityCanon                             CapitalCityCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

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
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            SocialPolicyBonusLogic = socialPolicyBonusLogic;
            CapitalCityCanon       = capitalCityCanon;
            CityPossessionCanon    = cityPossessionCanon;
        }

        #region from ICityModifierLogic<T>

        public T GetValueForCity(ICity city) {
            T retval = default(T);

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var ownerCapital = CapitalCityCanon.GetCapitalOfCiv(cityOwner);

            if(city == ownerCapital) {
                T capitalMod = SocialPolicyBonusLogic.ExtractBonusFromCiv(
                    cityOwner, DataForExtraction.CapitalBonusesExtractor, DataForExtraction.Aggregator
                );

                retval = DataForExtraction.Aggregator(retval, capitalMod);
            }

            T cityMod = SocialPolicyBonusLogic.ExtractBonusFromCiv(
                cityOwner, DataForExtraction.CityBonusesExtractor, DataForExtraction.Aggregator
            );

            retval = DataForExtraction.Aggregator(retval, cityMod);

            return DataForExtraction.Aggregator(DataForExtraction.UnitaryValue, retval);
        }

        #endregion

        #endregion
        
    }

}
