using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface IUnit {

        #region properties

        IUnitTemplate Template { get; }

        int Health { get; set; }

        int CurrentMovement { get; set; }

        #endregion

        #region methods



        #endregion

    }

}
