using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Visibility;

namespace Assets.UI.Units {

    /*
     * Since the visibility of units and cells doesn't come in precisely
     * the right order, this class often defers icon refresh until the end
     * of the frame. That way, we can be sure that all the visibility
     * calculations have been applied before determining whether an icon
     * should be displayed over a unit or not. 
     */
    public class UnitMapIconManager : MonoBehaviour, IUnitMapIconManager {

        #region instance fields and properties

        [SerializeField] private RectTransform UnitIconContainer;

        private Dictionary<IUnit, UnitMapIcon> IconOfUnit =
            new Dictionary<IUnit, UnitMapIcon>();

        private Dictionary<IHexCell, UnitIconSlot> SlotOfCell =
            new Dictionary<IHexCell, UnitIconSlot>();

        private Dictionary<IHexCell, Coroutine> CellResetRoroutines =
            new Dictionary<IHexCell, Coroutine>();

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private IUnitFactory                                  UnitFactory;
        private IUnitPositionCanon                            UnitPositionCanon;
        private UnitMapIcon.Pool                              IconPool;
        private UnitIconSlot.Pool                             IconSlotPool;
        private IVisibilityCanon                              VisibilityCanon;
        private UnitSignals                                   UnitSignals;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private VisibilitySignals                             VisibilitySignals;
        private IHexMapRenderConfig                           HexMapRenderConfig;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IUnitFactory unitFactory, IUnitPositionCanon unitPositionCanon, UnitMapIcon.Pool iconPool,
            UnitIconSlot.Pool iconSlotPool, IVisibilityCanon visibilityCanon, UnitSignals unitSignals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            VisibilitySignals visibilitySignals, IHexMapRenderConfig hexMapRenderConfig
        ) {
            UnitFactory         = unitFactory;
            UnitPositionCanon   = unitPositionCanon;
            IconPool            = iconPool;
            IconSlotPool        = iconSlotPool;
            VisibilityCanon     = visibilityCanon;
            UnitPossessionCanon = unitPossessionCanon;
            UnitSignals         = unitSignals;
            VisibilitySignals   = visibilitySignals;
            HexMapRenderConfig  = hexMapRenderConfig;
        }

        #region Unity messages

        private void OnEnable() {
            SignalSubscriptions.Add(UnitSignals.NewUnitCreated  .Subscribe(OnNewUnitCreated));
            SignalSubscriptions.Add(UnitSignals.BeingDestroyed  .Subscribe(DestroyIconOfUnit));
            SignalSubscriptions.Add(UnitSignals.GainedNewOwner  .Subscribe(OnUnitChangedOwner));
            SignalSubscriptions.Add(UnitSignals.HitpointsChanged.Subscribe(OnUnitChangedHitpoints));
            SignalSubscriptions.Add(UnitSignals.LeftLocation    .Subscribe(OnUnitLeftLocation));
            SignalSubscriptions.Add(UnitSignals.EnteredLocation .Subscribe(OnUnitEnteredLocation));

            SignalSubscriptions.Add(VisibilitySignals.CellVisibilityModeChangedSignal .Subscribe(empty => Reset()));
            SignalSubscriptions.Add(VisibilitySignals.CellExplorationModeChangedSignal.Subscribe(empty => Reset()));
            SignalSubscriptions.Add(VisibilitySignals.CellBecameExploredByCivSignal   .Subscribe(data => ResetCell(data.Item1)));
            SignalSubscriptions.Add(VisibilitySignals.CellBecameVisibleToCivSignal    .Subscribe(data => ResetCell(data.Item1)));
        }

        private void OnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

        private void LateUpdate() {
            RepositionIcons();
        }

        #endregion

        #region from IUnitMapIconManager

        public void BuildIcons() {
            foreach(var unit in UnitFactory.AllUnits) {
                if(IsUnitValidForIcon(unit)) {
                    BuildIconForUnit(unit);
                }
            }
        }

        public void RepositionIcons() {
            foreach(var cellWithSlot in SlotOfCell.Keys) {
                Vector3 centerOfNWCorner = cellWithSlot.AbsolutePosition + (
                    HexMapRenderConfig.GetFirstCorner(HexDirection.NW) + HexMapRenderConfig.GetSecondCorner(HexDirection.NW)
                ) / 2f;

                var cornerInScreen = Camera.main.WorldToScreenPoint(centerOfNWCorner);

                SlotOfCell[cellWithSlot].RectTransform.position = cornerInScreen;
            }
        }

        public void ClearIcons() {
            foreach(var unit in UnitFactory.AllUnits) {
                DestroyIconOfUnit(unit);
            }

            foreach(var iconSlot in SlotOfCell.Values) {
                IconSlotPool.Despawn(iconSlot);
            }

            SlotOfCell.Clear();
            IconOfUnit.Clear();
        }

        public void SetActive(bool isActive) {
            enabled = isActive;
        }

        public void Reset() {
            ClearIcons();
            BuildIcons();
        }

        public void ResetCell(IHexCell cell) {
            if(!CellResetRoroutines.ContainsKey(cell)) {
                CellResetRoroutines[cell] = StartCoroutine(ResetCall_Coroutine(cell));
            }
        }

        private IEnumerator ResetCall_Coroutine(IHexCell cell) {
            yield return new WaitForEndOfFrame();

            var unitsAt = UnitPositionCanon.GetPossessionsOfOwner(cell);

            foreach(var unit in unitsAt) {
                if(IsUnitValidForIcon(unit)) {
                    if(!IconOfUnit.ContainsKey(unit)) {
                        BuildIconForUnit(unit);
                    }
                }else {
                    DestroyIconOfUnit(unit);
                }                
            }

            CellResetRoroutines.Remove(cell);
        }

        #endregion

        private bool IsUnitValidForIcon(IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return VisibilityCanon.IsCellVisible(unitLocation) && unit.Type != UnitType.City;
        }

        private void BuildIconForUnit(IUnit unit) {
            var unitLocation = UnitPositionCanon  .GetOwnerOfPossession(unit);
            var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);

            var newIcon = IconPool.Spawn();

            newIcon.transform.SetParent(UnitIconContainer, false);

            newIcon.UnitToDisplay = unit;
            newIcon.UnitOwner     = unitOwner;
            newIcon.Refresh();

            PlaceIconIntoCorrectSlot(newIcon, unitLocation);

            IconOfUnit[unit] = newIcon;
        }

        private void DestroyIconOfUnit(IUnit unit) {
            UnitMapIcon icon;

            if(IconOfUnit.TryGetValue(unit, out icon)) {
                IconPool.Despawn(icon);

                IconOfUnit.Remove(unit);
            }
        }

        private void PlaceIconIntoCorrectSlot(UnitMapIcon icon, IHexCell unitLocation) {
            UnitIconSlot slotOfLocation;

            if(unitLocation == null) {
                icon.transform.SetParent(null, false);

            }else {
                if(!SlotOfCell.TryGetValue(unitLocation, out slotOfLocation)) {
                    slotOfLocation = IconSlotPool.Spawn();

                    slotOfLocation.transform.SetParent(UnitIconContainer, false);

                    SlotOfCell[unitLocation] = slotOfLocation;
                }

                slotOfLocation.AddIconToSlot(icon);
            }
        }

        private void OnUnitChangedOwner(Tuple<IUnit, ICivilization> data) {
            var unit     = data.Item1;
            var newOwner = data.Item2;

            UnitMapIcon icon;

            if(IconOfUnit.TryGetValue(unit, out icon) && IsUnitValidForIcon(unit)) {
                icon.UnitOwner = newOwner;
                icon.Refresh();
            }
        }

        private void OnUnitChangedHitpoints(IUnit unit) {
            UnitMapIcon icon;

            if(IconOfUnit.TryGetValue(unit, out icon) && IsUnitValidForIcon(unit)) {
                icon.Refresh();
            }
        }

        private void OnNewUnitCreated(IUnit unit) {
            if(IsUnitValidForIcon(unit)) {
                BuildIconForUnit(unit);
            }
        }

        private void OnUnitLeftLocation(Tuple<IUnit, IHexCell> data) {
            var unit        = data.Item1;
            var oldLocation = data.Item2;

            StartCoroutine(OnUnitLeftLocation_Coroutine(unit, oldLocation));
        }

        private IEnumerator OnUnitLeftLocation_Coroutine(IUnit unit, IHexCell oldLocation) {
            yield return new WaitForEndOfFrame();

            UnitMapIcon icon;
            if(IconOfUnit.TryGetValue(unit, out icon)) {
                icon.transform.SetParent(null, false);
            }
        }

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> data) {
            var unit        = data.Item1;
            var newLocation = data.Item2;

            StartCoroutine(OnUnitEnteredLocation_Coroutine(unit, newLocation));
        }

        private IEnumerator OnUnitEnteredLocation_Coroutine(IUnit unit, IHexCell newLocation) {
            yield return new WaitForEndOfFrame();

            UnitMapIcon icon;
            if(IconOfUnit.TryGetValue(unit, out icon) && IsUnitValidForIcon(unit)) {
                PlaceIconIntoCorrectSlot(icon, newLocation);
            }
        }

        #endregion

    }

}
