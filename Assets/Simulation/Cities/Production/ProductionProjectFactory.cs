using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.Production {

    public class ProductionProjectFactory : IProductionProjectFactory {

        #region instance fields and properties

        private DiContainer Container;

        private IBuildingFactory BuildingFactory;

        private IUnitFactory UnitFactory;

        #endregion

        #region constructors

        [Inject]
        public ProductionProjectFactory(DiContainer container, IBuildingFactory buildingFactory, IUnitFactory unitFactory) {
            Container       = container;
            BuildingFactory = buildingFactory;
            UnitFactory     = unitFactory;
        }

        #endregion

        #region instance methods

        #region from IProductionProjectFactory

        public IProductionProject ConstructBuildingProject(IBuildingTemplate template) {
            return new BuildingProductionProject(template, BuildingFactory);
        }

        public IProductionProject ConstructUnitProject(IUnitTemplate template) {
            return Container.Instantiate<UnitProductionProject>( new List<object>() { template, UnitFactory } );
        }

        #endregion

        #endregion
        
    }

}
