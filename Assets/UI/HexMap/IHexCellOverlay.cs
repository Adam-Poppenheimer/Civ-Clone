using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UI.HexMap {

    public interface IHexCellOverlay {

        #region methods

        void SetDisplayType(CellOverlayType type);
        void Clear();

        void Show();
        void Hide();

        #endregion

    }

}
