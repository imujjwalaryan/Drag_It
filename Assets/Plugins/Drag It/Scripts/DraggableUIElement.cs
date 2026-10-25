using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RyujinInteractive.CustomUI
{
    /// <summary>
    /// Makes a Unity UI element and its children draggable at runtime while Edit Mode is enabled.
    /// The element is kept within its parent's RectTransform, and its local X and Y position
    /// are stored in PlayerPrefs. Supports pixel-grid snapping and Unity UI graphics, including TextMesh Pro UGUI.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DraggableUIElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>Prefix used to build the PlayerPrefs keys for the saved local X and Y position.</summary>
        [Tooltip("Unique key prefix for saving the control position")]
        [SerializeField]
        private string saveKey = "controls_group_pos";

        /// <summary>Whether the container currently responds to drag input.</summary>
        [Tooltip("Dragging only works while true. Toggle from an 'Edit Controls' button.")]
        [SerializeField]
        private bool editMode = false;

        /// <summary>Grid interval in screen pixels. Set to zero to turn snapping off.</summary>
        [Tooltip("Grid snap interval in pixels. Set to 0 to disable snapping.")]
        [Range(0f, 100f)]
        [SerializeField]
        private float snapStepPixels = 10f;

        /// <summary>RectTransform attached to this GameObject.</summary>
        private RectTransform rect;
        /// <summary>RectTransform that bounds this control container.</summary>
        private RectTransform parentRect;
        /// <summary>Graphic that receives pointer events for dragging.</summary>
        private Graphic handle;
        /// <summary>Child graphics whose raycast settings are temporarily changed in Edit Mode.</summary>
        private Graphic[] children;
        /// <summary>Original raycastTarget values restored when Edit Mode is disabled.</summary>
        private bool[] childRaycastDefaults;
        /// <summary>Offset from the pointer to the container position at the start of a drag.</summary>
        private Vector2 grabOffset;

        /// <summary>Invoked after Edit Mode is enabled.</summary>
        public UnityEvent OnEditModeEnter;
        /// <summary>Invoked after Edit Mode is disabled.</summary>
        public UnityEvent OnEditModeExit;

        /// <summary>Gets or sets whether dragging is enabled.</summary>
        public bool EditMode
        {
            get => editMode;
            set
            {
                editMode = value;
                ApplyMode();
            }
        }
        

       
        /// <summary>Enables dragging and invokes <see cref="OnEditModeEnter"/>.</summary>
        public void EnableEditMode()
        {
            EditMode = true;
            OnEditModeEnter?.Invoke();
        }
        
        /// <summary>Disables dragging and invokes <see cref="OnEditModeExit"/>.</summary>
        public void DisableEditMode()
        {
            EditMode = false;
            OnEditModeExit?.Invoke();
        }

        /// <summary>Initializes the drag handle and child raycast state, then restores the saved position.</summary>
        private void Awake()
        {
            rect = (RectTransform)transform;
            parentRect = (RectTransform)rect.parent;

            handle = GetComponent<Graphic>();
            if (handle == null)
            {
                Image img = gameObject.AddComponent<Image>();
                img.color = Color.clear;
                handle = img;
            }

            CacheChildren();
            ApplyMode();
            LoadPosition();
        }

        /// <summary>Caches child graphics and their original raycastTarget settings.</summary>
        private void CacheChildren()
        {
            var list = new List<Graphic>();
            foreach (Graphic g in GetComponentsInChildren<Graphic>(true))
                if (g != handle)
                    list.Add(g);

            children = list.ToArray();
            childRaycastDefaults = new bool[children.Length];
            for (int i = 0; i < children.Length; i++)
                childRaycastDefaults[i] = children[i].raycastTarget;
        }

        /// <summary>Keeps the container raycastable and switches child raycasts based on Edit Mode.</summary>
        private void ApplyMode()
        {
            if (handle == null) return;

            // Keep the container's drag surface raycastable; Edit Mode gates dragging in the event handlers.
            handle.raycastTarget = true;
            for (int i = 0; i < children.Length; i++)
                children[i].raycastTarget = editMode ? false : childRaycastDefaults[i];
        }

        /// <summary>Records the pointer offset at drag start to prevent the container from snapping.</summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!editMode) return;
            if (TryGetLocalPoint(eventData, out Vector2 local))
                grabOffset = (Vector2)rect.localPosition - local;
        }

        /// <summary>Moves the element with the pointer, snaps its position, and clamps it within the parent.</summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!editMode) return;
            if (!TryGetLocalPoint(eventData, out Vector2 local)) return;

            Vector2 targetPosition = SnapToGrid(local + grabOffset);
            rect.localPosition = ClampToParent(targetPosition);
        }

        /// <summary>Snaps a parent-local position to the configured grid interval in canvas pixels.</summary>
        private Vector2 SnapToGrid(Vector2 position)
        {
            if (snapStepPixels <= 0f) return position;

            float canvasScale = 1f;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.rootCanvas.scaleFactor > 0f)
                canvasScale = canvas.rootCanvas.scaleFactor;

            float step = snapStepPixels / canvasScale;
            position.x = Mathf.Round(position.x / step) * step;
            position.y = Mathf.Round(position.y / step) * step;
            return position;
        }

        /// <summary>Saves the local position when an Edit Mode drag ends.</summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!editMode) return;
            SavePosition();
        }

        /// <summary>Converts the pointer screen position into the parent's local coordinate space.</summary>
        private bool TryGetLocalPoint(PointerEventData e, out Vector2 local)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, e.position, e.pressEventCamera, out local);
        }

        /// <summary>Clamps the container position so its pivot-aware bounds remain within the parent rect.</summary>
        private Vector2 ClampToParent(Vector2 pos)
        {
            Rect p = parentRect.rect;
            Vector2 size = Vector2.Scale(rect.rect.size, rect.localScale);
            Vector2 pivot = rect.pivot;

            float minX = p.xMin + size.x * pivot.x;
            float maxX = p.xMax - size.x * (1f - pivot.x);
            float minY = p.yMin + size.y * pivot.y;
            float maxY = p.yMax - size.y * (1f - pivot.y);

            pos.x = Mathf.Clamp(pos.x, minX, Mathf.Max(minX, maxX));
            pos.y = Mathf.Clamp(pos.y, minY, Mathf.Max(minY, maxY));
            return pos;
        }

        /// <summary>Saves the container's local X and Y coordinates in PlayerPrefs (remove and use your own save logic here).</summary>
        private void SavePosition()
        {
            Vector3 position = rect.localPosition;
            PlayerPrefs.SetFloat(saveKey + "_x", position.x);
            PlayerPrefs.SetFloat(saveKey + "_y", position.y);
            PlayerPrefs.Save();
        }

        /// <summary>Loads the saved local X and Y coordinates and clamps them to the parent rect (remove and use your own load logic here).</summary>
        private void LoadPosition()
        {
            string xKey = saveKey + "_x";
            string yKey = saveKey + "_y";
            if (!PlayerPrefs.HasKey(xKey) || !PlayerPrefs.HasKey(yKey)) return;

            Vector3 position = rect.localPosition;
            position.x = PlayerPrefs.GetFloat(xKey);
            position.y = PlayerPrefs.GetFloat(yKey);
            rect.localPosition = ClampToParent(position);
        }
    }
}
