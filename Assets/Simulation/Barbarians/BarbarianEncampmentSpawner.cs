using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public class BarbarianEncampmentSpawner : IBarbarianEncampmentSpawner {

        #region instance fields and properties

        private IEncampmentFactory               EncampmentFactory;
        private IHexGrid                         Grid;
        private IWeightedRandomSampler<IHexCell> CellSampler;
        private IBarbarianSpawningTools          SpawningTools;
        private IBarbarianUnitSpawner            UnitSpawner;

        #endregion

        #region constructors

        [Inject]
        public BarbarianEncampmentSpawner(
            IEncampmentFactory encampmentFactory, IHexGrid grid, IWeightedRandomSampler<IHexCell> cellSampler,
            IBarbarianSpawningTools spawningTools, IBarbarianUnitSpawner unitSpawner
        ) {
            EncampmentFactory = encampmentFactory;
            Grid              = grid;
            CellSampler       = cellSampler;
            SpawningTools     = spawningTools;
            UnitSpawner       = unitSpawner;
        }

        #endregion

        #region instance methods

        #region from IBarbarianSpawner

        public void TrySpawnEncampment(BarbarianInfluenceMaps maps) {
            var validCells = Grid.Cells.Where(SpawningTools.EncampmentValidityFilter).ToList();

            if(validCells.Any()) {
                var candidate = CellSampler.SampleElementsFromSet(
                    validCells, 1, SpawningTools.BuildEncampmentWeightFunction(maps)
                ).FirstOrDefault();

                if(candidate != null) {
                    var newEncampment = EncampmentFactory.CreateEncampment(candidate);

                    UnitSpawner.TrySpawnUnit(newEncampment);
                }
            }
        }

        #endregion

        #endregion
        
    }

}
