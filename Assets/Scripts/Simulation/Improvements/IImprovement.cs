using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Improvements {

    public interface IImprovement {

        #region properties

        IImprovementTemplate Template { get; }

        bool IsConstructed { get; }

        bool IsPillaged { get; }

        bool IsReadyToConstruct { get; }

        float WorkInvested { get; set; }

        Transform transform { get; }

        #endregion

        #region methods

        void Construct();

        void Pillage();

        void Destroy(bool immediateMode = false);

        #endregion

    }

}
