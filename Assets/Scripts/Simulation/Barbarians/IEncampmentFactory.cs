using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public interface IEncampmentFactory {

        #region properties

        ReadOnlyCollection<IEncampment> AllEncampments { get; }

        #endregion

        #region methods

        bool        CanCreateEncampment(IHexCell location);
        IEncampment CreateEncampment   (IHexCell location);

        void DestroyEncampment(IEncampment encampment);

        #endregion

    }

}
