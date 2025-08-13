using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class BuffManagerWindow : EditorWindow
{
    private Vector2 scrollPos;
    private List<Buff> buffs;

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

                // Asset rename
                EditorGUILayout.BeginHorizontal();
                string newName = EditorGUILayout.TextField("Buff Asset Name", buff.name);
                if (newName != buff.name && !string.IsNullOrEmpty(newName))
                {
                    string path = AssetDatabase.GetAssetPath(buff);
                    AssetDatabase.RenameAsset(path, newName);
                    buff.buffName = newName; // Sync
                    EditorUtility.SetDirty(buff);
                    AssetDatabase.SaveAssets();
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

                // Icon + details
                EditorGUILayout.BeginHorizontal();
                buff.icon = (Sprite)EditorGUILayout.ObjectField(buff.icon, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));

                EditorGUILayout.BeginVertical();
                buff.buffName = EditorGUILayout.TextField("Display Name", buff.buffName);
                buff.statBuff = EditorGUILayout.FloatField("Stat Buff", buff.statBuff);
                buff.buffLevel = EditorGUILayout.IntField("Buff Level", buff.buffLevel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                // Description
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
}
