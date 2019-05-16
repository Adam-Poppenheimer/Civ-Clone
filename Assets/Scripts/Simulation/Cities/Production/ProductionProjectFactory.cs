using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The standard implementation of IProductionProjectFactory.
    /// </summary>
    public class ProductionProjectFactory : IProductionProjectFactory {

        #region instance fields and properties

        private IBuildingFactory                              BuildingFactory;
        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IStartingExperienceLogic                      StartingExperienceLogic;
        private ILocalPromotionLogic                          LocalPromotionLogic;

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
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IStartingExperienceLogic startingExperienceLogic,
            ILocalPromotionLogic localPromotionLogic
        ){
            BuildingFactory         = buildingFactory;
            UnitFactory             = unitFactory;
            CityPossessionCanon     = cityPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            StartingExperienceLogic = startingExperienceLogic;
            LocalPromotionLogic     = localPromotionLogic;
        }

        #endregion

        #region instance methods

        #region from IProductionProjectFactory

        /// <inheritdoc/>
        public IProductionProject ConstructProject(IBuildingTemplate template) {
            return new ProductionProject(template, BuildingFactory);
        }

        /// <inheritdoc/>
        public IProductionProject ConstructProject(IUnitTemplate template) {
            return new ProductionProject(
                template, UnitFactory, CityPossessionCanon,
                CityLocationCanon, StartingExperienceLogic,
                LocalPromotionLogic
            );
        }

        #endregion

        #endregion
        
    }

}
