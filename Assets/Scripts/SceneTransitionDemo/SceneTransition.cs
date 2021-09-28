
using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public enum Screen
    {
        NONE = 0,
        MAIN_MENU_SCREEN,
        GAME_SCREEN,
        RESULT_SCREEN
    }

    private readonly string[] SCREEN_NAME = new string[] { "", "MainMenuScreen", "GameScreen", "ResultsScreen" };

    [SerializeField] private CurtainController m_CurtainController = null;
    [SerializeField] private CanvasGroup m_CanvasGroup = null;

    [SerializeField] private Screen m_StartingScreen = Screen.MAIN_MENU_SCREEN;

    public bool IsTransitioning { get; private set; }

    private Screen m_CurrentScreen = Screen.NONE;

    public void Start()
    {
        SetScreen(m_StartingScreen);
    }

    public void SetScreen(Screen screen)
    {
        if(m_CurrentScreen != screen)
        {
            StartCoroutine(LoadScreenAsync(screen));
        }
    }

    public void PreviousScreen()
    {
        if(!IsTransitioning)
        {
            int curScreenIndex = (int)m_CurrentScreen;
            curScreenIndex--;

            int screenLen = Enum.GetNames(typeof(Screen)).Length;
            if (curScreenIndex < 1)
            {
                curScreenIndex = screenLen - 1;
            }

            SetScreen((Screen)curScreenIndex);
        }
    }

    public void NextScreen()
    {
        if(!IsTransitioning)
        {
            int curScreenIndex = (int)m_CurrentScreen;
            curScreenIndex++;

            int screenLen = Enum.GetNames(typeof(Screen)).Length;
            if(curScreenIndex >= screenLen)
            {
                curScreenIndex = 1;
            }

            SetScreen((Screen)curScreenIndex);
        }
    }

    private IEnumerator LoadScreenAsync(Screen newScreen)
    {
        IsTransitioning = true;
        m_CanvasGroup.interactable = false;

        yield return StartCoroutine(CloseCurtainRoutine());

        int screenIndex = (int)newScreen;
        string screenName = SCREEN_NAME[screenIndex];

        if(!string.IsNullOrEmpty(screenName))
        {
            AsyncOperation loadScreenAsync = SceneManager.LoadSceneAsync(screenName, LoadSceneMode.Additive);

            while (!loadScreenAsync.isDone)
            {
                yield return null;
            }

            yield return StartCoroutine(UnloadPreviousScreenAsync(m_CurrentScreen));

            Scene scene = SceneManager.GetSceneByName(screenName);
            SceneManager.SetActiveScene(scene);

            m_CurrentScreen = newScreen;
        }

        yield return new WaitForSeconds(0.5f);


        yield return StartCoroutine(OpenCurtainRoutine());

        IsTransitioning = false;
        m_CanvasGroup.interactable = true;
    }

    private IEnumerator UnloadPreviousScreenAsync(Screen prevScreen)
    {
        int screenIndex = (int)prevScreen;
        string screenName = SCREEN_NAME[screenIndex];

        if(!string.IsNullOrEmpty(screenName))
        {
            AsyncOperation unloadAsync = SceneManager.UnloadSceneAsync(screenName);

            while(!unloadAsync.isDone)
            {
                yield return null;
            }
        }
    }

    private IEnumerator CloseCurtainRoutine()
    {
        m_CurtainController.CloseCurtain();

        while (m_CurtainController.IsAnimating())
        {
            yield return null;
        }
    }

    private IEnumerator OpenCurtainRoutine()
    {
        m_CurtainController.OpenCurtain();

        while (m_CurtainController.IsAnimating())
        {
            yield return null;
        }
    }
}
