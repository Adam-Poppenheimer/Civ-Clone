using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public class CityCenterYieldLogic : ICityCenterYieldLogic {

        #region instance fields and properties

        private IIncomeModifierLogic                          IncomeModifierLogic;
        private ICityConfig                                   CityConfig;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICapitalCityCanon                             CapitalCityCanon;
        private ISocialPolicyBonusLogic                       SocialPolicyBonusLogic;
        

        #endregion

        #region constructors

        [Inject]
        public CityCenterYieldLogic(
            IIncomeModifierLogic incomeModifierLogic, ICityConfig cityConfig,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICapitalCityCanon capitalCityCanon, ISocialPolicyBonusLogic socialPolicyYieldLogic
        ) {
            IncomeModifierLogic    = incomeModifierLogic;
            CityPossessionCanon    = cityPossessionCanon;
            CityConfig             = cityConfig;
            CapitalCityCanon       = capitalCityCanon;
            SocialPolicyBonusLogic = socialPolicyYieldLogic;
        }

        #endregion

        #region instance methods

        #region from ICityCenterYieldLogic

        public YieldSummary GetYieldOfCityCenter(ICity city) {
            YieldSummary centerYield = CityConfig.CityCenterBaseYield;

            centerYield += new YieldSummary(science: 1) * city.Population;

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var ownerCapital = CapitalCityCanon.GetCapitalOfCiv(cityOwner);

            if(ownerCapital == city) {
                centerYield += SocialPolicyBonusLogic.GetBonusCapitalYieldForCiv(cityOwner);
            }

            centerYield += SocialPolicyBonusLogic.GetBonusCityYieldForCiv(cityOwner);

            var cityMultipliers = IncomeModifierLogic.GetYieldMultipliersForCity(city);
            var civMultipliers  = IncomeModifierLogic.GetYieldMultipliersForCivilization(CityPossessionCanon.GetOwnerOfPossession(city));

            centerYield *= (YieldSummary.Ones + cityMultipliers + civMultipliers);

            return centerYield;
        }

        #endregion

        #endregion
        
    }

}
