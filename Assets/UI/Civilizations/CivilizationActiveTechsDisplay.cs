﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

using Assets.UI.Technology;

namespace Assets.UI.Civilizations {

    public class CivilizationActiveTechsDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private TechnologyRecord CurrentTechRecord;


        private ICivilizationYieldLogic YieldLogic;

        private ITechCanon TechCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICivilizationYieldLogic yieldLogic, ITechCanon techCanon) {
            YieldLogic = yieldLogic;
            TechCanon  = techCanon;
        }

        #region from CivilizationDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            if(ObjectToDisplay.TechQueue.Count > 0) {
                var activeTech = ObjectToDisplay.TechQueue.Peek();

                CurrentTechRecord.TechToDisplay   = activeTech;
                CurrentTechRecord.Status          = TechnologyRecord.TechStatus.BeingResearched;
                CurrentTechRecord.CurrentProgress = TechCanon.GetProgressOnTechByCiv(activeTech, ObjectToDisplay);

                CurrentTechRecord.TurnsToResearch = (int)Math.Ceiling(
                    (activeTech.Cost - CurrentTechRecord.CurrentProgress) /
                    YieldLogic.GetYieldOfCivilization(ObjectToDisplay)[Simulation.ResourceType.Science]
                );
            }else {
                CurrentTechRecord.TechToDisplay = null;
            }

            CurrentTechRecord.Refresh();
        }

        #endregion

        #endregion

    }

}
