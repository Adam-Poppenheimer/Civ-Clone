using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Players {

    [CreateAssetMenu(menuName = "Civ Clone/Players/Player Config")]
    public class PlayerConfig : ScriptableObject, IPlayerConfig {

        #region instance fields and properties

        #region from IPlayerConfig

        public int UnitMaxInfluenceRadius {
            get { return _unitMaxInfluenceRadius; }
        }
        [SerializeField] private int _unitMaxInfluenceRadius;

        public float WanderSelectionWeight_Distance {
            get { return _wanderSelectionWeight_Distance; }
        }
        [SerializeField] private float _wanderSelectionWeight_Distance;

        public float WanderSelectionWeight_Allies {
            get { return _wanderSelectionWeight_Allies; }
        }
        [SerializeField] private float _wanderSelectionWeight_Allies;

        #endregion

        #endregion

    }

}
