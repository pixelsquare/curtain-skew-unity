
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class CurtainAnimator : MonoBehaviour
{
    public enum CurtainState
    {
        CLOSE = 0,
        OPEN
    }

    private const float CURTAIN_SKEW_TIME_THRESHOLD = 0.85f;
    private const float CURTAIN_BOTTOM_SKEW_SPEED_MULTIPLIER = 1.15f;

    [Range(-1.0f, 1.0f)]
    [SerializeField] private int m_Direction = 0;

    [SerializeField] private string m_SortingLayer = "Default";
    [SerializeField] private int m_SortingOrder = 0;

    private Mesh m_Mesh = null;
    private MeshRenderer m_MeshRenderer = null;

    private Vector2[] m_MeshUvs = null;
    private Vector2[] m_MeshOriginalUvs = null;

    private float m_CurtainTimer = 0.0f;
    private float m_CurtainDuration = 0.0f;

    private CurtainState m_CurtainState = CurtainState.CLOSE;

    public void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        m_Mesh = meshFilter.GetMesh();
        m_MeshUvs = m_Mesh.uv;
        m_MeshOriginalUvs = m_Mesh.uv;
    }

    public void Start()
    {
        m_CurtainTimer = m_CurtainDuration;
        SetSortingLayer(m_SortingLayer, m_SortingOrder);
    }

    public void Update()
    {
        if(m_MeshUvs == null || m_CurtainTimer <= 0.0f)
        {
            return;
        }

        if(m_CurtainTimer >= m_CurtainDuration * CURTAIN_SKEW_TIME_THRESHOLD)
        { 
            // Start animating top UV ahead of time.
            m_MeshUvs[1].x += m_Direction * GetCurtainStateDirection(m_CurtainState) * (Time.deltaTime / m_CurtainDuration);
            m_MeshUvs[3].x += m_Direction * GetCurtainStateDirection(m_CurtainState) * (Time.deltaTime / m_CurtainDuration);
        }
        else
        {
            m_MeshUvs[0].x += m_Direction * GetCurtainStateDirection(m_CurtainState) * (Time.deltaTime / m_CurtainDuration) * CURTAIN_BOTTOM_SKEW_SPEED_MULTIPLIER;
            m_MeshUvs[1].x += m_Direction * GetCurtainStateDirection(m_CurtainState) * (Time.deltaTime / m_CurtainDuration);
            m_MeshUvs[2].x += m_Direction * GetCurtainStateDirection(m_CurtainState) * (Time.deltaTime / m_CurtainDuration) * CURTAIN_BOTTOM_SKEW_SPEED_MULTIPLIER;
            m_MeshUvs[3].x += m_Direction * GetCurtainStateDirection(m_CurtainState) * (Time.deltaTime / m_CurtainDuration);
        }

        if(m_CurtainTimer > 0)
        {
            m_CurtainTimer -= Time.deltaTime;

            if (m_CurtainTimer <= 0.0f)
            {
                // Snap the values to whole number
                for (int i = 0; i < m_MeshUvs.Length; i++)
                {
                    m_MeshUvs[i].x = Mathf.Round(m_MeshUvs[i].x);
                    m_MeshUvs[i].y = Mathf.Round(m_MeshUvs[i].y);
                }
            }
        }

        m_Mesh.uv = m_MeshUvs;
    }

    public void OpenCurtain(float duration)
    {
        if(m_CurtainState != CurtainState.OPEN)
        {
            m_CurtainState = CurtainState.OPEN;
            ShowCurtain();

            m_CurtainDuration = duration;
            m_CurtainTimer = m_CurtainDuration;
        }
    }

    public void CloseCurtain(float duration)
    {
        if(m_CurtainState != CurtainState.CLOSE)
        {
            m_CurtainState = CurtainState.CLOSE;
            HideCurtain();

            m_CurtainDuration = duration;
            m_CurtainTimer = m_CurtainDuration;

        }
    }

    public void ShowCurtain()
    {
        for(int i = 0; i < m_Mesh.uv.Length; i++)
        {
            m_MeshUvs[i] = m_MeshOriginalUvs[i];
        }

        m_Mesh.uv = m_MeshUvs;
    }

    public void HideCurtain()
    {
        for(int i = 0; i < m_Mesh.uv.Length; i++)
        {
            m_MeshUvs[i].x = m_MeshOriginalUvs[i].x + m_Direction;
            m_MeshUvs[i].y = m_MeshOriginalUvs[i].y;
        }

        m_Mesh.uv = m_MeshUvs;
    }

    public void SetSortingLayer(string sortingName, int sortingOrder)
    {
        if(m_MeshRenderer != null)
        {
            m_MeshRenderer.sortingLayerID = SortingLayer.NameToID(sortingName);
            m_MeshRenderer.sortingOrder = sortingOrder;
        }
    }

    private int GetCurtainStateDirection(CurtainState curtainState)
    {
        int result = 0;

        if(curtainState == CurtainState.OPEN)
        {
            result = 1;
        }
        else if(curtainState == CurtainState.CLOSE)
        {
            result = -1;
        }

        return result;
    }
}
