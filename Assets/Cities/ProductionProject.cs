using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public class ProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        public int ProductionToComplete {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region instance methods

        #region from IProductionProject

        public void ExecuteProject(ICity targetCity) {
            throw new NotImplementedException();
        }


        #endregion

        #endregion

    }

}
