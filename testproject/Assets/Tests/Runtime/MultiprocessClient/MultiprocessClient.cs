using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class MultiprocessClient
{
    private const string k_SceneToLoad = "MultiprocessTestScene";
    private Scene m_SceneLoaded;
    private TestCoordinator m_TestCoordinator;
    private bool m_MultiprocessTestFinished;

    private const bool k_DebugLocally = true;
    private float m_LocalDebugTimeToRun;

    [UnitySetUp]
    public IEnumerator OnSetUp()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.LoadSceneAsync(k_SceneToLoad, LoadSceneMode.Additive);
        yield return new WaitUntil(IsSceneLoaded);
        m_TestCoordinator = Object.FindObjectOfType<TestCoordinator>();
        Assert.IsNotNull(m_TestCoordinator);
        m_TestCoordinator.MultiprocessTestFinished += MultiprocessTestFinished;
        m_MultiprocessTestFinished = false;
        if (k_DebugLocally)
        {
            m_LocalDebugTimeToRun = Time.realtimeSinceStartup + 20.0f;
        }

    }

    private void MultiprocessTestFinished()
    {
        m_MultiprocessTestFinished = true;
    }

    private IEnumerator WaitforTestToComplete()
    {
        var waitPeriod = new WaitForSeconds(0.250f);

        while (!m_MultiprocessTestFinished)
        {
            yield return waitPeriod;

            if (k_DebugLocally && m_LocalDebugTimeToRun < Time.realtimeSinceStartup)
            {
                break;
            }
        }
    }

    [UnityTest]
    public IEnumerator ClientMultiprocessMockTest()
    {
        yield return WaitforTestToComplete();
    }

    private bool IsSceneLoaded()
    {
        if(m_SceneLoaded.IsValid() && m_SceneLoaded.isLoaded)
        {
            if (SceneManager.GetActiveScene().handle != m_SceneLoaded.handle)
            {
                SceneManager.SetActiveScene(m_SceneLoaded);
            }
            return true;
        }
        return false;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == k_SceneToLoad && mode == LoadSceneMode.Additive)
        {
            m_SceneLoaded = scene;
        }
    }
}
