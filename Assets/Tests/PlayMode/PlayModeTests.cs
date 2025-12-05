using NUnit.Framework;
using UnityEngine;

public class PlayModeTests
{
    // Helper: create a TrialController with a valid sphere prefab & trial area
    private TrialController CreateTrialController(int nObjects = 5, int nTargets = 2)
    {
        GameObject holder = new GameObject("TrialControllerHolder");
        TrialController tc = holder.AddComponent<TrialController>();

        // Create a prefab with a SphereController on it
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = "SpherePrefabForTests";
        prefab.AddComponent<SphereController>();   // <-- important!

        tc.spherePrefab = prefab;
        tc.trialArea = new GameObject("TrialArea").transform;

        tc.Setup(nObjects, nTargets, 1f, new Vector3(3, 3, 3));

        return tc;
    }

    [Test]
    public void BuildSpheres_Creates_Correct_Number()
    {
        // Arrange
        TrialController tc = CreateTrialController(nObjects: 5, nTargets: 2);

        // Act
        tc.BuildSpheres();

        // Assert
        Assert.AreEqual(5, tc.trialArea.childCount);
    }

    [Test]
    public void Sphere_StartMoving_Sets_Velocity()
    {
        // Arrange
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SphereController sc = go.AddComponent<SphereController>();

        sc.Initialize(0, new Vector3(2, 2, 2), 3f);

        // Pre-check: velocity should start at zero
        Assert.AreEqual(Vector3.zero, sc.velocity);

        // Act
        sc.StartMoving();

        // Assert: velocity should now be non-zero
        Assert.AreNotEqual(Vector3.zero, sc.velocity);
    }

    [Test]
    public void Sphere_StopMoving_Zeroes_Velocity()
    {
        // Arrange
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SphereController sc = go.AddComponent<SphereController>();

        sc.Initialize(0, new Vector3(2, 2, 2), 3f);
        sc.StartMoving();

        // Act
        sc.StopMoving();

        // Assert
        Assert.AreEqual(Vector3.zero, sc.velocity);
    }

    [Test]
    public void EvaluateSelection_Reports_Success_When_All_Targets_Selected()
    {
        // Arrange
        TrialController tc = CreateTrialController(nObjects: 3, nTargets: 1);
        tc.BuildSpheres();

        var selected = new System.Collections.Generic.List<int> { 1 };
        var trueTargets = new System.Collections.Generic.List<int> { 1 };

        bool result = false;
        tc.OnTrialComplete += success => result = success;

        // Act
        tc.EvaluateSelection(selected, trueTargets);

        // Assert
        Assert.IsTrue(result);
    }
}
