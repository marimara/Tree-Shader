# Spec 005 — Stylized Directional Lighting

## Implementado
Somente a Spec 005. N dot L usa as normais da Spec 004, remapeado de [-1,1] para [0,1] e suavizado ao redor do limiar. A atenuação de sombra URP modifica essa máscara. A máscara mistura dois extremos derivados de BaseMap * BaseColor, somando o ambiente existente. Não há multiplicação do RGB final pela sombra nem pelo NdotL bruto.

O extremo sombreado usa baseColor * exp2(-ShadowStrength): o controle é expresso em stops, evitando que seu máximo produza preto por construção. A direção e a cor/intensidade da Main Light continuam sendo entradas reais. Bias zero segue a luz sem deslocamento.

## Assets modificados
- Assets/TreeShader/Shaders/StylizedFoliage.shader
- Assets/TreeShader/Materials/MAT_StylizedFoliage_Test.mat
- Este relatório e capturas Spec005_*.png nesta pasta.
- TreeShader_TestScene foi salva após restaurar o Sun; nenhum ajuste intencional de conteúdo da cena.
Nenhum asset importado, material de tronco ou mesh foi alterado.

## Propriedades adicionadas
| Propriedade | Faixa | Default |
| --- | --- | --- |
| _ShadowThreshold | 0–1 (sinal Lambert remapeado) | 0.5 |
| _ShadowSoftness | 0–1 (largura total da transição) | 0.5 |
| _ShadowStrength | 0–4 stops | 1.5 |
| _LightDirectionBias | vetor world-space, magnitude limitada internamente a 0.25 | (0,0,0,0) |

Strength 0 conserva a cor base no extremo sombreado; 1 reduz esse extremo à metade; 4 a 1/16, antes do ambiente. Este controle regula contraste da sombra, não a intensidade da sombra projetada no chão. Softness 0 utiliza uma largura numérica mínima para evitar smoothstep degenerado. Bias máximo desvia a direção aparente em até aproximadamente 14.5 graus e não muda a projeção real da sombra.

## Validação Unity MCP
Unity 6000.6.0f1; URP PC_RPAsset; cena Assets/TreeShader/Test/TreeShader_TestScene.unity.
Objetos: Tree_Test com leaves1, Foliage_Test e chão receptor; esfera Lit como comparação da direção real.

- Seis rotações de Sun: (45,325,0), (45,55,0), (45,235,0), (45,145,0), (80,325,0), (10,325,0).
- Capturas finais mostram deslocamento da região iluminada conforme o Sun, incluindo lados opostos, contraluz, sol alto e baixo.
- Threshold 0 versus 1 altera a extensão da região iluminada.
- Softness 0 versus 1 altera bordas mais marcadas para transições largas.
- Strength 0 versus 4 altera contraste; sombras continuam com contribuição de cor e ambiente.
- StylizedNormalStrength 0 versus 1: normais radiais reduzem a iluminação independente dos cards.
- Sombras do Sun desligadas versus Soft: muda o sombreamento recebido pela copa e a projeção no chão.
- AlphaClipThreshold 0.5 versus 0.95: folhas e sombras ficam mais recortadas juntas; restaurado para 0.5.
- Vista traseira mantém folhagem visível; vista de Foliage_Test confirma a mesma resposta sem tronco.
- Bias (0.25,0,0) comparado ao padrão; restaurado para zero.
- ShadowCaster, DepthOnly, Cull Off, ZWrite, cálculos radiais e tratamento de backface não foram modificados.
- Materiais: MAT_Bark_Test permanece no tronco; MAT_StylizedFoliage_Test nas duas folhagens. BaseMap e AlphaMap preservados e válidos.
- CanopyCenterOffset preservado: (0.0140505442,-0.00211150665,-0.00386375748,0). StylizedNormalStrength=1.
- ShaderUtil: supported=True, ShaderHasError=False, zero mensagens.
- Console: nenhuma mensagem de erro de shader/propriedade/textura ou exceção de TreeShader. Permanecem mensagens de WebSocket MCP e NoSubscription do Unity AI; uma nova repetição de NoSubscription ocorreu durante o trabalho.
- Duas primeiras consultas execute_code tiveram falha de compilação do snippet de inspeção (escopo de variável e retorno obrigatório); foram corrigidas, sem adicionar scripts ao projeto.
- Assets salvos e cena salva via MCP. Sun restaurado para rotação original, intensidade 1, Soft, força 0.8. Câmera original preservada; capturas usam câmeras temporárias do MCP.

## Resultado visual e limitações
A copa inferior conserva cor em vez de depender apenas do ambiente mínimo quando NdotL é negativo. A transição ampla segue o volume radial, mantendo silhueta e sombras recortadas. A textura original ainda resulta em verde-oliva relativamente escuro e contém detalhe visível: esta etapa não estabelece a paleta final nem remove detalhe da textura.
Um único centro radial continua representando toda a copa, não cada ramo. Limitações de escala não uniforme da Spec 004 permanecem.
Strength alto pode deixar a imagem muito escura, embora a fórmula não zere o extremo sombreado. Textura ou BaseColor pretos continuam pretos. A resposta é artística e não modela escuridão física quando a luz é apagada.
Capturas MCP disponíveis foram produzidas em 438x452; comprovam massas e recorte geral, não equivalem a uma avaliação final em alta resolução.
Nenhuma implementação de paleta independente, interior/AO, detalhe, normal map, distância ou vento.

## Próxima Spec
006-color-system.md — não iniciada.
