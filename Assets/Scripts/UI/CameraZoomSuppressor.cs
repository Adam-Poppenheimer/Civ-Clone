using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.UI {

    public class CameraZoomSuppressor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        private bool PreviousSuppressZoomValue;

        private bool PointerIsOver;



        private IGameCamera GameCamera;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IGameCamera gameCamera) {
            GameCamera = gameCamera;
        }

        #region Unity messages

        private void OnDisable() {
            if(PointerIsOver) {
                PointerIsOver = false;
                GameCamera.SuppressZoom = PreviousSuppressZoomValue;
            }
        }

        #endregion

        #region EventSystem messages

        public void OnPointerEnter(PointerEventData eventData) {
            PointerIsOver = true;
            PreviousSuppressZoomValue = GameCamera.SuppressZoom;
            GameCamera.SuppressZoom = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            if(PointerIsOver) {
                PointerIsOver = false;
                GameCamera.SuppressZoom = PreviousSuppressZoomValue;
            }
        }

        #endregion

        #endregion

    }

}
