using System;
using UnityEngine;

/// <summary>
/// Cenário de validação: onde o Goal será colocado durante o teste.
/// Crie um objeto vazio na cena (ex.: GoalPos_V1), posicione sobre o NavMesh e arraste aqui.
/// O marcador NÃO deve ter collider.
/// </summary>
[Serializable]
public sealed class ValidationScenario
{
    public string name = "Validação";
    public Transform goalPosition;
}
