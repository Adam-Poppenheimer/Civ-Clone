using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.GameMap {

    public class PathIndicator : MonoBehaviour {

        #region internal types

        public class MemoryPool : MonoMemoryPool<PathIndicator> {

        }

        #endregion

    }

}
