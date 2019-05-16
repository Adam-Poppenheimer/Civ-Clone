using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation {

    public interface IRandomizer {

        #region methods

        float GetRandom01();

        int GetRandomRange(int min, int max);

        T TakeRandom<T>(IEnumerable<T> set);

        #endregion

    }

}
