using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Barbarians {

    public class BarbarianTurnExecuter : IBarbarianTurnExecuter {

        #region instance fields and properties

        private IEncampmentFactory          EncampmentFactory;
        private ICivilizationFactory        CivFactory;
        private IBarbarianSpawningTools     SpawningTools;
        private IRandomizer                 Randomizer;
        private IBarbarianConfig            BarbarianConfig;
        private IBarbarianEncampmentSpawner EncampmentSpawner;
        private IBarbarianUnitSpawner       UnitSpawner;

        #endregion

        #region constructors

        [Inject]
        public BarbarianTurnExecuter(
            IEncampmentFactory encampmentFactory, ICivilizationFactory civFactory,
            IBarbarianSpawningTools spawningTools, IRandomizer randomizer,
            IBarbarianConfig barbarianConfig, IBarbarianEncampmentSpawner encampmentSpawner,
            IBarbarianUnitSpawner unitSpawner
        ) {
            EncampmentFactory = encampmentFactory;
            CivFactory        = civFactory;
            SpawningTools     = spawningTools;
            Randomizer        = randomizer;
            BarbarianConfig   = barbarianConfig;
            EncampmentSpawner = encampmentSpawner;
            UnitSpawner       = unitSpawner;
        }

        #endregion

        #region instance methods

        #region from IBarbarianTurnExecuter

        public void PerformEncampmentSpawning(BarbarianInfluenceMaps maps) {
            int encampmentCount = EncampmentFactory.AllEncampments.Count;
            int nonBarbarianCivCount = CivFactory.AllCivilizations.Count(civ => !civ.Template.IsBarbaric);

            float spawnChance = SpawningTools.GetEncampmentSpawnChance(encampmentCount, nonBarbarianCivCount);

            if(spawnChance >= 1f || Randomizer.GetRandom01() < spawnChance) {
                EncampmentSpawner.TrySpawnEncampment(maps);
            }
        }

        public void PerformUnitSpawning() {
            foreach(var encampment in EncampmentFactory.AllEncampments) {
                encampment.SpawnProgress += Randomizer.GetRandomRange(
                    BarbarianConfig.MinEncampmentSpawnProgress, BarbarianConfig.MaxEncampmentSpawnProgress
                );

                if(encampment.SpawnProgress >= BarbarianConfig.ProgressNeededForUnitSpawn) {
                    UnitSpawner.TrySpawnUnit(encampment);
                    encampment.SpawnProgress -= BarbarianConfig.ProgressNeededForUnitSpawn;
                }
            }
        }

        #endregion

        #endregion

    }

}
