using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IMapCollisionLogic {

        #region methods

        Vector3 GetNearestMapPointToPoint(Vector3 point);

        #endregion

    }

}