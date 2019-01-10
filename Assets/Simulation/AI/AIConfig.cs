using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.AI {

    [CreateAssetMenu(menuName = "Civ Clone/AI/Config")]
    public class AIConfig : ScriptableObject, IAIConfig {

        #region instance fields and properties

        #region from IAIConfig

        public int UnitMaxInfluenceRadius {
            get { return _unitMaxInfluenceRadius; }
        }
        [SerializeField] private int _unitMaxInfluenceRadius;

        #endregion

        #endregion
        
    }

}
