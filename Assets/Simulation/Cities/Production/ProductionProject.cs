using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Production {

    public class ProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        public IBuildingTemplate BuildingToConstruct { get; private set; }

        public IUnitTemplate UnitToConstruct { get; private set; }

        public string Name {
            get {
                return BuildingToConstruct != null ? BuildingToConstruct.name : UnitToConstruct.Name;
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
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            if(unitToConstruct == null) {
                throw new ArgumentNullException("unitToConstruct");
            }

            UnitToConstruct     = unitToConstruct;
            UnitFactory         = unitFactory;
            CityPossessionCanon = cityPossessionCanon;
        }

        private ProductionProject() { }

        #endregion

        #region instance methods

        #region from IProductionProject

        public void Execute(ICity targetCity) {
            if(BuildingToConstruct != null) {
                BuildingFactory.Create(BuildingToConstruct, targetCity);
            }else {
                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(targetCity);
                UnitFactory.Create(targetCity.Location, UnitToConstruct, cityOwner);
            }
        }

        #endregion

        #endregion
        
    }

}
