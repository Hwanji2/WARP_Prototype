using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragManager : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject zonePrefab;
    private Vector2 dragStartPos;
    private GameObject currentZone;
    private GameObject blueZoneObj;
    private GameObject redZoneObj;
    private Rect blueZone;
    private Rect redZone;
    private bool isDragging = false;
    private int dragCount = 0;
    private float lastZoneTime = -10f;
    private float zoneCooldown = 1f;

    private class RewindData
    {
        public GameObject obj;
        public Vector3 startPosition;
    }
    private List<RewindData> rewindList = new List<RewindData>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastZoneTime > zoneCooldown)
        {
            dragStartPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;

            currentZone = Instantiate(zonePrefab);
            var sr = currentZone.GetComponent<SpriteRenderer>();
            var color = dragCount == 0 ? new Color(0, 0.6f, 1, 0.4f) : new Color(1, 0.3f, 0.3f, 0.4f);
            sr.color = color;

            lastZoneTime = Time.time;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            UpdateRectVisual(currentZone, dragStartPos, currentMousePos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Vector2 endPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Rect zone = GetRectFromPoints(dragStartPos, endPos);

            if (dragCount == 0)
            {
                blueZone = zone;
                blueZoneObj = currentZone;
                dragCount = 1;
                RecordRewindTargets();
            }
            else if (dragCount == 1)
            {
                redZone = zone;
                redZoneObj = currentZone;
                RemoveSubZoneObjectsFromRewind();
                TryMoveObjects();
            }
        }
    }

    void LateUpdate()
    {
        if (currentZone != null)
        {
            var sr = currentZone.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            c.a = 0.3f + Mathf.Sin(Time.time * 4f) * 0.1f;
            sr.color = c;
        }
    }

    void UpdateRectVisual(GameObject zoneObj, Vector2 start, Vector2 end)
    {
        Vector2 size = end - start;
        Vector2 center = start + size / 2f;
        zoneObj.transform.position = center;
        zoneObj.transform.localScale = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 1f);
    }

    Rect GetRectFromPoints(Vector2 p1, Vector2 p2)
    {
        Vector2 min = Vector2.Min(p1, p2);
        Vector2 max = Vector2.Max(p1, p2);
        return new Rect(min, max - min);
    }

    void RecordRewindTargets()
    {
        rewindList.Clear();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Draggable"))
        {
            if (!IsInsideZone(obj.transform.position, blueZone))
            {
                rewindList.Add(new RewindData
                {
                    obj = obj,
                    startPosition = obj.transform.position
                });
            }
        }
    }

    void RemoveSubZoneObjectsFromRewind()
    {
        Collider2D[] subHits = Physics2D.OverlapBoxAll(redZone.center, redZone.size, 0f);
        foreach (var col in subHits)
        {
            rewindList.RemoveAll(data => data.obj == col.gameObject);
        }
    }

    void TryMoveObjects()
    {
        List<GameObject> toMove = new List<GameObject>();
        Collider2D[] hits = Physics2D.OverlapBoxAll(redZone.center, redZone.size, 0f);
        bool hasConflict = false;

        foreach (var col in hits)
        {
            if (col.CompareTag("Draggable"))
            {
                GameObject obj = col.gameObject;
                Vector2 offset = (Vector2)obj.transform.position - redZone.position;
                Vector2 newPosition = blueZone.position + offset;

                Bounds objBounds = obj.GetComponent<Collider2D>().bounds;
                Vector2 objSize = objBounds.size;
                Rect movedRect = new Rect(newPosition - objSize / 2f, objSize);

                if (CheckConflictWithOthers(movedRect, obj))
                {
                    hasConflict = true;
                    break;
                }

                toMove.Add(obj);
            }
        }

        if (hasConflict)
        {
            StartCoroutine(DestroyZoneWithEffect(redZoneObj, Color.red, false, false));
            StartCoroutine(DestroyZoneWithEffect(blueZoneObj, Color.blue, false, true));
        }
        else
        {
            foreach (var obj in toMove)
            {
                Vector2 offset = (Vector2)obj.transform.position - redZone.position;
                obj.transform.position = blueZone.position + offset;
            }

            foreach (var data in rewindList)
            {
                StartCoroutine(RewindObject(data));
            }

            StartCoroutine(DestroyZoneWithEffect(redZoneObj, Color.red, true, false));
            StartCoroutine(DestroyZoneWithEffect(blueZoneObj, Color.blue, true, true));
        }
    }

    bool IsInsideZone(Vector2 pos, Rect zone)
    {
        return zone.Contains(pos);
    }

    bool CheckConflictWithOthers(Rect targetRect, GameObject self)
    {
        Collider2D[] others = Physics2D.OverlapBoxAll(targetRect.center, targetRect.size, 0f);
        foreach (var col in others)
        {
            if (col.gameObject != self && col.CompareTag("Draggable"))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator RewindObject(RewindData data)
    {
        SpriteRenderer sr = data.obj.GetComponent<SpriteRenderer>();
        Collider2D col = data.obj.GetComponent<Collider2D>();

        if (col != null) col.enabled = false;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.4f;
            sr.color = c;
        }

        float duration = 0.5f;
        float t = 0f;
        Vector3 from = data.obj.transform.position;
        Vector3 to = data.startPosition;

        while (t < duration)
        {
            t += Time.deltaTime;
            data.obj.transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }

        data.obj.transform.position = to;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        if (col != null) col.enabled = true;
    }

    IEnumerator DestroyZoneWithEffect(GameObject zone, Color color, bool merge, bool reset)
    {
        if (zone == null) yield break;

        var sr = zone.GetComponent<SpriteRenderer>();
        float t = 0f;
        float duration = 0.3f;
        Vector3 originalScale = zone.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            if (merge)
            {
                zone.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
                sr.color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0), progress);
            }
            else
            {
                float shake = Mathf.Sin(progress * 40f) * 0.1f;
                zone.transform.localScale = originalScale + new Vector3(shake, shake, 0);
                sr.color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0), progress);
            }

            yield return null;
        }

        Destroy(zone);

        if (reset)
        {
            ResetDrag();
        }
    }

    void ResetDrag()
    {
        dragCount = 0;
        blueZone = new Rect();
        redZone = new Rect();
        blueZoneObj = null;
        redZoneObj = null;
        rewindList.Clear();
    }
}
