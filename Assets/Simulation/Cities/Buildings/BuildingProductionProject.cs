using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Production;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// A construction project that, when executed, will construct a building of
    /// a particular template.
    /// </summary>
    public class BuildingProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        /// <inheritdoc/>
        public string Name {
            get { return BuildingTemplate.name; }
        }

        /// <inheritdoc/>
        public int ProductionToComplete {
            get { return BuildingTemplate.Cost; }
        }

        /// <inheritdoc/>
        public int Progress { get; set; }        

        #endregion

        /// <summary>
        /// The building template the project is seeking to construct.
        /// </summary>
        public IBuildingTemplate BuildingTemplate { get; private set; }

        private IBuildingFactory BuildingFactory;

        #endregion

        #region constructors

        /// <summary>
        /// Constructs a new BuildingProductionProject from a given template with a given factory used for execution.
        /// </summary>
        /// <param name="buildingTemplate">The template of the building to make when Execute is called</param>
        /// <param name="buildingFactory">The factory to use to construct the building</param>
        [Inject]
        public BuildingProductionProject(IBuildingTemplate buildingTemplate, IBuildingFactory buildingFactory) {
            BuildingTemplate = buildingTemplate;
            BuildingFactory = buildingFactory;
        }

        #endregion

        #region instance methods

        #region from IProductionProject

        /// <inheritdoc/>
        public void Execute(ICity targetCity) {
            BuildingFactory.Create(BuildingTemplate, targetCity);
        }

        #endregion

        #endregion

    }

}
