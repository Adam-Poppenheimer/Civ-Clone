using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public interface ITilePathDrawer {

        #region methods

        void ClearAllPaths();

        void DrawPath(List<IHexCell> path);

        #endregion

    }

}
