using System;

namespace Assets.Simulation.HexMap {

    public interface IRiverCornerValidityLogic {

        #region methods

        bool AreCornerFlowsValid(RiverFlow centerRight, RiverFlow? centerLeft, RiverFlow? leftRight);

        #endregion

    }

}