using UnityEngine;

/// <summary>
/// Painel IMGUI com os botões da tarefa. Usa OnGUI (sem Input antigo),
/// então funciona mesmo com o Input System novo do Unity 6.
/// </summary>
public sealed class EvolutionHud : MonoBehaviour
{
    [SerializeField] private EvolutionManager manager;
    [SerializeField, Range(0.75f, 3f)] private float scale = 1.25f;

    private void OnGUI()
    {
        if (manager == null)
            return;

        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        GUILayout.BeginArea(new Rect(10, 10, 480, Screen.height / scale - 20), GUI.skin.box);

        GUILayout.Label($"Modo: {manager.ModeLabel} | Geracao: {manager.Generation} | Seed: {manager.Seed}");
        GUILayout.Label($"Fitness da geracao: {manager.BestFitness:F1} (media {manager.AverageFitness:F1})");

        if (manager.HasBestGenome)
        {
            GUILayout.Label($"Melhor genoma: fitness {manager.BestEverFitness:F1}, geracao {manager.BestEverGeneration}");
        }
        else
        {
            GUILayout.Label("Melhor genoma: ainda nao avaliado");
        }

        GUILayout.Label($"Save existente: {(GenomeStore.HasSave ? "sim" : "nao")}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Salvar melhor")) manager.SaveBest();
        if (GUILayout.Button("Carregar melhor")) manager.LoadBestAndReplay();
        if (GUILayout.Button("Apagar save")) manager.DeleteSave();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Validar (3 cenarios)")) manager.RunValidation();
        if (GUILayout.Button("Retomar treino")) manager.ResumeTraining();
        if (GUILayout.Button("Abrir pasta JSON")) manager.OpenExportFolder();
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label(manager.LastMessage);

        EpisodeResult last = manager.LastEpisode;
        if (!string.IsNullOrEmpty(last.label))
        {
            GUILayout.Label($"Ultimo episodio ({last.label}): eficiencia {last.EfficiencyText}, " +
                            $"real {EpisodeResult.Fmt(last.realDistance)} m, ref {EpisodeResult.Fmt(last.referenceDistance)} m");
        }

        if (manager.ValidationResults.Count > 0)
        {
            GUILayout.Space(6);
            GUILayout.Label("Validacao:");
            foreach (EpisodeResult r in manager.ValidationResults)
            {
                GUILayout.Label($"{r.label}: {(r.reached ? "chegou" : "nao chegou")}, " +
                                $"colisao {(r.collided ? "sim" : "nao")}, {EpisodeResult.Fmt(r.time, "F1")}s, " +
                                $"real {EpisodeResult.Fmt(r.realDistance)} m, efic. {r.EfficiencyText}");
            }
            GUILayout.Label(manager.ValidationSummary);
        }

        GUILayout.EndArea();
    }
}
