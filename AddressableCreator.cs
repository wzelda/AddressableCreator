using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System;
using System.Collections.Generic;

public class AddressableCreator : EditorWindow
{
    //0-path,1-group,2-key
    static string[][] paths = {
        new string[]{"Assets/AddressableResources/VideoC0", "VideoC0", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoC1", "VideoC1", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoC2", "VideoC2", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoC3", "VideoC3", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoC4", "VideoC4", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoC5", "VideoC5", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/BR1", "VideoDungeonBR1", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/BR2", "VideoDungeonBR2", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/BR3", "VideoDungeonBR3", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/LL1", "VideoDungeonLL1", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/LL2", "VideoDungeonLL2", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/LL3", "VideoDungeonLL3", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/WFR1", "VideoDungeonWFR1", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/WFR2", "VideoDungeonWFR2", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/WFR3", "VideoDungeonWFR3", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/XZ1", "VideoDungeonXZ1", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/XZ2", "VideoDungeonXZ2", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/XZ3", "VideoDungeonXZ3", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/YQ1", "VideoDungeonYQ1", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/YQ2", "VideoDungeonYQ2", "Assets/AddressableResources/Video"},
        new string[]{"Assets/AddressableResources/VideoDungeon/YQ3", "VideoDungeonYQ3", "Assets/AddressableResources/Video"},
        new string[]{"Assets/UI", "UI", "UI"},
        new string[]{"Assets/Lua", "Lua", "Lua"},
        new string[]{"Assets/AddressableResources/Prefabs/Particle", "Effects", "Prefabs/Particle"},
        new string[]{"Assets/AddressableResources/Shaders", "Default Local Group"},
        new string[]{"Assets/AddressableResources/Audio", "Audio"},
        new string[]{"Assets/AddressableResources/Prefabs/Misc", "Shared", "Misc"},
        new string[]{"Assets/MiniGame/Prefab", "MiniGame", "MiniGame"},
        new string[]{"Assets/effect/Prefabs", "Effects", "Prefabs"},
        new string[]{"Assets/Scenes", "Scenes"},
    };

    //group, filter
    Dictionary<string, string> typeFilter = new Dictionary<string, string>
    {
        { "Scenes", "Scene" },
    };

    List<string> blackList = new List<string> {
        "Built In Data", "BehaviorTree",
    };

    private AddressableAssetSettings settings;
    private List<AddressableAssetGroup> addressableGroups;
    private Dictionary<AddressableAssetGroup, bool> groupToggles;
    bool selectAll = false;
    Vector2 scrollPosition = new Vector2();

    private const string SelectionKey = "AddressableGroupSelection";

    [MenuItem("Tools/Addressable Group Builder")]
    public static void ShowWindow()
    {
        GetWindow<AddressableCreator>("Addressable Group Builder");
    }

    void OnEnable()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        addressableGroups = new List<AddressableAssetGroup>();
        groupToggles = new Dictionary<AddressableAssetGroup, bool>();

        if (settings != null)
        {
            foreach (var group in settings.groups)
            {
                if(blackList.Contains(group.Name))
                {
                    continue;
                }

                addressableGroups.Add(group);
                groupToggles[group] = false; // Initialize toggles as false (unchecked)
            }
            addressableGroups.Sort(new AddressableAssetGroupComparer());

            LoadGroupSelection();
        }
    }

    private void OnGUI()
    {
        if (settings == null)
        {
            EditorGUILayout.HelpBox("Addressable Asset Settings not found.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Select Addressable Groups to Build:", EditorStyles.boldLabel);

        if (GUILayout.Button(selectAll ? "Deselect All" : "Select All"))
        {
            selectAll = !selectAll;
            foreach (var group in addressableGroups)
            {
                groupToggles[group] = selectAll;
            }
        }

        GUILayout.Space(20);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var group in addressableGroups)
        {
            groupToggles[group] = EditorGUILayout.Toggle(group.Name, groupToggles[group]);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.Space(20);

        if (GUILayout.Button("Build Selected Groups"))
        {
            BuildSelectedGroups();
        }

        GUILayout.Space(20);
    }

    private void BuildSelectedGroups()
    {
        SaveGroupSelection();
        var buildContent = new List<AddressableAssetGroup>();

        foreach (var group in addressableGroups)
        {
            if (groupToggles[group])
            {
                buildContent.Add(group);
            }
        }

        foreach (var item in paths)
        {
            var groupName = item[1];
            var group = settings.FindGroup(groupName);
            // Debug.Log(group.Name);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            }
            if(group == null)
            {
                Debug.LogError(groupName);
                continue;
            }
            if(!buildContent.Contains(group))
            {
                continue;
            }

            Debug.Log("start build group:"+group.Name);
            var fk = "";
            typeFilter.TryGetValue(group.Name, out fk);
            if(!String.IsNullOrEmpty(fk))
            {
                fk = "t:"+fk;
            }
            var guids = AssetDatabase.FindAssets(fk, new[] { item[0] });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = item.Length > 2? path.Replace(item[0], item[2]): path;
                Debug.Log(path+"=>"+group.Name);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Converted folder to Addressable Group.");
    }

    private void SaveGroupSelection()
    {
        List<string> selectedGroups = new List<string>();

        foreach (var group in addressableGroups)
        {
            if (groupToggles[group])
            {
                selectedGroups.Add(group.Name);
            }
        }

        EditorPrefs.SetString(SelectionKey, string.Join(",", selectedGroups.ToArray()));
    }

    private void LoadGroupSelection()
    {
        if (EditorPrefs.HasKey(SelectionKey))
        {
            string savedSelection = EditorPrefs.GetString(SelectionKey);
            string[] selectedGroups = savedSelection.Split(',');

            foreach (var groupName in selectedGroups)
            {
                var group = addressableGroups.Find(g => g.Name == groupName);
                if (group != null)
                {
                    groupToggles[group] = true;
                }
            }
        }
    }

    public static Type GetAssetTypeByGUID(string assetGUID)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
        return AssetDatabase.GetMainAssetTypeAtPath(assetPath);
    }
}

public class AddressableAssetGroupComparer : IComparer<AddressableAssetGroup>
{
    public int Compare(AddressableAssetGroup x, AddressableAssetGroup y)
    {
        if (x == null || y == null)
            return 0;
        
        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}