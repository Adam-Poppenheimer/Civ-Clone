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
using Assets.Simulation.Civilizations;

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
                int newHitpoints = value.Clamp(0, MaxHitpoints);

                if(newHitpoints != _hitpoints) {
                    _hitpoints = newHitpoints;

                    Signals.HitpointsChanged.OnNext(this);
                }                
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

        public bool IsMoving {
            get {
                var currentStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                return currentStateInfo.IsName("Moving");
            }
        }

        public IPromotionTree PromotionTree {
            get { return _promotionTree; }
            set {
                if(_promotionTree != null) {
                    _promotionTree.PromotionsChanged -= OnNewPromotionChosen;
                }

                _promotionTree = value;

                if(_promotionTree != null) {
                    _promotionTree.PromotionsChanged += OnNewPromotionChosen;
                }
            }
        }
        private IPromotionTree _promotionTree;

        public int Experience {
            get { return _experience; }
            set {
                if(_experience != value) {
                    int experienceGained = value - _experience;

                    _experience = value;
                    Signals.ExperienceChanged.OnNext(this);
                    if(experienceGained > 0) {
                        Signals.GainedExperience.OnNext(new UniRx.Tuple<IUnit, int>(this, experienceGained));
                    }
                }
            }
        }
        private int _experience;

        public int Level {
            get { return _level; }
            set {
                if(_level != value) {
                    _level = value;
                    Signals.LevelChanged.OnNext(this);
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

        private UnitMovementSummary ConcreteMovementSummary {
            get {
                if(_concreteMovementSummary == null) {
                    _concreteMovementSummary = new UnitMovementSummary();

                    PromotionParser.SetMovementSummary(_concreteMovementSummary, this);
                }

                return _concreteMovementSummary;
            }
        }
        private UnitMovementSummary _concreteMovementSummary;

        public IUnitCombatSummary CombatSummary {
            get { return ConcreteCombatSummary; }
        }

        private UnitCombatSummary ConcreteCombatSummary {
            get {
                if(_concreteCombatSummary == null) {
                    _concreteCombatSummary = new UnitCombatSummary();

                    PromotionParser.SetCombatSummary(_concreteCombatSummary, this);
                }

                return _concreteCombatSummary;
            }
        }
        private UnitCombatSummary _concreteCombatSummary;

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

            signals.GainedNewOwner.Subscribe(OnUnitGainedNewOwner);
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.BeingDestroyed.OnNext(this);
        }

        #endregion

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            Signals.Clicked.OnNext(this);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Signals.PointerEntered.OnNext(this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            Signals.PointerExited.OnNext(this);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            Signals.BeginDrag.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        public void OnDrag(PointerEventData eventData) {
            Signals.Drag.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        public void OnEndDrag(PointerEventData eventData) {
            Signals.EndDrag.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        #endregion

        #region from IUnit

        public void PerformMovement() {
            PerformMovement(false, () => { });
        }

        public void PerformMovement(bool ignoreMoveCosts) {
            PerformMovement(ignoreMoveCosts, () => { });
        }

        public void PerformMovement(bool ignoreMoveCosts, Action postMovementCallback) {
            bool shouldExecuteMovement = (ignoreMoveCosts || CurrentMovement > 0) && CurrentPath != null && CurrentPath.Count > 0;

            if(shouldExecuteMovement) {
                StopAllCoroutines();
                StartCoroutine(PerformMovementCoroutine(ignoreMoveCosts, postMovementCallback));
            }else {
                postMovementCallback();
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

        private IEnumerator PerformMovementCoroutine(bool ignoreMoveCosts, Action postMovementCallback) {
            Animator.SetTrigger("Moving Requested");

            IHexCell startingCell = PositionCanon.GetOwnerOfPossession(this);
            IHexCell currentCell = startingCell;

            Vector3 a, b, c = currentCell.AbsolutePosition;
            yield return LookAt(CurrentPath.First().AbsolutePosition);

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
                b = currentCell.AbsolutePosition;
                c = (b + nextCell.AbsolutePosition) * 0.5f;

                for(; t < 1f; t += Time.deltaTime * Config.TravelSpeedPerSecond) {
                    transform.position = Bezier.GetPoint(a, b, c, t);
                    Vector3 d = Bezier.GetFirstDerivative(a, b, c, t);
                    d.y = 0f;
                    transform.localRotation = Quaternion.LookRotation(d);
                    yield return null;
                }
                t -= 1f;

                currentCell = nextCell;
            }

            if(currentCell != startingCell) {
                a = c;
                b = currentCell.AbsolutePosition;
                c = b;

                for(; t < 1f; t += Time.deltaTime * Config.TravelSpeedPerSecond) {
                    transform.position = Bezier.GetPoint(a, b, c, t);
                    Vector3 d = Bezier.GetFirstDerivative(a, b, c, t);
                    d.y = 0f;
                    transform.localRotation = Quaternion.LookRotation(d);
                    yield return null;
                }
            }

            postMovementCallback();

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
            PromotionParser.SetMovementSummary(ConcreteMovementSummary, this);
            PromotionParser.SetCombatSummary  (ConcreteCombatSummary,   this);

            Signals.GainedPromotion.OnNext(this);
        }

        private void OnUnitGainedNewOwner(UniRx.Tuple<IUnit, ICivilization> data) {
            var unit     = data.Item1;
            var newOwner = data.Item2;

            if((this as IUnit) == unit) {
                var meshRenderer = GetComponentInChildren<MeshRenderer>();
                if(meshRenderer != null && newOwner != null) {
                    meshRenderer.material.color = newOwner.Template.Color;
                }

                PromotionParser.SetMovementSummary(ConcreteMovementSummary, this);
                PromotionParser.SetCombatSummary  (ConcreteCombatSummary,   this);
            }
        }

        #endregion

    }

}
