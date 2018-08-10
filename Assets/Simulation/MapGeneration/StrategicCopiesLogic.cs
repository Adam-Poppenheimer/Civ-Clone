using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class StrategicCopiesLogic : IStrategicCopiesLogic {

        #region instance fields and properties

        private List<int> CopyArray;



        private IMapGenerationConfig Config;

        #endregion

        #region constructors

        [Inject]
        public StrategicCopiesLogic(IMapGenerationConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from IStrategicCopiesLogic

        public int GetWeightedRandomCopies() {
            if(CopyArray == null) {
                SetCopyArray();
            }

            return CopyArray.Random();
        }

        #endregion

        //Creates a probability distribution similar to a 2d6 roll,
        //where every step towards the center of the distribution 
        //increases the number of samples for that number by 1.
        //(the smallest and largest have 1, the second smallest
        //and second largest have 2, the third largest and smallest 
        //have 3, and so on)
        private void SetCopyArray() {
            CopyArray = new List<int>();

            int averageCopies = Mathf.RoundToInt((Config.MinStrategicCopies + Config.MaxStrategicCopies) / 2f);

            for(int copies = Config.MinStrategicCopies; copies <= Config.MaxStrategicCopies; copies++) {
                int chancesForCopies = averageCopies - Math.Abs(averageCopies - copies) - 1;

                for(int i = 0; i < chancesForCopies; i++) {
                    CopyArray.Add(copies);
                }
            }
        }

        #endregion
        
    }

}
