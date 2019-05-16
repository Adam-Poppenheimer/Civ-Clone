using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IGridPartitionLogic {

        #region methods

        GridPartition GetPartitionOfGrid(IHexGrid grid, IMapTemplate template);

        #endregion

    }
}