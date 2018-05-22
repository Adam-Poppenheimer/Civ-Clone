using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Simulation.SocialPolicies {

    public class SocialPolicyCostLogic : ISocialPolicyCostLogic {

        #region instance fields and properties

        private ISocialPolicyCanon                            PolicyCanon;
        private ICivilizationConfig                           CivConfig;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public SocialPolicyCostLogic(
            ISocialPolicyCanon policyCanon, ICivilizationConfig civConfig,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            PolicyCanon         = policyCanon;
            CivConfig           = civConfig;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ISocialPolicyCostLogic

        public int GetCostOfNextPolicyForCiv(ICivilization civ) {
            int cityCount = CityPossessionCanon.GetPossessionsOfOwner(civ).Count();
            int policyCount = PolicyCanon.GetPoliciesUnlockedFor(civ).Count();

            float fromPolicies = CivConfig.BasePolicyCost + (
                CivConfig.PolicyCostPerPolicyCoefficient * 
                (Mathf.Pow(policyCount, CivConfig.PolicyCostPerPolicyExponent))
            );

            float fromCities = CivConfig.PolicyCostPerCityCoefficient * Math.Max(cityCount - 1, 1);

            float unroundedValue = fromPolicies * fromCities;

            int roundRem5 = (int)unroundedValue % 5;

            return Mathf.FloorToInt(unroundedValue) + 5 - roundRem5;
        }

        #endregion

        #endregion

    }

}
