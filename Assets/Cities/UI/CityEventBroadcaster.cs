using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Cities.UI {

    public class CityEventBroadcaster : ICityEventBroadcaster {

        #region instance fields and properties

        private HashSet<ICityClickedEventHandler> CityClickedHandlers;

        #endregion

        #region constructors

        [Inject]
        public CityEventBroadcaster() {
            CityClickedHandlers = new HashSet<ICityClickedEventHandler>();
        }

        #endregion

        #region instance methods

        #region from ICityEventBroadcaster

        public void SubscribeCityClickedHandler(ICityClickedEventHandler handler) {
            if(handler == null) {
                throw new ArgumentNullException("handler");
            }
            CityClickedHandlers.Add(handler);
        }

        public void UnsubscribeCityClickedHandler(ICityClickedEventHandler handler) {
            if(handler == null) {
                throw new ArgumentNullException("handler");
            }
            CityClickedHandlers.Remove(handler);
        }

        public void BroadcastCityClicked(ICity city, PointerEventData eventData) {
            if(city == null) {
                throw new NotImplementedException("city");
            }else if(eventData == null) {
                throw new NotImplementedException("eventData");
            }

            foreach(var handler in CityClickedHandlers) {
                handler.OnCityClicked(city, eventData);
            }
        }

        #endregion

        #endregion

    }

}
