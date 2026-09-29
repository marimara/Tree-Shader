# SPEC-011.6 — implementação e avaliação visual

> Segunda passada (Revision B, 2026-09-26): veja [diagnóstico, seis experimentos e motivo da interrupção no checkpoint reto](RevisionB_Report.md). A candidata não foi promovida: o shader de produção permanece no baseline da primeira passada. A revisão continua sem aprovação artística.

## Resultado

O modo de produção **Painterly Flow (3)** usa translação contínua no chart UV2, com massas largas e detalhes menores derivados exclusivamente do Noise existente. A comparação mostra melhora clara de continuidade, peso das pinceladas e hierarquia. Não foram implementados foam, reflection, refraction, waves ou SPEC-012.

Veja primeiro [antes / depois / referência em movimento](Before_After_Reference.mp4). Colunas: implementação anterior, proposta escolhida, recorte do vídeo de referência. O recorte inclui elementos fora do escopo; foam não foi usado como critério de aprovação do base pattern.

Esta é a avaliação técnica e visual da implementação, com evidências para revisão artística. Não representa uma aprovação humana já recebida do usuário.

## Diagnóstico da pipeline anterior

Foram isolados A–F antes de escolher G. [Painel dos estágios](Diagnosis_Stages.png), vídeos [A](Stage1.mp4), [B](Stage2.mp4), [C](Stage3.mp4), [D](Stage4.mp4), [E](Stage5.mp4), [F](F_BaselineStraight.mp4).

| Estágio | Observação |
|---|---|
| A: Noise sem shaping | A fonte contém silhuetas úteis. A anisotropia do próprio Noise, combinada com as métricas direcionais, já favorece linhas estreitas. |
| B: animação | O blend de duas posições diferentes faz marcas perderem intensidade enquanto outras aparecem. Há deslocamento, mas a identidade visual não sobrevive bem à mistura. |
| C: distortion | A deformação procedural altera a silhueta authored sem resolver a substituição temporal; acrescenta ondulações sem benefício suficiente. |
| D: contraste/threshold | Seleciona principalmente núcleos claros; estreita marcas e amplifica pequenas diferenças de UV. |
| E: selective mask | A máscara anterior fragmenta ainda mais as marcas e reduz seu peso. Não recupera continuidade. |
| F: conjunto anterior | Shaping não linear por fase seguido de crossfade mantém a troca perceptível de formas. Na curva/obstáculo, deslocar novamente a coordenada transversal amplifica resíduos RG/frame. |

Na malha existente com obstáculo, a auditoria de 98.426 centroides válidos encontrou componente transversal normalizada absoluta de aproximadamente 0,025 na mediana, 0,121 no percentil 90 e 0,459 no percentil 99. Esses resíduos eram multiplicados pelo deslocamento de cada fase. O threshold tornava a deformação muito mais evidente. Dados em [FrameResiduals.txt](FrameResiduals.txt).

Não foi necessário aumentar resolução do Flow Map, densidade da malha ou alterar o solver. A investigação identifica uma interação entre interpolação do campo/frame, advecção transversal redundante e shaping sensível; não atribui todo contorno angular a um defeito da malha. Algumas bordas angulares pertencem ao próprio Noise de 256 × 256.

## Abordagens avaliadas

1. Translação contínua com duas escalas, sem gate: continuidade boa, mas massas conectadas pareciam veios repetidos. [Trial 1](G_StraightTrial1.mp4).
2. Threshold 0,62, stretch 9,8 e largura 3: separava formas, porém voltava a estreitar as pinceladas.
3. Scale 1,65, stretch 7, threshold 0,56 e largura 4: melhor peso, mas conexões e repetição continuavam evidentes.
4. Escolhida: threshold 0,50, largura 4 e gate suave obtido do mesmo Noise em escala menor. O gate viaja junto com as duas escalas, separando massas sem introduzir um relógio ou máscara procedural independente.

Mover o threshold para depois do crossfade foi considerado conceitualmente, mas não é apresentado como experimento executado: ainda interpolaria duas formas distintas. A solução escolhida elimina essa mistura no caminho com chart.

## Alterações no shader

- `GetStableChannelPattern`: translação longitudinal do UV2 com um relógio compartilhado. RG, convertido pelo frame UV3, determina o sentido longitudinal; o chart já contém o contorno dos obstáculos.
- O período é exatamente compatível com as frequências inteiras das três amostras. O `frac` limita o tempo sem substituir a textura por outra fase.
- `ShapeAuthoredMark`: elevação suave dos meios-tons por raiz quadrada, contraste existente e suavidade mínima por `fwidth`. Recupera áreas largas sem borrar globalmente o padrão.
- Uma amostra primária, uma secundária menor e mais fraca, e uma amostra de baixa frequência para distribuição das massas. Todas compartilham deslocamento.
- Removidas distortion e máscara senoidal do modo 3. Modos 0–2 continuam úteis como comparação/debug.
- Novos controles: `Primary Mark Width` e `Secondary Mark Strength`. Valores escolhidos: 4 e 0,24; Pattern Scale 1,65, Stretch 7, Threshold 0,50, Softness 0,085 e Noise Contrast 1,55 nos materiais avaliados.

Flow Strength B continua controlando caráter, alongamento, densidade longitudinal e intensidade. O relógio consulta B em UV de mapa (0,5; 0,5), garantindo uma fase comum. Em B constante, conserva a resposta de velocidade por strength. Em B variável, a velocidade aparente depende da frequência espacial longitudinal estática, em vez de acumular relógios locais divergentes. Isso preserva estabilidade, mas não equivale à integração física geral de velocidades arbitrárias.

Foram preservados os dados RG/B, UV2, UV3, métricas Transform-aware, bake e steering. O fallback uniforme conserva sua convenção anterior de direção. A cena e os scripts do baker não foram regravados nesta implementação.

## Validação em movimento

As capturas são renders GPU reais do shader no Unity, com câmera fixa e tempo explícito por quadro. O helper usa objetos transitórios dentro da cena técnica e não salva alterações nela. Os vídeos isolam o base pattern com cor plana; [Production_TestScene.png](Production_TestScene.png) mostra também o shader de produção na cena real com sua composição existente.

Não se trata de uma gravação contínua de 138 segundos em Play Mode: há sequências determinísticas desde zero e janelas adicionais começando em 120 segundos. Não foi medido frame time de GPU.

O canal reto foi estabilizado antes das capturas de curva e obstáculo.

| Caso | Evidência | Resultado observado |
|---|---|---|
| Reto, B constante 0,66, sem obstáculo | [18 s, 30 fps](G_StraightGateMotion.mp4) | Marcas identificáveis viajam downstream; mais de três períodos, sem reset/crossfade perceptível. |
| S-shaped sem obstáculo | [18 s, 20 fps](H_Curved.mp4) | Mesma linguagem de formas acompanha as curvas sem a deformação transversal adicional. |
| Obstáculo existente | [novo, 18 s](I_Obstacle.mp4), [anterior](I_OldObstacle.mp4) | Massas contornam a silhueta e continuam downstream; fragmentação dinâmica anterior reduzida. |
| Scale 1 | [18 s](J_Scale1.mp4) | Peso e movimento coerentes. |
| Scale (10, 1, 2) | [18 s](J_NonUniform.mp4) | Geometria equivalente em mundo, rotação de 17° em ambos; espessura, densidade e velocidade comparáveis. |
| Calm, B = 0 | [72 s, 10 fps](K_Calm.mp4) | Mais largo, discreto e lento; cerca de três períodos, visualmente distinto do River. |
| River, B = 0,66 | [18 s](G_StraightGateMotion.mp4) | Hierarquia de massas e leitura direcional claras. |
| Fast, B = 1 | [18 s](K_Fast.mp4) | Mais alongado, dominante e rápido; mais de três períodos. |
| Tempo avançado, obstáculo | [120–138 s](L_LongRun.mp4) | Sem deriva acumulada ou colapso na janela amostrada. |
| B variável suave | [120–138 s](M_StrengthGradient.mp4) | Falloff moderado preserva continuidade; não valida todo campo B arbitrário. |
| Uniform-flow fallback | [6 s](N_Uniform.mp4) | Animação e convenção de direção anteriores preservadas, com o novo shaping. |
| Flow Speed = 0 | Dois quadros separados por 1 s | SHA-256 idêntico; superfície parada. |

Os períodos aproximados dos casos constantes são 5,7 s (River), 4,9 s (Fast) e 23,1 s (Calm). As imagens de sequência ajudam a acompanhar marcas: [reto](G_StraightMotionSequence.jpg), [curva](H_MotionSequence.jpg), [obstáculo](I_ObstacleMotionSequence.jpg).

### Medidas auxiliares

No canal reto, o deslocamento estimado em um segundo foi de 142 pixels tanto antes quanto depois. A correlação após compensar essa translação passou de **0,751 para 0,99998**: o avanço deixa de depender da substituição de formas. A mediana dos segmentos verticais acima de um limiar fixo passou de **1 para 4 pixels**, e o percentil 90 de **2 para 10 pixels**. A cobertura desse limiar passou de 3,86% para 5,74%, preservando bastante espaço negativo.

Nos 540 quadros novos, a diferença absoluta média entre quadros teve média 2,17 e máximo 2,43, sem pico de reset nessa sequência. Esses números são proxies de imagem sob a câmera de teste, não medidas físicas de largura nem substitutos da revisão visual. Dados e script: [Measurements.json](Measurements.json), [summarize_validation.py](summarize_validation.py).

## Comparação explícita com a referência

| Critério | Anterior | Nova versão frente à referência |
|---|---|---|
| Stroke width | Predominância de filamentos | Massas médias/largas, mais próximas do peso gráfico da referência. |
| Stroke length | Linhas fragmentadas pelo shaping/blend | Trechos longos e curtos coexistem; ainda há repetição reconhecível da fonte. |
| Painterly character | Aparência de thresholded noise deformado | Silhueta authored mais preservada; menos riqueza de borda e variedade que a referência. |
| Negative space | Muito vazio entre linhas muito finas | Espaços entre massas mais legíveis, sem preencher toda a superfície. |
| Hierarquia | Escala visual pouco diferenciada | Primárias largas acompanhadas por detalhes menores e mais fracos. |
| Continuidade temporal | Dissolução/substituição de marcas | Tradução coerente; acompanha-se a mesma marca por distância perceptível. |
| Direcionalidade | Direção geral correta, leitura prejudicada pelo blend | Movimento e forma comunicam downstream nas curvas e ao redor do obstáculo. |

O Noise atual foi suficiente para uma melhora substancial do base pattern. Nenhuma textura foi criada, substituída ou teve import settings alterados. A nova versão não reproduz toda a riqueza artística da referência, nem suas camadas fora do escopo.

## Custo e limitações

Contagem aproximada por fragmento, incluindo depth: caminho chart de produção passa de **4 para 6 leituras** (2 Flow Map + 3 Noise + 1 depth), enquanto remove distortion procedural e shaping duplicado por fase. Uniforme ganha duas leituras de Noise. O fallback legado de Flow Map sem chart passa de aproximadamente 10 para 14 leituras, pois ainda avalia duas fases com tracing. Não há benchmark de milissegundos.

- A correção temporal completa se aplica ao modo 3 com UV2/UV3 válidos. Modos de comparação e fallback de campo arbitrário sem chart ainda usam dual-phase e podem apresentar crossfade.
- O chart e RG devem representar o mesmo fluxo; mudar o campo independentemente do bake não produz transporte geral correto.
- Gradientes B abruptos/extremos e campos com inversões internas não foram aprovados. A amostra central de B determina o relógio global; o teste inclui apenas falloff suave representativo.
- Persistem repetição finita e algumas bordas angulares próprias do Noise. A deformação facetada animada anterior foi reduzida sem prometer ausência de qualquer aresta em qualquer aproximação de câmera.
- Estados constantes e um gradiente espacial foram testados; não há captura dedicada de uma mudança temporal interativa de todos os sliders.

## Compilação e reprodução

Unity 6000.6.0f1: shader principal, Baseline e ProductionCapture retornaram zero mensagens via ShaderUtil; helper Editor compilou sem erros. Há erros preexistentes de assinatura do Unity AI no console, sem relação com WaterShader.

O menu `Tools/WaterShader/SPEC-011.6` permite recapturar reto, obstáculo existente, Calm, Fast e os dois Transforms. Abra `WaterShader_TestScene` antes. O helper cria uma cópia de captura do shader atual substituindo apenas o relógio por tempo explícito. Os frames locais ficam em `.frames/`, ignorados pelo Git/Unity para evitar milhares de imports; os MP4s e resultados estão preservados nesta pasta. Os shaders Candidate/Baseline são instrumentos de comparação, não materiais de produção.

As alterações preexistentes do usuário em cena, baker e assets foram preservadas. O diff global já continha whitespace em YAML da cena; não foi corrigido como parte desta Spec.
