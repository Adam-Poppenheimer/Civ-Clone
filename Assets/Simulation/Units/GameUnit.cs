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

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units {

    public class GameUnit : MonoBehaviour, IUnit, IPointerClickHandler, IBeginDragHandler,
        IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        #region from IUnit

        public string Name {
            get { return Template.Name; }
        }

        public int MaxMovement {
            get { return Template.MaxMovement; }
        }

        public int CurrentMovement { get; set; }

        public UnitType Type {
            get { return Template.Type; }
        }

        public bool IsAquatic {
            get { return Type == UnitType.WaterMilitary || Type == UnitType.WaterCivilian; }
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

        public int Health {
            get { return _health; }
            set {
                _health = value.Clamp(0, Config.MaxHealth);
            }
        }
        private int _health;

        public int MaxHealth {
            get { return Config.MaxHealth; }
        }

        public List<IHexCell> CurrentPath { get; set; }

        public int VisionRange {
            get { return Config.VisionRange; }
        }

        #endregion

        public IUnitTemplate Template { get; set; }



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

            Health = Config.MaxHealth;
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
        }

        #endregion

        #endregion

    }

}
