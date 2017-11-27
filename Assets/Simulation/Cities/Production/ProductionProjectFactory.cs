using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.Production {

    public class ProductionProjectFactory : IProductionProjectFactory {

        #region instance fields and properties

        private IBuildingFactory BuildingFactory;

        #endregion

        #region constructors

        [Inject]
        public ProductionProjectFactory(IBuildingFactory buildingFactory) {
            BuildingFactory = buildingFactory;
        }

        #endregion

        #region instance methods

        #region from IProductionProjectFactory

        public IProductionProject ConstructBuildingProject(IBuildingTemplate template) {
            return new BuildingProductionProject(template, BuildingFactory);
        }

        #endregion

        #endregion
        
    }

}
