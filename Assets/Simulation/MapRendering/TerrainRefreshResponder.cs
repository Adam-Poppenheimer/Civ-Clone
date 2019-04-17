using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapRendering {

    public class TerrainRefreshResponder {

        #region instance fields and properties

        private IHexGrid                                 Grid;
        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        public TerrainRefreshResponder(
            IHexGrid grid, IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            CitySignals citySignals, CivilizationSignals civSignals
        ) {
            Grid                = grid;
            CellPossessionCanon = cellPossessionCanon;

            citySignals.LostCellFromBoundaries.Subscribe(data => HandleCultureChange(data.Item2));
            citySignals.GainedCellToBoundaries.Subscribe(data => HandleCultureChange(data.Item2));

            civSignals.CivGainedCity.Subscribe(data => HandleCultureChange(data.Item2));
            civSignals.CivLostCity  .Subscribe(data => HandleCultureChange(data.Item2));
        }

        #endregion

        #region instance methods

        private void HandleCultureChange(IHexCell modifiedCell) {
            foreach(var affectedChunk in Grid.GetCellsInRadius(modifiedCell, 1).SelectMany(cell => cell.OverlappingChunks)) {
                affectedChunk.RefreshCulture();
            }
        }

        private void HandleCultureChange(ICity modifiedCity) {
            foreach(var cell in CellPossessionCanon.GetPossessionsOfOwner(modifiedCity)) {
                HandleCultureChange(cell);
            }
        }

        #endregion

    }

}
