using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class HexCellOverlay : MonoBehaviour, IHexCellOverlay {

        #region internal types

        public class Pool : MonoMemoryPool<HexCellOverlay> {

            protected override void Reinitialize(HexCellOverlay item) {
                item.CellToDisplay = null;
            }

        }

        #endregion

        #region instance fields and properties

        [SerializeField] private Text CoordinateLabel;

        [SerializeField] private RectTransform PathIndicator;
        [SerializeField] private RectTransform AttackIndicator;
        [SerializeField] private RectTransform UnreachableIndicator;
        [SerializeField] private RectTransform SelectedIndicator;

        public IHexCell CellToDisplay {
            get { return _cellToDisplay; }
            set {
                _cellToDisplay = value;

                if(_cellToDisplay != null) {
                    transform.position = _cellToDisplay.OverlayAnchorPoint;
                }
            }
        }
        private IHexCell _cellToDisplay;

        #endregion

        #region instance methods

        #region from IHexCellOverlay

        public void Clear() {
            CoordinateLabel     .gameObject.SetActive(false);
            PathIndicator       .gameObject.SetActive(false);
            AttackIndicator     .gameObject.SetActive(false);
            UnreachableIndicator.gameObject.SetActive(false);
            SelectedIndicator   .gameObject.SetActive(false);
        }

        public void SetDisplayType(CellOverlayType type) {
            Clear();

            if(type == CellOverlayType.Labels) {
                CoordinateLabel.text = CellToDisplay.Coordinates.ToStringOnSeparateLines();
                CoordinateLabel.gameObject.SetActive(true);

            }else if(type == CellOverlayType.PathIndicator) {
                PathIndicator.gameObject.SetActive(true);

            }else if(type == CellOverlayType.AttackIndicator) {
                AttackIndicator.gameObject.SetActive(true);

            }else if(type == CellOverlayType.UnreachableIndicator) {
                UnreachableIndicator.gameObject.SetActive(true);

            }else if(type == CellOverlayType.SelectedIndicator) {
                SelectedIndicator.gameObject.SetActive(true);
            }
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        #endregion

        #endregion
        
    }

}
