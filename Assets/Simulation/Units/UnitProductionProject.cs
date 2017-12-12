using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;

namespace Assets.Simulation.Units {

    public class UnitProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        public string Name {
            get { return TemplateToConstruct.Name; }
        }

        public int ProductionToComplete {
            get { return TemplateToConstruct.ProductionCost; }
        }

        public int Progress { get; set; }

        #endregion

        private IUnitTemplate TemplateToConstruct;

        private IUnitFactory UnitFactory;

        #endregion

        #region constructors

        [Inject]
        public UnitProductionProject(IUnitTemplate template, IUnitFactory unitFactory) {
            TemplateToConstruct = template;
            UnitFactory = unitFactory;
        }

        #endregion

        #region instance methods

        #region from IProductionProject

        public void Execute(ICity targetCity) {
            UnitFactory.Create(targetCity.Location, TemplateToConstruct);
        }

        #endregion

        #endregion

    }

}
