using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Manual test runner for verifying NUnit tests compile and run correctly.
/// Attach this script to a GameObject in a Unity scene and press Play to run tests.
/// </summary>
public class TestRunner : MonoBehaviour
{
    private List<TestResult> results = new List<TestResult>();
    private int currentTestIndex = 0;
    private bool isRunning = false;

    private readonly string[] testClassNames = new string[]
    {
        "PlayerControllerTests",
        "PlayerHealthTests",
        "WeaponSystemTests",
        "BotAITests",
        "BulletTests",
        "GrenadeSystemTests",
        "HUDManagerTests",
        "MedkitSystemTests",
        "RoundManagerTests"
    };

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 600, 800));

        GUILayout.Label("=== Unity NUnit Test Runner ===", GUI.skin.box);

        if (!isRunning && GUILayout.Button("Run All Tests", GUILayout.Height(40)))
        {
            StartCoroutine(RunAllTests());
        }

        if (isRunning)
        {
            GUILayout.Label($"Running test {currentTestIndex + 1}/{results.Count}...", GUI.skin.box);
        }

        GUILayout.Space(10);

        // Show summary
        int passed = 0;
        int failed = 0;
        foreach (var r in results)
        {
            if (r.Passed) passed++;
            else failed++;
        }

        GUILayout.Label($"Results: {passed} passed, {failed} failed, {results.Count} total", GUI.skin.box);

        GUILayout.Space(10);

        // Scroll view for details
       GUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(580), GUILayout.Height(600));
        foreach (var result in results)
        {
            string status = result.Passed ? "[PASS]" : "[FAIL]";
            string style = result.Passed ? "label" : "box";

            if (!result.Passed)
            {
                GUILayout.Box($"{status} {result.TestName}: {result.Message}", GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label($"{status} {result.TestName}", GUILayout.ExpandWidth(true));
            }
        }
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    private IEnumerator RunAllTests()
    {
        isRunning = true;
        results.Clear();

        foreach (var className in testClassNames)
        {
            yield return RunTestsForClass(className);
        }

        isRunning = false;
        Debug.Log($"Test run complete: {results.Count} tests");
    }

    private IEnumerator RunTestsForClass(string className)
    {
        System.Type testClass = System.Type.GetType(className);
        if (testClass == null)
        {
            results.Add(new TestResult
            {
                TestName = className,
                Passed = false,
                Message = $"Class {className} not found"
            });
            yield break;
        }

        // Get setup method
        MethodInfo unitySetup = testClass.GetMethod("UnitySetup", BindingFlags.Public | BindingFlags.Instance);
        MethodInfo setup = testClass.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Instance);

        // Get teardown method
        MethodInfo tearDown = testClass.GetMethod("TearDown", BindingFlags.Public | BindingFlags.Instance);

        // Get all test methods
        var testMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in testMethods)
        {
            var attributes = method.GetCustomAttributes(typeof(TestAttribute), true);
            if (attributes.Length > 0)
            {
                results.Add(new TestResult { TestName = $"{className}.{method.Name}" });
                currentTestIndex++;

                // Create instance
                object instance = System.Activator.CreateInstance(testClass);

                // Call setup
                if (unitySetup != null)
                {
                    var enumerator = (IEnumerator)unitySetup.Invoke(instance, null);
                    while (enumerator.MoveNext()) yield return enumerator.Current;
                }
                else if (setup != null)
                {
                    setup.Invoke(instance, null);
                }

                // Run test
                try
                {
                    var returnType = method.Invoke(instance, null);
                    if (returnType is IEnumerator)
                    {
                        var testEnumerator = (IEnumerator)returnType;
                        while (testEnumerator.MoveNext()) yield return testEnumerator.Current;
                    }
                    results[results.Count - 1].Passed = true;
                }
                catch (System.Exception ex)
                {
                    results[results.Count - 1].Passed = false;
                    results[results.Count - 1].Message = ex.InnerException?.Message ?? ex.Message;
                }

                // Call teardown
                try
                {
                    tearDown?.Invoke(instance, null);
                }
                catch { }
            }
        }
    }

    private class TestResult
    {
        public string TestName;
        public bool Passed;
        public string Message;
    }
}