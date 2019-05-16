using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Units;

namespace Assets.Simulation.AI {

    public class UnitInfluenceSource : IInfluenceSource {

        #region instance fields and properties

        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IUnitStrengthEstimator                        UnitStrengthEstimator;
        private IInfluenceMapApplier                          InfluenceMapApplier;
        private IAIConfig                                     AIConfig;
        private IWarCanon                                     WarCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitInfluenceSource(
            IUnitFactory unitFactory, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IUnitStrengthEstimator unitStrengthEstimator,
            IInfluenceMapApplier influenceMapApplier, IAIConfig aiConfig, IWarCanon warCanon
        ) {
            UnitFactory           = unitFactory;
            UnitPossessionCanon   = unitPossessionCanon;
            UnitPositionCanon     = unitPositionCanon;
            UnitStrengthEstimator = unitStrengthEstimator;
            InfluenceMapApplier   = influenceMapApplier;
            AIConfig              = aiConfig;
            WarCanon              = warCanon;
        }

        #endregion

        #region instance methods

        #region from IInfluenceSource

        public void ApplyToMaps(InfluenceMaps maps, ICivilization targetCiv) {
            foreach(var unit in UnitFactory.AllUnits) {
                var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);
                var unitLocation = UnitPositionCanon  .GetOwnerOfPossession(unit);

                float unitStrength = UnitStrengthEstimator.EstimateUnitStrength(unit);

                if(unitOwner == targetCiv) {
                    InfluenceMapApplier.ApplyInfluenceToMap(
                        unitStrength, maps.AllyPresence, unitLocation, AIConfig.UnitMaxInfluenceRadius,
                        InfluenceMapApplier.PowerOfTwoRolloff, InfluenceMapApplier.ApplySum
                    );
                }else if(WarCanon.AreAtWar(unitOwner, targetCiv)) {
                    InfluenceMapApplier.ApplyInfluenceToMap(
                        unitStrength, maps.EnemyPresence, unitLocation, AIConfig.UnitMaxInfluenceRadius,
                        InfluenceMapApplier.PowerOfTwoRolloff, InfluenceMapApplier.ApplySum
                    );
                }
            }
        }

        #endregion

        #endregion
        
    }

}
