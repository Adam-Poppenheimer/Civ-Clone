using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Visibility;
using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public class BarbarianSpawningTools : IBarbarianSpawningTools {

        #region instance fields and properties

        #region from IBarbarianSpawningTools

        public Func<IHexCell, bool> EncampmentValidityFilter {
            get { return IsCellValidForEncampment; }
        }

        #endregion

        private ICivilizationFactory     CivFactory;
        private IBarbarianConfig         BarbarianConfig;
        private IEncampmentFactory       EncampmentFactory;
        private IVisibilityCanon         VisibilityCanon;
        private IEncampmentLocationCanon EncampmentLocationCanon;
        private IHexGrid                 Grid;
        private IRandomizer              Randomizer;

        #endregion

        #region constructors

        [Inject]
        public BarbarianSpawningTools(
            ICivilizationFactory civFactory, IBarbarianConfig barbarianConfig,
            IEncampmentFactory encampmentFactory, IVisibilityCanon visibilityCanon,
            IEncampmentLocationCanon encampmentLocationCanon, IHexGrid grid,
            IRandomizer randomizer
        ) {
            CivFactory              = civFactory;
            BarbarianConfig         = barbarianConfig;
            EncampmentFactory       = encampmentFactory;
            VisibilityCanon         = visibilityCanon;
            EncampmentLocationCanon = encampmentLocationCanon;
            Grid                    = grid;
            Randomizer              = randomizer;
        }

        #endregion

        #region instance methods

        #region from IEncampmentSpawnChanceLogic

        public float GetEncampmentSpawnChance(int encampmentCount, int nonBarbarianCivCount) {
            int minEncampments = BarbarianConfig.MinEncampmentsPerPlayer * nonBarbarianCivCount;
            int maxEncampments = BarbarianConfig.MaxEncampmentsPerPlayer * nonBarbarianCivCount;

            if(encampmentCount < minEncampments) {
                return 1f;

            }else if(encampmentCount < maxEncampments) {
                return (float)(maxEncampments - encampmentCount) / (maxEncampments - minEncampments);

            }else {
                return 0f;
            }
        }

        public bool IsCellValidForEncampment(IHexCell cell) {
            return EncampmentFactory.CanCreateEncampment(cell)
                && !CivFactory.AllCivilizations.Any(civ => !civ.Template.IsBarbaric && VisibilityCanon.IsCellVisibleToCiv(cell, civ));
        }

        public Func<IHexCell, int> BuildEncampmentWeightFunction(InfluenceMaps maps) {
            return delegate(IHexCell cell) {
                float allyWeight  = maps.AllyPresence [cell.Index] * BarbarianConfig.AllyEncampmentSpawnWeight;
                float enemyWeight = maps.EnemyPresence[cell.Index] * BarbarianConfig.EnemyEncampmentSpawnWeight;

                return Math.Max(
                    1, Mathf.RoundToInt(BarbarianConfig.BaseEncampmentSpawnWeight / (1 + allyWeight + enemyWeight))
                );
            };
        }

        public UnitSpawnInfo TryGetValidSpawn(
            IEncampment encampment, Func<IHexCell, IEnumerable<IUnitTemplate>> unitSelector
        ) {
            var encampmentLocation = EncampmentLocationCanon.GetOwnerOfPossession(encampment);

            foreach(var cell in Grid.GetCellsInRadius(encampmentLocation, 1, true)) {
                var availableUnits = unitSelector(cell);

                if(availableUnits.Any()) {
                    return new UnitSpawnInfo() {
                        IsValidSpawn = true, LocationOfUnit = cell,
                        TemplateToBuild = Randomizer.TakeRandom(availableUnits)
                    };
                }
            }

            return new UnitSpawnInfo() {
                IsValidSpawn = false, LocationOfUnit = null, TemplateToBuild = null
            };
        }

        #endregion

        #endregion

    }

}
