using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Promotions {

    [CreateAssetMenu(menuName = "Civ Clone/Units/Promotion Tree")]
    public class PromotionTreeTemplate : ScriptableObject, IPromotionTreeTemplate {

        #region instance fields and properties

        public IEnumerable<IPromotionPrerequisiteData> PrerequisiteData {
            get {
                return _prerequisiteData.Cast<IPromotionPrerequisiteData>();
            }
        }
        [SerializeField] private List<PromotionPrerequisiteData> _prerequisiteData;

        #endregion

    }

}
