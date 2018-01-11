using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class HexCellOverlay : MonoBehaviour, IHexCellOverlay {

        #region instance fields and properties

        [SerializeField] private Text CoordinateLabel;

        [SerializeField] private RectTransform PathIndicator;

        public IHexCell Parent { get; set; }

        #endregion

        #region instance methods

        #region from IHexCellOverlay

        public void Clear() {
            CoordinateLabel.gameObject.SetActive(false);
            PathIndicator.gameObject.SetActive(false);
        }

        public void SetDisplayType(CellOverlayType type) {
            Clear();

            if(type == CellOverlayType.Labels) {
                CoordinateLabel.text = Parent.Coordinates.ToStringOnSeparateLines();
                CoordinateLabel.gameObject.SetActive(true);

            }else if(type == CellOverlayType.PathIndicator) {
                PathIndicator.gameObject.SetActive(true);

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
