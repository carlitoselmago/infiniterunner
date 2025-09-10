/*using UnityEngine;

public class ResettableSection : MonoBehaviour
{
    private Transform goodiesParent;
    private Transform[] goodies;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private bool[] initialActives;

    void Awake()
    {
        // Find the container (may not exist in all sections)
        goodiesParent = transform.Find("goodies");

        if (goodiesParent == null)
        {
            Debug.Log($"{name} has no 'goodies' child, skipping resettable setup.");
            return;
        }

        // Collect all descendants (deep search, not just direct children)
        goodies = goodiesParent.GetComponentsInChildren<Transform>(true);

        // Skip index 0 because it's the goodiesParent itself
        int count = goodies.Length - 1;
        initialPositions = new Vector3[count];
        initialRotations = new Quaternion[count];
        initialActives = new bool[count];

        for (int i = 1; i < goodies.Length; i++)
        {
            Transform g = goodies[i];
            int idx = i - 1;
            initialPositions[idx] = g.localPosition;
            initialRotations[idx] = g.localRotation;
            initialActives[idx] = g.gameObject.activeSelf;
        }
    }

    public void ResetSection()
    {
        if (goodiesParent == null || goodies == null) return;

        for (int i = 1; i < goodies.Length; i++) // start at 1 to skip goodiesParent
        {
            Transform g = goodies[i];
            int idx = i - 1;
            if (g == null) continue;

            g.localPosition = initialPositions[idx];
            g.localRotation = initialRotations[idx];
            g.gameObject.SetActive(initialActives[idx]);
        }
    }
}
*/