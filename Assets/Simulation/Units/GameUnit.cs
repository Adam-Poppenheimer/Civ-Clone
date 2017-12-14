using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using UnityCustomUtilities.Extensions;
using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public class GameUnit : MonoBehaviour, IUnit, IPointerClickHandler, IBeginDragHandler,
        IDragHandler, IEndDragHandler {

        #region instance fields and properties

        #region from IUnit

        public IUnitTemplate Template { get; set; }

        public List<IMapTile> CurrentPath { get; set; }

        #endregion

        public int Health {
            get { return _health; }
            set {
                _health = value.Clamp(0, Config.MaxHealth);
            }
        }
        private int _health;

        public int CurrentMovement { get; set; }

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

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            Signals.UnitClickedSignal.OnNext(this);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            Signals.UnitBeginDragSignal.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        public void OnDrag(PointerEventData eventData) {
            Signals.UnitDragSignal.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        public void OnEndDrag(PointerEventData eventData) {
            Signals.UnitEndDragSignal.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(this, eventData));
        }

        #endregion

        #region from IUnit

        public void PerformMovement() {
            IMapTile tileToTravelTo = null;

            while(CurrentMovement > 0 && CurrentPath != null && CurrentPath.Count > 0) {
                tileToTravelTo = CurrentPath.First();
                CurrentPath.RemoveAt(0);

                CurrentMovement = Math.Max(0,
                    CurrentMovement - TerrainCostLogic.GetCostToMoveUnitIntoTile(this, tileToTravelTo)
                );
            }

            if(tileToTravelTo != null) {
                PositionCanon.ChangeOwnerOfPossession(this, tileToTravelTo);
            }
        }

        #endregion

        #endregion

    }

}
