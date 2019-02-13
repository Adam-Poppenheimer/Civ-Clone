using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapRendering {

    public static class ListPool<T> {

        #region static fields and properties

        private static Stack<List<T>> Stack = new Stack<List<T>>();

        #endregion

        #region static methods

        public static List<T> Get() {
            if(Stack.Count > 0) {
                return Stack.Pop();
            }

            return new List<T>();
        }

        public static void Add(List<T> list) {
            list.Clear();
            Stack.Push(list);
        }

        #endregion

    }

}
