using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    private const float PreviewHeight = 180f;
    private Sprite quickIllustration;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CardData card = (CardData)target;

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Быстрая картинка", EditorStyles.boldLabel);

        quickIllustration = (Sprite)EditorGUILayout.ObjectField(
            "Sprite для карточки",
            quickIllustration,
            typeof(Sprite),
            false);

        using (new EditorGUI.DisabledScope(quickIllustration == null))
        {
            if (GUILayout.Button("Назначить выбранный Sprite"))
            {
                Undo.RecordObject(card, "Assign card illustration");
                card.illustration = quickIllustration;
                EditorUtility.SetDirty(card);
            }
        }

        if (quickIllustration == null)
            EditorGUILayout.HelpBox("Перетащи Sprite из Project в поле выше, потом нажми кнопку, чтобы назначить его этой карточке.", MessageType.Info);

        if (card.illustration != null && GUILayout.Button("Очистить картинку"))
        {
            Undo.RecordObject(card, "Clear card illustration");
            card.illustration = null;
            EditorUtility.SetDirty(card);
        }

        DrawIllustrationPreview(card.illustration);
    }

    private static void DrawIllustrationPreview(Sprite sprite)
    {
        if (sprite == null)
            return;

        Texture2D texture = AssetPreview.GetAssetPreview(sprite);

        if (texture == null)
            texture = sprite.texture;

        if (texture == null)
            return;

        Rect rect = GUILayoutUtility.GetRect(0f, PreviewHeight, GUILayout.ExpandWidth(true));
        GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
    }
}
