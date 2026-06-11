using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemPreviewWindow : EditorWindow
{
    private PreviewPivot _previewPivot;
    private Vector3 _pivotPosition;
    private Quaternion _pivotRotation;
    private Quaternion _iconRotation;
    private float _editIconBoundsScale = 1f;
    private Vector3 _editScale = Vector3.one;
    private ItemPreview _itemPreview;
    private GameObject _itemToRender;
    private static ItemPreviewWindow _window;
    private bool _showPreview;
    private ItemPreview _itemPreviewInstance;
    private GameObject _itemToRenderInstance;
    private Scene _previewScene;
    private Texture2D _generatedIcon;
    private int _itemWidth = 1;
    private int _itemHeight = 1;
    private Quaternion _originalRotation;
    private Vector2 _scrollPos;
    private const float PreviewWidth = 400f;
    private const float PreviewHeight = 225f;
    private const float LeftPanelMaxWidth = 350f;
    private const float ColumnSpacing = 10f;
    private Quaternion _modelPreviewRotation;
    private RenderTexture _previewRT;
    private GameObject _pivotMarker;
    private bool _showPivotGizmo = true;
    private bool _rotateX = true;
    private bool _rotateY = true;
    private readonly bool _rotateZ = true;
    private float _zRotationValue;
    private float _lastZRotation;
    private float _gizmoScaleFactor = 0.4f;
    private bool _invertRotation;
    private const string InvertRotationPrefKey = "ItemPreview_InvertRotation";

    [MenuItem("Custom Windows/Item Preview")]
    static void Init()
    {
        _window = GetWindow<ItemPreviewWindow>();
        _window.Focus(); 
        _window.titleContent = new GUIContent("Item Preview");
    }

    void OnEnable()
    {
        wantsMouseMove = true;  
        wantsMouseEnterLeaveWindow = true; 
        _window = this;
        _itemPreview = AssetDatabase
            .LoadAssetAtPath<GameObject>("Assets/Scripts/Custom/Item Preview/iconPreviewPrefab.prefab")
            .GetComponent<ItemPreview>();

        _previewRT = new RenderTexture((int)PreviewWidth, (int)PreviewHeight, 24, RenderTextureFormat.ARGB32);
        _previewRT.Create();

        _invertRotation = EditorPrefs.GetBool(InvertRotationPrefKey, false);

        Vector2 fixedSize = new Vector2(800, 850);
        minSize = fixedSize;
        maxSize = fixedSize;
    }

    private void OnDisable()
    {
        EditorPrefs.SetBool(InvertRotationPrefKey, _invertRotation);
        ClosePreviewWindow();
        _showPreview = false;
        if (_previewRT != null)
        {
            _previewRT.Release();
            DestroyImmediate(_previewRT);
        }
    }
    private void SanitizeLoadedPreviewValues()
    {
        if (!IsFinite(_pivotPosition)) _pivotPosition = Vector3.zero;
        if (!IsFinite(_editScale) || Mathf.Approximately(_editScale.x, 0f) || Mathf.Approximately(_editScale.y, 0f) || Mathf.Approximately(_editScale.z, 0f))
            _editScale = Vector3.one;

        if (!IsFinite(_iconRotation))
            _iconRotation = Quaternion.identity;

        if (_editIconBoundsScale <= 0f || float.IsNaN(_editIconBoundsScale) || float.IsInfinity(_editIconBoundsScale))
            _editIconBoundsScale = 0.9f;
    }

    private bool IsFinite(Vector3 v) =>
        !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
          float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));

    private bool IsFinite(Quaternion q) =>
        !(float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w) ||
          float.IsInfinity(q.x) || float.IsInfinity(q.y) || float.IsInfinity(q.z) || float.IsInfinity(q.w));
    void OnGUI()
    {
        var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.MaxWidth(LeftPanelMaxWidth), GUILayout.ExpandWidth(true));
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
        DrawLeftPanelContent(style);
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(ColumnSpacing);

        GUILayout.BeginVertical(GUILayout.Width(PreviewWidth));
        GUILayout.Label("Model Preview", style);
        GUILayout.Space(5f);
        Rect previewRect = GUILayoutUtility.GetRect(PreviewWidth, PreviewHeight,
            GUILayout.Width(PreviewWidth),
            GUILayout.Height(PreviewHeight));
        RenderPreviewIntoRect(previewRect);
        GUILayout.Space(5f);
        GUILayout.Label("Drag = Rotate | Scroll = Zoom",
            new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 11 });
        GUILayout.EndVertical();

        GUILayout.EndHorizontal(); 

        GUILayout.Space(15f);
        GUILayout.Label("Rendered Icon", style);
        DrawIconPreviewPanel();

        GUILayout.EndVertical(); 
    }

    private void DrawLeftPanelContent(GUIStyle style)
    {
        GUILayout.BeginVertical(GUILayout.MaxWidth(LeftPanelMaxWidth), GUILayout.ExpandWidth(true));
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

        GUILayout.Space(5f);

        EditorGUI.BeginChangeCheck();
        _itemToRender =
            (GameObject)EditorGUILayout.ObjectField("Item to preview", _itemToRender, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            ClosePreviewWindow();
            if (_itemToRender == null)
            {
                _showPreview = false;
                Repaint();
 
                return;
            }

            _showPreview = true;
            SetupItemPreviewWindow(loadFromComponent: true);
            Focus();
            Repaint();
        }

        GUILayout.Space(10f);
        GUILayout.Label("PreviewPivot Controls", style);

        if (_previewPivot != null)
        {
            Vector3 oldPivotPos = _pivotPosition;
            Vector3 oldScale = _editScale;

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Pivot Position", EditorStyles.label);
            _pivotPosition.x = DraggableFloat("X", _pivotPosition.x, 0.001f);
            _pivotPosition.y = DraggableFloat("Y", _pivotPosition.y, 0.001f);
            _pivotPosition.z = DraggableFloat("Z", _pivotPosition.z, 0.001f);

            _editIconBoundsScale = EditorGUILayout.FloatField("Icon Bounds Scale", _editIconBoundsScale);
            _editScale = EditorGUILayout.Vector3Field("Scale", _editScale);

            if (EditorGUI.EndChangeCheck() && _itemToRenderInstance != null)
            {
                var mesh = _itemToRenderInstance.transform;

                if (_pivotPosition != oldPivotPos)
                {
                    Vector3 localOffset = _iconRotation * _pivotPosition;
                    mesh.localPosition = -localOffset;
                }

                if (_editScale != oldScale)
                {
                    mesh.localScale = _editScale;
                }

                Repaint();
            }
        }
        else
        {
            GUILayout.Label("No PreviewPivot found", EditorStyles.helpBox);
        }

        GUILayout.Space(5f);
        EditorGUI.BeginChangeCheck();
        _showPivotGizmo = EditorGUILayout.Toggle("Show Pivot Gizmo", _showPivotGizmo);
        GUILayout.Space(5f);
        _gizmoScaleFactor = EditorGUILayout.Slider("Gizmo Size", _gizmoScaleFactor, 0.22f, 3f);
        if (_pivotMarker != null)
        {
            _pivotMarker.transform.localScale = Vector3.one * _gizmoScaleFactor;
        }

        if (EditorGUI.EndChangeCheck())
        {
            if (_showPivotGizmo)
            {
                CreatePivotMarker();
            }
            else
            {
                if (_pivotMarker != null)
                {
                    DestroyImmediate(_pivotMarker);
                    _pivotMarker = null;
                }
            }

            Repaint();
        }

        GUILayout.Space(5f);
        GUILayout.Label("Rotation Axes", EditorStyles.boldLabel);
        GUILayout.Space(5f);
        _invertRotation = EditorGUILayout.Toggle("Invert Drag Rotation", _invertRotation);
        EditorGUILayout.BeginHorizontal();
        _rotateX = GUILayout.Toggle(_rotateX, "X", "Button");
        _rotateY = GUILayout.Toggle(_rotateY, "Y", "Button");
        if (_rotateZ)
        {
            EditorGUI.BeginChangeCheck();
            float newZ = EditorGUILayout.Slider("Z Rotation", _zRotationValue, -45f, 45f); 
            if (EditorGUI.EndChangeCheck())
            {
                float deltaZ = newZ - _lastZRotation;
                ApplyZDelta(deltaZ);
                _lastZRotation = newZ;
                _zRotationValue = newZ;
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15f);
        GUILayout.Label("Icon Generator", style);
        GUILayout.Space(5f);

        _itemHeight = EditorGUILayout.IntField("Item Height", _itemHeight);
        _itemWidth = EditorGUILayout.IntField("Item Width", _itemWidth);

        if (GUILayout.Button("↩ Reset to Component Values", GUILayout.Height(24f)))
        {
            if (_previewPivot != null)
            {
                bool confirm = EditorUtility.DisplayDialog("Confirm Reset",
                    "Reset all values to the component's saved values? Any unsaved changes will be lost.",
                    "Yes", "Cancel");
                if (confirm)
                {
                    _pivotPosition = _previewPivot.pivotPosition;
                    _pivotRotation = _previewPivot.pivotRotation;
                    _iconRotation = _previewPivot.Icon.rotation;
                    _editIconBoundsScale = _previewPivot.Icon.boundsScale;
                    _editScale = _previewPivot.scale;
                    _zRotationValue = 0;
                    _lastZRotation = 0;

                    if (_itemToRenderInstance != null)
                    {
                        var mesh = _itemToRenderInstance.transform;
                        mesh.localRotation = _iconRotation;
                        mesh.localPosition = -(_iconRotation * _pivotPosition);
                        mesh.localScale = _editScale;
                    }

                    Repaint();
                    Debug.Log("↩ Reset to component values.");
                }
            }
        }

        if (GUILayout.Button("↩ Reset ALL Values", GUILayout.Height(24f)))
        {
            bool confirm = EditorUtility.DisplayDialog("Confirm Reset",
                "Reset ALL values to zero/default? This will set pivot position to zero, scale to one, rotation to identity, etc.",
                "Yes", "Cancel");
            if (confirm)
            {
                ResetAllPreviewPivotValues();
            }
        }
        if (GUILayout.Button("Render Icon", GUILayout.Height(28f)))
        {
            if (_previewPivot != null && _itemToRenderInstance != null && _itemToRender != null)
            {
                RenderIcon();
                Debug.Log($"💾 Rendered icon preview for {_previewPivot.name}");
            }
        }
        
        if (GUILayout.Button("Save & Render Icon", GUILayout.Height(28f)))
        {
            if (_previewPivot != null && _itemToRenderInstance != null && _itemToRender != null)
            {
                Undo.RecordObject(_previewPivot, "Save PreviewPivot values");

                var mesh = _itemToRenderInstance.transform;

                _previewPivot.pivotPosition = _pivotPosition;
                _previewPivot.pivotRotation = _pivotRotation;

                _previewPivot.Icon.rotation = mesh.localRotation;
                _previewPivot.Icon.boundsScale = _editIconBoundsScale;
                _previewPivot.scale = _editScale;

                EditorUtility.SetDirty(_previewPivot);
                PrefabUtility.RecordPrefabInstancePropertyModifications(_previewPivot);
                AssetDatabase.SaveAssets();

                _pivotPosition = _previewPivot.pivotPosition;
                _pivotRotation = _previewPivot.pivotRotation;
                _iconRotation = _previewPivot.Icon.rotation;
                _editIconBoundsScale = _previewPivot.Icon.boundsScale;
                _editScale = _previewPivot.scale;
                Debug.Log($"💾 Saved ALL PreviewPivot values to {_previewPivot.name}");
                RenderIcon();
                Debug.Log($"💾 Rendered icon preview for {_previewPivot.name}");
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical(); 
    }
    private void ApplyZDelta(float deltaZ)
    {
        if (_itemToRenderInstance == null || _itemPreviewInstance == null) return;

        Camera cam = _itemPreviewInstance.previewCamera;
        Quaternion zDeltaRot = Quaternion.AngleAxis(deltaZ, cam.transform.forward);
    
        _iconRotation = zDeltaRot * _iconRotation;

        Transform mesh = _itemToRenderInstance.transform;
        mesh.localRotation = _iconRotation;
        mesh.localPosition = -(_iconRotation * _pivotPosition);
        Repaint();
    }
    private void DrawIconPreviewPanel()
    {
        if (_generatedIcon == null)
        {
            GUILayout.Box("No icon rendered yet. Adjust settings and click 'Save & Render Icon'.",
                GUILayout.ExpandWidth(true), GUILayout.Height(60));
            return;
        }

        float aspect = (float)_itemWidth / _itemHeight;
        float maxWidth = position.width - 30f;
        float maxHeight = 300f;

        float previewWidth = maxWidth;
        float previewHeight = previewWidth / aspect;
        if (previewHeight > maxHeight)
        {
            previewHeight = maxHeight;
            previewWidth = previewHeight * aspect;
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Rect iconRect = GUILayoutUtility.GetRect(previewWidth, previewHeight,
            GUILayout.Width(previewWidth),
            GUILayout.Height(previewHeight));
        EditorGUI.DrawPreviewTexture(iconRect, _generatedIcon, null, ScaleMode.ScaleToFit);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Label($"Rendered size: {_itemWidth} x {_itemHeight} (aspect {aspect:F2})",
            EditorStyles.centeredGreyMiniLabel);
    }

    private void CreatePivotMarker()
    {
        if (_pivotMarker != null || _itemPreviewInstance == null) return;

        _pivotMarker = new GameObject("PivotGizmo");
        _pivotMarker.transform.SetParent(_itemPreviewInstance.previewPivot, false);
        _pivotMarker.transform.localPosition = Vector3.zero;
        _pivotMarker.transform.localRotation = Quaternion.identity;
        _pivotMarker.transform.localScale = Vector3.one;

        Material ringMat = new Material(Shader.Find("Unlit/Color"))
        {
            color = new Color(1f, 0.2f, 0.2f, 0.6f),
            renderQueue = 3000
        };

        Material centerMat = new Material(Shader.Find("Unlit/Color"))
        {
            color = new Color(1f, 0.5f, 0.5f, 0.9f),
            renderQueue = 3000
        };

        GameObject centerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(centerSphere.GetComponent<Collider>());
        centerSphere.name = "Center";
        centerSphere.transform.SetParent(_pivotMarker.transform, false);
        centerSphere.transform.localPosition = Vector3.zero;
        centerSphere.transform.localScale = Vector3.one * 0.02f;
        centerSphere.GetComponent<MeshRenderer>().material = centerMat;

        GameObject ringHoriz = new GameObject("RingHorizontal");
        ringHoriz.transform.SetParent(_pivotMarker.transform, false);
        ringHoriz.transform.localPosition = Vector3.zero;
        ringHoriz.transform.localRotation = Quaternion.identity;
        ringHoriz.transform.localScale = Vector3.one;

        LineRenderer lrH = ringHoriz.AddComponent<LineRenderer>();
        lrH.useWorldSpace = false;
        lrH.loop = true;
        lrH.positionCount = 32;
        lrH.startWidth = 0.003f;
        lrH.endWidth = 0.003f;
        lrH.material = ringMat;
        lrH.startColor = ringMat.color;
        lrH.endColor = ringMat.color;

        float radius = 0.075f; 
        Vector3[] pointsH = new Vector3[32];
        for (int i = 0; i < 32; i++)
        {
            float angle = (i / 32f) * Mathf.PI * 2;
            pointsH[i] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }

        lrH.SetPositions(pointsH);

        GameObject ringVert = new GameObject("RingVertical");
        ringVert.transform.SetParent(_pivotMarker.transform, false);
        ringVert.transform.localPosition = Vector3.zero;
        ringVert.transform.localRotation = Quaternion.Euler(90, 0, 0);
        ringVert.transform.localScale = Vector3.one;

        LineRenderer lrV = ringVert.AddComponent<LineRenderer>();
        lrV.useWorldSpace = false;
        lrV.loop = true;
        lrV.positionCount = 32;
        lrV.startWidth = 0.003f;
        lrV.endWidth = 0.003f;
        lrV.material = ringMat;
        lrV.startColor = ringMat.color;
        lrV.endColor = ringMat.color;

        Vector3[] pointsV = new Vector3[32];
        for (int i = 0; i < 32; i++)
        {
            float angle = (i / 32f) * Mathf.PI * 2;
            pointsV[i] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }

        lrV.SetPositions(pointsV);
    }

    private void RenderPreviewIntoRect(Rect previewRect)
    {
        if (!_showPreview || _itemPreviewInstance == null) return;

        Camera cam = _itemPreviewInstance.previewCamera;

        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.targetTexture = _previewRT;
        cam.aspect = PreviewWidth / PreviewHeight;
        var b = cam.renderingPath == RenderingPath.Forward;
        if (!b)
            cam.renderingPath = RenderingPath.Forward;
        cam.Render();

        GUI.DrawTexture(previewRect, _previewRT, ScaleMode.ScaleToFit, false);
        GUI.Box(previewRect, "", new GUIStyle("box"));
        cam.targetTexture = null;

        var e = Event.current;
        if (previewRect.Contains(e.mousePosition))
        {
            switch (e.type)
            {
                case EventType.MouseDrag when e.button == 0:
                    if (_itemToRenderInstance != null)
                    {
                        float rotSpeed = 0.5f;
                        Quaternion deltaRot = Quaternion.identity;

                        float dx = _invertRotation ? e.delta.x : -e.delta.x;
                        float dy = _invertRotation ? e.delta.y : -e.delta.y;

                        if (_rotateY)
                        {
                            Quaternion yawRot = Quaternion.AngleAxis(dx * rotSpeed, Vector3.up);
                            deltaRot = yawRot * deltaRot;
                        }

                        if (_rotateX)
                        {
                            Quaternion pitchRot = Quaternion.AngleAxis(-dy * rotSpeed, Vector3.right);
                            deltaRot = pitchRot * deltaRot;
                        }

                        _iconRotation = deltaRot * _iconRotation;

                        _zRotationValue = 0;
                        _lastZRotation = 0;

                        Transform mesh = _itemToRenderInstance.transform;
                        mesh.localRotation = _iconRotation;
                        mesh.localPosition = -(_iconRotation * _pivotPosition);
                        Repaint();
                    }

                    e.Use();
                    break;


                case EventType.ScrollWheel:
                    _itemPreviewInstance.Zoom(-e.delta.y / 20f);
                    Repaint();
                    e.Use();
                    break;
            }
        }
    }

    private void ResetAllPreviewPivotValues()
    {
        _pivotPosition = Vector3.zero;
        _pivotRotation = Quaternion.identity;
        _iconRotation = Quaternion.identity;
        _editScale = Vector3.one;
        _editIconBoundsScale = 0.9f;
        _zRotationValue = 0;
        _lastZRotation = 0;

        if (_itemPreviewInstance != null && _itemToRenderInstance != null)
        {
            var pp = _itemPreviewInstance.previewPivot;
            var mesh = _itemToRenderInstance.transform;

            pp.localPosition = Vector3.zero;
            pp.localRotation = Quaternion.identity;
            pp.localScale = Vector3.one;

            mesh.localPosition = Vector3.zero;
            mesh.localRotation = Quaternion.identity;
            mesh.localScale = Vector3.one;

            mesh.localRotation = _iconRotation;
            Vector3 localOffset = _iconRotation * _pivotPosition;
            mesh.localPosition = -localOffset;
            mesh.localScale = _editScale;

            Repaint();
        }
    }

    private float DraggableFloat(string label, float value, float step = 0.01f)
    {
        Rect rect = EditorGUILayout.GetControlRect();

        float labelWidth = 18f;
        Rect miniLabelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
        Rect fieldRect = new Rect(rect.x + labelWidth + 2f, rect.y, rect.width - labelWidth - 2f, rect.height);

        GUI.Label(miniLabelRect, label, EditorStyles.label);

        EditorGUIUtility.AddCursorRect(miniLabelRect, MouseCursor.SlideArrow);

        string text = EditorGUI.TextField(fieldRect, value.ToString("0.#####"));
        if (float.TryParse(text, out float parsed))
            value = parsed;

        int id = GUIUtility.GetControlID(FocusType.Passive, miniLabelRect);
        Event e = Event.current;

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:
                if (miniLabelRect.Contains(e.mousePosition) && e.button == 0)
                {
                    GUIUtility.hotControl = id;
                    e.Use();
                }

                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    float delta = e.delta.x * step;
                    value += delta;

                    GUI.changed = true;
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }

                break;
        }

        return value;
    }

    private void SetupItemPreviewWindow(bool loadFromComponent = false)
    {
        if (_itemPreviewInstance != null)
        {
            DestroyImmediate(_itemPreviewInstance.gameObject);
            _itemPreviewInstance = null;
        }

        if (_itemToRenderInstance != null)
        {
            DestroyImmediate(_itemToRenderInstance.gameObject);
            _itemToRenderInstance = null;
        }

        if (_previewScene.IsValid())
        {
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        _previewScene = EditorSceneManager.NewPreviewScene();
        _itemPreviewInstance = Instantiate(_itemPreview);
        SceneManager.MoveGameObjectToScene(_itemPreviewInstance.gameObject, _previewScene);
        _itemPreviewInstance.previewCamera.scene = _previewScene;

        _itemToRenderInstance = Instantiate(_itemToRender, _itemPreviewInstance.previewPivot);

        if (_pivotMarker != null) DestroyImmediate(_pivotMarker);
        if (_showPivotGizmo)
        {
            CreatePivotMarker();
        }

        _zRotationValue = 0;
        _lastZRotation = 0;
        _previewPivot = _itemToRender.GetComponent<PreviewPivot>();

        if (_previewPivot != null)
        {
            if (loadFromComponent || _itemPreviewInstance == null)
            {
                // Initial load from component
                _pivotPosition       = _previewPivot.pivotPosition;
                _pivotRotation       = _previewPivot.pivotRotation;
                _iconRotation        = _previewPivot.Icon.rotation;
                _editIconBoundsScale = _previewPivot.Icon.boundsScale;
                _editScale           = _previewPivot.scale;
                
                SanitizeLoadedPreviewValues();

                if (Mathf.Approximately(_editIconBoundsScale, 1f))
                    _editIconBoundsScale = 0.9f;
            }

            var pp   = _itemPreviewInstance.previewPivot;
            var mesh = _itemToRenderInstance.transform;

            pp.localPosition  = Vector3.zero;
            pp.localRotation  = Quaternion.identity;
            pp.localScale     = Vector3.one;

            // Always apply CURRENT editor values
            mesh.localRotation = _iconRotation;
            Vector3 localOffset = _iconRotation * _pivotPosition;
            mesh.localPosition  = -localOffset;
            mesh.localScale     = _editScale;
        }
        else
        {
            _itemToRenderInstance.transform.localPosition = ItemPreview.GetBounds(_itemToRenderInstance).center;
            _itemToRenderInstance.transform.localScale = Vector3.one;
        }
    }

    private void ClosePreviewWindow()
    {
        if (_pivotMarker != null) DestroyImmediate(_pivotMarker);
        DestroyImmediate(_itemPreviewInstance);
        DestroyImmediate(_itemToRenderInstance);
        EditorSceneManager.ClosePreviewScene(_previewScene);
    }

    private void RenderIcon()
    {
        ClosePreviewWindow();
        SetupItemPreviewWindow();

        if (_pivotMarker != null) _pivotMarker.SetActive(false);

        _itemPreviewInstance.ChangeLights();
        _generatedIcon = GenerateIcon();

        if (_pivotMarker != null) _pivotMarker.SetActive(true);

        ClosePreviewWindow();
        SetupItemPreviewWindow();
    }

    private Texture2D GenerateIcon()
    {
        var itemSize = new Vector2(_itemWidth, _itemHeight) * (64f * 3);
        _itemPreviewInstance.previewCamera.orthographic = true;
        _itemPreviewInstance.previewCamera.aspect = itemSize.x / itemSize.y;
        _itemToRenderInstance.transform.localScale = Vector3.one;

        var previewPivot = _itemToRenderInstance.GetComponent<PreviewPivot>();
        if (previewPivot != null)
        {
            PoseByPivot(previewPivot);
        }
        else
        {
            var bounds = ItemPreview.GetBounds(_itemToRenderInstance);
            PoseModelByBounds(bounds);
        }

        int x = (int)itemSize.x;
        int width = x * 2;
        int y = (int)itemSize.y;
        int height = y * 2;
        var temporary = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default, 8);
        temporary.name = "IconCreator TextureDouble";
        _itemPreviewInstance.previewCamera.gameObject.SetActive(true);
        _itemPreviewInstance.previewCamera.targetTexture = temporary;
        _itemPreviewInstance.previewCamera.clearFlags = CameraClearFlags.Color;
        _itemPreviewInstance.previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        _itemPreviewInstance.previewCamera.useOcclusionCulling = false;
        RenderTexture temporary2 = RenderTexture.GetTemporary(x, y);
        ClearTexture(temporary2);
        _itemPreviewInstance.previewCamera.Render();
        Graphics.Blit(temporary, temporary2);
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = temporary2;
        Texture2D texture2D = new Texture2D(x, y, TextureFormat.ARGB32, false)
            { filterMode = FilterMode.Trilinear, name = "icon" };
        texture2D.ReadPixels(
            new Rect(0f, 0f, _itemPreviewInstance.previewCamera.pixelWidth,
                _itemPreviewInstance.previewCamera.pixelHeight), 0, 0, false);
        texture2D.Apply();
        RenderTexture.active = active;
        _itemPreviewInstance.previewCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(temporary);
        RenderTexture.ReleaseTemporary(temporary2);
        return texture2D;
    }

    private void PoseByPivot(PreviewPivot previewPivot)
    {
        var pp = _itemPreviewInstance.previewPivot;
        var mesh = _itemToRenderInstance.transform;

        pp.localPosition = Vector3.zero;
        pp.localRotation = Quaternion.identity;

        mesh.localRotation = _iconRotation;
        Vector3 localOffset = _iconRotation * _pivotPosition;
        mesh.localPosition = -localOffset;
        mesh.localScale = _editScale;
        var bounds = ItemPreview.GetBounds(_itemToRenderInstance);
        float num = bounds.extents.x / bounds.extents.y;

        if (num > _itemPreviewInstance.previewCamera.aspect)
        {
            _itemPreviewInstance.previewCamera.orthographicSize =
                bounds.extents.x / _itemPreviewInstance.previewCamera.aspect / previewPivot.Icon.boundsScale;
        }
        else
        {
            _itemPreviewInstance.previewCamera.orthographicSize =
                bounds.extents.y / previewPivot.Icon.boundsScale;
        }
    }

    private void PoseModelByBounds(in Bounds bounds)
    {
        _itemPreviewInstance.previewCamera.orthographicSize = bounds.extents.y;
    }

    private void ClearTexture(RenderTexture tex)
    {
        RenderTexture active = RenderTexture.active;
        Graphics.SetRenderTarget(tex);
        GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
        RenderTexture.active = active;
    }
}