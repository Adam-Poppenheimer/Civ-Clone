using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The standard implementation of IProductionLogic.
    /// </summary>
    public class ProductionLogic : IProductionLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private IResourceGenerationLogic GenerationLogic;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="generationLogic"></param>
        public ProductionLogic(ICityConfig config, IResourceGenerationLogic generationLogic) {
            Config = config;
            GenerationLogic = generationLogic;
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

            return Mathf.FloorToInt(GenerationLogic.GetTotalYieldForCity(city)[ResourceType.Production]);
        }

        #endregion

        #endregion

    }

}
