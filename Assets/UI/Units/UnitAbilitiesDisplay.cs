using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;

namespace Assets.UI.Units {

    public class UnitAbilitiesDisplay : UnitDisplayBase {

        #region instance fields and properties

        [SerializeField] private Button RangedAttackButton;

        private List<IAbilityDisplay> ActiveAbilityDisplays = new List<IAbilityDisplay>();



        private AbilityDisplayMemoryPool                      AbilityDisplayPool;
        private ITechCanon                                    TechCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IAbilityExecuter                              AbilityExecuter;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            AbilityDisplayMemoryPool abilityDisplayPool, ITechCanon techCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IAbilityExecuter abilityExecuter, UnitSignals unitSignals
        ){
            AbilityDisplayPool  = abilityDisplayPool;
            TechCanon           = techCanon;
            UnitPossessionCanon = unitPossessionCanon;
            AbilityExecuter     = abilityExecuter;


            unitSignals.StoppedMovingSignal      .Subscribe(CheckForRefreshCondition);
            unitSignals.SetUpForBombardmentSignal.Subscribe(CheckForRefreshCondition);
        }

        #region from UnitDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            foreach(var display in ActiveAbilityDisplays) {
                AbilityDisplayPool.Despawn(display);
            }
            ActiveAbilityDisplays.Clear();

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(ObjectToDisplay);

            foreach(var ability in ObjectToDisplay.Abilities) {
                if( !TechCanon.IsAbilityResearchedForCiv(ability, unitOwner) ||
                    !AbilityExecuter.CanExecuteAbilityOnUnit(ability, ObjectToDisplay)
                ) {
                    continue;
                }

                var cachedAbility = ability;

                var newDisplay = AbilityDisplayPool.Spawn();

                newDisplay.transform.SetParent(transform, false);
                newDisplay.UnitToInvokeOn = ObjectToDisplay;
                newDisplay.AbilityToDisplay = cachedAbility;
                newDisplay.Refresh();

                newDisplay.ExecuteButton.onClick.AddListener(() => this.Refresh());

                ActiveAbilityDisplays.Add(newDisplay);
            }

            RangedAttackButton.gameObject.SetActive(
                ObjectToDisplay.CanAttack && ObjectToDisplay.CurrentMovement > 0 &&
                ObjectToDisplay.PreparedForRangedAttack
            );
        }

        #endregion

        private void CheckForRefreshCondition(IUnit unit) {
            if(unit == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
