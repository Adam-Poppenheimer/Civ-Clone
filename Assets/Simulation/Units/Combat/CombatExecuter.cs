﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.Units.Combat {

    public class CombatExecuter : ICombatExecuter {

        #region instance fields and properties

        private IUnitPositionCanon                            UnitPositionCanon;
        private IHexGrid                                      Grid;
        private ILineOfSightLogic                             LineOfSightLogic;
        private ICombatModifierLogic                          CombatModifierLogic;
        private IUnitConfig                                   UnitConfig;
        private UnitSignals                                   UnitSignals;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IWarCanon                                     WarCanon;

        #endregion

        #region constructors

        [Inject]
        public CombatExecuter(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, ILineOfSightLogic lineOfSightLogic,
            ICombatModifierLogic combatModifierLogic, IUnitConfig unitConfig, UnitSignals unitSignals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon, IWarCanon warCanon
        ){
            UnitPositionCanon   = unitPositionCanon;
            Grid                = grid;
            LineOfSightLogic    = lineOfSightLogic;
            CombatModifierLogic = combatModifierLogic;
            UnitConfig          = unitConfig;
            UnitSignals         = unitSignals;
            UnitPossessionCanon = unitPossessionCanon;
            WarCanon            = warCanon;
        }

        #endregion

        #region instance methods

        #region from ICombatExecuter

        public bool CanPerformMeleeAttack(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            if(attacker == defender) {
                return false;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);
            var defenderOwner = UnitPossessionCanon.GetOwnerOfPossession(defender);

            if(attackerOwner == defenderOwner) {
                return false;
            }

            if(!WarCanon.AreAtWar(attackerOwner, defenderOwner)) {
                return false;
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > 1) {
                return false;
            }

            if(HexMetrics.GetEdgeType(attackerLocation, defenderLocation) == HexEdgeType.Cliff) {
                return false;
            }

            if(!UnitPositionCanon.CanPlaceUnitAtLocation(attacker, defenderLocation, true)) {
                return false;
            }

            if(attacker.CurrentMovement <= 0 || attacker.HasAttacked) {
                return false;
            }

            if(!LineOfSightLogic.CanUnitSeeCell(attacker, defenderLocation)) {
                return false;
            }

            if(attacker.CombatStrength <= 0) {
                return false;
            }

            return true;
        }

        public bool CanPerformMeleeAttack(IUnit attacker, ICity city) {
            return CanPerformMeleeAttack(attacker, city.CombatFacade);
        }

        public bool CanPerformRangedAttack(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            if(attacker == defender) {
                return false;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);
            var defenderOwner = UnitPossessionCanon.GetOwnerOfPossession(defender);

            if(attackerOwner == defenderOwner) {
                return false;
            }

            if(!WarCanon.AreAtWar(attackerOwner, defenderOwner)) {
                return false;
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > attacker.AttackRange) {
                return false;
            }

            if(attacker.CurrentMovement <= 0 || attacker.HasAttacked) {
                return false;
            }

            if(!LineOfSightLogic.CanUnitSeeCell(attacker, defenderLocation)) {
                return false;
            }

            if(attacker.RangedAttackStrength <= 0) {
                return false;
            }

            return true;
        }

        public bool CanPerformRangedAttack(IUnit attacker, ICity city) {
            return CanPerformRangedAttack(attacker, city.CombatFacade);
        }

        public void PerformMeleeAttack(IUnit attacker, IUnit defender) {
            int attackerStartingHealth = attacker.Hitpoints;
            int defenderStartingHealth = defender.Hitpoints;

            PerformMeleeAttack_NoEvent(attacker, defender);

            UnitSignals.MeleeCombatWithUnitSignal.OnNext(
                new UnitUnitCombatData(
                    attacker, defender,
                    attackerStartingHealth - attacker.Hitpoints,
                    defenderStartingHealth - defender.Hitpoints
                )
            );
        }

        public void PerformMeleeAttack(IUnit attacker, ICity city) {
            int attackerStartingHealth = attacker.Hitpoints;
            int defenderStartingHealth = city.CombatFacade.Hitpoints;

            PerformMeleeAttack_NoEvent(attacker, city.CombatFacade);

            UnitSignals.MeleeCombatWithCitySignal.OnNext(
                new UnitCityCombatData(
                    attacker, city,
                    attackerStartingHealth - attacker.Hitpoints,
                    defenderStartingHealth - city.CombatFacade.Hitpoints
                )
            );
        }

        private void PerformMeleeAttack_NoEvent(IUnit attacker, IUnit defender) {
            if(!CanPerformMeleeAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformMeleeCombat must return true");
            }

            float attackerStrength, defenderStrength;

            CalculateMeleeAttackStrengths(attacker, out attackerStrength, defender, out defenderStrength);

            PerformCombat(attacker, attackerStrength, defender, defenderStrength, true);
        }

        public void PerformRangedAttack(IUnit attacker, IUnit defender) {
            int attackerStartingHealth = attacker.Hitpoints;
            int defenderStartingHealth = defender.Hitpoints;

            PerformRangedAttack_NoEvent(attacker, defender);

            UnitSignals.RangedCombatWithUnitSignal.OnNext(
                new UnitUnitCombatData(
                    attacker, defender,
                    attackerStartingHealth - attacker.Hitpoints,
                    defenderStartingHealth - defender.Hitpoints
                )
            );
        }

        public void PerformRangedAttack(IUnit attacker, ICity city) {
            int attackerStartingHealth = attacker.Hitpoints;
            int defenderStartingHealth = city.CombatFacade.Hitpoints;

            PerformRangedAttack_NoEvent(attacker, city.CombatFacade);

            UnitSignals.RangedCombatWithCitySignal.OnNext(
                new UnitCityCombatData(
                    attacker, city,
                    attackerStartingHealth - attacker.Hitpoints,
                    defenderStartingHealth - city.CombatFacade.Hitpoints
                )
            );
        }

        private void PerformRangedAttack_NoEvent(IUnit attacker, IUnit defender) {
            if(!CanPerformRangedAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformRangedCombat must return true");
            }

            float attackerStrength, defenderStrength;
            CalculateRangedAttackStrengths(attacker, out attackerStrength, defender, out defenderStrength);

            PerformCombat(attacker, attackerStrength, defender, defenderStrength, false);
        }

        public UnitUnitCombatData EstimateMeleeAttackResults(IUnit attacker, IUnit defender) {
            float attackerStrength, defenderStrength;
            CalculateMeleeAttackStrengths(attacker, out attackerStrength, defender, out defenderStrength);

            Tuple<int, int> results = CalculateCombat(attacker, attackerStrength, defender, defenderStrength, true);

            return new UnitUnitCombatData(attacker, defender, results.Item1, results.Item2);
        }

        public UnitCityCombatData EstimateMeleeAttackResults(IUnit attacker, ICity city) {
            float attackerStrength, defenderStrength;
            CalculateMeleeAttackStrengths(attacker, out attackerStrength, city.CombatFacade, out defenderStrength);

            Tuple<int, int> results = CalculateCombat(attacker, attackerStrength, city.CombatFacade, defenderStrength, true);

            return new UnitCityCombatData(attacker, city, results.Item1, results.Item2);
        }

        public UnitUnitCombatData EstimateRangedAttackResults(IUnit attacker, IUnit defender) {
            float attackerStrength, defenderStrength;
            CalculateRangedAttackStrengths(attacker, out attackerStrength, defender, out defenderStrength);

            Tuple<int, int> results = CalculateCombat(attacker, attackerStrength, defender, defenderStrength, false);

            return new UnitUnitCombatData(attacker, defender, results.Item1, results.Item2);
        }

        public UnitCityCombatData EstimateRangedAttackResults(IUnit attacker, ICity city) {
            float attackerStrength, defenderStrength;
            CalculateRangedAttackStrengths(attacker, out attackerStrength, city.CombatFacade, out defenderStrength);

            Tuple<int, int> results = CalculateCombat(attacker, attackerStrength, city.CombatFacade, defenderStrength, false);

            return new UnitCityCombatData(attacker, city, results.Item1, results.Item2);
        }

        #endregion

        private void CalculateRangedAttackStrengths(
            IUnit attacker, out float attackerStrength,
            IUnit defender, out float defenderStrength
        ){
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var attackerModifier = CombatModifierLogic.GetRangedOffensiveModifierAtLocation(attacker, defender, defenderLocation);
            var defenderModifier = CombatModifierLogic.GetRangedDefensiveModifierAtLocation(attacker, defender, defenderLocation);

            attackerStrength = attacker.RangedAttackStrength * (1f + attackerModifier);
            defenderStrength = defender.CombatStrength       * (1f + defenderModifier);
        }

        private void CalculateMeleeAttackStrengths(
            IUnit attacker, out float attackerStrength,
            IUnit defender, out float defenderStrength
        ) {
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var attackerModifier = CombatModifierLogic.GetMeleeOffensiveModifierAtLocation(attacker, defender, defenderLocation);
            var defenderModifier = CombatModifierLogic.GetMeleeDefensiveModifierAtLocation(attacker, defender, defenderLocation);

            attackerStrength = attacker.CombatStrength * (1f + attackerModifier);
            defenderStrength = defender.CombatStrength * (1f + defenderModifier);
        }

        private void PerformCombat(
            IUnit attacker, float attackerStrength, IUnit defender, float defenderStrength,
            bool attackerReceivesDamage
        ) {
            if(attacker.Template.CanMoveAfterAttacking) {
                attacker.CurrentMovement--;
            }else {
                attacker.CurrentMovement = 0;
            }

            Tuple<int, int> results = CalculateCombat(attacker, attackerStrength, defender, defenderStrength, attackerReceivesDamage);

            attacker.Hitpoints -= results.Item1;
            defender.Hitpoints -= results.Item2;

            attacker.HasAttacked = true;
        }

        private Tuple<int, int> CalculateCombat(
            IUnit attacker, float attackerStrength, IUnit defender, float defenderStrength,
            bool attackerReceivesDamage
        ){
            int attackerDamage = 0, defenderDamage = 0;

            if(attackerStrength == 0) {
                defenderDamage = attacker.Hitpoints;

            }else if(defenderStrength == 0) {
                attackerDamage = defender.Hitpoints;

            }else {
                float attackerDefenderRatio = attackerStrength / defenderStrength;
                float defenderAttackerRatio = defenderStrength / attackerStrength;

                attackerDamage = Mathf.RoundToInt(
                    attackerDefenderRatio * UnitConfig.CombatBaseDamage
                );

                defenderDamage = Mathf.RoundToInt(
                    defenderAttackerRatio * UnitConfig.CombatBaseDamage
                );
            }

            return new Tuple<int, int>(
                attackerReceivesDamage ? Math.Min(defenderDamage, attacker.Hitpoints) : 0,
                Math.Min(attackerDamage, defender.Hitpoints)
            );
        }

        #endregion

    }

}
