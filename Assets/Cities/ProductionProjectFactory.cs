using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Cities.Buildings;

namespace Assets.Cities {

    public class ProductionProjectFactory : IFactory<IBuildingTemplate, IProductionProject> {

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

        public IProductionProject Create(IBuildingTemplate buildingTemplate) {
            return new BuildingProductionProject(buildingTemplate, BuildingFactory);
        }

        #endregion

        #endregion

    }

}
