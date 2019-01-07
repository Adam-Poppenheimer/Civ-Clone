using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation {

    public class WeightedRandomSampler<T> : IWeightedRandomSampler<T> {

        #region instance methods

        public List<T> SampleElementsFromSet(
            IEnumerable<T> set, int count, Func<T, int> weightFunction
        ) {
            int totalWeight = 0;
            var weights = new Dictionary<T, int>();

            foreach(var element in set) {
                int weight = weightFunction(element);

                totalWeight += weight;
                weights[element] = weight;
            }

            var sample = new List<T>();

            for(int i = 0; i < count; i++) {
                T selectedElement = default(T);
                int selectedWeight = 0;
                int randomNumber = UnityEngine.Random.Range(0, totalWeight);

                foreach(var element in weights.Keys) {
                    selectedWeight = weights[element];

                    if(randomNumber < selectedWeight) {
                        selectedElement = element;
                        break;
                    }

                    randomNumber -= selectedWeight;
                }

                if(selectedElement != null) {
                    sample.Add(selectedElement);
                    totalWeight -= selectedWeight;
                    weights.Remove(selectedElement);
                }else {
                    break;
                }
            }

            return sample;
        }

        public List<T> SampleElementsFromSet(
            IEnumerable<T> set, int count, Func<T, int> startingWeightFunction,
            Func<T, List<T>, int> dynamicWeightFunction, Func<T, IEnumerable<T>> dynamicElementFilter
        ) {
            int totalWeight = 0;
            var weights = new Dictionary<T, int>();

            foreach(var element in set) {
                int weight = startingWeightFunction(element);

                totalWeight += weight;
                weights[element] = weight;
            }

            var sample = new List<T>();

            for(int i = 0; i < count; i++) {
                T selectedElement = default(T);
                int selectedWeight = 0;
                int randomNumber = UnityEngine.Random.Range(0, totalWeight);

                foreach(var element in weights.Keys) {
                    selectedWeight = weights[element];

                    if(randomNumber < selectedWeight) {
                        selectedElement = element;
                        break;
                    }

                    randomNumber -= selectedWeight;
                }

                if(selectedElement == null) {
                    Debug.LogWarning("Failed to sample an element from the set. Aborting search");
                    break;
                }

                sample.Add(selectedElement);
                totalWeight -= selectedWeight;
                weights.Remove(selectedElement);

                foreach(var affectedElement in dynamicElementFilter(selectedElement)) {
                    if(!weights.ContainsKey(affectedElement)) {
                        continue;
                    }

                    int oldWeight = weights[affectedElement];
                    int newWeight = dynamicWeightFunction(affectedElement, sample);

                    totalWeight += newWeight - oldWeight;
                    weights[affectedElement] = newWeight;
                }
            }

            return sample;
        }

        #endregion

    }

}
