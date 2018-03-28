using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units {

    public class GameUnit : MonoBehaviour, IUnit, IPointerClickHandler, IBeginDragHandler,
        IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        #region from IUnit

        public string Name {
            get { return Template.name; }
        }

        public float MaxMovement {
            get { return Template.MaxMovement; }
        }

        public float CurrentMovement { get; set; }

        public UnitType Type {
            get { return Template.Type; }
        }

        public bool IsAquatic {
            get { return Template.IsAquatic; }
        }

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return Template.Abilities; }
        }

        public int AttackRange {
            get { return Template.AttackRange; }
        }

        public int CombatStrength {
            get { return Template.CombatStrength; }
        }

        public int RangedAttackStrength {
            get { return Template.RangedAttackStrength; }
        }

        public int Hitpoints {
            get { return _hitpoints; }
            set {
                _hitpoints = value.Clamp(0, MaxHitpoints);
            }
        }
        private int _hitpoints;

        public int MaxHitpoints {
            get { return Template.MaxHitpoints; }
        }

        public List<IHexCell> CurrentPath { get; set; }

        public int VisionRange {
            get { return Template.VisionRange; }
        }

        public bool HasAttacked { get; set; }

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return Template.RequiredResources; }
        }

        public IUnitTemplate Template { get; set; }

        public Animator Animator {
            get {
                if(_stateMachine == null) {
                    _stateMachine = GetComponent<Animator>();
                }
                return _stateMachine;
            }
        }
        private Animator _stateMachine;

        public bool PermittedToBombard {
            get {
                if(Template.MustSetUpToBombard) {
                    var currentStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                    return currentStateInfo.IsName("Set Up To Bombard");
                }else {
                    var currentStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                    return currentStateInfo.IsName("Idling");
                }
            }
        }

        #endregion

        private IUnitConfig Config;

        private UnitSignals Signals;

        private IUnitTerrainCostLogic TerrainCostLogic;

        private IUnitPositionCanon PositionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IUnitConfig config, UnitSignals signals,
            IUnitTerrainCostLogic terrainCostLogic, IUnitPositionCanon positionCanon
        ){
            Config           = config;
            Signals          = signals;
            TerrainCostLogic = terrainCostLogic;
            PositionCanon    = positionCanon;
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.UnitBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            Signals.ClickedSignal.OnNext(this);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Signals.PointerEnteredSignal.OnNext(this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            Signals.PointerExitedSignal.OnNext(this);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            Signals.BeginDragSignal.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        public void OnDrag(PointerEventData eventData) {
            Signals.DragSignal.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        public void OnEndDrag(PointerEventData eventData) {
            Signals.EndDragSignal.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        #endregion

        #region from IUnit

        public void PerformMovement() {
            if(Animator != null) {
                Animator.SetTrigger("Moving Requested");
            }

            IHexCell currentTile = PositionCanon.GetOwnerOfPossession(this);
            IHexCell tileToTravelTo = null;

            while(CurrentMovement > 0 && CurrentPath != null && CurrentPath.Count > 0) {
                var nextTile = CurrentPath.First();
                if(!PositionCanon.CanChangeOwnerOfPossession(this, nextTile)) {
                    CurrentPath.Clear();
                    break;
                }

                tileToTravelTo = nextTile;
                CurrentPath.RemoveAt(0);

                CurrentMovement = Math.Max(0,
                    CurrentMovement - TerrainCostLogic.GetTraversalCostForUnit(this, currentTile, tileToTravelTo)
                );
                currentTile = tileToTravelTo;
            }

            if(tileToTravelTo != null) {
                PositionCanon.ChangeOwnerOfPossession(this, tileToTravelTo);
            }

            if(Animator != null) {
                Animator.SetTrigger("Idling Requested");
            }
        }

        #endregion

        #endregion

    }

}
