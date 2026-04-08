using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 摄像机控制器 - RTS风格的摄像机
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("移动设置")]
        public float moveSpeed = 10f;
        public float zoomSpeed = 5f;
        public float rotationSpeed = 3f;

        [Header("边界设置")]
        public Vector2 mapBounds = new Vector2(50f, 50f);
        public float minHeight = 5f;
        public float maxHeight = 30f;

        [Header("鼠标边缘滚动")]
        public float edgeScrollMargin = 20f;
        public bool enableEdgeScrolling = true;

        private float currentZoom;
        private Quaternion targetRotation;
        private Vector3 dragStartPosition;
        private bool isDragging = false;

        void Start()
        {
            currentZoom = transform.position.y;
            targetRotation = transform.rotation;
        }

        void Update()
        {
            HandleZoom();
            HandleRotation();
            HandleMovement();
        }

        void HandleMovement()
        {
            Vector3 movement = Vector3.zero;

            // WASD 移动
            if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
            if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
            if (Input.GetKey(KeyCode.D)) movement += Vector3.right;

            // 鼠标边缘滚动
            if (enableEdgeScrolling && !isDragging)
            {
                Vector3 mousePos = Input.mousePosition;

                if (mousePos.x < edgeScrollMargin) movement += Vector3.left;
                if (mousePos.x > Screen.width - edgeScrollMargin) movement += Vector3.right;
                if (mousePos.y < edgeScrollMargin) movement += Vector3.back;
                if (mousePos.y > Screen.height - edgeScrollMargin) movement += Vector3.forward;
            }

            // 鼠标中键拖拽
            if (Input.GetMouseButtonDown(2))
            {
                isDragging = true;
                dragStartPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2) && isDragging)
            {
                Vector3 delta = Input.mousePosition - dragStartPosition;
                movement = new Vector3(-delta.x, 0, -delta.y);
                dragStartPosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }

            // 应用移动
            if (movement != Vector3.zero)
            {
                // 根据摄像机朝向调整移动方向
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = transform.right;
                right.y = 0;
                right.Normalize();

                Vector3 moveDirection = (forward * movement.z + right * movement.x);
                transform.position += moveDirection * moveSpeed * Time.deltaTime;

                // 限制在地图边界内
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, -mapBounds.x, mapBounds.x),
                    transform.position.y,
                    Mathf.Clamp(transform.position.z, -mapBounds.y, mapBounds.y)
                );
            }
        }

        void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minHeight, maxHeight);

                Vector3 pos = transform.position;
                pos.y = currentZoom;
                transform.position = pos;
            }
        }

        void HandleRotation()
        {
            // 按住右键旋转
            if (Input.GetMouseButton(1))
            {
                float rotation = Input.GetAxis("Mouse X") * rotationSpeed;
                transform.Rotate(0, rotation, 0, Space.World);
            }
        }
    }
}
