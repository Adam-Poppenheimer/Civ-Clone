﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class VegetationPainter : IVegetationPainter {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;
        private IRiverCanon            RiverCanon;
        private IGridTraversalLogic    GridTraversalLogic;
        private IMapGenerationConfig   Config;

        #endregion

        #region constructors

        [Inject]
        public VegetationPainter(
            ICellModificationLogic modLogic, IHexGrid grid, IRiverCanon riverCanon,
            IGridTraversalLogic gridTraversalLogic, IMapGenerationConfig config
        ) {
            ModLogic           = modLogic;
            Grid               = grid;
            RiverCanon         = riverCanon;
            GridTraversalLogic = gridTraversalLogic;
            Config             = config;
        }

        #endregion

        #region instance methods

        #region from IVegetationPainter

        public void PaintVegetation(MapRegion region, IRegionTemplate template) {
            var treeType = template.AreTreesJungle ? CellVegetation.Jungle : CellVegetation.Forest;

            var openCells = new List<IHexCell>();

            foreach(var cell in region.LandCells) {
                if(ShouldBeMarsh(cell, template)) {
                    ModLogic.ChangeVegetationOfCell(cell, CellVegetation.Marsh);

                }else if(ModLogic.CanChangeVegetationOfCell(cell, treeType)) {
                   openCells.Add(cell);
                }
            }

            int treeCount = Mathf.RoundToInt(template.TreePercentage * openCells.Count * 0.01f);

            var treeSeeds = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                openCells, UnityEngine.Random.Range(template.MinTreeClumps, template.MaxTreeClumps),
                GetTreeSeedWeightFunction(treeType, template)
            );

            var treeCells = new List<IHexCell>();

            var treeCrawlers = treeSeeds.Select(
                seed => GridTraversalLogic.GetCrawlingEnumerator(
                    seed, openCells, treeCells, GetTreeCrawlingCostFunction(treeType, template)
                )
            ).ToList();

            for(int i = 0; i < treeCount; i++) {
                if(treeCrawlers.Count == 0) {
                    Debug.LogWarning("Failed to paint correct number of trees into starting area");
                    break;
                }

                var crawler = treeCrawlers.Random();

                if(crawler.MoveNext()) {
                    treeCells.Add(crawler.Current);
                    openCells.Remove(crawler.Current);
                }else {
                    treeCrawlers.Remove(crawler);
                    i--;
                }
            }

            foreach(var treeCell in treeCells) {
                ModLogic.ChangeVegetationOfCell(treeCell, treeType);
            }
        }

        #endregion

        private bool ShouldBeMarsh(IHexCell cell, IRegionTemplate template) {
            if(cell.Terrain != CellTerrain.Grassland || cell.Shape != CellShape.Flatlands) {
                return false;
            }

            int adjacentWater = Grid.GetNeighbors(cell).Where(
                neighbor => neighbor.Terrain.IsWater()
            ).Count();

            int adjacentRivers = EnumUtil.GetValues<HexDirection>().Where(
                direction => RiverCanon.HasRiverAlongEdge(cell, direction)
            ).Count();

            float chanceOfMarsh = template.MarshChanceBase
                                + adjacentWater  * template.MarshChancePerAdjacentWater
                                + adjacentRivers * template.MarshChancePerAdjacentRiver;

            return UnityEngine.Random.value < chanceOfMarsh;
        }

        private Func<IHexCell, int> GetTreeSeedWeightFunction(
            CellVegetation treeType, IRegionTemplate template
        ) {
            return delegate(IHexCell cell) {
                if( cell.Vegetation == CellVegetation.Marsh || cell.Feature != CellFeature.None ||
                    !ModLogic.CanChangeVegetationOfCell(cell, treeType)
                ) {
                    return 0;
                }else {
                    int terrainCost  = template.GetTreePlacementCostForTerrain(cell.Terrain);
                    int shapeCost    = template.GetTreePlacementCostForShape  (cell.Shape);

                    return 1000 - 200 * (terrainCost + shapeCost);
                }
            };
        }

        private CrawlingWeightFunction GetTreeCrawlingCostFunction(
            CellVegetation treeType, IRegionTemplate template
        ) {
            return delegate(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
                if( cell.Vegetation == CellVegetation.Marsh || cell.Feature != CellFeature.None ||
                    !ModLogic.CanChangeVegetationOfCell(cell, treeType)
                ) {
                    return -1;
                }else {
                    int terrainCost  = template.GetTreePlacementCostForTerrain(cell.Terrain);
                    int shapeCost    = template.GetTreePlacementCostForShape  (cell.Shape);
                    int distanceCost = Grid.GetDistance(seed, cell);

                    return terrainCost + shapeCost + distanceCost;
                }
            };
        }

        #endregion
        
    }

}
