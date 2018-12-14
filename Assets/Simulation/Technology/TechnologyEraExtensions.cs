using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Technology {

    public static class TechnologyEraExtensions {

        #region static methods

        public static bool HasPreviousEra(this TechnologyEra currentEra) {
            return currentEra != TechnologyEra.Ancient;
        }

        public static TechnologyEra GetPreviousEra(this TechnologyEra currentEra) {
            if(!HasPreviousEra(currentEra)) {
                throw new InvalidOperationException(string.Format("Era {0} has no previous era"));
            }else {
                return (TechnologyEra)(((int)currentEra) - 1);
            }
        }

        public static bool HasNextEra(this TechnologyEra currentEra) {
            return currentEra != TechnologyEra.Medieval;
        }

        public static TechnologyEra GetNextEra(this TechnologyEra currentEra) {
            if(!HasNextEra(currentEra)) {
                throw new InvalidOperationException(string.Format("Era {0} has no next era"));
            }else {
                return (TechnologyEra)(((int)currentEra) + 1);
            }
        }

        #endregion

    }

}
