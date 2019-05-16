using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation {

    public class Randomizer : IRandomizer {

        #region instance methods

        #region from IRandomizer

        public float GetRandom01() {
            return UnityEngine.Random.value;
        }

        public int GetRandomRange(int min, int max) {
            return UnityEngine.Random.Range(min, max);
        }

        public T TakeRandom<T>(IEnumerable<T> set) {
            return set.Random();
        }

        #endregion

        #endregion

    }

}
