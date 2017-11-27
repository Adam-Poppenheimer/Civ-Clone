using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Cities {

    public static class TileComparisonUtil {

        #region static methods

        public static Comparison<IMapTile> BuildFocusedComparisonAscending(ICity sourceCity,
            ResourceType focusedResource, IResourceGenerationLogic generationLogic) {

            return delegate(IMapTile firstTile, IMapTile secondTile) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstTile.WorkerSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondTile.WorkerSlot, sourceCity);

                var focusComparison = firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
                if(focusComparison == 0) {
                    focusComparison = firstYield[ResourceType.Food].CompareTo(secondYield[ResourceType.Food]);
                }
                if(focusComparison == 0) {
                    focusComparison = firstYield.Total.CompareTo(secondYield.Total);
                }

                return focusComparison;
            };

        }

        public static Comparison<IMapTile> BuildResourceComparisonAscending(ResourceType focusedResource, ICity sourceCity,
            IResourceGenerationLogic generationLogic) {

            return delegate(IMapTile firstTile, IMapTile secondTile) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstTile.WorkerSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondTile.WorkerSlot, sourceCity);

                return firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
            };

        }

        public static Comparison<IMapTile> BuildTotalYieldComparisonAscending(ICity sourceCity,
            IResourceGenerationLogic generationLogic) {

            return delegate(IMapTile firstTile, IMapTile secondTile) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstTile.WorkerSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondTile.WorkerSlot, sourceCity);

                return firstYield.Total.CompareTo(secondYield.Total);
            };

        }

        #endregion

    }

}
