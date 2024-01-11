using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // INTERNAL VARIABLES
    bool snapWarning = false;
    private float cameraZ;

    // CAMERA SETTINGS
    public Vector2 cameraOffset;
    public bool offsetAppliesToSnapPoints = false;
    public bool playerVelocityEffectsZ = false;
    public float effectAmount = 0.5f;
    public float effectLimit = 5f;
    
    // PLAYER SETTINGS
    public GameObject player;
    public FollowMode followMode = FollowMode.Smooth;
    public float stiffness = 5f;
    public bool useUnscaledTime = false;

    // SNAP SETTINGS
    public bool snappingEnabled = true;
    public GameObject[] snapPoints = {};
    public float snapDistance = 10f;
    public float snapStiffness = 7f;

    public float GenerateZ() {
        float newZ = cameraZ;
        if (playerVelocityEffectsZ) {
            if (GetComponent<Camera>().orthographic) {
                throw new NotImplementedException("The Player Velocity Zoom effect currently only works with cameras in perspective mode.");
            }
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) {
                float velo = rb.velocity.magnitude;
                velo *= effectAmount;
                if (effectLimit != 0) {
                    if (velo < effectLimit) {
                        newZ = cameraZ - velo;
                        return newZ;
                    }
                    else {
                        return Mathf.Clamp(velo, 0, effectLimit);
                    }
                }
                else {
                    newZ = cameraZ - velo;
                    return newZ;
                }
            }
            else {
                Debug.LogError("Failed to get velocity from player. Player object does not have a Rigidbody2D component attatched");
                return cameraZ;
            }
        }
        else {
            return cameraZ;
        }
    }

    void Start() {
        cameraZ = transform.position.z;

        // EXTRA CODE HERE (Start)
    }

    void Update() {
        bool isSnapped = false;
        float newZ = GenerateZ();
        Vector3 newPos = new Vector3(player.transform.position.x + cameraOffset.x, player.transform.position.y + cameraOffset.y, newZ);
        if (snappingEnabled) {
            if (snapPoints.Length > 0) {
                foreach (GameObject point in snapPoints) {
                    if (Vector2.Distance(new Vector2(point.transform.position.x, point.transform.position.y), new Vector2(newPos.x, newPos.y)) < snapDistance) {
                        if (offsetAppliesToSnapPoints) {
                            newPos = new Vector3(point.transform.position.x + cameraOffset.x, point.transform.position.y + cameraOffset.y, newZ);
                        }
                        else {
                            newPos = new Vector3(point.transform.position.x, point.transform.position.y, newZ);
                        }
                        isSnapped = true;
                        break;
                    }
                }
            }
            else {
                if (!snapWarning) {
                    Debug.Log("Snapping is enabled but no snap points have been added. Make sure to add points to enable this functionality.");
                    snapWarning = true;
                }
            }
        }
        if (isSnapped) {
            if (useUnscaledTime) {
                if (followMode == FollowMode.Smooth)
                    transform.position = Vector3.Lerp(transform.position, newPos, snapStiffness * Time.unscaledDeltaTime);
                else if (followMode == FollowMode.Stuck)
                    transform.position = newPos;
            }
            else {
                if (followMode == FollowMode.Smooth)
                    transform.position = Vector3.Lerp(transform.position, newPos, snapStiffness * Time.deltaTime);
                else if (followMode == FollowMode.Stuck)
                    transform.position = newPos;
            }
        }
        else {
            if (useUnscaledTime) {
                if (followMode == FollowMode.Smooth)
                    transform.position = Vector3.Lerp(transform.position, newPos, stiffness * Time.deltaTime);
                else if (followMode == FollowMode.Stuck)
                    transform.position = newPos;
            }
            else {
                if (followMode == FollowMode.Smooth)
                    transform.position = Vector3.Lerp(transform.position, newPos, stiffness * Time.deltaTime);
                else if (followMode == FollowMode.Stuck)
                    transform.position = newPos;
            }
        }
    }
    
    // EXTRA CODE HERE (Update)
    
}

public enum FollowMode {
    Smooth,
    Stuck
}

// REMOVE THIS SECTION IF YOU WANT TO USE THE DEFAULT UNITY INSPECTOR

#if UNITY_EDITOR
[CustomEditor(typeof(CameraController))]
class CameraControllerEditor : Editor {
    // statics
    static bool _playerSettingsDropDown = false;
    static bool _snapSettingsDropDown = false;
    static bool arrayOpen = true;

    // camera settings
    private Vector2 _cameraOffset;
    private bool _offsetAppliesToSnapPoints;
    private bool _playerVelocityEffectsZ;
    private float _effectAmount;
    private float _effectLimit;

    // player setup
    private GameObject _player;
    FollowMode _followMode;
    private float _stiffness;
    private bool _useUnscaledTime;
    int fm;

    // snap settings
    private bool _snappingEnabled;
    private GameObject[] _snapPoints;
    private float _snapDistance;
    private float _snapStiffness;
    public override void OnInspectorGUI()
    {
        // GET VALUES
        serializedObject.Update();
        CameraController cc = (CameraController)target;
        _cameraOffset = cc.cameraOffset;
        _offsetAppliesToSnapPoints = cc.offsetAppliesToSnapPoints;
        _playerVelocityEffectsZ = cc.playerVelocityEffectsZ;
        _effectAmount = cc.effectAmount;
        _effectLimit = cc.effectLimit;

        _player = cc.player;
        _followMode = cc.followMode;
        _stiffness = cc.stiffness;
        _useUnscaledTime = cc.useUnscaledTime;
        
        _snappingEnabled = cc.snappingEnabled;
        _snapPoints = cc.snapPoints;
        _snapDistance = cc.snapDistance;
        _snapStiffness = cc.snapStiffness;


        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.background = Texture2D.whiteTexture;
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = Color.black;

        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader);
        foldoutStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("Camera Controller", headerStyle, GUILayout.MinHeight(25));

        
        _cameraOffset = EditorGUILayout.Vector2Field("Camera Offset", _cameraOffset);
        _offsetAppliesToSnapPoints = EditorGUILayout.Toggle("Offset Applies To Snap Points", _offsetAppliesToSnapPoints);
        _playerVelocityEffectsZ = EditorGUILayout.Toggle("Player Velocity Effects Z", _playerVelocityEffectsZ);
        if (!_playerVelocityEffectsZ)
            EditorGUI.BeginDisabledGroup(true);
        _effectAmount = EditorGUILayout.Slider("Effect Amount", _effectAmount, 0, 1);
        _effectLimit = EditorGUILayout.FloatField("Effect Zoom Limit", _effectLimit);
        EditorGUI.EndDisabledGroup();

        _playerSettingsDropDown = EditorGUILayout.Foldout(_playerSettingsDropDown, "Player Setup", true, foldoutStyle);
        if (_playerSettingsDropDown) {
            EditorGUI.indentLevel++;
            _player = EditorGUILayout.ObjectField("Player", _player, typeof(GameObject), true) as GameObject;
            fm = EditorGUILayout.IntPopup("Follow Mode", fm, new string[]{"Smooth", "Stuck"}, new int[]{0, 1});
            switch (fm) {
                case 0:
                    _followMode = FollowMode.Smooth;
                    break;
                case 1:
                    EditorGUI.BeginDisabledGroup(true);
                    _followMode = FollowMode.Stuck;
                    break;
            }
            _stiffness = EditorGUILayout.FloatField("Stiffness", _stiffness);
            EditorGUI.EndDisabledGroup();

            _useUnscaledTime = EditorGUILayout.Toggle("Use Unscaled Time", _useUnscaledTime);
            
            EditorGUI.indentLevel--;
        }

        _snapSettingsDropDown = EditorGUILayout.Foldout(_snapSettingsDropDown, "Snap Settings", true, foldoutStyle);
        if (_snapSettingsDropDown) {
            EditorGUI.indentLevel++;
            _snappingEnabled = EditorGUILayout.Toggle("Enable Snapping", _snappingEnabled);
            EditorGUI.BeginDisabledGroup(!_snappingEnabled);
            _snapPoints = GameObjectArrayField("Snap Points", ref arrayOpen, _snapPoints);

            _snapDistance = EditorGUILayout.FloatField("Snap Distance", _snapDistance);
            _snapStiffness = EditorGUILayout.FloatField("Snap Stiffness", _snapStiffness);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }


        // SETTING THE VARIABLES
        serializedObject.Update();
        cc.cameraOffset = _cameraOffset;
        cc.offsetAppliesToSnapPoints = _offsetAppliesToSnapPoints;
        cc.playerVelocityEffectsZ = _playerVelocityEffectsZ;
        cc.effectAmount = _effectAmount;
        cc.effectLimit = _effectLimit;

        cc.player = _player;
        cc.followMode = _followMode;
        cc.stiffness = _stiffness;
        cc.useUnscaledTime = _useUnscaledTime;
        
        cc.snappingEnabled = _snappingEnabled;
        cc.snapPoints = _snapPoints;
        cc.snapDistance = _snapDistance;
        cc.snapStiffness = _snapStiffness;
    }

    public GameObject[] GameObjectArrayField(string label, ref bool open, GameObject[] array) {
        GUIStyle bgStyle = new GUIStyle(EditorStyles.helpBox);
        bgStyle.fontSize = 12;

        // Create a foldout
        EditorGUILayout.BeginHorizontal();
        label = " " + label + " ";
        open = EditorGUILayout.Foldout(open, label, true, EditorStyles.foldoutHeader);
        GUILayout.FlexibleSpace();
        int newSize = array.Length;

        // Int-field to set array size
        newSize = EditorGUILayout.IntField(newSize);
        newSize = newSize < 0 ? 0 : newSize;

        // Resize if user input a new array length
        if (newSize != array.Length) {
            array = ResizeArray(array, newSize);
        }
        EditorGUILayout.EndHorizontal();

        // Show values if foldout was opened.
        if (open) {
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical();
            if (array.Length > 0) {
                // Make multiple object-fields based on the length given
                for (var i = 0; i < newSize; ++i) {
                    array[i] = EditorGUILayout.ObjectField($" Point {i} ", array[i], typeof(GameObject), true) as GameObject;
                }
            }
            else {
                EditorGUILayout.LabelField(" List is Empty ", bgStyle, GUILayout.Height(20));
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle leftButton = new GUIStyle(EditorStyles.miniButtonLeft);
            leftButton.fontSize = 17;
            GUIStyle rightButton = new GUIStyle(EditorStyles.miniButtonRight);
            rightButton.fontSize = 17;
            if (GUILayout.Button("  +  ", leftButton)) {
                array = ResizeArray(array, array.Length + 1);
            }
            if (GUILayout.Button("  -  ", rightButton)) {
                array = ResizeArray(array, array.Length - 1);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        return array;
    }

    private static T[] ResizeArray<T>(T[] array, int size) {
        T[] newArray = new T[size];

        for (var i = 0; i < size; i++) {
            if (i < array.Length) {
                newArray[i] = array[i];
            }
        }

        return newArray;
    }
}
#endif
