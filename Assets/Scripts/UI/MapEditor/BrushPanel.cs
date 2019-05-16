using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.UI.MapEditor {

    public class BrushPanel : MonoBehaviour {

        #region instance fields and properties

        private int BrushSize;

        private HashSet<CellPaintingPanelBase> SubscribedPainters = new HashSet<CellPaintingPanelBase>();

        #endregion

        #region instance methods

        public void SubscribePainter(CellPaintingPanelBase painter) {
            RefreshPainter(painter);

            SubscribedPainters.Add(painter);
        }

        public void UnsubscribePainter(CellPaintingPanelBase painter) {
            SubscribedPainters.Remove(painter);
        }

        public void SetBrushSize(float index) {
            BrushSize = Mathf.RoundToInt(index);

            Refresh();
        }

        private void Refresh() {
            foreach(var painter in SubscribedPainters) {
                RefreshPainter(painter);
            }
        }

        private void RefreshPainter(CellPaintingPanelBase painter) {
            painter.BrushSize = BrushSize;
        }

        #endregion

    }

}
