using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// RTS风格相机控制器 - 支持平移、旋转、缩放
    /// AI可以调整相机参数来改变观感
    /// </summary>
    public class RTSCameraController : MonoBehaviour
    {
        [Header("移动设置")]
        public float moveSpeed = 10f;
        public float rotationSpeed = 3f;
        public float zoomSpeed = 5f;
        public float minZoom = 5f;
        public float maxZoom = 20f;

        [Header("边界限制")]
        public bool enableBoundaries = true;
        public Vector2 mapSize = new Vector2(50f, 50f);

        [Header("键盘控制")]
        public KeyCode moveForward = KeyCode.W;
        public KeyCode moveBackward = KeyCode.S;
        public KeyCode moveLeft = KeyCode.A;
        public KeyCode moveRight = KeyCode.D;
        public KeyCode rotateLeft = KeyCode.Q;
        public KeyCode rotateRight = KeyCode.E;

        private Transform cameraTransform;
        private float currentZoom;
        private Vector3 lastMousePosition;
        private bool isMiddleMouseDragging = false;

        void Start()
        {
            cameraTransform = Camera.main.transform;
            currentZoom = cameraTransform.position.y;

            // 设置初始相机位置
            if (cameraTransform.parent == null)
            {
                cameraTransform.position = new Vector3(0, currentZoom, -currentZoom);
                cameraTransform.rotation = Quaternion.Euler(60f, 0f, 0f);
            }
        }

        void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            HandleZoom();
            ApplyBoundaries();
        }

        void HandleKeyboardInput()
        {
            // 移动控制
            Vector3 moveDirection = Vector3.zero;

            if (Input.GetKey(moveForward))
                moveDirection += Vector3.forward;
            if (Input.GetKey(moveBackward))
                moveDirection += Vector3.back;
            if (Input.GetKey(moveLeft))
                moveDirection += Vector3.left;
            if (Input.GetKey(moveRight))
                moveDirection += Vector3.right;

            if (moveDirection != Vector3.zero)
            {
                MoveCamera(moveDirection);
            }

            // 旋转控制
            if (Input.GetKey(rotateLeft))
            {
                RotateCamera(-1f);
            }

            if (Input.GetKey(rotateRight))
            {
                RotateCamera(1f);
            }
        }

        void HandleMouseInput()
        {
            // 中键拖拽平移
            if (Input.GetMouseButtonDown(2)) // 中键
            {
                isMiddleMouseDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(2))
            {
                isMiddleMouseDragging = false;
            }

            if (isMiddleMouseDragging)
            {
                Vector3 deltaPosition = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                // 转换屏幕空间移动到世界空间
                Vector3 worldDelta = cameraTransform.right * -deltaPosition.x * 0.05f +
                                     cameraTransform.forward * -deltaPosition.y * 0.05f;
                worldDelta.y = 0; // 保持水平移动

                MoveCamera(worldDelta);
            }

            // 边缘平移
            float edgeThreshold = 20f;
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.x < edgeThreshold)
            {
                MoveCamera(Vector3.left);
            }
            else if (mousePos.x > Screen.width - edgeThreshold)
            {
                MoveCamera(Vector3.right);
            }

            if (mousePos.y < edgeThreshold)
            {
                MoveCamera(Vector3.back);
            }
            else if (mousePos.y > Screen.height - edgeThreshold)
            {
                MoveCamera(Vector3.forward);
            }
        }

        void HandleZoom()
        {
            // 鼠标滚轮缩放
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                ZoomCamera(-scrollDelta);
            }
        }

        void MoveCamera(Vector3 direction)
        {
            if (cameraTransform == null)
                return;

            // 根据相机朝向调整移动方向
            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0;
            right.Normalize();

            Vector3 movement = (forward * direction.z + right * direction.x) * moveSpeed * Time.deltaTime;

            // 如果有父物体（旋转中心），移动父物体
            if (cameraTransform.parent != null)
            {
                cameraTransform.parent.position += movement;
            }
            else
            {
                cameraTransform.position += movement;
            }
        }

        void RotateCamera(float direction)
        {
            // 围绕Y轴旋转
            if (cameraTransform != null)
            {
                Transform targetTransform = cameraTransform.parent != null ? cameraTransform.parent : cameraTransform;
                targetTransform.RotateAround(targetTransform.position, Vector3.up, direction * rotationSpeed);
            }
        }

        void ZoomCamera(float delta)
        {
            currentZoom += delta * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            if (cameraTransform != null)
            {
                Vector3 position = cameraTransform.localPosition;
                position.y = currentZoom;
                position.z = -currentZoom;
                cameraTransform.localPosition = position;
            }
        }

        void ApplyBoundaries()
        {
            if (!enableBoundaries || cameraTransform == null)
                return;

            Transform targetTransform = cameraTransform.parent != null ? cameraTransform.parent : cameraTransform;
            Vector3 position = targetTransform.position;

            // 限制在地图范围内
            position.x = Mathf.Clamp(position.x, -mapSize.x / 2f, mapSize.x / 2f);
            position.z = Mathf.Clamp(position.z, -mapSize.y / 2f, mapSize.y / 2f);

            targetTransform.position = position;
        }

        /// <summary>
        /// 聚焦到指定位置
        /// </summary>
        public void FocusOnPosition(Vector3 targetPosition)
        {
            if (cameraTransform == null)
                return;

            Transform targetTransform = cameraTransform.parent != null ? cameraTransform.parent : cameraTransform;
            targetTransform.position = new Vector3(targetPosition.x, 0, targetPosition.z);
        }

        /// <summary>
        /// 设置相机参数
        /// </summary>
        public void SetCameraParameters(float newMoveSpeed, float newZoomSpeed, float newMinZoom, float newMaxZoom)
        {
            moveSpeed = newMoveSpeed;
            zoomSpeed = newZoomSpeed;
            minZoom = newMinZoom;
            maxZoom = newMaxZoom;
        }

        /// <summary>
        /// 设置地图大小
        /// </summary>
        public void SetMapSize(Vector2 newSize)
        {
            mapSize = newSize;
        }

        void OnDrawGizmosSelected()
        {
            // 显示相机边界
            if (enableBoundaries)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize.x, 1f, mapSize.y));
            }
        }
    }
}