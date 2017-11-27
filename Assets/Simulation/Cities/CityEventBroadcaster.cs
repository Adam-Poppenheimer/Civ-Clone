using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.Cities {

    public class CityEventBroadcaster : ICityEventBroadcaster {

        #region instance fields and properties

        #region from ICityEventBroadcaster

        public ICity LastClickedCity { get; private set; }

        #endregion

        private HashSet<ICityClickedEventHandler> CityClickedHandlers;

        private CityClickedSignal ClickedSignal;

        #endregion

        #region constructors

        [Inject]
        public CityEventBroadcaster(CityClickedSignal clickedSignal) {
            CityClickedHandlers = new HashSet<ICityClickedEventHandler>();

            ClickedSignal = clickedSignal;
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

            LastClickedCity = city;

            foreach(var handler in CityClickedHandlers) {
                handler.OnCityClicked(city, eventData);
            }

            ClickedSignal.Fire(city);
        }

        #endregion

        #endregion

    }

}
