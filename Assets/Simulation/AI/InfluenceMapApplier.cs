using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.AI {

    public class InfluenceMapApplier : IInfluenceMapApplier {

        #region instance fields and properties

        #region from IInfluenceApplier

        public InfluenceRolloff PowerOfTwoRolloff {
            get { return (strength, distance) => strength / Mathf.Pow(2, distance); }
        }

        public InfluenceRolloff LinearRolloff {
            get { return (strength, distance) => strength / distance; }
        }

        public InfluenceApplier ApplyHighest {
            get { return (previous, calculated) => Mathf.Max(previous, calculated); }
        }

        public InfluenceApplier ApplySum {
            get { return (previous, calculated) => previous + calculated; }
        }

        #endregion

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public InfluenceMapApplier(IHexGrid grid) {
            Grid = grid;
        }

        #endregion

        #region instance methods

        #region from IInfluenceMapApplier

        public void ApplyInfluenceToMap(
            float strength, float[] map, IHexCell center, int maxDistance,
            InfluenceRolloff rolloff, InfluenceApplier applier
        ) {
            map[center.Index] = applier(map[center.Index], strength);

            for(int i = 1; i <= maxDistance; i++) {
                foreach(var cellInRing in Grid.GetCellsInRing(center, i)) {
                    float effectiveStrength = rolloff(strength, i);

                    map[cellInRing.Index] = applier(map[cellInRing.Index], effectiveStrength);
                }
            }
        }

        #endregion

        #endregion
        
    }

}
