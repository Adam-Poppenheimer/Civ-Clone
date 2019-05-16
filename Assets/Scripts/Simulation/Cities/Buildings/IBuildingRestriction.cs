using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    public interface IBuildingRestriction {

        #region methods

        bool IsTemplateValidForCity(IBuildingTemplate template, ICity city, ICivilization cityOwner);

        #endregion

    }

}
