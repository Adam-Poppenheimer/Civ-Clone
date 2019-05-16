using System;
using System.Collections.Generic;

namespace Assets.Simulation.MapGeneration {

    public delegate int ExpansionWeightFunction(MapSection section, List<MapSection> currentChunk);

    public interface ISectionSubdivisionLogic {

        #region methods

        List<List<MapSection>> DivideSectionsIntoChunks(
            HashSet<MapSection> unassignedSections, GridPartition partition,
            int chunkCount, int maxCellsPerChunk, int minSeedSeparation,
            ExpansionWeightFunction expansionWeightFunction, IMapTemplate mapTemplate
        );

        #endregion

    }

}