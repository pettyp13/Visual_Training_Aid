

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TrialController trialController;
    public Transform trialArea;
    public GameObject spherePrefab;
    public TMP_Text infoText;
    public Button startButton;

    AdaptiveDifficulty adaptive;

    List<int> currentTrueTargets;
    List<int> playerSelections = new List<int>();

    void Start()
    {
        // initial difficulty
        adaptive = new AdaptiveDifficulty(startSpeed: 1.2f, nObjects: 8, nTargets: 4);

        // wire up trial controller
        trialController.spherePrefab = spherePrefab;
        trialController.trialArea = trialArea;
        trialController.OnTrialEndSelectionPhase += OnSelectionPhaseStart;
        trialController.OnTrialComplete += OnTrialComplete;

        startButton.onClick.AddListener(OnStartClicked);
        infoText.text = "Press Start to begin.";
    }

    void OnStartClicked()
    {
        startButton.interactable = false;
        StartCoroutine(RunSessionCoroutine());
    }

    IEnumerator RunSessionCoroutine()
    {
        int trials = 10;
        for (int i = 0; i < trials; i++)
        {
            // setup trial from adaptive
            trialController.Setup(adaptive.currentNObjects, adaptive.currentNTargets, adaptive.currentSpeed, new Vector3(5, 5, 5));
            trialController.BuildSpheres();
            infoText.text = $"Trial {i + 1}/{trials}\nWatch the highlighted targets.";
            yield return new WaitForSeconds(1f);

            // Subscribe to capture true targets on selection phase
            bool selectionPhaseCompleted = false;
            currentTrueTargets = null;

            trialController.OnTrialEndSelectionPhase += (List<int> trueTargets) =>
            {
                currentTrueTargets = trueTargets;
                selectionPhaseCompleted = true;
            };

            trialController.StartTrial();

            // Wait until trialController signals selection-phase start
            while (!selectionPhaseCompleted) yield return null;

            // Now allow player to click spheres — we will collect selections for a fixed time or until count reached
            playerSelections.Clear();
            infoText.text = "Select the targets you tracked. Press Space to confirm when done.";
            // Wait for the user to click Confirm (we will implement simple confirmation via spacebar)
            bool confirmed = false;
            System.Action confirmAction = () => confirmed = true;
            // simple instruction: user presses space to confirm
            while (!confirmed)
            {
                HandleMouseSelection();
                if (Input.GetKeyDown(KeyCode.Space)) confirmed = true;
                yield return null;
            }

            // Evaluate
            trialController.EvaluateSelection(playerSelections, currentTrueTargets);

            // Wait for trial complete event to fire and update adaptive; OnTrialComplete will set adaptive
            bool trialDone = false;
            System.Action<bool> doneAction = (bool success) => trialDone = true;
            trialController.OnTrialComplete += doneAction;

            while (!trialDone) yield return null;

            // Small pause between trials
            yield return new WaitForSeconds(1f);
        }

        infoText.text = "Session complete.";
        startButton.interactable = true;
    }

    void OnSelectionPhaseStart(List<int> trueTargets)
    {
        // store the true target ids for evaluation
        currentTrueTargets = trueTargets;
        // clear previous player selections
        playerSelections.Clear();
    }

    void HandleMouseSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                SphereController sc = hit.collider.GetComponent<SphereController>();
                if (sc != null)
                {
                    if (!playerSelections.Contains(sc.id))
                    {
                        playerSelections.Add(sc.id);
                        // visual feedback
                        sc.MarkSelected(true);
                    }
                }
            }
        }
    }

    void OnTrialComplete(bool success)
    {
        infoText.text = success ? "Correct! Difficulty increased." : "Incorrect. Difficulty decreased.";
        adaptive.UpdateDifficulty(success);
    }
}
