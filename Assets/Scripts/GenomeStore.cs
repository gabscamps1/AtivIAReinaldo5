using System;
using UnityEngine;

/// <summary>
/// Persistência do melhor genoma em uma única chave de PlayerPrefs.
/// Nenhum método lança exceção para o fluxo normal: falhas voltam como mensagem.
/// </summary>
public static class GenomeStore
{
    public const string Key = "neuroevolution.best.v1";

    public static bool HasSave => PlayerPrefs.HasKey(Key);

    public static void Save(EvolutionSnapshot snapshot)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(snapshot));
        PlayerPrefs.Save(); // SetString não grava em disco sozinho
    }

    public static bool TryLoad(out EvolutionSnapshot snapshot, out string message)
    {
        snapshot = null;

        if (!PlayerPrefs.HasKey(Key))
        {
            message = "Nenhum save encontrado. Treine e use 'Salvar melhor' primeiro.";
            return false;
        }

        string json = PlayerPrefs.GetString(Key, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            message = "O save existe, mas está vazio. Apague o save e salve de novo.";
            return false;
        }

        EvolutionSnapshot loaded;
        try
        {
            loaded = JsonUtility.FromJson<EvolutionSnapshot>(json);
        }
        catch (Exception e)
        {
            message = $"Save corrompido (JSON inválido): {e.Message}";
            return false;
        }

        if (loaded == null || loaded.weights == null)
        {
            message = "Save corrompido: o snapshot não contém pesos.";
            return false;
        }

        if (loaded.version != EvolutionSnapshot.CurrentVersion)
        {
            message = $"Save recusado: versão {loaded.version} do formato; esperado {EvolutionSnapshot.CurrentVersion}.";
            return false;
        }

        if (loaded.weights.Length != NeuralNetwork.WeightCount)
        {
            message = $"Save incompatível: {loaded.weights.Length} pesos salvos, mas a rede atual usa {NeuralNetwork.WeightCount}.";
            return false;
        }

        for (int i = 0; i < loaded.weights.Length; i++)
        {
            if (float.IsNaN(loaded.weights[i]) || float.IsInfinity(loaded.weights[i]))
            {
                message = $"Save corrompido: o peso {i} não é um número válido.";
                return false;
            }
        }

        snapshot = loaded;
        message = "OK";
        return true;
    }

    public static string Delete()
    {
        if (!PlayerPrefs.HasKey(Key))
            return "Não havia save para apagar.";

        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        return $"Save apagado (chave '{Key}').";
    }
}
