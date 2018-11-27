using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class FreeBuildingApplier : IFreeBuildingApplier {

        #region instance fields and properties

        private DictionaryOfLists<ICity, IBuildingTemplate> BestowedFreeBuildingsOfCity =
            new DictionaryOfLists<ICity, IBuildingTemplate>();



        private IBuildingProductionValidityLogic BuildingProductionValidityLogic;
        private IBuildingFactory                 BuildingFactory;

        #endregion

        #region constructors

        [Inject]
        public FreeBuildingApplier(
            IBuildingProductionValidityLogic buildingProductionValidityLogic,
            IBuildingFactory buildingFactory
        ) {
            BuildingProductionValidityLogic = buildingProductionValidityLogic;
            BuildingFactory                 = buildingFactory;
        }

        #endregion

        #region instance methods

        #region from IFreeBuildingApplier

        public bool CanApplyFreeBuildingToCity(IEnumerable<IBuildingTemplate> validTemplates, ICity city) {
            var hasValidTemplates = validTemplates.Any(
                template => BuildingProductionValidityLogic.IsTemplateValidForCity(template, city)
            );

            var alreadyReceivedBuilding = BestowedFreeBuildingsOfCity[city].Intersect(validTemplates).Any();

            return hasValidTemplates && !alreadyReceivedBuilding;
        }

        public void ApplyFreeBuildingToCity(IEnumerable<IBuildingTemplate> validTemplates, ICity city) {
            foreach(var template in validTemplates) {

                if(BuildingProductionValidityLogic.IsTemplateValidForCity(template, city)) {
                    BuildingFactory.BuildBuilding(template, city);

                    BestowedFreeBuildingsOfCity[city].Add(template);

                    return;
                }

            }
        }

        public IEnumerable<IBuildingTemplate> GetTemplatesAppliedToCity(ICity city) {
            return BestowedFreeBuildingsOfCity[city];
        }

        public void OverrideTemplatesAppliedToCity(ICity city, IEnumerable<IBuildingTemplate> newTemplates) {
            BestowedFreeBuildingsOfCity[city] = newTemplates.ToList();
        }

        public void Clear() {
            BestowedFreeBuildingsOfCity.Clear();
        }

        #endregion

        #endregion
        
    }

}
