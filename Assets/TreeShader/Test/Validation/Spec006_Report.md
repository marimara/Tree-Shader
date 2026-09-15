# Spec 006 — Artistic Foliage Color System

## Implementado

Somente a Spec 006. A máscara de iluminação estilizada da Spec 005 agora percorre uma rampa suave `Deep Shadow → Shadow → Midtone → Light`. As quatro cores são propriedades editáveis do material e definem o hue da folhagem. Não foram adicionadas bandas discretas nem recursos da Spec 007.

## Tratamento da BaseMap

`leaf0_diff` continua sendo amostrada uma única vez, mas seu RGB não multiplica mais diretamente a cor artística. O shader extrai luminância da textura e a remapeia para uma modulação de detalhe limitada a 0.65–1.35. Assim, diferenças locais entre folhas e variação de valor permanecem visíveis, enquanto o verde-oliva da textura não pode dominar o hue final. `_BaseColor` permanece como tint global da paleta.

## Iluminação

As normais estilizadas, Main Light, máscara suavizada, atenuação de sombra, `_ShadowThreshold`, `_ShadowSoftness`, `_ShadowStrength` em stops e `_LightDirectionBias` foram preservados. A cor da Main Light contribui suavemente nas áreas iluminadas. O ambiente usa `SampleSH`, mas sua contribuição é multiplicada pela própria cor da paleta e limitada a 35%, evitando desaturação e washout.

## Assets modificados

- `Assets/TreeShader/Shaders/StylizedFoliage.shader`
- `Assets/TreeShader/Materials/MAT_StylizedFoliage_Test.mat`
- Este relatório e capturas `Spec006_*.png` nesta pasta.

`Assets/TreeShader/Test/TreeShader_TestScene.unity` foi usada e salva após os testes, com o Sun restaurado exatamente ao estado inicial; o arquivo não possui alteração de conteúdo em relação à linha de base. Nenhum source asset, mesh ou material de tronco foi alterado.

## Propriedades adicionadas

| Propriedade | Paleta inicial |
| --- | --- |
| `_LightColor` | amarelo-verde luminoso `(0.75, 1.00, 0.22)` |
| `_MidColor` | verde fresco saturado `(0.12, 0.72, 0.20)` |
| `_ShadowColor` | emerald/blue-green `(0.025, 0.46, 0.30)` |
| `_DeepShadowColor` | teal escuro `(0.02, 0.32, 0.30)` |

## Validação

- Unity 6000.6.0f1, URP, cena `TreeShader_TestScene`.
- Objetos verificados: `Tree_Test`, filho `leaves1`, `Foliage_Test`, Ground e Sun.
- Cada uma das quatro cores foi substituída temporariamente por uma cor de diagnóstico; cada controle alterou somente sua região esperada da rampa e foi restaurado.
- BaseMap real versus textura branca: a BaseMap real recupera variação local de valor; sua remoção deixa a copa mais plana. O hue oliva não reaparece na configuração final.
- Sun testado em `(45,325,0)`, `(45,55,0)`, `(45,235,0)`, `(45,145,0)`, `(80,325,0)` e `(10,325,0)`. As massas coloridas acompanham frente, lado, direção oposta, traseira, sol alto e baixo.
- Sombras None versus Soft confirmaram que a atenuação realtime continua alimentando a rampa; sombras Soft foram restauradas com força 0.8.
- `_ShadowThreshold` 0/1, `_ShadowSoftness` 0/1, `_ShadowStrength` 0/4 e `_StylizedNormalStrength` 0/1 produziram diferenças claras. `_LightDirectionBias=(0.25,0,0)` deslocou a resposta e foi restaurado a zero.
- `_AlphaClipThreshold=0.95` confirmou recorte conjunto de folhas e sombras; restaurado a 0.5. `Cull Off`, ShadowCaster e DepthOnly não foram modificados.
- Tronco permanece em `MAT_Bark_Test`; folhas permanecem em `MAT_StylizedFoliage_Test`.
- Shader suportado, `ShaderHasError=False`, zero mensagens de compilação.

## Limitações

- A rampa utiliza um único centro radial por objeto, mantendo as limitações já documentadas na Spec 004 para copas compostas e escala não uniforme.
- `_ShadowStrength` alto ainda pode tornar a região profunda muito escura, embora ela permaneça cromática e receba ambiente.
- O detalhe da BaseMap é deliberadamente apenas de luminância; variações cromáticas originais não são preservadas por design.
- As capturas MCP validam a leitura geral em resolução moderada, não substituem avaliação artística final em tela calibrada.

## Próxima Spec

`Spec 007 — Canopy Depth and Interior Shading` — não implementada nesta tarefa.
