using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class BuffManagerWindow : EditorWindow
{
    private Vector2 scrollPos;
    private List<Buff> buffs;
    private Dictionary<Buff, string> editingNames = new Dictionary<Buff, string>();

    [MenuItem("Tools/Buff Manager")]
    public static void OpenWindow()
    {
        GetWindow<BuffManagerWindow>("Buff Manager");
    }

    private void OnEnable()
    {
        LoadAllBuffs();
    }

    private void LoadAllBuffs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Buff");
        buffs = guids.Select(guid =>
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<Buff>(path);
        }).ToList();

        // Init editing cache
        editingNames.Clear();
        foreach (var buff in buffs)
        {
            if (buff != null)
                editingNames[buff] = buff.buffName;
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Create New Buff", GUILayout.Height(25)))
        {
            CreateNewBuff();
        }

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (buffs != null)
        {
            foreach (var buff in buffs)
            {
                if (buff == null) continue;

                EditorGUILayout.BeginVertical("box");

                // --- Name Field with Apply/Revert ---
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName("NameField_" + buff.GetInstanceID());
                editingNames[buff] = EditorGUILayout.TextField("Name", editingNames[buff]);

                // Detect Enter key while typing in this field
                if (Event.current.type == EventType.KeyDown &&
                    Event.current.keyCode == KeyCode.Return &&
                    GUI.GetNameOfFocusedControl() == "NameField_" + buff.GetInstanceID())
                {
                    ApplyRename(buff);
                    Event.current.Use(); // consume event so it doesn’t "ding"
                }

                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    ApplyRename(buff);
                }

                if (GUILayout.Button("Revert", GUILayout.Width(60)))
                {
                    // Reset input to actual asset name
                    editingNames[buff] = buff.buffName;
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    string assetPath = AssetDatabase.GetAssetPath(buff);
                    AssetDatabase.DeleteAsset(assetPath);
                    LoadAllBuffs();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();

                // --- Icon + type ---
                EditorGUILayout.BeginHorizontal();
                buff.icon = (Sprite)EditorGUILayout.ObjectField(buff.icon, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));

                EditorGUILayout.BeginVertical();
                buff.buffType = (Buff.BuffType)EditorGUILayout.EnumPopup("Buff Type", buff.buffType);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                // --- Description ---
                EditorGUILayout.LabelField("Description:");
                buff.description = EditorGUILayout.TextArea(buff.description, GUILayout.MinHeight(40));

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(buff);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void CreateNewBuff()
    {
        Buff newBuff = ScriptableObject.CreateInstance<Buff>();
        newBuff.buffName = "New Buff";
        newBuff.description = "Describe this buff here.";
        newBuff.buffType = Buff.BuffType.MovementSpeed;

        string path = "Assets/Resources/ScriptableObjects/Buffs";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Resources/ScriptableObjects", "Buffs");
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/New Buff.asset");
        AssetDatabase.CreateAsset(newBuff, assetPath);
        AssetDatabase.SaveAssets();
        LoadAllBuffs();
    }

    private void ApplyRename(Buff buff)
    {
        string newName = editingNames[buff];
        if (!string.IsNullOrEmpty(newName) && newName != buff.buffName)
        {
            // Update scriptable object field
            buff.buffName = newName;

            // Rename asset file
            string path = AssetDatabase.GetAssetPath(buff);
            AssetDatabase.RenameAsset(path, newName);

            EditorUtility.SetDirty(buff);
            AssetDatabase.SaveAssets();
        }
    }

}
