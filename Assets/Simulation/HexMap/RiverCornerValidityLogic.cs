using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class RiverCornerValidityLogic : IRiverCornerValidityLogic {

        #region instance methods

        #region from IRiverCornerValidityLogic

        public bool AreCornerFlowsValid(RiverFlow centerRight, RiverFlow? centerLeft, RiverFlow? leftRight) {
            if(centerLeft != null) {
                if(leftRight != null) {
                    switch(centerRight) {
                        case RiverFlow.Clockwise:        return !(centerLeft == RiverFlow.Counterclockwise && leftRight == RiverFlow.Counterclockwise);
                        case RiverFlow.Counterclockwise: return !(centerLeft == RiverFlow.Clockwise        && leftRight == RiverFlow.Clockwise);
                        default: throw new NotImplementedException();
                    }
                }else {
                    return centerLeft != centerRight.Opposite();
                }
            }else {
                return leftRight != centerRight.Opposite();
            }
        }

        #endregion

        #endregion

    }

}
