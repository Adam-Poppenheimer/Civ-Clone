using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// Utility class that builds slot comparisons for certain Logic classes.
    /// </summary>
    public static class SlotComparisonUtil {

        #region static methods

        /// <summary>
        /// Constructs a comparison that orders slots based on the amount of a particular
        /// resource type they generate for the argued city.
        /// </summary>
        /// <remarks>
        /// The effective yield is ordered from the smallest expected yield to the largest.
        /// Ties are broken first by the amount of food the resources yield, then by total
        /// yield on the tile. Both tiebreakers are organized in ascending order
        /// </remarks>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="focusedResource">The ResourceType to be maximized</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IWorkerSlots from least to most yield of the focused resource</returns>
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

        /// <summary>
        /// Constructs a comparison that orders slots based on the amount of a particular
        /// resource type they generate for the argued city.
        /// </summary>
        /// <remarks>
        /// The effective yield is ordered from smallest expected yield to largest.
        /// Unlike <ref>SlotComparisonUtil.BuildFocusedComparisonAscending</ref>, no
        /// additional comparisons are made beyond the yield of the specified resource
        /// </remarks>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="focusedResource">The ResourceType to be maximized</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IWorkerSlots from least to most yield of the focused resource</returns>
        public static Comparison<IWorkerSlot> BuildResourceComparisonAscending(ResourceType focusedResource, ICity sourceCity,
            IResourceGenerationLogic generationLogic) {
            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = generationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = generationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

                return firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
            };
        }

        /// <summary>
        /// Constructs a comparison that orders slots based on the total yield that slot is expected
        /// to make for the argued city.
        /// </summary>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IWorkerSlots from least to most total yield</returns>
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
