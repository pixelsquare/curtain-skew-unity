
using UnityEngine;
using UnityEngine.Events;

public class CurtainController : MonoBehaviour
{
    private const float DEFAULT_CURTAIN_DURATION = 1.5f;

    public static event UnityAction OnCurtainAnimationEnd = null;

    [SerializeField] private float m_CurtainDuration = 1.0f;

    private float m_CurtainTimer = 0.0f;
    private CurtainAnimator[] m_CurtainAnimators = null;

    private bool m_CurtainToggle = false;

    public void Awake()
    {
        m_CurtainAnimators = GetComponentsInChildren<CurtainAnimator>(true);
        m_CurtainDuration = (m_CurtainDuration <= 0.0f) ? DEFAULT_CURTAIN_DURATION : m_CurtainDuration;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            ShowCurtain();
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            HideCurtain();
        }

        if(Input.GetMouseButtonDown(0) && !IsAnimating())
        {
            m_CurtainToggle = !m_CurtainToggle;

            if(m_CurtainToggle)
            {
                OpenCurtain();
            }
            else
            {
                CloseCurtain();
            }
        }

        if(m_CurtainTimer > 0.0f)
        {
            m_CurtainTimer -= Time.deltaTime;

            if(m_CurtainTimer <= 0.0f)
            {
                if(OnCurtainAnimationEnd != null)
                {
                    OnCurtainAnimationEnd();
                }
            }
        }
    }

    public void OpenCurtain()
    {
        if(!IsAnimating())
        {
            SetCurtainsActive(true);

            m_CurtainTimer = m_CurtainDuration;

            for (int i = 0; i < m_CurtainAnimators.Length; i++)
            {
                m_CurtainAnimators[i].OpenCurtain(m_CurtainDuration);
            }
        }
    }

    public void CloseCurtain()
    {
        if(!IsAnimating())
        {
            SetCurtainsActive(true);

            m_CurtainTimer = m_CurtainDuration;

            for (int i = 0; i < m_CurtainAnimators.Length; i++)
            {
                m_CurtainAnimators[i].CloseCurtain(m_CurtainDuration);
            }
        }
    }

    public void ShowCurtain()
    {
        for(int i = 0; i < m_CurtainAnimators.Length; i++)
        {
            m_CurtainAnimators[i].ShowCurtain();
        }

        SetCurtainsActive(true);
    }

    public void HideCurtain()
    {
        for(int i = 0; i < m_CurtainAnimators.Length; i++)
        {
            m_CurtainAnimators[i].HideCurtain();
        }

        SetCurtainsActive(false);
    }

    public bool IsAnimating()
    {
        return m_CurtainTimer > 0;
    }

    private void SetCurtainsActive(bool active)
    {
        for(int i = 0; i < m_CurtainAnimators.Length; i++)
        {
            m_CurtainAnimators[i].gameObject.SetActive(active);
        }
    }
}
