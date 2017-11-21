using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Cities.Production;

namespace Assets.Cities.Buildings {

    public class BuildingProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        public int ProductionToComplete {
            get { return BuildingTemplate.Cost; }
        }

        public int Progress { get; set; }

        #endregion

        private IBuildingTemplate BuildingTemplate;

        private IBuildingFactory BuildingFactory;

        #endregion

        #region constructors

        [Inject]
        public BuildingProductionProject(IBuildingTemplate buildingTemplate, IBuildingFactory buildingFactory) {
            BuildingTemplate = buildingTemplate;
            BuildingFactory = buildingFactory;
        }

        #endregion

        #region instance methods

        #region from IProductionProject

        public void Execute(ICity targetCity) {
            BuildingFactory.Create(BuildingTemplate, targetCity);
        }

        #endregion

        #endregion

    }

}
