using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation {

    public static class EnumerableExtensions {

        #region static methods

        public static T MinElement<T>(this IEnumerable<T> set, Func<T, float> valueFunction) {
            T retval = default(T);
            float lastMin = float.MaxValue;

            foreach(T element in set) {
                float elementValue = valueFunction(element);

                if(elementValue <= lastMin) {
                    retval = element;
                }
            }

            return retval;
        }

        public static T MaxElement<T>(this IEnumerable<T> set, Func<T, float> valueFunction) {
            T retval = default(T);
            float lastMax = float.MinValue;

            foreach(T element in set) {
                float elementValue = valueFunction(element);

                if(elementValue >= lastMax) {
                    retval = element;
                    lastMax = elementValue;
                }
            }

            return retval;
        }

        #endregion

    }

}
