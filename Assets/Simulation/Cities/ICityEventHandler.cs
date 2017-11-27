using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Simulation.Cities {

    public interface ICityClickedEventHandler : IEventSystemHandler {

        #region methods

        void OnCityClicked(ICity city, PointerEventData eventData);

        #endregion

    }

}
