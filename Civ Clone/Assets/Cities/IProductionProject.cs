using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IProductionProject {

        #region properties

        int ProductionToComplete { get; }

        #endregion

        #region methods

        void ExecuteProject(ICity targetCity);

        #endregion

    }

}
