using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The base interface for all building factories.
    /// </summary>
    public interface IBuildingFactory {

        #region properties

        IEnumerable<IBuilding> AllBuildings { get; }

        #endregion

        #region methods

        IBuilding BuildBuilding(IBuildingTemplate template, ICity city);

        void DestroyBuilding(IBuilding building);

        #endregion

    }

}
