using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public sealed class EvolutionManager : MonoBehaviour
{
    public enum Mode { Training, Replay }

    [Header("Cena")]
    [SerializeField] private EvolvingShip shipPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform goal;

    [Header("Rota de referencia (NavMesh)")]
    [Tooltip("Se vazio, o componente e criado automaticamente neste objeto.")]
    [SerializeField] private ReferencePath referencePath;

    [Header("Validacao (3 cenarios novos)")]
    [SerializeField] private List<ValidationScenario> validationScenarios = new();

    [Header("Evolucao")]
    [SerializeField, Min(4)] private int populationSize = 30;
    [SerializeField, Min(1)] private int eliteCount = 2;
    [SerializeField, Range(0f, 1f)] private float mutationRate = 0.08f;
    [SerializeField, Min(0f)] private float mutationStrength = 0.25f;
    [SerializeField, Min(1f)] private float episodeSeconds = 15f;
    [SerializeField] private int randomSeed = 12345;

    [Header("Fitness")]
    [SerializeField] private float progressWeight = 10f;
    [SerializeField] private float goalBonus = 100f;
    [SerializeField] private float collisionPenalty = -50f;
    [SerializeField] private float timePenalty = 1f;

    private readonly List<EvolvingShip> ships = new();
    private readonly List<Genome> population = new();
    private System.Random random;
    private int finishedCount;

    // Execucao isolada (carregar save / validacao)
    private EvolvingShip replayShip;
    private bool replayFinished;
    private Coroutine routine;
    private Mode mode = Mode.Training;

    // Melhor genoma encontrado (copia independente da populacao)
    private Genome bestGenome;
    private int bestGeneration;
    private int bestSeed;
    private EpisodeResult bestEpisode;

    // Rota de referencia do cenario de treino
    private float trainingReference;
    private Vector3 trainingGoalPosition;

    public int Generation { get; private set; }
    public float BestFitness { get; private set; }
    public float AverageFitness { get; private set; }
    public float ProgressWeight => progressWeight;
    public float GoalBonus => goalBonus;
    public float CollisionPenalty => collisionPenalty;
    public float TimePenalty => timePenalty;
    public float EpisodeSeconds => episodeSeconds;

    // Acesso para o HUD
    public int Seed => randomSeed;
    public string ModeLabel => mode == Mode.Training ? "Treino" : "Execucao sem evolucao";
    public bool HasBestGenome => bestGenome != null;
    public float BestEverFitness => bestGenome != null ? bestGenome.Fitness : float.NaN;
    public int BestEverGeneration => bestGeneration;
    public EpisodeResult BestEpisode => bestEpisode;
    public EpisodeResult LastEpisode { get; private set; }
    public string LastMessage { get; private set; } = "Pronto.";
    public string ValidationSummary { get; private set; } = string.Empty;
    public List<EpisodeResult> ValidationResults { get; } = new();

    private void Start()
    {
        if (shipPrefab == null || spawnPoint == null || goal == null)
        {
            Debug.LogError("EvolutionManager precisa de Ship Prefab, Spawn Point e Goal.", this);
            enabled = false;
            return;
        }

        if (referencePath == null)
            referencePath = GetComponent<ReferencePath>();
        if (referencePath == null)
            referencePath = gameObject.AddComponent<ReferencePath>();

        trainingGoalPosition = goal.position;
        trainingReference = ComputeReference("Treino", true);

        random = new System.Random(randomSeed);
        for (int i = 0; i < populationSize; i++)
            population.Add(new Genome(NeuralNetwork.WeightCount, random));

        // Nave usada para executar um genoma isolado (carregar save e validar)
        replayShip = Instantiate(shipPrefab, spawnPoint.position, spawnPoint.rotation, transform);
        replayShip.gameObject.SetActive(false);

        BeginGeneration();
    }

    // ------------------------------------------------------------------
    // Treino
    // ------------------------------------------------------------------
    public void ReportFinished(EvolvingShip ship)
    {
        if (ship == replayShip)
        {
            replayFinished = true;
            return;
        }

        if (mode != Mode.Training)
            return;

        finishedCount++;
        if (finishedCount >= ships.Count)
            Evolve();
    }

    private void BeginGeneration()
    {
        mode = Mode.Training;
        finishedCount = 0;

        while (ships.Count < population.Count)
            ships.Add(Instantiate(shipPrefab, spawnPoint.position, spawnPoint.rotation, transform));

        for (int i = 0; i < population.Count; i++)
            ships[i].Begin(this, population[i], goal, spawnPoint.position, spawnPoint.rotation, trainingReference);
    }

    private void Evolve()
    {
        int reachedCount = 0;
        float bestShipFitness = float.NegativeInfinity;
        EpisodeResult bestShipResult = default;
        float bestEfficiency = float.NegativeInfinity;
        float bestEfficiencyFitness = 0f;

        foreach (EvolvingShip ship in ships)
        {
            ship.Genome.Fitness = ship.Fitness;
            EpisodeResult r = ship.Result;

            if (r.reached) reachedCount++;
            if (ship.Fitness > bestShipFitness)
            {
                bestShipFitness = ship.Fitness;
                bestShipResult = r;
            }
            if (r.HasValidEfficiency && r.Efficiency > bestEfficiency)
            {
                bestEfficiency = r.Efficiency;
                bestEfficiencyFitness = ship.Fitness;
            }
        }

        population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
        BestFitness = population[0].Fitness;
        AverageFitness = population.Average(genome => genome.Fitness);

        // Guarda uma COPIA dos pesos: a populacao continua sendo alterada pela evolucao.
        if (bestGenome == null || BestFitness > bestGenome.Fitness)
        {
            bestGenome = population[0].Clone();
            bestGeneration = Generation;
            bestSeed = randomSeed;
            bestEpisode = bestShipResult;
        }

        string efficiencyInfo = bestEfficiency > float.NegativeInfinity
            ? $"mais eficiente {EpisodeResult.Fmt(bestEfficiency)} (fitness {bestEfficiencyFitness:F2})"
            : "nenhuma eficiencia valida";
        Debug.Log($"Geracao {Generation}: melhor {BestFitness:F2}, media {AverageFitness:F2}, " +
                  $"chegaram {reachedCount}/{ships.Count}, melhor nave: eficiencia {bestShipResult.EfficiencyText}, " +
                  $"tempo {EpisodeResult.Fmt(bestShipResult.time, "F1")}s; {efficiencyInfo}");

        var next = new List<Genome>(populationSize);
        int protectedCount = Mathf.Min(eliteCount, population.Count);
        for (int i = 0; i < protectedCount; i++)
            next.Add(population[i].Clone());

        while (next.Count < populationSize)
        {
            Genome parentA = TournamentPick();
            Genome parentB = TournamentPick();
            Genome child = Genome.Crossover(parentA, parentB, random);
            child.Mutate(random, mutationRate, mutationStrength);
            next.Add(child);
        }

        population.Clear();
        population.AddRange(next);
        Generation++;
        BeginGeneration();
    }

    private Genome TournamentPick()
    {
        const int tournamentSize = 3;
        Genome best = population[random.Next(population.Count)];
        for (int i = 1; i < tournamentSize; i++)
        {
            Genome contender = population[random.Next(population.Count)];
            if (contender.Fitness > best.Fitness)
                best = contender;
        }
        return best;
    }

    // ------------------------------------------------------------------
    // Salvar / carregar / apagar (Parte C)
    // ------------------------------------------------------------------
    public void SaveBest()
    {
        if (bestGenome == null)
        {
            Say("Nada para salvar: nenhuma geracao foi avaliada ainda.");
            return;
        }

        var snapshot = new EvolutionSnapshot
        {
            seed = bestSeed,
            generation = bestGeneration,
            bestFitness = bestGenome.Fitness,
            weights = (float[])bestGenome.Weights.Clone(),
            populationSize = populationSize,
            mutationRate = mutationRate,
            mutationStrength = mutationStrength,
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        GenomeStore.Save(snapshot);
        Say($"Melhor genoma salvo: seed {snapshot.seed}, geracao {snapshot.generation}, fitness {snapshot.bestFitness:F2}.");
    }

    public void LoadBestAndReplay()
    {
        if (!GenomeStore.TryLoad(out EvolutionSnapshot snapshot, out string message))
        {
            Say(message); // sem excecao: o fluxo continua normalmente
            return;
        }

        EnterReplay();

        bestGenome = MakeGenome(snapshot.weights, snapshot.bestFitness);
        bestGeneration = snapshot.generation;
        bestSeed = snapshot.seed;

        Say($"Save carregado: seed {snapshot.seed}, geracao {snapshot.generation}, fitness {snapshot.bestFitness:F2}, " +
            $"populacao {snapshot.populationSize}, mutacao {snapshot.mutationRate:F2}/{snapshot.mutationStrength:F2}. " +
            "Executando sem criar nova geracao.");

        routine = StartCoroutine(ReplayRoutine(bestGenome.Clone(), "Save carregado"));
    }

    public void DeleteSave()
    {
        Say(GenomeStore.Delete());
    }

    public void ResumeTraining()
    {
        if (mode == Mode.Training)
        {
            Say("O treino ja esta em andamento.");
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        replayShip.Stop();
        RestoreGoal();
        Say($"Treino retomado na geracao {Generation} (a geracao atual recomeca, sem avancar).");
        BeginGeneration();
    }

    // ------------------------------------------------------------------
    // Validacao em cenarios novos (Parte B)
    // ------------------------------------------------------------------
    public void RunValidation()
    {
        if (bestGenome == null)
        {
            Say("Sem genoma para validar: evolua ao menos uma geracao ou carregue um save.");
            return;
        }

        if (validationScenarios.Count == 0 || validationScenarios.Any(s => s == null || s.goalPosition == null))
        {
            Say("Configure os cenarios de validacao no Inspector (nome + marcador de posicao do Goal).");
            return;
        }

        EnterReplay();
        routine = StartCoroutine(ValidationRoutine(bestGenome.Clone()));
    }

    private IEnumerator ValidationRoutine(Genome genome)
    {
        ValidationResults.Clear();
        ValidationSummary = string.Empty;
        Say($"Validando o melhor genoma (geracao {bestGeneration}, seed {bestSeed}) em {validationScenarios.Count} cenarios, sem evoluir.");

        foreach (ValidationScenario scenario in validationScenarios)
        {
            goal.position = scenario.goalPosition.position;
            Physics.SyncTransforms();

            EpisodeResult result = default;
            yield return RunEpisode(genome, scenario.name, r => result = r);
            ValidationResults.Add(result);
        }

        RestoreGoal();

        int passed = ValidationResults.Count(r => r.Passed);
        bool validated = passed >= 2;
        ValidationSummary = validated
            ? $"VALIDADO: {passed}/{ValidationResults.Count} cenarios sem colisao e com chegada."
            : $"NAO VALIDADO: {passed}/{ValidationResults.Count} cenarios (minimo 2).";

        var sb = new StringBuilder();
        sb.AppendLine($"[Validacao] seed {bestSeed}, geracao {bestGeneration}, fitness {bestGenome.Fitness:F2}");
        sb.AppendLine("| Cenario | Chegou | Colidiu | Tempo (s) | Dist. real (m) | Dist. ref. (m) | Eficiencia |");
        sb.AppendLine("|---|---|---|---|---|---|---|");
        foreach (EpisodeResult r in ValidationResults)
        {
            sb.AppendLine($"| {r.label} | {(r.reached ? "sim" : "nao")} | {(r.collided ? "sim" : "nao")} | " +
                          $"{EpisodeResult.Fmt(r.time, "F1")} | {EpisodeResult.Fmt(r.realDistance)} | " +
                          $"{EpisodeResult.Fmt(r.referenceDistance)} | {r.EfficiencyText} |");
        }
        sb.AppendLine(ValidationSummary);
        Debug.Log(sb.ToString(), this);

        Say(ValidationSummary);
        routine = null;
    }

    // ------------------------------------------------------------------
    // Execucao isolada de um genoma
    // ------------------------------------------------------------------
    private IEnumerator ReplayRoutine(Genome genome, string label)
    {
        yield return RunEpisode(genome, label, r =>
            Say($"{label}: chegou={(r.reached ? "sim" : "nao")}, colidiu={(r.collided ? "sim" : "nao")}, " +
                $"tempo={EpisodeResult.Fmt(r.time, "F1")}s, real={EpisodeResult.Fmt(r.realDistance)}m, " +
                $"ref={EpisodeResult.Fmt(r.referenceDistance)}m, eficiencia={r.EfficiencyText}"));
        routine = null;
    }

    private IEnumerator RunEpisode(Genome genome, string label, Action<EpisodeResult> done)
    {
        yield return null; // um frame para a posicao do Goal sincronizar com a fisica

        float referenceDistance = ComputeReference(label, false);
        replayFinished = false;
        replayShip.Begin(this, genome, goal, spawnPoint.position, spawnPoint.rotation, referenceDistance);

        while (!replayFinished)
            yield return null;

        EpisodeResult result = replayShip.Result;
        result.label = label;
        LastEpisode = result;
        done?.Invoke(result);
    }

    private void EnterReplay()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        mode = Mode.Replay;
        foreach (EvolvingShip ship in ships)
            ship.Stop();
        replayShip.Stop();
        RestoreGoal();
    }

    private void RestoreGoal()
    {
        goal.position = trainingGoalPosition;
        Physics.SyncTransforms();
    }

    private float ComputeReference(string label, bool logCorners)
    {
        if (!referencePath.Compute(spawnPoint.position, goal.position, out float length))
        {
            Debug.LogWarning($"[{label}] Rota de referencia invalida: a eficiencia sera marcada como invalida.", this);
            return 0f;
        }

        if (logCorners)
            referencePath.LogCorners(label);
        return length;
    }

    private static Genome MakeGenome(float[] weights, float fitness)
    {
        // Random separado para nao mexer na sequencia aleatoria do treino
        var genome = new Genome(NeuralNetwork.WeightCount, new System.Random(0));
        Array.Copy(weights, genome.Weights, NeuralNetwork.WeightCount);
        genome.Fitness = fitness;
        return genome;
    }

    private void Say(string message)
    {
        LastMessage = message;
        Debug.Log("[Neuroevolucao] " + message, this);
    }
}
