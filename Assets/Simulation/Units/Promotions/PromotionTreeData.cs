using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Promotions {

    [CreateAssetMenu(menuName = "Civ Clone/Promotion Tree Data")]
    public class PromotionTreeData : ScriptableObject, IPromotionTreeData {

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
