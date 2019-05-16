using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Units {

    public class CanBuildCityLogic : ICanBuildCityLogic {

        #region constructors

        [Inject]
        public CanBuildCityLogic() { }

        #endregion

        #region instance methods

        #region from ICanBuildCityLogic

        public bool CanUnitBuildCity(IUnit unit) {
            var possibleCommandRequests = unit.Abilities.SelectMany(ability => ability.CommandRequests);

            return possibleCommandRequests.Where(request => request.Type == Abilities.AbilityCommandType.FoundCity).Any();
        }

        #endregion

        #endregion
        
    }

}
