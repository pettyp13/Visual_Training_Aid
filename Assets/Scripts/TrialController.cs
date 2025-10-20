using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialController : MonoBehaviour
{
    public GameObject spherePrefab;
    public Transform trialArea; // parent transform to hold spheres; position is center
    public Vector3 bounds = new Vector3(5f, 5f, 5f);

    List<SphereController> spheres = new List<SphereController>();
    public int nObjects = 8;
    public int nTargets = 4;
    public float highlightDuration = 1.2f;
    public float movementDuration = 5f;
    public float speed = 1.2f;

    System.Random rng = new System.Random();

    // Events to notify GameManager
    public System.Action<List<int>> OnTrialEndSelectionPhase; // pass IDs
    public System.Action<bool> OnTrialComplete;

    public void Setup(int objects, int targets, float objSpeed, Vector3 _bounds)
    {
        nObjects = objects;
        nTargets = targets;
        speed = objSpeed;
        bounds = _bounds;
    }

    public void BuildSpheres()
    {
        Cleanup();
        spheres.Clear();
        for (int i = 0; i < nObjects; i++)
        {
            GameObject go = Instantiate(spherePrefab, trialArea);
            go.transform.localScale = Vector3.one * 0.5f;
            SphereController sc = go.GetComponent<SphereController>();
            sc.Initialize(i, bounds, speed); // note: Initialize sets isMoving = false
            spheres.Add(sc);
        }
    }

    public void Cleanup()
    {
        foreach (Transform t in trialArea) Destroy(t.gameObject);
    }

    public void StartTrial()
    {
        StartCoroutine(TrialRoutine());
    }

    IEnumerator TrialRoutine()
    {
        // 1) randomly pick targets
        List<int> targetIds = new List<int>();
        while (targetIds.Count < nTargets)
        {
            int pick = rng.Next(0, nObjects);
            if (!targetIds.Contains(pick)) targetIds.Add(pick);
        }

        // 2) set highlight briefly (spheres are NOT moving right now)
        foreach (var s in spheres)
        {
            if (targetIds.Contains(s.id)) s.SetTargetHighlight(true);
            else s.SetTargetHighlight(false);
        }

        // wait so player can memorize the targets
        yield return new WaitForSeconds(highlightDuration);

        // 3) turn off highlight
        foreach (var s in spheres) s.SetTargetHighlight(false);

        // 4) start movement on all spheres
        foreach (var s in spheres) s.StartMoving(speed);

        // 5) movement phase
        float tStart = Time.time;
        while (Time.time - tStart < movementDuration)
        {
            yield return null;
        }

        // 6) Stop movement
        foreach (var s in spheres) s.StopMoving();

        // 7) Enter selection phase: notify UI/game manager that selection should begin
        OnTrialEndSelectionPhase?.Invoke(targetIds);

        // wait for GameManager to call EvaluateSelection when user confirms (GameManager handles this)
    }

    public void EvaluateSelection(List<int> selectedIds, List<int> trueTargetIds)
    {
        // mark spheres colors
        int correctCount = 0;
        foreach (var s in spheres)
        {
            bool selected = selectedIds.Contains(s.id);
            bool actual = trueTargetIds.Contains(s.id);
            if (selected && actual) { s.MarkSelected(true); correctCount++; }
            else if (selected && !actual) { s.MarkSelected(false); }
            // missed targets keep base color
        }

        bool success = (correctCount == trueTargetIds.Count); // strict success: all targets found
        OnTrialComplete?.Invoke(success);
    }
}
