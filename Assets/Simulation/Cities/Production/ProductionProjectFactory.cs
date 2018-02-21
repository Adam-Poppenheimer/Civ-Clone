using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The standard implementation of IProductionProjectFactory.
    /// </summary>
    public class ProductionProjectFactory : IProductionProjectFactory {

        #region instance fields and properties

        private IBuildingFactory BuildingFactory;

        private IUnitFactory UnitFactory;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="buildingFactory"></param>
        /// <param name="unitFactory"></param>
        [Inject]
        public ProductionProjectFactory(
            IBuildingFactory buildingFactory, IUnitFactory unitFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            BuildingFactory     = buildingFactory;
            UnitFactory         = unitFactory;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IProductionProjectFactory

        /// <inheritdoc/>
        public IProductionProject ConstructBuildingProject(IBuildingTemplate template) {
            return new ProductionProject(template, BuildingFactory);
        }

        /// <inheritdoc/>
        public IProductionProject ConstructUnitProject(IUnitTemplate template) {
            return new ProductionProject(template, UnitFactory, CityPossessionCanon);
        }

        #endregion

        #endregion
        
    }

}
