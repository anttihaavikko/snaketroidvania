using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PaletteAssigner : EditorWindow
{
    private List<Color> palette;
    private Color newColor = Color.white;

    [MenuItem("Window/Palette")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(PaletteAssigner));
        var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Icons/palette.png");
        GUIContent titleContent = new GUIContent("Palette", icon);
        window.titleContent = titleContent;
    }

    private void OnEnable()
    {
        palette = new List<Color>();
        LoadPalette();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Assign color");
        EditorGUILayout.BeginHorizontal();
        var index = 0;
        var perRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / 60);

        var removeIndex = -1;

        foreach (var color in palette)
        {
            var style = new GUIStyle
            {
                 fontSize = 40,
                 stretchWidth = false,
                 padding = new RectOffset(5, 5, 5, 5)
            };
            var hex = ColorUtility.ToHtmlStringRGB(color);
            if (GUILayout.Button("<color=#"+hex+">██</a>", style))
            {
                if (Event.current.alt)
                {
                    removeIndex = index;
                }
                if (Event.current.shift)
                {
                    EditorGUIUtility.systemCopyBuffer = hex;
                }
                else
                {
                    AssignColor(color);
                }
            }

            index++;

            if (index % perRow == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();

        if(removeIndex > -1)
        {
            palette.RemoveAt(removeIndex);
            SavePalette();
        }

        EditorGUILayout.LabelField("Add color");
        newColor = EditorGUILayout.ColorField(newColor);
        if(GUILayout.Button("Add color"))
        {
            palette.Add(newColor);
            SavePalette();
        }
    }

    void LoadPalette()
    {
        var key = GetKeyName();
        if (EditorPrefs.HasKey(key))
        {
            var json = EditorPrefs.GetString(key);
            var data = JsonUtility.FromJson<Palette>(json);
            palette = data.ToList();

        }
        else
        {
            palette.Add(Color.black);
            palette.Add(Color.white);
        }
    }

    void SavePalette()
    {
        var data = new Palette(palette);
        var json = JsonUtility.ToJson(data);
        EditorPrefs.SetString(GetKeyName(), json);
    }

    string GetKeyName()
    {
        var dp = Application.dataPath;
        var s = dp.Split("/"[0]);
        return s[s.Length - 2] + "SavedPalette";
    }

    void AssignColor(Color color)
    {
        foreach(var obj in Selection.gameObjects)
        {
            // sprite
            var sprite = obj.GetComponent<SpriteRenderer>();
            if(sprite) sprite.color = color;

            // textmeshpro
            var text = obj.GetComponent<TMPro.TMP_Text>();
            if (text) text.color = color;

            // ui image
            var image = obj.GetComponent<Image>();
            if (image) image.color = color;
        }
    }
}

[System.Serializable]
public class Palette
{
    public List<string> colors;

    public Palette(List<Color> values)
    {
        colors = values.Select(ColorUtility.ToHtmlStringRGB).ToList();
    }

    public List<Color> ToList()
    {
        return colors.Select(c => {
            ColorUtility.TryParseHtmlString("#" + c, out var parsed);
            return parsed;
        }).ToList();
    }
}