using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;   // ⬅ NEW

public class GameManager : MonoBehaviour
{
    public TrialController trialController;
    public Transform trialArea;
    public GameObject spherePrefab;
    public TMP_Text infoText;
    public TMP_Text titleText;   //  Your "Visual Training Aid" title
    public Button startButton;
    public Button highScoreButton;   // ⬅ NEW: button to open High Score screen

    AdaptiveDifficulty adaptive;

    List<int> currentTrueTargets;
    List<int> playerSelections = new List<int>();

    bool playerHasConfirmed = false;
    bool trialDone = false;

    //  Score tracking variables
    int totalTrials = 10;
    int currentTrial = 0;
    int correctCount = 0;
    int incorrectCount = 0;

    void Start()
    {
        // Initial difficulty
        adaptive = new AdaptiveDifficulty(startSpeed: 1.2f, nObjects: 8, nTargets: 4);

        // Wire up trial controller
        trialController.spherePrefab = spherePrefab;
        trialController.trialArea = trialArea;
        trialController.OnTrialEndSelectionPhase += OnSelectionPhaseStart;
        trialController.OnTrialComplete += OnTrialComplete;

        startButton.onClick.AddListener(OnStartClicked);

        // ⬇ NEW: wire High Score button if assigned
        if (highScoreButton != null)
        {
            highScoreButton.onClick.AddListener(OpenHighScoreScreen);
        }

        infoText.text = "Press Start to begin.";

        //  Show title at startup
        if (titleText != null)
            titleText.gameObject.SetActive(true);
    }

    void OnStartClicked()
    {
        //  Hide the title once game starts
        if (titleText != null)
            titleText.gameObject.SetActive(false);

        //  Reset score for a new session
        correctCount = 0;
        incorrectCount = 0;
        currentTrial = 0;

        startButton.interactable = false;
        infoText.text = "Starting new session...";
        StartCoroutine(SessionRoutine());
    }

    IEnumerator SessionRoutine()
    {
        for (currentTrial = 0; currentTrial < totalTrials; currentTrial++)
        {
            // Reset round flags
            playerHasConfirmed = false;
            trialDone = false;

            // Setup trial
            trialController.Setup(adaptive.currentNObjects, adaptive.currentNTargets, adaptive.currentSpeed, new Vector3(5, 5, 5));
            trialController.BuildSpheres();

            infoText.text = $"Trial {currentTrial + 1}/{totalTrials}\nScore: {correctCount}/{totalTrials}\nWatch the highlighted targets.";
            yield return new WaitForSeconds(1.5f);

            // Start the trial
            trialController.StartTrial();

            // Wait for selection phase
            while (currentTrueTargets == null)
                yield return null;

            // Let player select
            playerSelections.Clear();
            infoText.text = $"Trial {currentTrial + 1}/{totalTrials}\nScore: {correctCount}/{totalTrials}\nSelect the targets you tracked. Press SPACE to confirm.";

            while (!playerHasConfirmed)
            {
                HandleMouseSelection();
                if (Input.GetKeyDown(KeyCode.Space))
                    playerHasConfirmed = true;
                yield return null;
            }

            // Evaluate
            trialController.EvaluateSelection(playerSelections, currentTrueTargets);

            // Wait for trial to complete (OnTrialComplete event sets trialDone)
            while (!trialDone)
                yield return null;

            // Pause before next round
            yield return new WaitForSeconds(2f);

            // Reset for next round
            currentTrueTargets = null;
        }

        //  After all trials, show session summary
        float accuracy = (float)correctCount / totalTrials * 100f;
        infoText.text = $" Session Complete!\n" +
                        $"Correct: {correctCount}\n" +
                        $"Incorrect: {incorrectCount}\n" +
                        $"Accuracy: {accuracy:F1}%\n\n" +
                        $"Click Start to try again!";

        // --- SAVE SCORES TO PLAYERPREFS (for High Score screen) --- //
        PlayerPrefs.SetFloat("LastAccuracy", accuracy);  // last session

        float previousHigh = PlayerPrefs.GetFloat("HighScoreAccuracy", 0f);
        if (accuracy > previousHigh)
        {
            PlayerPrefs.SetFloat("HighScoreAccuracy", accuracy);  // new high score
            PlayerPrefs.Save();
            infoText.text += "\n\nNEW HIGH SCORE!";
        }
        else
        {
            PlayerPrefs.Save();
        }
        // ---------------------------------------------------------- //

        //  Show title again after finishing
        if (titleText != null)
            titleText.gameObject.SetActive(true);

        startButton.interactable = true;
    }

    void OnSelectionPhaseStart(List<int> trueTargets)
    {
        currentTrueTargets = trueTargets;
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
                        sc.MarkSelected(true); // Give feedback
                    }
                }
            }
        }
    }

    void OnTrialComplete(bool success)
    {
        if (success)
        {
            correctCount++;
            infoText.text = $" Correct! Difficulty increased.\nScore: {correctCount}/{totalTrials}";
        }
        else
        {
            incorrectCount++;
            infoText.text = $" Incorrect. Difficulty decreased.\nScore: {correctCount}/{totalTrials}";
        }

        adaptive.UpdateDifficulty(success);
        trialDone = true;
    }

    // ⬇ NEW: called by the High Score button
    public void OpenHighScoreScreen()
    {
        SceneManager.LoadScene("HighScoreScene");   // make sure you create a scene with this name
    }
}
