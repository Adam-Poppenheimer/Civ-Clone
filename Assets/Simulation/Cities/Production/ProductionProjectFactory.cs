using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The standard implementation of IProductionProjectFactory.
    /// </summary>
    public class ProductionProjectFactory : IProductionProjectFactory {

        #region instance fields and properties

        private DiContainer Container;

        private IBuildingFactory BuildingFactory;

        private IUnitFactory UnitFactory;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="buildingFactory"></param>
        /// <param name="unitFactory"></param>
        [Inject]
        public ProductionProjectFactory(DiContainer container, IBuildingFactory buildingFactory, IUnitFactory unitFactory) {
            Container       = container;
            BuildingFactory = buildingFactory;
            UnitFactory     = unitFactory;
        }

        #endregion

        #region instance methods

        #region from IProductionProjectFactory

        /// <inheritdoc/>
        public IProductionProject ConstructBuildingProject(IBuildingTemplate template) {
            return new BuildingProductionProject(template, BuildingFactory);
        }

        /// <inheritdoc/>
        public IProductionProject ConstructUnitProject(IUnitTemplate template) {
            return Container.Instantiate<UnitProductionProject>( new List<object>() { template, UnitFactory } );
        }

        #endregion

        #endregion
        
    }

}
