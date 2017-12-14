using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.UI.GameMap {

    public interface ITilePathDrawer {

        #region methods

        void ClearAllPaths();

        void DrawPath(List<IMapTile> path);

        #endregion

    }

}
