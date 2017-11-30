using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Distribution;

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

        public static Comparison<IMapTile> BuildComparisonAscending(ICity sourceCity, ResourceFocusType focus,
            IResourceGenerationLogic generationLogic) {

            switch(focus) {
                case ResourceFocusType.Food:       return BuildResourceComparisonAscending(ResourceType.Food,       sourceCity, generationLogic);
                case ResourceFocusType.Gold:       return BuildResourceComparisonAscending(ResourceType.Gold,       sourceCity, generationLogic);
                case ResourceFocusType.Production: return BuildResourceComparisonAscending(ResourceType.Production, sourceCity, generationLogic);
                case ResourceFocusType.Culture:    return BuildResourceComparisonAscending(ResourceType.Culture,    sourceCity, generationLogic);
                case ResourceFocusType.TotalYield: return BuildTotalYieldComparisonAscending(sourceCity, generationLogic);
                default: return null;
            }
        }

        #endregion

    }

}
