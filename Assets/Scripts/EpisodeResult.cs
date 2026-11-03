using System;
using System.Globalization;

/// <summary>Métricas de um episódio (uma nave do início até chegar, colidir ou estourar o tempo).</summary>
[Serializable]
public struct EpisodeResult
{
    public string label;
    public bool reached;           // chegou ao alvo
    public bool collided;          // colidiu com obstáculo
    public float time;             // segundos até o fim do episódio
    public float realDistance;     // soma dos deslocamentos da nave
    public float referenceDistance; // comprimento do caminho NavMesh

    private const float MinDistance = 0.001f;

    /// <summary>Eficiência só existe se chegou ao alvo e as duas distâncias são maiores que zero.</summary>
    public readonly bool HasValidEfficiency =>
        reached && realDistance > MinDistance && referenceDistance > MinDistance;

    /// <summary>distância de referência / distância real (NaN quando inválida).</summary>
    public readonly float Efficiency =>
        HasValidEfficiency ? referenceDistance / realDistance : float.NaN;

    /// <summary>Chegou ao alvo sem colidir.</summary>
    public readonly bool Passed => reached && !collided;

    public readonly string EfficiencyText
    {
        get
        {
            if (HasValidEfficiency) return Fmt(Efficiency);
            return reached ? "inválida (distância 0)" : "inválida (não chegou)";
        }
    }

    public static string Fmt(float value, string format = "F2")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
}
