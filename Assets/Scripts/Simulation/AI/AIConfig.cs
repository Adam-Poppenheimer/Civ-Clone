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

        public int EncampmentInfluenceRadius {
            get { return _encampmentInfluenceRadius; }
        }
        [SerializeField] private int _encampmentInfluenceRadius;

        public float RoadPillageValue {
            get { return _roadPillageValue; }
        }
        [SerializeField] private float _roadPillageValue;

        public float NormalImprovementPillageValue {
            get { return _normalImprovementPillageValue; }
        }
        [SerializeField] private float _normalImprovementPillageValue;

        public float ExtractingImprovementPillageValue {
            get { return _extractingImprovementPillageValue; }
        }
        [SerializeField] private float _extractingImprovementPillageValue;

        #endregion

        #endregion
        
    }

}
