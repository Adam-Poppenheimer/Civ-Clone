using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Cities;

namespace Assets.Cities.Buildings {

    public interface ITemplateValidityLogic {

        #region methods

        IEnumerable<IBuildingTemplate> GetTemplatesValidForCity(ICity city);

        bool IsTemplateValidForCity(IBuildingTemplate template, ICity city);

        #endregion

    }

}
