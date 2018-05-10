using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Units {

    public interface IUnitFactory {

        #region properties

        IEnumerable<IUnit> AllUnits { get; }

        #endregion

        #region methods

        bool CanBuildUnit(IHexCell location, IUnitTemplate template, ICivilization owner);

        IUnit BuildUnit(IHexCell location, IUnitTemplate template, ICivilization owner);
        IUnit BuildUnit(IHexCell location, IUnitTemplate template, ICivilization owner, IPromotionTree promotionTree);

        #endregion

    }

}
