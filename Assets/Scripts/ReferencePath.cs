using System.Text;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Rota de referência determinística entre SpawnPoint e Goal usando NavMesh.CalculatePath.
/// Ela NÃO controla a nave: serve só para medir o comprimento e comparar com o percurso real.
/// Não precisa do pacote AI Navigation no código; o pacote só é necessário para fazer o bake.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public sealed class ReferencePath : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float sampleDistance = 2f;
    [SerializeField] private Color lineColor = Color.green;
    [SerializeField, Min(0.01f)] private float lineWidth = 0.15f;
    [SerializeField] private float lineHeight = 0.2f;

    private readonly Vector3[] corners = new Vector3[128];
    private NavMeshPath path;
    private LineRenderer line;
    private int cornerCount;

    public bool Valid { get; private set; }
    public float Length { get; private set; }

    private void Awake()
    {
        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.widthMultiplier = lineWidth;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.positionCount = 0;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            line.material = new Material(shader);
    }

    /// <summary>Calcula a rota. Retorna false (sem exceção) se algum ponto estiver fora do NavMesh ou sem caminho.</summary>
    public bool Compute(Vector3 from, Vector3 to, out float length)
    {
        length = 0f;
        Valid = false;
        cornerCount = 0;
        line.positionCount = 0;

        if (!NavMesh.SamplePosition(from, out NavMeshHit start, sampleDistance, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(to, out NavMeshHit end, sampleDistance, NavMesh.AllAreas))
        {
            Debug.LogWarning("[ReferencePath] SpawnPoint ou Goal fora do NavMesh. " +
                             "Faça o bake do NavMeshSurface e posicione os dois sobre a área navegável.", this);
            return false;
        }

        if (!NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) ||
            path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning($"[ReferencePath] Sem caminho completo entre spawn e goal (status: {path.status}).", this);
            return false;
        }

        cornerCount = Mathf.Min(path.GetCornersNonAlloc(corners), corners.Length);
        for (int i = 1; i < cornerCount; i++)
            length += PlanarDistance(corners[i - 1], corners[i]);

        Valid = cornerCount >= 2 && length > 0.001f;
        Draw();
        return Valid;
    }

    /// <summary>Lista os cantos da rota no Console (para o vídeo/README).</summary>
    public void LogCorners(string label)
    {
        var sb = new StringBuilder();
        sb.Append($"[ReferencePath] {label}: {cornerCount} cantos, comprimento {EpisodeResult.Fmt(Length > 0 ? Length : 0f)} m\n");
        for (int i = 0; i < cornerCount; i++)
            sb.Append($"  {i}: ({EpisodeResult.Fmt(corners[i].x)}, {EpisodeResult.Fmt(corners[i].z)})\n");
        Debug.Log(sb.ToString(), this);
    }

    /// <summary>Distância no plano XZ (a nave tem Y congelado).</summary>
    public static float PlanarDistance(Vector3 a, Vector3 b)
    {
        float dx = b.x - a.x;
        float dz = b.z - a.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private void Draw()
    {
        Length = 0f;
        for (int i = 1; i < cornerCount; i++)
            Length += PlanarDistance(corners[i - 1], corners[i]);

        line.positionCount = cornerCount;
        for (int i = 0; i < cornerCount; i++)
            line.SetPosition(i, corners[i] + Vector3.up * lineHeight);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 1; i < cornerCount; i++)
            Gizmos.DrawLine(corners[i - 1] + Vector3.up * lineHeight, corners[i] + Vector3.up * lineHeight);
    }
}
