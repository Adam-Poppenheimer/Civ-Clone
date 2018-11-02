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
using Assets.Simulation.MapResources;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Units.Combat;

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
            get { return Template.MaxMovement + MovementSummary.BonusMovement; }
        }

        public float CurrentMovement { get; set; }

        public UnitType Type {
            get { return Template.Type; }
        }

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return Template.Abilities; }
        }

        public int AttackRange {
            get { return Template.AttackRange + CombatSummary.BonusRange; }
        }

        public int CombatStrength {
            get { return Template.CombatStrength; }
        }

        public int RangedAttackStrength {
            get { return Template.RangedAttackStrength; }
        }

        public int CurrentHitpoints {
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
            get { return Template.VisionRange + MovementSummary.BonusVision; }
        }

        public bool CanAttack { get; set; }

        public bool IsReadyForRangedAttack {
            get {
                if(Template.MustSetUpToBombard) {
                    return IsSetUpToBombard;
                }else {
                    return true;
                }
            }
        }

        public IEnumerable<IResourceDefinition> RequiredResources {
            get { return Template.RequiredResources; }
        }

        public IUnitTemplate Template { get; set; }

        public bool IsSetUpToBombard {
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

        public bool IsIdling {
            get {
                var currentStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                return currentStateInfo.IsName("Idling");
            }
        }

        public bool IsFortified {
            get {
                var currentStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                return currentStateInfo.IsName("Fortified");
            }
        }

        public IEnumerable<IPromotion> Promotions {
            get { return Template.StartingPromotions.Concat(PromotionTree.GetChosenPromotions()); }
        }

        public IPromotionTree PromotionTree {
            get { return _promotionTree; }
            set {
                if(_promotionTree != null) {
                    _promotionTree.NewPromotionChosen -= OnNewPromotionChosen;
                }

                _promotionTree = value;

                if(_promotionTree != null) {
                    _promotionTree.NewPromotionChosen += OnNewPromotionChosen;
                }
            }
        }
        private IPromotionTree _promotionTree;

        public int Experience {
            get { return _experience; }
            set {
                if(_experience != value) {
                    _experience = value;
                    Signals.ExperienceChangedSignal.OnNext(this);
                }
            }
        }
        private int _experience;

        public int Level {
            get { return _level; }
            set {
                if(_level != value) {
                    _level = value;
                    Signals.LevelChangedSignal.OnNext(this);
                }
            }
        }
        private int _level;

        public bool IsWounded {
            get { return MaxHitpoints - CurrentHitpoints >= Config.WoundedThreshold; }
        }

        public IUnitMovementSummary MovementSummary {
            get { return ConcreteMovementSummary; }
        }
        private UnitMovementSummary ConcreteMovementSummary;

        public IUnitCombatSummary CombatSummary {
            get { return ConcreteCombatSummary; }
        }
        private UnitCombatSummary ConcreteCombatSummary;

        #endregion

        [SerializeField] private Animator Animator;

        private Coroutine RelocationCoroutine;



        private IUnitConfig           Config;
        private UnitSignals           Signals;
        private IUnitPositionCanon    PositionCanon;
        private IHexGrid              Grid;
        private IPromotionParser      PromotionParser;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IUnitConfig config, UnitSignals signals, IUnitPositionCanon positionCanon,
            IHexGrid grid, IPromotionParser promotionParser
        ){
            Config          = config;
            Signals         = signals;
            PositionCanon   = positionCanon;
            Grid            = grid;
            PromotionParser = promotionParser;
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
            PerformMovement(false);
        }

        public void PerformMovement(bool ignoreMoveCosts) {
            bool shouldExecuteMovement = (ignoreMoveCosts || CurrentMovement > 0) && CurrentPath != null && CurrentPath.Count > 0;

            if(shouldExecuteMovement) {
                StopAllCoroutines();
                StartCoroutine(PerformMovementCoroutine(ignoreMoveCosts));
            }
        }

        public bool CanRelocate(IHexCell newLocation) {
            if(newLocation == null) {
                throw new ArgumentNullException("newLocation");
            }

            return PositionCanon.CanChangeOwnerOfPossession(this, newLocation);
        }

        public void Relocate(IHexCell newLocation) {
            if(!CanRelocate(newLocation)) {
                throw new InvalidOperationException("CanRelocate must return true on the given arguments");
            }

            if(CurrentPath != null) {
                CurrentPath.Clear();
            }
            
            PositionCanon.ChangeOwnerOfPossession(this, newLocation);

            if(RelocationCoroutine != null) {
                StopCoroutine(RelocationCoroutine);
            }

            RelocationCoroutine = StartCoroutine(PlaceUnitOnGridCoroutine(newLocation.AbsolutePosition));            
        }

        private IEnumerator PlaceUnitOnGridCoroutine(Vector3 xzPosition) {
            yield return new WaitForEndOfFrame();

            transform.position = Grid.PerformIntersectionWithTerrainSurface(xzPosition);
        }

        public void SetUpToBombard() {
            if(!IsSetUpToBombard) {
                Animator.SetTrigger("Set Up To Bombard Requested");
            }
        }

        public void LockIntoConstruction() {
            if(!LockedIntoConstruction) {
                Animator.SetTrigger("Locked Into Construction Requested");
            }
        }

        public void BeginIdling() {
            if(!IsIdling) {
                Animator.SetTrigger("Idling Requested");
            }
        }

        public void BeginFortifying() {
            if(!IsFortified) {
                Animator.SetTrigger("Fortified Requested");
            }
        }

        public void Destroy() {
            if(Application.isPlaying) {
                Destroy(gameObject);
            }else {
                DestroyImmediate(gameObject);
            }
        }

        #endregion

        public void SetSummaries(UnitMovementSummary movementSummary, UnitCombatSummary combatSummary) {
            ConcreteMovementSummary = movementSummary;
            ConcreteCombatSummary   = combatSummary;

            PromotionParser.SetMovementSummary(ConcreteMovementSummary, Promotions);
            PromotionParser.SetCombatSummary  (ConcreteCombatSummary,   Promotions);
        }

        private IEnumerator PerformMovementCoroutine(bool ignoreMoveCosts) {
            Animator.SetTrigger("Moving Requested");

            IHexCell startingCell = PositionCanon.GetOwnerOfPossession(this);
            IHexCell currentCell = startingCell;

            Vector3 a, b, c = currentCell.UnitAnchorPoint;
            yield return LookAt(CurrentPath.First().UnitAnchorPoint);

            float t = Time.deltaTime * Config.TravelSpeedPerSecond;

            while((ignoreMoveCosts || CurrentMovement > 0) && CurrentPath != null && CurrentPath.Count > 0) {
                var nextCell = CurrentPath.FirstOrDefault();
                if(!PositionCanon.CanChangeOwnerOfPossession(this, nextCell) || nextCell == null) {
                    CurrentPath.Clear();
                    break;
                }

                if(!ignoreMoveCosts) {
                    CurrentMovement = Math.Max(0,
                        CurrentMovement - PositionCanon.GetTraversalCostForUnit(this, currentCell, nextCell, false)
                    );
                }                

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

        private void OnNewPromotionChosen(object sender, EventArgs e) {
            PromotionParser.SetMovementSummary(ConcreteMovementSummary, Promotions);
            PromotionParser.SetCombatSummary  (ConcreteCombatSummary,   Promotions);

            Signals.UnitGainedPromotionSignal.OnNext(this);
        }

        #endregion

    }

}
