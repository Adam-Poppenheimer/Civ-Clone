using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IProductionProject {

        #region properties

        int Progress { get; set; }
        int ProductionToComplete { get; }

        #endregion

        #region methods

        void Execute(ICity targetCity);

        #endregion

    }

}
