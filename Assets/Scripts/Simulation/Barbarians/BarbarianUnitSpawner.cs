using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Barbarians {

    public class BarbarianUnitSpawner : IBarbarianUnitSpawner {

        #region instance fields and properties
              
        private ICivilizationFactory          CivFactory;
        private IBarbarianConfig              BarbarianConfig;
        private IUnitFactory                  UnitFactory;
        private IRandomizer                   Randomizer;
        private IBarbarianSpawningTools       SpawningTools;
        private IBarbarianAvailableUnitsLogic AvailableUnitsLogic;

        #endregion

        #region constructors

        [Inject]
        public BarbarianUnitSpawner(
            ICivilizationFactory civFactory, IBarbarianConfig barbarianConfig,
            IUnitFactory unitFactory, IRandomizer randomizer,
            IBarbarianSpawningTools spawningTools,
            IBarbarianAvailableUnitsLogic availableUnitsLogic
        ) {
            CivFactory          = civFactory;
            BarbarianConfig     = barbarianConfig;
            UnitFactory         = unitFactory;
            Randomizer          = randomizer;
            SpawningTools       = spawningTools;
            AvailableUnitsLogic = availableUnitsLogic;
        }

        #endregion

        #region instance methods

        #region from IBarbarianUnitSpawner

        public bool TrySpawnUnit(IEncampment encampment) {
            if(Randomizer.GetRandom01() <= BarbarianConfig.WaterSpawnChance) {
                var navalInfo = SpawningTools.TryGetValidSpawn(encampment, AvailableUnitsLogic.NavalTemplateSelector);

                if(navalInfo.IsValidSpawn) {
                    UnitFactory.BuildUnit(navalInfo.LocationOfUnit, navalInfo.TemplateToBuild, CivFactory.BarbarianCiv);

                    return true;
                }
            }

            var landInfo = SpawningTools.TryGetValidSpawn(encampment, AvailableUnitsLogic.LandTemplateSelector);

            if(landInfo.IsValidSpawn) {
                UnitFactory.BuildUnit(landInfo.LocationOfUnit, landInfo.TemplateToBuild, CivFactory.BarbarianCiv);

                return true;
            }else {
                return false;
            }
        }

        #endregion

        #endregion
        
    }

}
