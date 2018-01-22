using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

using Assets.UI.Technology;

namespace Assets.UI.Civilizations {

    public class CivilizationActiveTechsDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private TechnologyRecord CurrentTechRecord;

        #endregion

        #region instance methods

        #region from CivilizationDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay != null && ObjectToDisplay.TechQueue.Count > 0) {
                CurrentTechRecord.TechToDisplay = ObjectToDisplay.TechQueue.Peek();
                CurrentTechRecord.Status = TechnologyRecord.TechStatus.BeingResearched;
                CurrentTechRecord.Refresh();
            }
        }

        #endregion

        #endregion

    }

}
