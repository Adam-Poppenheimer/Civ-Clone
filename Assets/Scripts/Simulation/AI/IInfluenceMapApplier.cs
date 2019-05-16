using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.AI {

    public delegate float InfluenceRolloff(float strength, int distance);
    public delegate float InfluenceApplier(float current, float calculated);

    public interface IInfluenceMapApplier {

        #region properties

        InfluenceRolloff PowerOfTwoRolloff { get; }
        InfluenceRolloff LinearRolloff     { get; }

        InfluenceApplier ApplySum     { get; }
        InfluenceApplier ApplyHighest { get; }

        #endregion

        #region methods

        void ApplyInfluenceToMap(
            float strength, float[] map, IHexCell center, int maxDistance,
            InfluenceRolloff rolloff, InfluenceApplier applier
        );

        #endregion

    }

}
