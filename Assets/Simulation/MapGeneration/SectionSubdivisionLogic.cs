using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class SectionSubdivisionLogic : ISectionSubdivisionLogic {

        #region instance fields and properties

        private IHexGrid Grid;
        private IMapGenerationConfig Config;

        #endregion

        #region constructors

        [Inject]
        public SectionSubdivisionLogic(IHexGrid grid, IMapGenerationConfig config) {
            Grid   = grid;
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ISectionSubdivisionLogic

        public List<List<MapSection>> DivideSectionsIntoChunks(
            HashSet<MapSection> unassignedSections, GridPartition partition,
            int chunkCount, int maxCellsPerChunk, int minSeedSeparation,
            bool forceFullAssignment, ExpansionWeightFunction weightFunction
        ) {
            if(chunkCount > unassignedSections.Count()) {
                throw new ArgumentOutOfRangeException("Not enough sections to assign at least one per chunk");
            }

            var finishedChunks   = new List<List<MapSection>>();
            var unfinishedChunks = new List<List<MapSection>>();

            var startingSections = new List<MapSection>();

            for(int i = 0; i < chunkCount; i++) {
                var startingSection = GetStartingSection(unassignedSections, startingSections, minSeedSeparation);

                unfinishedChunks.Add(new List<MapSection>() { startingSection });

                startingSections.Add(startingSection);

                unassignedSections.Remove(startingSection);
            }

            while(unfinishedChunks.Count > 0 && unassignedSections.Count > 0) {
                var chunk = unfinishedChunks.Random();

                if(TryExpandChunk(chunk, unassignedSections, partition, weightFunction)) {
                    int cellsInChunk = chunk.Sum(section => section.Cells.Count);

                    if(cellsInChunk >= maxCellsPerChunk) {
                        finishedChunks.Add(chunk);
                        unfinishedChunks.Remove(chunk);
                    }
                }else {
                    Debug.LogWarning("Failed to assign a new section to a chunk");
                    finishedChunks.Add(chunk);
                    unfinishedChunks.Remove(chunk);
                }
            }

            finishedChunks.AddRange(unfinishedChunks);

            while(forceFullAssignment && unassignedSections.Count > 0) {
                var orphan = unassignedSections.Last();

                var neighbors = partition.GetNeighbors(orphan);

                var neighboringChunks = finishedChunks.Where(chunk => chunk.Intersect(neighbors).Any()); 

                if(neighboringChunks.Any()) {
                    neighboringChunks.Random().Add(orphan);
                    unassignedSections.Remove(orphan);
                }else {
                    throw new InvalidOperationException("Failed to assign orphaned section to any chunk");
                }
            }
            
            return finishedChunks;
        }

        #endregion

        private MapSection GetStartingSection(
            HashSet<MapSection> unassignedSections, IEnumerable<MapSection> startingSections,
            int minSeparation
        ) {
            var candidates = new List<MapSection>();

            foreach(var unassignedSection in unassignedSections) {
                if(unassignedSection.Cells.Count == 0 || IsWithinSoftBorder(unassignedSection.CentroidCell)) {
                    continue;
                }

                bool unassignedIsValid = true;
                foreach(var startingSection in startingSections) {
                    if(Grid.GetDistance(unassignedSection.CentroidCell, startingSection.CentroidCell) < minSeparation) {
                        unassignedIsValid = false;
                        break;
                    }
                }

                if(unassignedIsValid) {
                    candidates.Add(unassignedSection);
                }
            }

            if(candidates.Count == 0) {
                throw new InvalidOperationException("Failed to acquire a valid starting location");
            }else {
                return candidates.Random();
            }
        }

        private bool TryExpandChunk(
            List<MapSection> chunk, HashSet<MapSection> unassignedSections,
            GridPartition partition, ExpansionWeightFunction weightFunction
        ) {
            var expansionCandidates = new HashSet<MapSection>();

            foreach(var section in chunk) {
                foreach(var neighbor in partition.GetNeighbors(section)) {
                    if(unassignedSections.Contains(neighbor)) {
                        expansionCandidates.Add(neighbor);
                    }
                }
            }

            if(expansionCandidates.Any()) {
                var newSection = WeightedRandomSampler<MapSection>.SampleElementsFromSet(
                    expansionCandidates, 1, section => weightFunction(section, chunk)
                ).FirstOrDefault();

                if(newSection != null) {
                    unassignedSections.Remove(newSection);
                    chunk.Add(newSection);
                    return true;
                }
            }

            return false;
        }

        private bool IsWithinSoftBorder(IHexCell cell) {
            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= Config.SoftMapBorderX || Grid.CellCountX - xOffset <= Config.SoftMapBorderX
                || zOffset <= Config.SoftMapBorderZ || Grid.CellCountZ - zOffset <= Config.SoftMapBorderZ;
        }

        #endregion

    }

}
