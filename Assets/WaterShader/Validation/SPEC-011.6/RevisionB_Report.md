# SPEC-011.6 Revision B - segunda passada

Status: checkpoint reto NAO satisfeito; sem aprovacao artistica. Nenhuma candidata foi promovida a producao. O shader principal foi restaurado byte a byte ao estado encontrado no inicio desta passada, incluindo as alteracoes preexistentes do usuario.

## Entrega para revisao

- [Comparacao em movimento](RevisionB_Comparison.mp4): da esquerda para a direita, pre-011.6, primeira passada, candidata experimental 6, referencia. Recortes redimensionados para comparacao de linguagem, nao de tamanho fisico. A referencia contem foam fora do escopo.
- [Comparacao estatica](RevisionB_Comparison.png).
- [Seis experimentos](RevisionB_Trials.jpg), todos no mesmo canal reto, B constante 0.66, sem obstaculos, camera fixa.
- [Candidata 6 em movimento, 18 segundos](RevisionB_Trial6.mp4) e [sequencia temporal](RevisionB_MotionSequence.png).
- [Shader experimental isolado](RevisionB_Candidate.shader), nome Hidden/Water/SPEC0116RevisionBCandidate. Nao atribuido aos materiais da cena.
- [Medidas](RevisionB_Measurements.json) e [script de analise](revision_b_analysis.py).
- [Snapshot da primeira passada](RevisionB_FirstPass.shader.txt).

## Diagnostico

### Elongacao em trechos retos

A escala fisica final depende de tres fatores: anisotropia da fonte, escala longitudinal/transversal do chart e escala aplicada pelo pattern. Nao depende apenas do slider Stretch.

No caminho de producao anterior, com strength constante:

```
character = smoothstep(0, 1, B)
stretch = lerp(1.15, PatternStretch, character)
scale = PatternScale * lerp(0.58, 1, character)
primaryUV = channelUV * (2 * scale / stretch, scale / PrimaryMarkWidth)
```

Assim, o fator geometrico de aspecto relativo a fonte e aproximadamente:

```
(worldUnitsPerChartU / worldUnitsPerChartV) * stretch / (2 * PrimaryMarkWidth)
```

O fixture reto existente tem 20 x 4 unidades de mundo, mas UV2 ocupa 4 x 2.53. Portanto, seu fator chart e aproximadamente 3.16, nao 1. Isso e uma escolha anisotropica do fixture, nao prova de defeito do baker. O objeto CurvedWater_Channel informa FlowCoordinateWorldScale=(7.2, 2.2), tambem anisotropico. A geracao atual foi preservada.

Com B=0.66 e Stretch=7, stretch efetivo e aproximadamente 5.43. Com Width=4, o fator relativo a fonte no reto e aproximadamente 2.15. A propria fonte ja possui caudas longas. Em B=1, stretch chega a 7, ampliando mais essas caudas. Escala global altera tamanho/densidade, mas nao resolve esse aspecto em B constante.

O clock e comum e a translacao e longitudinal; no reto constante nao ha um mecanismo distinto para entrada ou saida. A elongacao aparece em todas as regioes conforme a mesma forma atravessa o canal. Campos B variaveis e deformacao de curvas nao foram usados para explicar esse teste.

### Silhuetas fibrosas e papel das amostras

Noise 1.png possui ramificacoes estreitas, serrilhas finas e faixas conectadas. O alongamento amplia essas ramificacoes. A raiz quadrada recupera meios-tons, mas nao remove a topologia fibrosa. Threshold/contraste ainda convertem variacoes da fonte em pontas e conexoes estreitas.

| Amostra anterior | Papel real | Limitacao |
|---|---|---|
| Primary em uv | Massa principal, sqrt + contraste + threshold | Continua herdando ramificacoes e silhueta recorrente da fonte |
| Secondary em uv*2 + offset | Mesma forma, metade do tamanho em ambos os eixos, peso 0.24 | Mesmo shaping, hierarquia baseada sobretudo em escala/intensidade |
| Mass em uv*0.5 + offset | Gate suave de distribuicao, aplicado a ambas | Suprime grupos inteiros; reforca intervalos vazios e concentracoes |

A composicao anterior era `gate * (primary + (1-primary)*secondary*strength)`. Ela e limitada a 1: nao soma duas primarias brancas independentes. O problema de massas claras pode vir de area/concentracao e cor HDR, nao de overflow aditivo das tres amostras. Nao foi demonstrado defeito no obstacle steering.

Distortion procedural ja estava ausente do modo 3 da primeira passada. Nao foi tratada como causa ativa desse modo nem reintroduzida.

## Experimentos executados

Todos usaram renders GPU reais pelo helper existente, tempo explicito por quadro e a mesma camera. Os MP4s estao nesta pasta. A avaliacao temporal usou sequencias de quadros e medidas de translacao; nao foi uma sessao continua de Play Mode nem uma inspecao humana dos videos.

| Teste | Alteracao | Resultado |
|---|---|---|
| [1](RevisionB_Trial1.mp4), 18 s / 30 fps | Frequencia longitudinal primary 2 para 4; restante baseline | Encurta caudas, mas comprime a mesma linguagem serrilhada e torna repeticao mais evidente |
| [2](RevisionB_Trial2.mp4), 12 s / 20 fps | Teste 1 + mip bias 2 na primary; gate reorientado, limites 0.12/0.35; max compositing, secondary independente | Distribuicao em faixas recorrentes; descartado |
| [3](RevisionB_Trial3.mp4), 12 s / 20 fps | Teste 1 + filtro transversal de tres taps; sem gate; secondary independente com metade do peso | Menos pontas finas, mas revela faixas conectadas da fonte; descartado |
| [4](RevisionB_Trial4.mp4), 12 s / 20 fps | Fonte reorientada 90 graus, frequencia longitudinal 2, filtro e gate original; secondary independente com metade do peso | Formas em arco e ritmo artificial; descartado |
| [5](RevisionB_Trial5.mp4), 18 s / 20 fps | Orientacao original; stretch com resposta de raiz; filtro transversal; gate original; secondary independente com metade do peso | Primarias mais contidas, mas secondary expoe uma rede repetitiva |
| [6](RevisionB_Trial6.mp4), 18 s / 20 fps | Teste 5 + recuperacao adicional de meios-tons; gate compartilhado; secondary com peso original; max compositing | Melhor candidata entre estes testes para corpo/elongacao, ainda insuficiente artisticamente |

O filtro usa centro com peso 0.5 e vizinhos transversais +/-0.035 com peso 0.25 cada, todos com mip bias 2. A candidata 6 aplica `pow(body,0.65)` antes do shaping existente. Seu stretch e `lerp(1.15,1+sqrt(max(PatternStretch-1,0)),character)`. A composicao e `max(primary,secondary*SecondaryMarkStrength)*gate`.

Nenhuma dessas operacoes altera RG/B, UV2/UV3, frame, steering ou a equacao de translacao. O clock ainda usa o mesmo calculo de metricas da amostragem; as frequencias continuam compativeis com o periodo inteiro, sem crossfade.

## Reto: entrada, centro e saida

Candidata 6: primary com mais corpo e comprimento menor. Ainda ha pontas serrilhadas, grupos com identidade quase igual e espacamento pouco deliberado. O encurtamento torna o tile reconhecivel mais frequentemente. A recuperacao de corpo tambem produz algumas massas compactas sem a articulacao painterly desejada.

| Regiao | Cobertura media, red > 50 | Avaliacao |
|---|---|---|
| Entrada | 9.14% | Formas entram continuamente, mas silhueta/repeticao nao satisfazem o checkpoint |
| Centro | 9.16% | Corpo melhor; composicao ainda concentrada em poucos grupos |
| Saida | 9.16% | Mesmo comportamento da entrada, sem defeito temporal exclusivo da borda |

Cobertura semelhante entre regioes nao prova boa composicao: e uma media temporal de um padrao que se translada. Os valores sao proxies de imagem sob esta camera/cor, nao area fisica de espuma ou criterio automatico de aprovacao.

Em 360 quadros, a translacao estimada foi 142 pixels/s, com correlacao compensada 0.999974. MAE entre quadros: media 6.375, maximo 6.620. A sequencia cobre aproximadamente seis ciclos de 2.98 s. Nao apareceu um pico global de reset nessa medida; quadros sequenciais preservam formas reconheciveis. Isso sustenta continuidade no fixture, nao certifica todos os estados ou ausencia universal de blinking.

## Validacoes posteriores

Nao executadas nesta segunda passada porque o checkpoint reto nao foi satisfeito:

- S-curve: compressao interna e smear externo da candidata NAO validados.
- Obstacle: halo/crescente e recombinacao da candidata NAO validados.
- Calm / River / Fast: somente River constante B=0.66 foi capturado nesta passada. Calm e Fast NAO validados.
- Scale 1 / non-uniform: NAO revalidados com a candidata.
- Fallback uniforme: caminho preservado no codigo, comportamento visual da candidata NAO revalidado.

Os videos e resultados anteriores continuam preservados, mas nao sao apresentados como validacao desta candidata. WaterObstacleContours, baker, materiais e cena nao foram editados nesta passada.

## Custo

Baseline chart: 3 leituras de Noise + 2 de Flow Map + 1 depth = aproximadamente 6 leituras/fragmento.

Candidata 6 chart: 5 leituras de Noise (3 primary, 1 secondary, 1 gate) + 2 de Flow Map + 1 depth = aproximadamente 8. Acrescenta duas leituras e uma potencia para recuperar corpo, mais sqrt na metrica de stretch. Sem benchmark GPU. O fallback sem chart duplicaria esse aumento de Noise por avaliar duas fases. Aumento nao justificado para promover este resultado incompleto.

Producao restaurada: custo permanece o da primeira passada.

## Decisao sobre o Noise e proxima necessidade artistica

O Noise atual permite reduzir elongacao e recuperar corpo sem perder translacao. Nos experimentos executados, nao forneceu variedade suficiente de massas painterly: filtrar remove detalhes, mas preserva faixas/ramificacoes; girar troca essas faixas por arcos; retirar o gate revela conexoes; manter o gate conserva agrupamentos repetidos.

Minha conclusao de trabalho e que a topologia da fonte e um limitador importante para o proximo ganho artistico. Isso nao prova que toda composicao possivel com ela falhara: esta passada tambem nao resolveu integralmente a distribuicao. Interrompi sem promover uma solucao insuficiente e sem contornar o checkpoint da Spec.

Uma futura fonte ou Water Pattern Generator deveria fornecer:

- Massas isoladas com nucleos largos e area interior util, sem espinha conectando muitos filamentos.
- Variacao independente de largura/comprimento, com poucas caudas muito longas.
- Bordas quebradas em escalas medias, com menos serrilhas uniformes e pontas finas.
- Primary com identidades variadas, nao varias escalas da mesma ramificacao.
- Distribuicao com intervalos negativos variados, evitando tanto redes conectadas quanto grandes blocos vazios.
- Tile sem emendas, com repeticao menos reconhecivel; silhouettes estaveis sob mipmaps e threshold moderado.

Nenhuma textura nova, gerador ou feature futura foi implementada. A decisao sobre mudar de recurso pertence a revisao artistica; SPEC-011.6 segue sem aprovacao.

## Integridade e compilacao

Unity 6000.6.0f1: ShaderUtil retornou zero mensagens para o shader principal restaurado e a candidata. A consulta final do console retornou zero warnings/errors. Nenhum script C# foi alterado. A cena ativa continua WaterShader_TestScene; objetos transitorios foram removidos pelo helper apos captura, sem salvar a cena.

As mudancas persistentes desta passada sao artefatos de validacao e este registro. O estado preexistente, incluindo arquivos modificados/deletados e arquivos fora de WaterShader, foi preservado. Nao houve alteracao de packages, URP ou referencias.
