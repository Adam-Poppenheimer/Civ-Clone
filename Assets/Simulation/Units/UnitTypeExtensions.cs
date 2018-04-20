using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public static class UnitTypeExtensions {

        #region static methods

        public static bool IsCivilian(this UnitType type) {
            return type == UnitType.Civilian;
        }

        public static bool IsLandMilitary(this UnitType type) {
            return type != UnitType.Civilian    && type != UnitType.NavalMelee
                && type != UnitType.NavalRanged && type != UnitType.City;
        }

        public static bool IsWaterMilitary(this UnitType type) {
            return type == UnitType.NavalMelee || type == UnitType.NavalRanged;
        }

        public static bool HasSameSupertypeAs(this UnitType thisType, UnitType otherType) {
            if(thisType.IsCivilian()) {
                return otherType.IsCivilian();

            }else if(thisType.IsLandMilitary()) {
                return otherType.IsLandMilitary();

            }else if(thisType.IsWaterMilitary()) {
                return otherType.IsWaterMilitary();
            }else {
                return false;
            }
        }

        #endregion

    }

}
