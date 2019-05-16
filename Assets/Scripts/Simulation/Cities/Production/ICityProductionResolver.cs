using System;

namespace Assets.Simulation.Cities.Production {

    public interface ICityProductionResolver {

        #region methods

        void MakeProductionRequest(IProductionProject project, ICity requestingCity);

        void ResolveBuildingConstructionRequests();

        #endregion

    }
}