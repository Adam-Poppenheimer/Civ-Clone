using System;
using System.Collections.Generic;

namespace Assets.Simulation {

    public interface IWeightedRandomSampler<T> {

        #region methods

        List<T> SampleElementsFromSet(IEnumerable<T> set, int count, Func<T, int> weightFunction);

        List<T> SampleElementsFromSet(
            IEnumerable<T> set, int count,
            Func<T, int> startingWeightFunction,
            Func<T, List<T>, int> dynamicWeightFunction,
            Func<T, IEnumerable<T>> dynamicElementFilter
        );

        #endregion

    }

}