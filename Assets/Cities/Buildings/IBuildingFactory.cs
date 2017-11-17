using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Cities;

namespace Assets.Cities.Buildings {

    public interface IBuildingFactory : IFactory<IBuildingTemplate, ICity, IBuilding> {

        #region methods

        bool CanConstructTemplateInCity(IBuildingTemplate template, ICity city);

        #endregion

    }

}
