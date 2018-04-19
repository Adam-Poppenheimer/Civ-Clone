using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IDiplomaticExchange {

        #region properties

        bool RequiresIntegerInput { get; }

        int IntegerInput { get; set; }

        #endregion

        #region methods

        bool OverlapsWithExchange(IDiplomaticExchange exchange);

        bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv);

        IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv);

        string GetSummary();

        #endregion

    }

}
