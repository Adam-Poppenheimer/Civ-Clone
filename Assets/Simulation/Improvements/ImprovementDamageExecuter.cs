using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.Improvements {

    public class ImprovementDamageExecuter : IImprovementDamageExecuter {

        #region instance fields and properties
        
        private IImprovementLocationCanon                       ImprovementLocationCanon;
        private IWarCanon                                       WarCanon;
        private IPossessionRelationship<ICivilization, IUnit>   UnitPossessionCanon;
        private ICivilizationTerritoryLogic                     CivTerritoryLogic;
        private IUnitFactory                                    UnitFactory;
        private IUnitPositionCanon                              UnitPositionCanon;
        private IHexGrid                                        Grid;

        #endregion

        #region constructors

        [Inject]
        public ImprovementDamageExecuter(
            IImprovementLocationCanon improvementLocationCanon, IHexGrid grid,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ICivilizationTerritoryLogic civTerritoryLogic, IWarCanon warCanon,
            IUnitPositionCanon unitPositionCanon
        ) {
            ImprovementLocationCanon = improvementLocationCanon;
            Grid                     = grid;
            UnitPossessionCanon      = unitPossessionCanon;
            CivTerritoryLogic        = civTerritoryLogic;
            WarCanon                 = warCanon;
            UnitPositionCanon        = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementDamageExecuter

        public void PerformDamageOnUnitFromImprovements(IUnit unit) {
            var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            float highestDamage = 0f;

            foreach(var nearbyCell in Grid.GetCellsInRadius(unitLocation, 1)) {
                var ownerOfCell = CivTerritoryLogic.GetCivClaimingCell(nearbyCell);

                if(!WarCanon.AreAtWar(unitOwner, ownerOfCell)) {
                    continue;
                }

                foreach(var improvement in ImprovementLocationCanon.GetPossessionsOfOwner(nearbyCell)) {
                    highestDamage = Mathf.Max(highestDamage, improvement.Template.AdjacentEnemyDamagePercentage);
                }
            }

            if(highestDamage > 0f) {
                unit.CurrentHitpoints -= Mathf.RoundToInt(unit.MaxHitpoints * highestDamage);
            }
        }

        #endregion

        #endregion
        
    }

}
