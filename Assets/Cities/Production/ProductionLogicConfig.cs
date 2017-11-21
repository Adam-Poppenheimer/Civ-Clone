using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Cities.Production {

    [CreateAssetMenu(menuName = "Civ Clone/Production Logic Config")]
    public class ProductionLogicConfig : ScriptableObject, IProductionLogicConfig {

        #region instance fields and properties

        #region from IProductionLogicConfig

        public float HurryCostPerProduction {
            get { return _hurryCostProduction; }
        }
        [SerializeField] private float _hurryCostProduction;

        #endregion

        #endregion
        
    }

}
