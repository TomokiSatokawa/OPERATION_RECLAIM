using System.Collections.Generic;
using Graph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// LineRendererのクリック判定を管理する
/// </summary>
public class LineClickDetector : MonoBehaviour
{
    [SerializeField] private List<LineRenderer> _lineRenderers = new();
    [SerializeField] private Camera _camera;
    [SerializeField] private float _clickDistance = 10f;

    /// <summary>
    /// クリックされた線を取得する
    /// </summary>
    public LineRenderer GetClickedLine()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        LineRenderer closestLine = null;
        float closestDistance = _clickDistance;

        foreach (LineRenderer lineRenderer in _lineRenderers)
        {
            if (lineRenderer == null || !lineRenderer.enabled)
            {
                continue;
            }

            for (int i = 0; i < lineRenderer.positionCount - 1; i++)
            {
                Vector3 start = lineRenderer.GetPosition(i);
                Vector3 end = lineRenderer.GetPosition(i + 1);

                if (!lineRenderer.useWorldSpace)
                {
                    start = lineRenderer.transform.TransformPoint(start);
                    end = lineRenderer.transform.TransformPoint(end);
                }

                Vector3 screenStart = _camera.WorldToScreenPoint(start);
                Vector3 screenEnd = _camera.WorldToScreenPoint(end);

                if (screenStart.z < 0f || screenEnd.z < 0f)
                {
                    continue;
                }

                float distance = Vector2.Distance(
                    mousePosition,
                    GetClosestPointOnSegment(
                        mousePosition,
                        screenStart,
                        screenEnd));

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLine = lineRenderer;
                }
            }
        }

        return closestLine;
    }

    /// <summary>
    /// 線分上で指定位置に最も近い点を取得する
    /// </summary>
    private Vector2 GetClosestPointOnSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 segment = end - start;
        float length = segment.sqrMagnitude;

        if (length == 0f)
        {
            return start;
        }

        float t = Mathf.Clamp01(
            Vector2.Dot(point - start, segment) / length);

        return start + segment * t;
    }

    public void AddLine(LineRenderer lineRenderer)
    {
        _lineRenderers.Add(lineRenderer);
    }

    public void RemveLine(LineRenderer lineRenderer)
    {
        _lineRenderers.Remove(lineRenderer);
    }
}