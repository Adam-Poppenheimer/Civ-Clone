using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Cities.Buildings;

namespace Assets.Cities.Production {

    public class ProductionLogic : IProductionLogic {

        #region instance fields and properties

        private IProductionLogicConfig Config;

        private IResourceGenerationLogic GenerationLogic;

        #endregion

        #region constructors

        public ProductionLogic(IProductionLogicConfig config, IResourceGenerationLogic generationLogic) {
            Config = config;
            GenerationLogic = generationLogic;
        }

        #endregion

        #region instance methods

        #region from IProductionLogic

        public int GetGoldCostToHurryProject(ICity city, IProductionProject project) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(project == null) {
                throw new ArgumentNullException("project");
            }

            return Mathf.FloorToInt((project.ProductionToComplete - project.Progress) * Config.HurryCostPerProduction);
        }

        public int GetProductionProgressPerTurnOnProject(ICity city, IProductionProject project) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(project == null) {
                throw new ArgumentNullException("project");
            }

            return GenerationLogic.GetTotalYieldForCity(city)[ResourceType.Production];
        }

        #endregion

        #endregion

    }

}
