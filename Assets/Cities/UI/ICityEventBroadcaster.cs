﻿using System;

using UnityEngine.EventSystems;

namespace Assets.Cities.UI {

    public interface ICityEventBroadcaster {

        #region properties

        ICity LastClickedCity { get; }

        #endregion

        #region methods

        void BroadcastCityClicked(ICity city, PointerEventData eventData);

        void SubscribeCityClickedHandler  (ICityClickedEventHandler handler);
        void UnsubscribeCityClickedHandler(ICityClickedEventHandler handler);

        #endregion

    }

}