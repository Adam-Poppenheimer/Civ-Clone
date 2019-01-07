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

namespace Assets.Simulation.Players.Barbarians {

    public class BarbarianInfluenceMapGenerator : IBarbarianInfluenceMapGenerator {

        #region instance fields and properties

        private BarbarianInfluenceMaps Maps = new BarbarianInfluenceMaps();




        private IHexGrid                                      Grid;
        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IUnitStrengthEstimator                        UnitStrengthEstimator;
        private IPlayerConfig                                 PlayerConfig;

        #endregion

        #region constructors

        [Inject]
        public BarbarianInfluenceMapGenerator(
            IHexGrid grid, IUnitFactory unitFactory, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IUnitStrengthEstimator unitStrengthEstimator, IPlayerConfig playerConfig
        ) {
            Grid                  = grid;
            UnitFactory           = unitFactory;
            UnitPossessionCanon   = unitPossessionCanon;
            UnitPositionCanon     = unitPositionCanon;
            UnitStrengthEstimator = unitStrengthEstimator;
            PlayerConfig          = playerConfig; 
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
                    ApplyStrengthToMap(
                        unitStrength, Maps.AllyPresence, unitLocation,
                        PlayerConfig.UnitMaxInfluenceRadius
                    );
                }else {
                    ApplyStrengthToMap(
                        unitStrength, Maps.EnemyPresence, unitLocation,
                        PlayerConfig.UnitMaxInfluenceRadius
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

        //Exhibits exponential rolloff (1/2 strength, 1/4 strength, 1/8 strength, etc)
        private void ApplyStrengthToMap(float strength, float[] map, IHexCell center, int maxDistance) {
            map[center.Index] += strength;

            for(int i = 1; i <= maxDistance; i++) {
                foreach(var cellInRing in Grid.GetCellsInRing(center, i)) {
                    float effectiveStrength = strength / Mathf.Pow(2, i);

                    map[cellInRing.Index] += effectiveStrength;
                }
            }
        }

        #endregion

    }

}
