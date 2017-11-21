using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities.Production {

    public class ProductionProject : IProductionProject {

        #region instance fields and properties

        #region from IProductionProject

        public int Progress {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public int ProductionToComplete {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region instance methods

        #region from IProductionProject

        public void Execute(ICity targetCity) {
            throw new NotImplementedException();
        }


        #endregion

        #endregion

    }

}
