using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Territory {

    public class BorderExpansionModifierLogic : IBorderExpansionModifierLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ISocialPolicyCanon                            SocialPolicyCanon;
        private ICapitalCityCanon                             CapitalCityCanon;

        #endregion

        #region constructors

        [Inject]
        public BorderExpansionModifierLogic(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ISocialPolicyCanon socialPolicyCanon, ICapitalCityCanon capitalCityCanon
        ) {
            BuildingPossessionCanon = buildingPossessionCanon;
            CityPossessionCanon     = cityPossessionCanon;
            SocialPolicyCanon       = socialPolicyCanon;
            CapitalCityCanon        = capitalCityCanon;
        }

        #endregion

        #region instance methods

        #region from IBorderExpansionModifierLogic

        public float GetBorderExpansionModifierForCity(ICity city) {
            float localBuildingModifiers = BuildingPossessionCanon.GetPossessionsOfOwner(city).Sum(
                building => building.Template.LocalBorderExpansionModifier
            );

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var citiesOfOwner = CityPossessionCanon.GetPossessionsOfOwner(cityOwner).ToArray();

            var buildingsOfOwner = new List<IBuilding>();

            foreach(var ownedCity in citiesOfOwner) {
                buildingsOfOwner.AddRange(BuildingPossessionCanon.GetPossessionsOfOwner(ownedCity));
            }

            float globalBuildingModifiers = buildingsOfOwner.Sum(building => building.Template.GlobalBorderExpansionModifier);

            var policyBonuses = SocialPolicyCanon.GetPolicyBonusesForCiv(cityOwner);

            float policyModifiers = policyBonuses.Sum(bonuses => bonuses.CityBorderExpansionModifier);

            if(CapitalCityCanon.GetCapitalOfCiv(cityOwner) == city) {
                policyModifiers += policyBonuses.Sum(bonuses => bonuses.CapitalBorderExpansionModifier);
            }

            return 1f + localBuildingModifiers + globalBuildingModifiers + policyModifiers;
        }

        #endregion

        #endregion

    }

}
