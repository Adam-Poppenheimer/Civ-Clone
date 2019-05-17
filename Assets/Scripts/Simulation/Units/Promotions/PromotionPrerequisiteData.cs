using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Promotions {

    [Serializable]
    public class PromotionPrerequisiteData : IPromotionPrerequisiteData {

        #region instance fields and properties

        #region IPromotionPrerequisiteData

        public IPromotion Promotion {
            get { return _promotion; }
        }
        [SerializeField] private Promotion _promotion = null;

        public IEnumerable<IPromotion> Prerequisites {
            get { return _prerequisites.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _prerequisites = null;

        #endregion

        #endregion

    }

}
