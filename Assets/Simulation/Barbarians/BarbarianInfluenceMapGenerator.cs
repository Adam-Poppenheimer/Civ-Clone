using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public class BarbarianInfluenceMapGenerator : IBarbarianInfluenceMapGenerator {

        #region instance fields and properties

        private BarbarianInfluenceMaps Maps = new BarbarianInfluenceMaps();




        private IHexGrid                                      Grid;
        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IUnitStrengthEstimator                        UnitStrengthEstimator;
        private IAIConfig                                     AIConfig;
        private IInfluenceMapApplier                          InfluenceMapApplier;

        #endregion

        #region constructors

        [Inject]
        public BarbarianInfluenceMapGenerator(
            IHexGrid grid, IUnitFactory unitFactory, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IUnitStrengthEstimator unitStrengthEstimator, IAIConfig aiConfig,
            IInfluenceMapApplier influenceMapApplier
        ) {
            Grid                  = grid;
            UnitFactory           = unitFactory;
            UnitPossessionCanon   = unitPossessionCanon;
            UnitPositionCanon     = unitPositionCanon;
            UnitStrengthEstimator = unitStrengthEstimator;
            AIConfig              = aiConfig;
            InfluenceMapApplier   = influenceMapApplier;
        }

        #endregion

        #region instance methods

        #region from IBarbarianInfluenceMapGenerator

        public BarbarianInfluenceMaps GenerateMaps() {
            if(Maps.AllyPresence  == null) { Maps.AllyPresence  = new float[Grid.Cells.Count]; }
            if(Maps.EnemyPresence == null) { Maps.EnemyPresence = new float[Grid.Cells.Count]; }

            foreach(var cell in Grid.Cells) {
                Maps.AllyPresence [cell.Index] = 0f;
                Maps.EnemyPresence[cell.Index] = 0f;
            }

            foreach(var unit in UnitFactory.AllUnits) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                float unitStrength = UnitStrengthEstimator.EstimateUnitStrength(unit);

                if(unitOwner.Template.IsBarbaric) {
                    InfluenceMapApplier.ApplyInfluenceToMap(
                        unitStrength, Maps.AllyPresence, unitLocation, AIConfig.UnitMaxInfluenceRadius,
                        InfluenceMapApplier.PowerOfTwoRolloff, InfluenceMapApplier.ApplySum
                    );
                }else {
                    InfluenceMapApplier.ApplyInfluenceToMap(
                        unitStrength, Maps.EnemyPresence, unitLocation, AIConfig.UnitMaxInfluenceRadius,
                        InfluenceMapApplier.PowerOfTwoRolloff, InfluenceMapApplier.ApplySum
                    );
                }
            }

            return Maps;
        }

        public void ClearMaps() {
            Maps.AllyPresence  = null;
            Maps.EnemyPresence = null;
        }

        #endregion

        #endregion

    }

}
