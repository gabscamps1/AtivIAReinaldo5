Neuroevolução - Unity 6 (Aula 05, versão experimental)
Feito por:
Rodrigo Mamede Laurino
Gabriel de Campos
Nos acabamos fazendo o trabalho em dupla pois já estavamos trabalhando nele juntos em aula.

Rede neural 3-4-2 (26 pesos) evoluída por algoritmo genético, sem backpropagation. Compara a "nave" com uma rota de referência (linha verde na cena), valida o melhor genoma em três cenários fora do treino e salva/carrega o melhor genoma.

Arquivos de vídeo
Arquivo	Conteúdo
vídeo_de_evolução.mp4 (63 s)	Vídeo único de demonstração: evolução, salvamento, carregamento e validação
cenários1.mp4	Validação com o genoma da geração 30 (Cenário 1 concluído)
cenários2.mp4	Validação com o genoma da geração 14 (Cenário 2 concluído)
cenários3.mp4	Validação com o genoma da geração 18 (Cenário 3 concluído)
Cena e painel
Arena quadrada com paredes e obstáculos; SpawnPoint, Goal (e posições de Goal usadas nos cenários) e um NavMeshSurface para a rota de referência.
A rota de referência é desenhada em verde (Gizmos) e não controla a nave: serve só de comparação.
Painel na tela: modo, geração, seed, melhor genoma (fitness e geração), existência de save e os botões Salvar melhor, Carregar melhor, Apagar save, Validar (3 cenários), Retomar treino e Abrir pasta JSON.
A linha Ultimo episodio mostra, a cada episódio: eficiência, distância real e distância de referência.
Métricas por episódio

chegou, colidiu, tempo (s), distância real, distância de referência e eficiência = referência / real. A eficiência é inválida quando a nave não chega ao alvo (painel: "efic. inválida (não chegou)").

fitness = (distância_inicial - distância_atual_ao_Goal) * 10
          + 100 se chegou | -50 se colidiu | -1 se timeout
Parâmetros e seed
Seed: 12345 (a mesma em todos os vídeos).
População: 30 · taxa de mutação: 0,10 · força de mutação: 0,25 (valores exibidos pelo painel ao carregar o save).
Elite: 2.
Vídeo único de demonstração (vídeo_de_evolução.mp4, 63 s)

Seed 12345. Tempos aproximados.

Tempo	O que acontece	O que o painel mostra
0:00	Treino começa; já existia um save de antes ("Save existente: sim")	Geração 1, melhor fitness 9,5
0:03 a 0:42	Evolução ao longo de 17 gerações, com a rota verde visível	Melhor fitness sobe de 55,3 (ger. 1) para 145,1 (ger. 16)
~0:43	Clique em Salvar melhor	"Melhor genoma salvo: seed 12345, geracao 16, fitness 145,13. JSON exportado em ...\GenomeExports\best-genome.json"
~0:47	Surge um genoma bem melhor	Geração 18, fitness 277,5 (salto de 145,1)
~0:50	Segundo Salvar melhor	"Melhor genoma salvo: seed 12345, geracao 18, fitness 277,46. JSON exportado ..."
~0:51	Clique em Carregar melhor	"Save carregado: seed 12345, geracao 18, fitness 277,46, populacao 30, mutacao 0,10/0,25. Executando sem criar nova geracao." Modo: "Execução sem evolução"
0:54	Episódio do genoma carregado	Save carregado: eficiência 1,02, real 19,07 m, ref 19,52 m
0:55 a 1:00	Clique em Validar (3 cenários)	Validando o melhor genoma (geração 18, seed 12345) em 3 cenários, sem evoluir
1:00	Resultado final	NÃO VALIDADO: 1/3 cenários (mínimo 2)
Progresso do melhor fitness (do painel "Melhor genoma")
Geração	0	1	3	5	7	8	9	13	15	16	17	18
Melhor fitness	9,5	55,3	69,6	79,3	113,3	135,7	135,8	143,1	144,8	145,1	145,1	277,5

O salto de 145,1 para 277,5 na geração 18 é compatível com um indivíduo que passou a alcançar o Goal (bônus de chegada de +100 mais progresso); confirme observando o episódio.

Cobertura dos itens pedidos no vídeo
Item pedido	No vídeo
Rota de referência desenhada ou listada	A linha verde aparece durante todo o vídeo, mas a câmera está aproximada e só mostra um trecho dela
Evolução até uma boa geração	Sim (geração 1 até 19)
Salvamento do melhor genoma	Sim (duas vezes, gerações 16 e 18)
Reinício da cena e carregamento do genoma	Carregamento sim; o reinício da cena não aparece (o contador de geração não volta a 1)
Execução de validação	Sim (3 cenários, resultado no painel)
Validação nos três cenários, na ordem dos vídeos

Cada vídeo executa "Validar (3 cenários)" com um genoma salvo diferente, sem evoluir (o painel mostra "Validando o melhor genoma ... em 3 cenarios, sem evoluir"). Em cada vídeo, um dos três cenários foi concluído com sucesso, o que coincide com o número do vídeo.

Cenário 1

Genoma restaurado: geração 350, fitness 211,7, seed 12345. O contador do painel está em "Geração: 1", ou seja, a cena foi reiniciada e o genoma veio do save.

Cenário	Chegou	Colidiu	Tempo (s)	Dist. real (m)	Dist. ref. (m)	Eficiência
1	sim	não	27,6	19,22	17,12	0,89
2	não	não	50,0	20,65	14,98	inválida
3	não	não	50,0	20,65	18,76	inválida

Resultado do painel: NÃO VALIDADO: 1/3 cenários (mínimo 2). Observação: no Cenário 1 a nave chegou ao alvo sem colisão com eficiência 0,89. Nos Cenários 2 e 3 ela não chegou e não colidiu: esgotou os 50 s com exatamente a mesma distância (20,65 m).

Cenário 2

Genoma restaurado: geração 14, fitness 239,7, seed 12345. Ao carregar o save, o painel mostrou o episódio de treino: chegou = sim, colidiu = não, tempo = 18,5 s, real = 15,32 m, ref = 14,93 m, eficiência = 0,97.

Cenário	Chegou	Colidiu	Tempo (s)	Dist. real (m)	Dist. ref. (m)	Eficiência
1	não	sim	21,5	17,90	16,94	inválida
2	sim	não	18,4	15,31	14,99	0,98
3	não	sim	21,5	17,90	18,87	inválida

Resultado do painel: NÃO VALIDADO: 1/3 cenários (mínimo 2). Observação: este foi o único genoma com colisões. Nos Cenários 1 e 3 ele bateu em 21,5 s, com a mesma distância (17,90 m). No Cenário 2 chegou com eficiência 0,98.

Cenário 3

Genoma restaurado: geração 18, fitness 277,5 (277,46 no save), seed 12345. Ao carregar o save, o painel mostrou o episódio de treino: chegou = sim, colidiu = não, tempo = 34,3 s, real = 19,07 m, ref = 19,52 m, eficiência = 1,02. Os mesmos números aparecem no vídeo único.

Cenário	Chegou	Colidiu	Tempo (s)	Dist. real (m)	Dist. ref. (m)	Eficiência
1	não	não	40,0	21,74	16,94	inválida
2	não	não	40,0	21,74	14,98	inválida
3	sim	não	33,0	18,29	18,87	1,03

Resultado do painel: NÃO VALIDADO: 1/3 cenários (mínimo 2). Observação: o genoma de maior fitness (277,5) não colidiu em nenhum cenário, mas chegou em apenas um. A validação deste genoma aparece idêntica nas duas gravações.

Resumo
Genoma (geração)	Fitness	Cenário 1	Cenário 2	Cenário 3	Cenários OK	Validado
350	211,7	chegou (0,89)	timeout	timeout	1/3	não
14	239,7	colidiu	chegou (0,98)	colidiu	1/3	não
18	277,5	timeout	timeout	chegou (1,03)	1/3	não

Critério do trabalho: alcançar o alvo sem colisão em pelo menos 2 de 3 cenários. Nenhum dos três genomas testados atingiu o critério.

Parte C - Salvar e carregar
Salvar melhor: grava o melhor genoma e exibe Melhor genoma salvo: seed, geracao, fitness. JSON exportado em ...GenomeExports\best-genome.json (pasta GenomeExports do projeto).
Carregar melhor: exibe Save carregado: seed 12345, geracao 18, fitness 277,46, populacao 30, mutacao 0,10/0,25. Executando sem criar nova geracao. O modo passa a "Execução sem evolução" e o contador de geração não avança.
Conteúdo do snapshot visível no painel: seed, geração, fitness, população, taxa e força de mutação (além dos pesos).
Save existente: o painel mostra "Save existente: sim/não".
Mecanismo e chave usados: JSON.
Não aparecem nos vídeos: o botão Apagar save, a mensagem quando não há save e a recusa de save com quantidade de pesos incompatível: ____.
Parte D - Análise

1. O melhor fitness correspondeu à rota mais eficiente? No episódio de treino, sim para os dois genomas em que o painel mostra o resultado: a geração 18 (fitness 277,5) teve eficiência 1,02 e a geração 14 (fitness 239,7) teve 0,97. Mas isso não se manteve na validação: o genoma de maior fitness (geração 18) chegou em um só cenário (eficiência 1,03), enquanto o de fitness menor (geração 14) chegou em outro com 0,98; ambos chegaram em 1/3. Além disso, a eficiência acima de 1 (1,02 e 1,03) mostra que a nave percorreu menos que a rota de referência, ou seja, a rota não é um limite inferior estrito (a nave "corta" onde o caminho de referência mantém folga). Outro dado: ao evoluir, o fitness ficou preso em ~145 por várias gerações (13 a 17) e só saltou para 277,5 na geração 18, então o fitness mede se chegou, mas não quão eficiente foi o caminho. Para a geração 30 saltou para 210,5.

2. Em qual cenário o agente teve pior desempenho? Por quê? Depende do genoma, pois cada um foi bem em um cenário diferente. O pior caso observado foi o do genoma da geração 14, que colidiu bastante antes de aprender. Os outros dois genomas falharam por timeout sem colisão (50 s e 40 s). Um indício forte da causa: em cada vídeo, dois cenários diferentes produziram tempo e distância idênticos (20,65 m; 17,90 m; 21,74 m). Como a rede recebe só 3 sensores de distância e não vê o Goal, a trajetória do genoma depende só da geometria da pista, não de onde está o Goal. Ele chega apenas quando o Goal daquele cenário cai no caminho que o genoma já faz, e nos demais cenários repete a mesma trajetória sem sucesso.

3. Qual alteração no fitness poderia induzir um comportamento indesejado? O progresso usa a distância em linha reta ao Goal, sem considerar paredes. Se a penalidade de colisão for reduzida (por exemplo, de -50 para -5), a nave passa a ganhar mais "disparando" contra a parede mais próxima do Goal: acumula progresso × 10 e perde pouco ao bater. O oposto também aparece nos vídeos: como o timeout custa só -1, contra -50 da colisão, ficar vivo sem chegar sai barato (os timeouts de 40-50 s sem colisão). Aumentar o peso de tempo ou de chegada rápida sem rebalancear a colisão pode levar a batidas rápidas em vez de contornar obstáculos.

4. O genoma salvo reproduziu o comportamento esperado após fechar e reabrir a cena?

Geração 18, seed 12345 (vídeo único): salvo com fitness 277,46 e carregado em seguida (população 30, mutação 0,10/0,25), executando sem criar geração nova. Resultado do episódio: eficiência 1,02, real 19,07 m, ref 19,52 m. Na validação: Cenário 3 concluído (33,0 s, 18,29 m, eficiência 1,03), Cenários 1 e 2 em timeout.
Reprodução entre gravações: o vídeo cenários3.mp4 mostra o mesmo genoma (geração 18, fitness 277,5) com resultados idênticos (eficiência 1,02, real 19,07 m, ref 19,52 m; validação 21,74 m, 40,0 s e 18,29 m, 33,0 s). Isso indica que o comportamento é reprodutível com a seed 12345.
Cena reiniciada: o vídeo 1 começa com "Geração: 1" e o genoma da geração 350 (fitness 211,7) já restaurado. No vídeo único o reinício da cena não é mostrado (o contador chega a 20 e continua); resultado depois de fechar e reabrir a cena com a geração 18: ____.
Como executar
Abra a cena SampleScene, confirme que o EvolutionManager tem Ship Prefab, SpawnPoint, Goal e a rota de referência configurados, e defina a seed (12345).
Aperte Play e deixe evoluir; use Salvar melhor em uma boa geração.
Reinicie a cena e use Carregar melhor.
Use Validar (3 cenários) e leia a tabela no painel.
