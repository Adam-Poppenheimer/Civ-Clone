using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class MeshMemoryPool : MemoryPool<Mesh> {

        #region instance methods

        #region from MemoryPool<Mesh>

        protected override void Reinitialize(Mesh item) {
            item.Clear();
            item.ClearBlendShapes();
        }

        #endregion

        #endregion

    }



}
