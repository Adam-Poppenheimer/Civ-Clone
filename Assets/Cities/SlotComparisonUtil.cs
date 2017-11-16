using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public static class SlotComparisonUtil {

        #region static methods

        public static Comparison<IWorkerSlot> BuildFocusedComparisonAscending(ICity sourceCity,
            ResourceType focusedResource, IResourceGenerationLogic generationLogic) {

            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

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

        public static Comparison<IWorkerSlot> BuildResourceComparisonAscending(ResourceType focusedResource, ICity sourceCity,
            IResourceGenerationLogic generationLogic) {
            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

                return firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
            };
        }

        public static Comparison<IWorkerSlot> BuildTotalYieldComparisonAscending(ICity sourceCity,
            IResourceGenerationLogic generationLogic) {

            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

                return firstYield.Total.CompareTo(secondYield.Total);
            };

        }

        #endregion

    } 

}
