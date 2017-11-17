using System;

using Assets.Cities;

namespace Assets.Core {

    public interface ITurnExecuter {

        #region methods

        void BeginTurnOnCity(ICity city);
        void EndTurnOnCity(ICity city);

        #endregion

    }

}