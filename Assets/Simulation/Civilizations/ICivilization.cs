using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface ICivilization {

        #region properties

        string Name { get; }

        int GoldStockpile    { get; set; }
        int CultureStockpile { get; set; }

        #endregion

        #region methods

        void PerformIncome();

        #endregion

    }

}
