using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Cities.Production {

    public class ProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        public IBuildingTemplate BuildingToConstruct { get; private set; }

        public IUnitTemplate UnitToConstruct { get; private set; }

        public string Name {
            get {
                return BuildingToConstruct != null ? BuildingToConstruct.name : UnitToConstruct.name;
            }
        }

        public int Progress { get; set; }

        public int ProductionToComplete {
            get {
                return BuildingToConstruct != null ? BuildingToConstruct.ProductionCost : UnitToConstruct.ProductionCost;
            }
        }        

        #endregion

        private IBuildingFactory                              BuildingFactory;
        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IStartingExperienceLogic                      StartingExperienceLogic;

        #endregion

        #region constructors

        public ProductionProject(IBuildingTemplate buildingToConstruct, IBuildingFactory buildingFactory){
            if(buildingToConstruct == null) {
                throw new ArgumentNullException("buildingToConstruct");
            }

            BuildingToConstruct = buildingToConstruct;
            BuildingFactory     = buildingFactory;
        }

        public ProductionProject(
            IUnitTemplate unitToConstruct, IUnitFactory unitFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IStartingExperienceLogic startingExperienceLogic
        ){
            if(unitToConstruct == null) {
                throw new ArgumentNullException("unitToConstruct");
            }

            UnitToConstruct         = unitToConstruct;
            UnitFactory             = unitFactory;
            CityPossessionCanon     = cityPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            StartingExperienceLogic = startingExperienceLogic;
        }

        private ProductionProject() { }

        #endregion

        #region instance methods

        #region from IProductionProject

        public void Execute(ICity targetCity) {
            if(BuildingToConstruct != null) {
                BuildingFactory.Create(BuildingToConstruct, targetCity);
            }else {
                var cityOwner    = CityPossessionCanon.GetOwnerOfPossession(targetCity);
                var cityLocation = CityLocationCanon.GetOwnerOfPossession(targetCity);

                var newUnit = UnitFactory.BuildUnit(cityLocation, UnitToConstruct, cityOwner);

                newUnit.Experience = StartingExperienceLogic.GetStartingExperienceForUnit(newUnit, targetCity);
            }
        }

        #endregion

        #endregion
        
    }

}
