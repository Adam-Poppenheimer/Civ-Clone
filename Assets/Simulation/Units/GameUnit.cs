using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Util;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Units.Promotions;

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

        public bool CanAttack { get; set; }

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return Template.RequiredResources; }
        }

        public IUnitTemplate Template { get; set; }

        public bool IsPermittedToBombard {
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

        public bool LockedIntoConstruction {
            get {
                var currentStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                return currentStateInfo.IsName("Locked Into Construction");
            }
        }

        public IEnumerable<IPromotion> Promotions {
            get { return Template.StartingPromotions; }
        }

        #endregion

        [SerializeField] private Animator Animator;



        private IUnitConfig           Config;
        private UnitSignals           Signals;
        private IUnitTerrainCostLogic TerrainCostLogic;
        private IUnitPositionCanon    PositionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IUnitConfig config, UnitSignals signals,  IUnitTerrainCostLogic terrainCostLogic,
            IUnitPositionCanon positionCanon
        ){
            Config                   = config;
            Signals                  = signals;
            TerrainCostLogic         = terrainCostLogic;
            PositionCanon            = positionCanon;
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
            bool shouldExecuteMovement = CurrentMovement > 0 && CurrentPath != null && CurrentPath.Count > 0;

            if(shouldExecuteMovement) {
                StopAllCoroutines();
                StartCoroutine(PerformMovementCoroutine());
            }
        }

        private IEnumerator PerformMovementCoroutine() {
            Animator.SetTrigger("Moving Requested");

            IHexCell startingCell = PositionCanon.GetOwnerOfPossession(this);
            IHexCell currentCell = startingCell;

            Vector3 a, b, c = currentCell.UnitAnchorPoint;
            yield return LookAt(CurrentPath.First().UnitAnchorPoint);

            float t = Time.deltaTime * Config.TravelSpeedPerSecond;

            while(CurrentMovement > 0 && CurrentPath != null && CurrentPath.Count > 0) {
                var nextCell = CurrentPath.FirstOrDefault();
                if(!PositionCanon.CanChangeOwnerOfPossession(this, nextCell) || nextCell == null) {
                    CurrentPath.Clear();
                    break;
                }

                CurrentMovement = Math.Max(0,
                    CurrentMovement - TerrainCostLogic.GetTraversalCostForUnit(this, currentCell, nextCell)
                );

                PositionCanon.ChangeOwnerOfPossession(this, nextCell);

                CurrentPath.RemoveAt(0);

                a = c;
                b = currentCell.UnitAnchorPoint;
                c = (b + nextCell.UnitAnchorPoint) * 0.5f;

                for(; t < 1f; t += Time.deltaTime * Config.TravelSpeedPerSecond) {
                    transform.position = Bezier.GetPoint(a, b, c, t);
                    Vector3 d = Bezier.GetDerivative(a, b, c, t);
                    d.y = 0f;
                    transform.localRotation = Quaternion.LookRotation(d);
                    yield return null;
                }
                t -= 1f;

                currentCell = nextCell;
            }

            if(currentCell != startingCell) {
                a = c;
                b = currentCell.UnitAnchorPoint;
                c = b;

                for(; t < 1f; t += Time.deltaTime * Config.TravelSpeedPerSecond) {
                    transform.position = Bezier.GetPoint(a, b, c, t);
                    Vector3 d = Bezier.GetDerivative(a, b, c, t);
                    d.y = 0f;
                    transform.localRotation = Quaternion.LookRotation(d);
                    yield return null;
                }
            }

            Animator.SetTrigger("Idling Requested");
        }

        public void SetUpToBombard() {
            if(!IsPermittedToBombard) {
                Animator.SetTrigger("Set Up To Bombard Requested");
            }
        }

        public void LockIntoConstruction() {
            if(!LockedIntoConstruction) {
                Animator.SetTrigger("Locked Into Construction Requested");
            }
        }

        private IEnumerator LookAt(Vector3 point) {
            point.y = transform.localPosition.y;
            Quaternion fromRotation = transform.localRotation;
            Quaternion toRotation = Quaternion.LookRotation(point - transform.position);

            float angle = Quaternion.Angle(fromRotation, toRotation);

            if(angle > 0f) {
                float speed = Config.RotationSpeedPerSecond / angle;

                for(float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed) {
                    transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                    yield return null;
                }
            }
        }

        #endregion

        #endregion

    }

}
