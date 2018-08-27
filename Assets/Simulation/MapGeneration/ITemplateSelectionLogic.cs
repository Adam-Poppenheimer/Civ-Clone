using System;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public interface ITemplateSelectionLogic {

        #region methods

        IRegionBiomeTemplate    GetBiomeForLandRegion    (MapRegion region, IMapTemplate mapTemplate);
        IRegionTopologyTemplate GetTopologyForLandRegion (MapRegion region, IMapTemplate mapTemplate);

        ICivHomelandTemplate GetHomelandTemplateForCiv(ICivilization civ, IMapTemplate mapTemplate);

        #endregion

    }

}