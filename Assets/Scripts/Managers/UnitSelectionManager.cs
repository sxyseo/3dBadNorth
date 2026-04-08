using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BadNorth3D
{
    /// <summary>
    /// 处理单位选择和RTS风格控制
    /// </summary>
    public class UnitSelectionManager : MonoBehaviour
    {
        public static UnitSelectionManager Instance { get; private set; }

        private List<SquadUnit> selectedUnits = new List<SquadUnit>();
        private bool isDragging = false;
        private Vector3 dragStartPosition;
        private GameObject selectionBox;

        [Header("选择框设置")]
        public Material boxMaterial;
        public float boxMinimumSize = 0.1f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            CreateSelectionBox();
        }

        void CreateSelectionBox()
        {
            selectionBox = new GameObject("SelectionBox");
            selectionBox.transform.SetParent(transform);

            // 创建线框选择框
            LineRenderer lineRenderer = selectionBox.AddComponent<LineRenderer>();
            lineRenderer.material = boxMaterial;
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 5;
            lineRenderer.enabled = false;

            selectionBox.SetActive(false);
        }

        void Update()
        {
            HandleSelection();

            if (selectedUnits.Count > 0)
            {
                HandleUnitCommands();
            }
        }

        void HandleSelection()
        {
            // 单击选择
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    SquadUnit unit = hit.collider.GetComponent<SquadUnit>();
                    if (unit != null)
                    {
                        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                        {
                            DeselectAllUnits();
                        }

                        if (!selectedUnits.Contains(unit))
                        {
                            SelectUnit(unit);
                        }
                        return;
                    }
                }

                // 开始框选
                dragStartPosition = Input.mousePosition;
                isDragging = true;
                selectionBox.SetActive(true);
            }

            // 拖拽选择框
            if (isDragging && Input.GetMouseButton(0))
            {
                UpdateSelectionBox();
            }

            // 释放选择框
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                SelectUnitsInBox();
                isDragging = false;
                selectionBox.SetActive(false);

                LineRenderer lineRenderer = selectionBox.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.enabled = false;
                }
            }

            // ESC取消选择
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectAllUnits();
            }
        }

        void UpdateSelectionBox()
        {
            Vector3 currentMousePosition = Input.mousePosition;

            Vector3 min = Vector3.Min(dragStartPosition, currentMousePosition);
            Vector3 max = Vector3.Max(dragStartPosition, currentMousePosition);

            LineRenderer lineRenderer = selectionBox.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;

                // 在屏幕空间绘制选择框
                lineRenderer.SetPosition(0, new Vector3(min.x, min.y, 0));
                lineRenderer.SetPosition(1, new Vector3(max.x, min.y, 0));
                lineRenderer.SetPosition(2, new Vector3(max.x, max.y, 0));
                lineRenderer.SetPosition(3, new Vector3(min.x, max.y, 0));
                lineRenderer.SetPosition(4, new Vector3(min.x, min.y, 0));
            }
        }

        void SelectUnitsInBox()
        {
            Vector3 min = Vector3.Min(dragStartPosition, Input.mousePosition);
            Vector3 max = Vector3.Max(dragStartPosition, Input.mousePosition);

            Rect selectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                DeselectAllUnits();
            }

            SquadUnit[] allUnits = FindObjectsOfType<SquadUnit>();
            foreach (SquadUnit unit in allUnits)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

                if (selectionRect.Contains(screenPos))
                {
                    if (!selectedUnits.Contains(unit))
                    {
                        SelectUnit(unit);
                    }
                }
            }
        }

        void SelectUnit(SquadUnit unit)
        {
            selectedUnits.Add(unit);
            unit.Select();

            // 播放选择音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }
        }

        void DeselectAllUnits()
        {
            foreach (SquadUnit unit in selectedUnits)
            {
                unit.Deselect();
            }
            selectedUnits.Clear();
        }

        void HandleUnitCommands()
        {
            // 移动命令
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // 检查是否点击敌人
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        // 攻击命令
                        AttackEnemy(enemy);
                    }
                    else
                    {
                        // 移动命令
                        MoveSelectedUnits(hit.point);
                    }
                }
            }
        }

        void MoveSelectedUnits(Vector3 targetPosition)
        {
            if (selectedUnits.Count == 0) return;

            // 创建阵型
            int columns = Mathf.CeilToInt(Mathf.Sqrt(selectedUnits.Count));
            int rows = Mathf.CeilToInt((float)selectedUnits.Count / columns);

            float spacing = 1.5f;
            Vector3[] positions = new Vector3[selectedUnits.Count];

            for (int i = 0; i < selectedUnits.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                Vector3 offset = new Vector3(
                    (col - (columns - 1) * 0.5f) * spacing,
                    0,
                    (row - (rows - 1) * 0.5f) * spacing
                );

                positions[i] = targetPosition + offset;
            }

            // 移动各单位
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                selectedUnits[i].MoveTo(positions[i]);
            }

            // 显示移动指示器
            ShowMoveIndicator(targetPosition);
        }

        void AttackEnemy(Enemy enemy)
        {
            Vector3 targetPosition = enemy.transform.position;

            for (int i = 0; i < selectedUnits.Count; i++)
            {
                selectedUnits[i].MoveTo(targetPosition);
            }
        }

        void ShowMoveIndicator(Vector3 position)
        {
            // 创建程序化移动指示器
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "MoveIndicator";
            indicator.transform.position = position + new Vector3(0, 0.1f, 0);
            indicator.transform.localScale = new Vector3(2f, 0.1f, 2f);

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0, 1, 0, 0.5f);
            indicator.GetComponent<MeshRenderer>().material = material;

            Destroy(indicator.GetComponent<Collider>());

            // 动画效果
            StartCoroutine(AnimateMoveIndicator(indicator));

            Destroy(indicator, 1f);
        }

        System.Collections.IEnumerator AnimateMoveIndicator(GameObject indicator)
        {
            float elapsed = 0f;
            float duration = 1f;
            Vector3 startScale = indicator.transform.localScale;
            Vector3 startRot = indicator.transform.eulerAngles;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 1f + t * 0.5f;
                indicator.transform.localScale = startScale * scale;
                indicator.transform.eulerAngles = startRot + new Vector3(0, t * 180f, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        public List<SquadUnit> GetSelectedUnits()
        {
            return selectedUnits;
        }
    }
}
