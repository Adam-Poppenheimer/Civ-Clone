using System;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public interface ITemplateSelectionLogic {

        #region

        IRegionTemplate GetTemplateForLandRegion(MapRegion region);

        ICivHomelandTemplate GetHomelandTemplateForCiv(ICivilization civ, IMapTemplate mapTemplate);

        #endregion

    }

}