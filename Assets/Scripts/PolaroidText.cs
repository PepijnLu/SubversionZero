using UnityEngine;
using TMPro;

public class TextMeshProWordColliderGenerator : MonoBehaviour
{
    public bool regenerateOnStart = true;
    public bool addDebugGizmos = false;

    private TextMeshProUGUI textMesh;
    private Camera cam;
    [SerializeField] float xOffset, yOffset;
    [SerializeField] Polaroid polaroid;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        if (regenerateOnStart)
            GenerateWordColliders();
    }

    public void GenerateWordColliders()
    {
        textMesh.ForceMeshUpdate();

        var textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.wordCount; i++)
        {
            if(polaroid.indexesOfBoldedWords.Contains(i))
            {
                TMP_WordInfo wordInfo = textInfo.wordInfo[i];

                float fontSize = textMesh.fontSize;
                int wordLength = wordInfo.characterCount;

                // First and last character indices in this word
                int firstCharIndex = wordInfo.firstCharacterIndex;
                int lastCharIndex = wordInfo.lastCharacterIndex;

                Vector3 min = Vector3.positiveInfinity;
                Vector3 max = Vector3.negativeInfinity;

                for (int j = firstCharIndex; j <= lastCharIndex; j++)
                {
                    var charInfo = textInfo.characterInfo[j];

                    if (!charInfo.isVisible)
                        continue;

                    for (int v = 0; v < 4; v++)
                    {
                        Vector3[] worldCorners = new Vector3[4];
                        worldCorners[0] = textMesh.transform.TransformPoint(charInfo.vertex_BL.position);
                        worldCorners[1] = textMesh.transform.TransformPoint(charInfo.vertex_TL.position);
                        worldCorners[2] = textMesh.transform.TransformPoint(charInfo.vertex_TR.position);
                        worldCorners[3] = textMesh.transform.TransformPoint(charInfo.vertex_BR.position);

                        foreach (var worldVertex in worldCorners)
                        {
                            min = Vector3.Min(min, worldVertex);
                            max = Vector3.Max(max, worldVertex);
                        }
                    }
                }

                Vector3 center = (min + max) / 2f;

                float xSize = fontSize * wordLength * xOffset;
                float ySize = fontSize * yOffset;
                float zSize = 20;
                Vector3 size = new Vector3(xSize, ySize, zSize);

                GameObject wordBox = new GameObject($"{wordInfo.GetWord()}");
                wordBox.transform.SetParent(this.transform);
                wordBox.transform.position = center;
                wordBox.transform.rotation = textMesh.transform.rotation;
                wordBox.transform.localScale = Vector3.one;
                wordBox.gameObject.layer = 13;

                BoxCollider box = wordBox.AddComponent<BoxCollider>();
                size.z = 20;
                box.size = size;

                // Optional: Add word data as tag, name, or component
                // wordBox.tag = "ClickableWord";
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!addDebugGizmos) return;

        Gizmos.color = Color.cyan;
        foreach (Transform child in transform)
        {
            var box = child.GetComponent<BoxCollider>();
            if (box)
            {
                Gizmos.matrix = child.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
    }
#endif
}
