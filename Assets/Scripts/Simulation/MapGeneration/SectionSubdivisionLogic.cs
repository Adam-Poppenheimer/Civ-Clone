﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class SectionSubdivisionLogic : ISectionSubdivisionLogic {

        #region instance fields and properties

        private IHexGrid                           Grid;
        private IWeightedRandomSampler<MapSection> MapSectionRandomSampler;

        #endregion

        #region constructors

        [Inject]
        public SectionSubdivisionLogic(
            IHexGrid grid, IWeightedRandomSampler<MapSection> mapSectionRandomSampler
        ) {
            Grid                    = grid;
            MapSectionRandomSampler = mapSectionRandomSampler;
        }

        #endregion

        #region instance methods

        #region from ISectionSubdivisionLogic

        public List<List<MapSection>> DivideSectionsIntoChunks(
            HashSet<MapSection> unassignedSections, GridPartition partition,
            int chunkCount, int maxCellsPerChunk, int minSeedSeparation,
            ExpansionWeightFunction weightFunction, IMapTemplate mapTemplate
        ) {
            if(chunkCount > unassignedSections.Count()) {
                throw new ArgumentOutOfRangeException("Not enough sections to assign at least one per chunk");
            }

            var finishedChunks   = new List<List<MapSection>>();
            var unfinishedChunks = new List<List<MapSection>>();

            var startingSections = new List<MapSection>();
            var forbiddenSections = new HashSet<MapSection>();

            if(mapTemplate.SeparateContinents) {
                DrawForbiddenLine(unassignedSections, forbiddenSections, partition, mapTemplate);
            }            

            for(int i = 0; i < chunkCount; i++) {
                var startingSection = GetStartingSection(
                    unassignedSections, startingSections, minSeedSeparation, mapTemplate
                );

                unfinishedChunks.Add(new List<MapSection>() { startingSection });

                startingSections.Add(startingSection);

                unassignedSections.Remove(startingSection);
            }

            int desiredContiguousCells = Mathf.RoundToInt(maxCellsPerChunk * mapTemplate.HomelandContiguousPercentage * 0.01f);

            while(unfinishedChunks.Count > 0 && unassignedSections.Count > 0) {
                var chunk = unfinishedChunks.Random();

                int cellCount = chunk.Sum(section => section.Cells.Count);

                bool mustBeContiguous = desiredContiguousCells > cellCount;

                if(TryExpandChunk(chunk, unassignedSections, partition, mapTemplate, weightFunction, mustBeContiguous)) {
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
            
            foreach(var section in forbiddenSections) {
                unassignedSections.Add(section);
            }

            return finishedChunks;
        }

        #endregion

        private void DrawForbiddenLine(
            HashSet<MapSection> unassignedSections, HashSet<MapSection> forbiddenSections,
            GridPartition partition, IMapTemplate mapTemplate
        ) {
            int lineXCoord = Mathf.RoundToInt(Grid.CellCountX * UnityEngine.Random.Range(
                mapTemplate.ContinentSeparationLineXMin, mapTemplate.ContinentSeparationLineXMax
            ));

            IHexCell startCell = Grid.GetCellAtCoordinates(HexCoordinates.FromOffsetCoordinates(lineXCoord, 0));
            IHexCell endCell   = Grid.GetCellAtCoordinates(HexCoordinates.FromOffsetCoordinates(lineXCoord, Grid.CellCountZ - 1));

            var cellLine = Grid.GetCellsInLine(startCell, endCell);

            var sectionsOnLine = cellLine.Select(cell => partition.GetSectionOfCell(cell)).Distinct();

            var sectionsToForbid =  sectionsOnLine.SelectMany(section => partition.GetNeighbors(section))
                                                  .Concat(sectionsOnLine).Distinct();

            foreach(var section in sectionsToForbid) {
                unassignedSections.Remove(section);
                forbiddenSections.Add(section);
            }            
        }

        private MapSection GetStartingSection(
            HashSet<MapSection> unassignedSections, IEnumerable<MapSection> startingSections,
            int minSeparation, IMapTemplate mapTemplate
        ) {
            var candidates = new List<MapSection>();

            foreach(var unassignedSection in unassignedSections) {
                if(unassignedSection.Cells.Count == 0 || IsWithinSoftBorder(unassignedSection.CentroidCell, mapTemplate)) {
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

        private HashSet<MapSection> expansionCandidates    = new HashSet<MapSection>();
        private HashSet<MapSection> nearbySections         = new HashSet<MapSection>();
        private HashSet<MapSection> sectionsWithinDistance = new HashSet<MapSection>();

        private bool TryExpandChunk(
            List<MapSection> chunk, HashSet<MapSection> unassignedSections,
            GridPartition partition, IMapTemplate mapTemplate,
            ExpansionWeightFunction weightFunction, bool mustBeContiguous
        ) {
            expansionCandidates.Clear();

            foreach(MapSection section in chunk) {
                nearbySections.Clear();

                sectionsWithinDistance.Clear();

                if(mustBeContiguous) {
                    foreach(var adjacentSection in partition.GetNeighbors(section)) {
                        if(unassignedSections.Contains(adjacentSection)) {
                            nearbySections.Add(adjacentSection);
                        }
                    }                  
                }else {
                    foreach(IHexCell nearbyCell in Grid.GetCellsInRadius(section.CentroidCell, mapTemplate.HomelandExpansionMaxCentroidSeparation)) {
                        MapSection sectionOfCell = partition.GetSectionOfCell(nearbyCell);

                        if(unassignedSections.Contains(sectionOfCell)) {
                            sectionsWithinDistance.Add(sectionOfCell);
                        }
                    }

                    var sectionsSurroundedByUnassigned = sectionsWithinDistance.Where(
                        nearby => partition.GetNeighbors(nearby).Count(neighbor => !unassignedSections.Contains(neighbor)) <= 0
                    );

                    if(sectionsSurroundedByUnassigned.Any()) {
                        foreach(var surroundedSection in sectionsSurroundedByUnassigned) {
                            nearbySections.Add(surroundedSection);
                        }
                    }else {
                        foreach(var sectionWithin in sectionsWithinDistance) {
                            nearbySections.Add(sectionWithin);
                        }
                    }
                }

                foreach(var nearby in nearbySections) {
                    expansionCandidates.Add(nearby);
                }
            }

            if(expansionCandidates.Count > 0) {
                var newSection = MapSectionRandomSampler.SampleElementsFromSet(
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

        private bool IsWithinSoftBorder(IHexCell cell, IMapTemplate template) {
            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= template.SoftMapBorderX || Grid.CellCountX - xOffset <= template.SoftMapBorderX
                || zOffset <= template.SoftMapBorderZ || Grid.CellCountZ - zOffset <= template.SoftMapBorderZ;
        }

        #endregion

    }

}
