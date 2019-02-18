using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IRiverbedHeightLogic {

        #region methods

        float GetHeightForRiverEdgeAtPoint(
            IHexCell center, IHexCell right, HexDirection direction, Vector3 position
        );

        float GetHeightForRiverPreviousCornerAtPoint(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Vector3 position,
            bool hasCenterRightRiver, bool hasCenterLeftRiver, bool hasLeftRightRiver
        );

        float GetHeightForRiverNextCornerAtPoint(
            IHexCell center, IHexCell right, IHexCell nextRight, HexDirection direction, Vector3 position,
            bool hasCenterRightRiver, bool hasCenterNextRightRiver, bool hasRightNextRightRiver
        );

        #endregion

    }

}