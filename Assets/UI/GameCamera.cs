using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI {

    public class GameCamera : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private float StickMinZoom;
        [SerializeField] private float StickMaxZoom;

        [SerializeField] private float SwivelMinZoom;
        [SerializeField] private float SwivelMaxZoom;

        [SerializeField] private float MoveSpeedMinZoom;
        [SerializeField] private float MoveSpeedMaxZoom;

        [SerializeField] private float RotationSpeed;

        private Transform Swivel, Stick;

        public float Zoom { get; private set; }

        public bool SuppressZoom      { get; set; }
        public bool SuppressRotation  { get; set; }
        public bool SuppressMovemnent { get; set; }

        private float RotationAngle;

        private IHexGrid Grid;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IHexGrid grid) {
            Grid = grid;
        }

        #region Unity messages

        private void Awake() {
            Zoom = 1f;
            Swivel = transform.GetChild(0);
            Stick  = Swivel.GetChild(0);
        }

        private void Update() {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if(!SuppressZoom && zoomDelta != 0f) {
                AdjustZoom(zoomDelta);
            }

            float rotationDelta = Input.GetAxis("Rotation");
            if(!SuppressRotation && rotationDelta != 0f) {
                AdjustRotation(rotationDelta);
            }

            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");
            if(!SuppressMovemnent && xDelta != 0f || zDelta != 0f) {
                AdjustPosition(xDelta, zDelta);
            }
        }

        #endregion

        private void AdjustZoom(float delta) {
            Zoom = Mathf.Clamp01(Zoom + delta);

            float distance = Mathf.Lerp(StickMinZoom, StickMaxZoom, Zoom);
            Stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(SwivelMinZoom, SwivelMaxZoom, Zoom);
            Swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private void AdjustPosition(float xDelta, float zDelta) {
            var direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));

            float distance = Mathf.Lerp(MoveSpeedMinZoom, MoveSpeedMaxZoom, Zoom) * damping * Time.deltaTime;

            transform.localPosition = ClampPosition(transform.localPosition + direction * distance);
        }

        private Vector3 ClampPosition(Vector3 position) {
            float xMax = (Grid.ChunkCountX * HexMetrics.ChunkSizeX - 0.5f) * (2f   * HexMetrics.InnerRadius);
            float zMax = (Grid.ChunkCountZ * HexMetrics.ChunkSizeZ - 1f)   * (1.5f * HexMetrics.OuterRadius);

            position.x = Mathf.Clamp(position.x, 0f, xMax);
            position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }

        private void AdjustRotation(float delta) {
            RotationAngle += delta * RotationSpeed * Time.deltaTime % 360f;
            transform.localRotation = Quaternion.Euler(0f, RotationAngle, 0f);
        }

        #endregion

    }

}
