using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Distribution;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// Utility class that builds cell comparisons for certain Logic classes.
    /// </summary>
    public static class CellComparisonUtil {

        #region static methods

        /// <summary>
        /// Constructs a comparison that orders cells based on the amount of a particular
        /// resource type they would generate for the argued city, with other considerations
        /// in case of ties.
        /// </summary>
        /// <remarks>
        /// The effective yield is ordered from the smallest expected yield to the largest.
        /// Ties are broken first by the amount of food the resources yield, then by total
        /// yield on the tile. Both tiebreakers are organized in ascending order
        /// </remarks>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="focusedResource">The ResourceType to be maximized</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IHexCells from least to most yield of the focused resource</returns>
        public static Comparison<IHexCell> BuildFocusedComparisonAscending(ICity sourceCity,
            YieldType focusedResource, IYieldGenerationLogic generationLogic) {

            return delegate(IHexCell firstCell, IHexCell secondCell) {
                var firstYield = generationLogic.GetYieldOfCellForCity(firstCell, sourceCity);
                var secondYield = generationLogic.GetYieldOfCellForCity(secondCell, sourceCity);

                var focusComparison = firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
                if(focusComparison == 0) {
                    focusComparison = firstYield[YieldType.Food].CompareTo(secondYield[YieldType.Food]);
                }
                if(focusComparison == 0) {
                    focusComparison = firstYield.Total.CompareTo(secondYield.Total);
                }

                return focusComparison;
            };

        }

        /// <summary>
        /// Constructs a comparison that orders cells based on the amount of a particular
        /// resource type they generate for the argued city.
        /// </summary>
        /// <remarks>
        /// The effective yield is ordered from smallest expected yield to largest.
        /// Unlike <ref>CellComparisonUtil.BuildFocusedComparisonAscending</ref>, no
        /// additional comparisons are made beyond the yield of the specified resource
        /// </remarks>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="focusedYield">The YieldType to be maximized</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IHexCells from least to most yield of the focused resource</returns>
        public static Comparison<IHexCell> BuildYieldComparisonAscending(
            YieldType focusedYield, ICity sourceCity, IYieldGenerationLogic generationLogic
        ) {
            return delegate(IHexCell firstCell, IHexCell secondCell) {
                var firstYield  = generationLogic.GetYieldOfCellForCity(firstCell, sourceCity);
                var secondYield = generationLogic.GetYieldOfCellForCity(secondCell, sourceCity);

                return firstYield[focusedYield].CompareTo(secondYield[focusedYield]);
            };

        }

        /// <summary>
        /// Constructs a comparison that orders cells based on the total yield that slot is expected
        /// to make for the argued city.
        /// </summary>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IHexCells from least to most total yield</returns>
        public static Comparison<IHexCell> BuildTotalYieldComparisonAscending(
            ICity sourceCity, IYieldGenerationLogic generationLogic
        ) {

            return delegate(IHexCell firstCell, IHexCell secondCell) {
                var firstYield = generationLogic.GetYieldOfCellForCity(firstCell, sourceCity);
                var secondYield = generationLogic.GetYieldOfCellForCity(secondCell, sourceCity);

                return firstYield.Total.CompareTo(secondYield.Total);
            };

        }

        /// <summary>
        /// Constructs a comparison that orders slots based on their capacity to satisfy a particular
        /// ResourceFocusType for the argued city.
        /// </summary>
        /// <param name="sourceCity">The city whose hypothetical yield is being considered</param>
        /// <param name="focus">The ResourceFocusType to be maximized for</param>
        /// <param name="generationLogic">The IResourceGenerationLogic used to determine the yield for the argued city</param>
        /// <returns>A Comparison that'll sort IHexCells based on their usefulness to the argued focus</returns>
        public static Comparison<IHexCell> BuildComparisonAscending(
            ICity sourceCity, YieldFocusType focus, IYieldGenerationLogic generationLogic
        ) {

            switch(focus) {
                case YieldFocusType.Food:       return BuildYieldComparisonAscending(YieldType.Food,       sourceCity, generationLogic);
                case YieldFocusType.Gold:       return BuildYieldComparisonAscending(YieldType.Gold,       sourceCity, generationLogic);
                case YieldFocusType.Production: return BuildYieldComparisonAscending(YieldType.Production, sourceCity, generationLogic);
                case YieldFocusType.Culture:    return BuildYieldComparisonAscending(YieldType.Culture,    sourceCity, generationLogic);
                case YieldFocusType.Science:    return BuildYieldComparisonAscending(YieldType.Science,    sourceCity, generationLogic);
                case YieldFocusType.TotalYield: return BuildTotalYieldComparisonAscending(sourceCity, generationLogic);
                default: return null;
            }
        }

        #endregion

    }

}
