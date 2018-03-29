using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SpecialtyResources {

    public interface IResourceNode {

        #region properties

        ISpecialtyResourceDefinition Resource { get; }

        int Copies { get; }

        bool IsVisible { get; set; }

        GameObject gameObject { get; }

        #endregion

    }

}
