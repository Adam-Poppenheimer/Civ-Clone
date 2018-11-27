using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Civilizations {

    public interface IFreeBuildingApplier {

        #region methods

        bool CanApplyFreeBuildingToCity(IEnumerable<IBuildingTemplate> validTemplates, ICity city);
        void ApplyFreeBuildingToCity   (IEnumerable<IBuildingTemplate> validTemplates, ICity city);

        IEnumerable<IBuildingTemplate> GetTemplatesAppliedToCity(ICity city);

        void OverrideTemplatesAppliedToCity(ICity city, IEnumerable<IBuildingTemplate> newTemplates);

        void Clear();

        #endregion

    }

}
