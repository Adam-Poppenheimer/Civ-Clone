using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public class CivilizationRoundExecuter : IRoundExecuter {

        #region instance fields and properties

        private ICivilizationFactory CivFactory;

        #endregion

        #region constructors

        [Inject]
        public CivilizationRoundExecuter(ICivilizationFactory civFactory) {
            CivFactory = civFactory;
        }

        #endregion

        #region instance methods

        #region from IRoundExecuter

        public void PerformStartOfRoundActions() {
            foreach(var civ in CivFactory.AllCivilizations) {
                if(!civ.Template.IsBarbaric) {
                    civ.PerformIncome();
                    civ.PerformResearch();
                    civ.PerformGreatPeopleGeneration();
                    civ.PerformGoldenAgeTasks();
                }
            }
        }

        public void PerformEndOfRoundActions() {
            
        }

        #endregion

        #endregion

    }

}
