using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The standard implementation of IProductionLogic.
    /// </summary>
    public class ProductionLogic : IProductionLogic {

        #region instance fields and properties

        private ICityConfig                                   Config;
        private IYieldGenerationLogic                         GenerationLogic;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ISocialPolicyCanon                            SocialPolicyCanon;

        #endregion

        #region constructors

        [Inject]
        public ProductionLogic(
            ICityConfig config, IYieldGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ISocialPolicyCanon socialPolicyCanon
        ){
            Config                  = config;
            GenerationLogic         = generationLogic;
            BuildingPossessionCanon = buildingPossessionCanon;
            CityPossessionCanon     = cityPossessionCanon;
            SocialPolicyCanon       = socialPolicyCanon;
        }

        #endregion

        #region instance methods

        #region from IProductionLogic

        /// <inheritdoc/>
        public int GetGoldCostToHurryProject(ICity city, IProductionProject project) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(project == null) {
                throw new ArgumentNullException("project");
            }

            return Mathf.FloorToInt((project.ProductionToComplete - project.Progress) * Config.HurryCostPerProduction);
        }

        /// <inheritdoc/>
        public int GetProductionProgressPerTurnOnProject(ICity city, IProductionProject project) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(project == null) {
                throw new ArgumentNullException("project");
            }

            float productionModifier = 0f;

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                if(building.Template.ProductionModifier.DoesModifierApply(project, city)) {
                    productionModifier += building.Template.ProductionModifier.Value;
                }
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            foreach(var policyBonuses in SocialPolicyCanon.GetPolicyBonusesForCiv(cityOwner)) {
                if(policyBonuses.ProductionModifier.DoesModifierApply(project, city)) {
                    productionModifier += policyBonuses.ProductionModifier.Value;
                }
            }

            float totalProduction = GenerationLogic.GetTotalYieldForCity(
                city, new YieldSummary(production: productionModifier)
            )[YieldType.Production];

            return Mathf.RoundToInt(totalProduction);
        }


        #endregion

        #endregion

    }

}
