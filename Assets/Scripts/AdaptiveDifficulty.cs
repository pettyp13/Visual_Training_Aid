using System.Collections.Generic;
using UnityEngine;

public class AdaptiveDifficulty
{
    // Simple 1-up/1-down staircase on speed; you can expand to other params
    public float currentSpeed;
    public int currentNObjects;
    public int currentNTargets;

    float minSpeed = 0.2f;
    float maxSpeed = 6f;
    float step = 0.3f;

    public AdaptiveDifficulty(float startSpeed = 1.2f, int nObjects = 8, int nTargets = 4)
    {
        currentSpeed = startSpeed;
        currentNObjects = nObjects;
        currentNTargets = nTargets;
    }

    // call after each trial
    public void UpdateDifficulty(bool success)
    {
        if (success)
        {
            // make it harder
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + step);
            // optionally increase number of objects or reduce tracking time externally
        }
        else
        {
            // make it easier
            currentSpeed = Mathf.Max(minSpeed, currentSpeed - step);
        }
    }
}
