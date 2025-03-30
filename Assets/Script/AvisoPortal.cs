using UnityEngine;
using UnityEngine.UI;

public class AvisoPortal : MonoBehaviour
{
    [System.Serializable]
    public class CompassTarget
    {
        public RectTransform compassImage;
        public Transform worldTarget;
        [HideInInspector] public Image imageComponent;
    }

    [Header("Configuración")]
    public CompassTarget[] targets;  // Array de objetivos
    public RectTransform boundaryPanel;
    public float smoothSpeed = 5f;

    [Header("Visibilidad")]
    public bool hideWhenTargetVisible = true;
    [Range(0, 1)] public float visibleThreshold = 0.95f;

    private Camera mainCamera;
    private Vector3[] panelCorners = new Vector3[4];
    private Rect panelRect;

    private void Start()
    {
        mainCamera = Camera.main;
        UpdatePanelBounds();

        // Inicializar componentes de imagen
        foreach (var target in targets)
        {
            if (target.compassImage != null)
            {
                target.imageComponent = target.compassImage.GetComponent<Image>();
            }
        }
    }

    private void Update()
    {
        UpdateAllCompassPositions();
    }

    private void UpdatePanelBounds()
    {
        boundaryPanel.GetWorldCorners(panelCorners);
        panelRect = new Rect(
            panelCorners[0].x,
            panelCorners[0].y,
            panelCorners[2].x - panelCorners[0].x,
            panelCorners[2].y - panelCorners[0].y
        );
    }

    private void UpdateAllCompassPositions()
    {
        foreach (var target in targets)
        {
            if (target.compassImage == null || target.worldTarget == null) continue;

            UpdateSingleCompass(target);
            CheckTargetVisibility(target);
        }
    }

    private void UpdateSingleCompass(CompassTarget target)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.worldTarget.position);
        if (screenPos.z < 0) screenPos *= -1;

        Vector2 edgePosition = GetPerimeterPosition(screenPos);
        target.compassImage.position = Vector2.Lerp(
            target.compassImage.position,
            edgePosition,
            smoothSpeed * Time.deltaTime
        );
    }

    private Vector2 GetPerimeterPosition(Vector2 targetScreenPos)
    {
        Vector2 panelCenter = new Vector2(
            panelRect.x + panelRect.width / 2,
            panelRect.y + panelRect.height / 2
        );

        Vector2 direction = (targetScreenPos - panelCenter).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x);
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float maxX = Mathf.Abs((panelRect.width / 2) / cos);
        float maxY = Mathf.Abs((panelRect.height / 2) / sin);

        return panelCenter + direction * Mathf.Min(maxX, maxY);
    }

    private void CheckTargetVisibility(CompassTarget target)
    {
        if (!hideWhenTargetVisible || target.imageComponent == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.worldTarget.position);
        bool isFullyVisible =
            viewportPos.z > 0 &&
            viewportPos.x > (1 - visibleThreshold) &&
            viewportPos.x < visibleThreshold &&
            viewportPos.y > (1 - visibleThreshold) &&
            viewportPos.y < visibleThreshold;

        target.imageComponent.enabled = !isFullyVisible;
    }
}