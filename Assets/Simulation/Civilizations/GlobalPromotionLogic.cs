using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Civilizations {

    public class GlobalPromotionLogic : IGlobalPromotionLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ISocialPolicyCanon                            SocialPolicyCanon;

        #endregion

        #region constructors

        [Inject]
        public GlobalPromotionLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ISocialPolicyCanon socialPolicyCanon
        ) {
            CityPossessionCanon     = cityPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
            SocialPolicyCanon       = socialPolicyCanon;
        }

        #endregion

        #region instance methods

        #region from IGlobalPromotionLogic

        public IEnumerable<IPromotion> GetGlobalPromotionsOfCiv(ICivilization civ) {
            var retval = new List<IPromotion>();

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                    retval.AddRange(building.Template.GlobalPromotions);
                }
            }

            foreach(var policy in SocialPolicyCanon.GetPoliciesUnlockedFor(civ)) {
                retval.AddRange(policy.Bonuses.GlobalPromotions);
            }

            foreach(var policyTree in SocialPolicyCanon.GetTreesUnlockedFor(civ)) {
                retval.AddRange(policyTree.UnlockingBonuses.GlobalPromotions);

                if(SocialPolicyCanon.IsTreeCompletedByCiv(policyTree, civ)) {
                    retval.AddRange(policyTree.CompletionBonuses.GlobalPromotions);
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
