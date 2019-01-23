using System;

namespace Assets.UI.Units {

    public interface IUnitMapIconManager {

        #region methods

        void BuildIcons();
        void ClearIcons();
        void RepositionIcons();

        void SetActive(bool isActive);

        #endregion

    }

}