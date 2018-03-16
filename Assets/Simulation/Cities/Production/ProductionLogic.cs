using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The standard implementation of IProductionLogic.
    /// </summary>
    public class ProductionLogic : IProductionLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private IResourceGenerationLogic GenerationLogic;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="generationLogic"></param>
        public ProductionLogic(
            ICityConfig config, IResourceGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            Config                  = config;
            GenerationLogic         = generationLogic;
            BuildingPossessionCanon = buildingPossessionCanon;
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

            if(project.UnitToConstruct != null) {

                if(project.UnitToConstruct.Type == UnitType.Mounted) {
                    foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                        productionModifier += building.Template.MountedUnitProductionBonus;
                    }
                }
                if(project.UnitToConstruct.Type.IsLandMilitary()) {
                    foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                        productionModifier += building.Template.LandUnitProductionBonus;
                    }
                }

            }

            float totalProduction = GenerationLogic.GetTotalYieldForCity(
                city, new ResourceSummary(production: productionModifier)
            )[ResourceType.Production];

            return Mathf.FloorToInt(totalProduction);
        }

        #endregion

        #endregion

    }

}
