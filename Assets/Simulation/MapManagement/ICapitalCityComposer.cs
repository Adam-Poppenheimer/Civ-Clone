using System;

namespace Assets.Simulation.MapManagement {

    public interface ICapitalCityComposer {

        #region methods

        void ComposeCapitalCities  (SerializableMapData mapData);
        void DecomposeCapitalCities(SerializableMapData mapData);

        #endregion

    }

}