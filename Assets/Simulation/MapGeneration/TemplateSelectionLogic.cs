using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using UnityCustomUtilities.Extensions;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public class TemplateSelectionLogic : ITemplateSelectionLogic {

        #region instance fields and properties

        private IEnumerable<IRegionTemplate> LandRegionTemplates;

        #endregion

        #region constructors

        public TemplateSelectionLogic(
            [Inject(Id = "Land Region Templates")] IEnumerable<IRegionTemplate> landRegionTemplates
            ) {
            LandRegionTemplates = landRegionTemplates;
        }

        #endregion

        #region instance methods

        #region from IRegionTemplateLogic

        public IRegionTemplate GetTemplateForLandRegion(MapRegion landRegion) {
            return LandRegionTemplates.Random();
        }

        public ICivHomelandTemplate GetHomelandTemplateForCiv(ICivilization civ, IMapTemplate mapTemplate) {
            return mapTemplate.HomelandTemplates.Random();
        }

        #endregion

        #endregion

    }

}
