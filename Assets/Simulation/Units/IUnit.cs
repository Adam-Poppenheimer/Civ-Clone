using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public interface IUnit {

        #region properties

        IUnitTemplate Template { get; }

        int Health { get; set; }

        int CurrentMovement { get; set; }

        GameObject gameObject { get; }

        List<IMapTile> CurrentPath { get; set; }

        #endregion

        #region methods

        void PerformMovement();

        #endregion

    }

}
