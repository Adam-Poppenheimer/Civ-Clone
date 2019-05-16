using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapResources {

    public interface IResourceNode {

        #region properties

        IResourceDefinition Resource { get; }

        int Copies { get; }

        #endregion

        #region methods

        void Destroy();

        #endregion

    }

}
