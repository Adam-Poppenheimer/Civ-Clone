using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianAvailableUnitsLogic {

        #region properties

        Func<IHexCell, IEnumerable<IUnitTemplate>> LandTemplateSelector  { get; }
        Func<IHexCell, IEnumerable<IUnitTemplate>> NavalTemplateSelector { get; }

        #endregion

        #region methods

        IEnumerable<IUnitTemplate> GetAvailableLandTemplates (IHexCell cell);
        IEnumerable<IUnitTemplate> GetAvailableNavalTemplates(IHexCell cell);

        #endregion

    }

}
