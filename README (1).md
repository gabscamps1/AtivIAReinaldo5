# Neuroevolução — validação, comparação e persistência (Unity 6)

Rede neural `3-4-2` (26 pesos) evoluída por algoritmo genético, com rota de referência NavMesh, métricas de eficiência, validação em 3 cenários novos e save/load do melhor genoma.

## Integrantes

- Nome 1 — @usuario1
- Nome 2 — @usuario2

## Como abrir e usar a cena

1. Abra o projeto no Unity 6 e a cena `Assets/Scenes/Main.unity`.
2. Pacote **AI Navigation** instalado (Window > Package Manager). Na cena existe um `NavMeshSurface` com o bake feito.
3. `EvolutionManager`: associe Ship Prefab, SpawnPoint, Goal e preencha a lista **Validation Scenarios** com 3 marcadores (objetos vazios sem collider, sobre o NavMesh).
4. Objeto com `EvolutionHud` apontando para o `EvolutionManager`.
5. Aperte Play. Botões do painel:
   - **Salvar melhor**: grava o melhor genoma em `PlayerPrefs` (chave `neuroevolution.best.v1`).
   - **Carregar melhor**: pausa o treino e executa o genoma salvo, sem criar geração nova.
   - **Apagar save**: remove a chave e confirma no Console e no painel.
   - **Validar (3 cenários)**: roda o melhor genoma nos 3 cenários, sem evoluir, e imprime a tabela no Console.
   - **Retomar treino**: volta a treinar na geração atual.

Vídeo (60–90 s): <!-- link ou "postado no Teams" -->

## Rota de referência

`NavMesh.CalculatePath` entre SpawnPoint e Goal, com os cantos desenhados em verde na Game view e listados no Console. A rota **não controla** a nave; serve só de comparação. Distâncias medidas no plano XZ (a nave tem Y congelado).

## Métricas por episódio

| Métrica | Como é medida |
|---|---|
| Chegou | `OnTriggerEnter` no Goal |
| Colidiu | `OnCollisionEnter` com a layer `Obstacle` |
| Tempo | soma de `fixedDeltaTime` até o fim do episódio |
| Distância real | soma dos deslocamentos a cada `FixedUpdate` |
| Distância de referência | comprimento do caminho NavMesh |
| Eficiência | referência / real, **só** se chegou e as duas distâncias forem > 0 |

Observação: o episódio termina ao tocar o trigger do Goal, antes do centro, então a eficiência pode passar um pouco de 1,0.

## Fitness (não foi alterado)

`fitness = progresso até o alvo × 10 + 100 (chegada) − 50 (colisão) − 1 (timeout)`

## Parâmetros do treino

| Parâmetro | Valor |
|---|---|
| Seed | |
| População | 30 |
| Elitismo | 2 |
| Taxa de mutação | 0.08 |
| Força de mutação | 0.25 |
| Duração do episódio (s) | 15 |

Geração do melhor genoma: _  | Fitness: _

## Validação em cenários novos

Genoma avaliado: seed _, geração _, fitness _.

| Cenário | Chegou | Colidiu | Tempo (s) | Distância real (m) | Distância ref. (m) | Eficiência |
|---|---|---|---|---|---|---|
| Validação 1 | | | | | | |
| Validação 2 | | | | | | |
| Validação 3 | | | | | | |

Resultado: _/3 cenários sem colisão e com chegada (validado se ≥ 2).

## Parte D — Análise

**1. O melhor fitness correspondeu à rota mais eficiente?**
_Use as linhas do Console por geração (campo "mais eficiente ... (fitness ...)") e cite números de um caso ou contraexemplo._

**2. Em qual cenário de validação o agente foi pior? Por quê?**
_Cite o cenário e o motivo (sensores, curva, obstáculo, posição do Goal)._

**3. Que alteração no fitness poderia induzir um comportamento indesejado?**
_Descreva o atalho possível (ex.: o que acontece se o bônus de chegada for pequeno perto do peso do progresso)._

**4. O genoma salvo reproduziu o comportamento após fechar e reabrir a cena?**
_Informe seed, geração e resultado._
