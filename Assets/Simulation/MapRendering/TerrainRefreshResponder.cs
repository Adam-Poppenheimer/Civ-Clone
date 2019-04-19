using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

using Assets.Simulation.Barbarians;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapRendering {

    public class TerrainRefreshResponder {

        #region instance fields and properties

        private IHexGrid                                 Grid;
        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        public TerrainRefreshResponder(
            IHexGrid grid, IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            CitySignals citySignals, CivilizationSignals civSignals, HexCellSignals cellSignals,
            ImprovementSignals improvementSignals, ResourceSignals resourceSignals
        ) {
            Grid                = grid;
            CellPossessionCanon = cellPossessionCanon;

            citySignals.LostCellFromBoundaries .Subscribe(OnCellPossessionChanged);
            citySignals.GainedCellToBoundaries .Subscribe(OnCellPossessionChanged);
            citySignals.CityAddedToLocation    .Subscribe(OnCityChangedLocation);
            citySignals.CityRemovedFromLocation.Subscribe(OnCityChangedLocation);

            civSignals.CivGainedCity.Subscribe(OnCityPossessionChanged);
            civSignals.CivLostCity  .Subscribe(OnCityPossessionChanged);

            cellSignals.TerrainChanged   .Subscribe(OnCellTerrainChanged);
            cellSignals.ShapeChanged     .Subscribe(OnCellShapeChanged);
            cellSignals.VegetationChanged.Subscribe(OnCellVegetationChanged);
            cellSignals.FeatureChanged   .Subscribe(OnCellFeatureChanged);
            cellSignals.RoadStatusChanged.Subscribe(OnCellRoadStatusChanged);
            cellSignals.GainedRiveredEdge.Subscribe(OnCellRiverChanged);
            cellSignals.LostRiveredEdge  .Subscribe(OnCellRiverChanged);
            cellSignals.GainedEncampment .Subscribe(OnCellEncampmentChanged);
            cellSignals.LostEncampment   .Subscribe(OnCellEncampmentChanged);

            improvementSignals.AddedToLocation    .Subscribe(OnCellImprovementsChanged);
            improvementSignals.RemovedFromLocation.Subscribe(OnCellImprovementsChanged);

            resourceSignals.NodeAddedToLocation    .Subscribe(OnCellNodesChanged);
            resourceSignals.NodeRemovedFromLocation.Subscribe(OnCellNodesChanged);
        }

        #endregion

        #region instance methods

        #region for CitySignals

        private void OnCellPossessionChanged(Tuple<ICity, IHexCell> data) {
            foreach(var affectedChunk in GetAffectedChunks(data.Item2)) {
                affectedChunk.Refresh(TerrainRefreshType.Culture | TerrainRefreshType.Visibility);
            }
        }

        private void OnCityChangedLocation(Tuple<ICity, IHexCell> data) {
            foreach(var chunk in GetAffectedChunks(data.Item2)) {
                chunk.Refresh(TerrainRefreshType.Features);
            }
        }

        #endregion

        #region for CivSignals

        private void OnCityPossessionChanged(Tuple<ICivilization, ICity> data) {
            var affectedChunks = CellPossessionCanon.GetPossessionsOfOwner(data.Item2)
                                                    .SelectMany(cell => Grid.GetCellsInRadius(cell, 1))
                                                    .SelectMany(cell => cell.OverlappingChunks)
                                                    .Distinct();

            foreach(var chunk in affectedChunks) {
                chunk.Refresh(TerrainRefreshType.Culture | TerrainRefreshType.Visibility);
            }
        }

        #endregion

        #region for CellSignals

        private void OnCellRoadStatusChanged(HexPropertyChangedData<bool> data) {
            foreach(var chunk in GetAffectedChunks(data.Cell)) {
                chunk.Refresh(TerrainRefreshType.Roads);
            }
        }

        private void OnCellTerrainChanged(HexPropertyChangedData<CellTerrain> data) {
            TerrainRefreshType refreshFlags = TerrainRefreshType.Alphamap;

            if(data.OldValue.IsWater() != data.NewValue.IsWater()) {
                refreshFlags |= TerrainRefreshType.Water | TerrainRefreshType.Heightmap;
            }

            if(data.NewValue.IsWater()) {
                refreshFlags |= TerrainRefreshType.Water;
            }

            foreach(var chunk in GetAffectedChunks(data.Cell)) {
                chunk.Refresh(refreshFlags);
            }
        }

        private void OnCellShapeChanged(HexPropertyChangedData<CellShape> data) {
            TerrainRefreshType refreshFlags = TerrainRefreshType.Heightmap;

            if((data.OldValue == CellShape.Mountains) != (data.NewValue == CellShape.Mountains)) {
                refreshFlags |= TerrainRefreshType.Alphamap;
            }

            foreach(var chunk in GetAffectedChunks(data.Cell)) {
                chunk.Refresh(refreshFlags);
            }
        }

        private void OnCellVegetationChanged(HexPropertyChangedData<CellVegetation> data) {
            foreach(var chunk in GetAffectedChunks(data.Cell)) {
                chunk.Refresh(TerrainRefreshType.Features | TerrainRefreshType.Visibility | TerrainRefreshType.Marshes);
            }
        }

        private void OnCellFeatureChanged(HexPropertyChangedData<CellFeature> data) {
            foreach(var chunk in GetAffectedChunks(data.Cell)) {
                chunk.Refresh(TerrainRefreshType.Features | TerrainRefreshType.Visibility);
            }
        }

        private void OnCellRiverChanged(IHexCell cell) {
            foreach(var chunk in GetAffectedChunks(cell)) {
                chunk.Refresh(
                    TerrainRefreshType.Heightmap | TerrainRefreshType.Alphamap |
                    TerrainRefreshType.Culture   | TerrainRefreshType.Farmland |
                    TerrainRefreshType.Rivers    | TerrainRefreshType.Roads    |
                    TerrainRefreshType.Marshes
                );
            }
        }

        private void OnCellEncampmentChanged(Tuple<IHexCell, IEncampment> data) {
            foreach(var chunk in GetAffectedChunks(data.Item1)) {
                chunk.Refresh(TerrainRefreshType.Features);
            }
        }

        #endregion

        #region for ImprovementSignals

        private void OnCellImprovementsChanged(Tuple<IImprovement, IHexCell> data) {
            TerrainRefreshType refreshFlags = TerrainRefreshType.Features;
            
            if(data.Item1.Template.ProducesFarmland) {
                refreshFlags |= TerrainRefreshType.Farmland;
            }

            if(data.Item1.Template.OverridesTerrain) {
                refreshFlags |= TerrainRefreshType.Alphamap;
            }

            foreach(var chunk in GetAffectedChunks(data.Item2)) {
                chunk.Refresh(refreshFlags);
            }
        }

        #endregion

        #region for ResourceSignals

        private void OnCellNodesChanged(Tuple<IResourceNode, IHexCell> data) {
            foreach(var chunk in data.Item2.OverlappingChunks) {
                chunk.Refresh(TerrainRefreshType.Features);
            }
        }

        #endregion

        private IEnumerable<IMapChunk> GetAffectedChunks(IHexCell centerCell) {
            return Grid.GetCellsInRadius(centerCell, 1).SelectMany(cell => cell.OverlappingChunks);
        }

        #endregion

    }

}
