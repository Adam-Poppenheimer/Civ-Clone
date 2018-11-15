using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Civilizations {

    public interface ICivilization {

        #region properties

        ICivilizationTemplate Template { get; }

        int GoldStockpile    { get; set; }
        int CultureStockpile { get; set; }

        Queue<ITechDefinition> TechQueue { get; }

        #endregion

        #region methods

        void PerformIncome();

        void PerformResearch();

        void PerformGreatPeopleGeneration();

        void Destroy();

        #endregion

    }

}
