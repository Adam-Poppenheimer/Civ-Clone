using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Civilizations {

    public interface IFreeBuildingsCanon {

        #region properties

        bool ApplyBuildingsToCities { get; set; }

        #endregion

        #region methods

        void SubscribeFreeBuildingToCiv(IEnumerable<IBuildingTemplate> validTemplates, ICivilization civ);

        void RemoveFreeBuildingFromCiv(IEnumerable<IBuildingTemplate> validTemplates, ICivilization civ);

        IEnumerable<IEnumerable<IBuildingTemplate>> GetFreeBuildingsForCiv(ICivilization civ);

        void ClearForCiv(ICivilization civ);
        void Clear();

        #endregion

    }

}
