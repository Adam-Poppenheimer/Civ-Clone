﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    public class BuildingProductionValidityLogic : IBuildingProductionValidityLogic {

        #region instance fields and properties

        private List<IBuildingTemplate> AvailableTemplates;

        private IBuildingPossessionCanon PossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildingProductionValidityLogic(List<IBuildingTemplate> availableTemplates,
            IBuildingPossessionCanon possessionCanon
        ){
            AvailableTemplates = availableTemplates;
            PossessionCanon = possessionCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingValidityLogic

        public IEnumerable<IBuildingTemplate> GetTemplatesValidForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return AvailableTemplates.Where(template => IsTemplateValidForCity(template, city));
        }

        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            var templatesAlreadyThere = PossessionCanon.GetBuildingsInCity(city).Select(building => building.Template);
            return AvailableTemplates.Contains(template) && !templatesAlreadyThere.Contains(template);
        }

        #endregion

        #endregion
        
    }

}