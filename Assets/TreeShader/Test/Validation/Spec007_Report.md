# Spec 007 — Canopy Depth and Interior Shading

## Implementado

Foi adicionado um sinal barato de densidade em espaço do objeto, baseado na distância a um elipsoide suave centrado em `_CanopyCenterOffset`. A transição larga separa casca externa, volume médio e núcleo sem traçar raios e sem formar um disco duro no centro geométrico.

O sinal mistura a cor já produzida pela rampa artística da Spec 006 com `_InteriorColor`. Não há multiplicação escura do RGB final: o interior converge para um verde profundo e saturado, enquanto regiões externas com máscara zero continuam usando integralmente a paleta, a iluminação e a modulação de luminância da BaseMap existentes.

Um gradiente vertical sutil em espaço local contribui apenas perto do corpo da copa. O shader também lê o Screen Space Ambient Occlusion do URP quando `_SCREEN_SPACE_OCCLUSION` está disponível. O AO acrescenta uma pequena contribuição ao tint interno, ponderada pela densidade existente, em vez de multiplicar a árvore em direção ao preto.

Nenhum sistema da Spec 008 ou posterior foi implementado.

## Assets modificados

- `Assets/TreeShader/Shaders/StylizedFoliage.shader`
- `Assets/TreeShader/Materials/MAT_StylizedFoliage_Test.mat`
- `Assets/TreeShader/Test/Validation/Spec007_Report.md`
- Capturas `Spec007_*.png` nesta pasta

`Assets/TreeShader/Test/TreeShader_TestScene.unity` foi usada, salva durante a validação e restaurada à sua serialização original; não possui diferença de conteúdo. Nenhum source asset, mesh ou material de tronco foi alterado.

## Propriedades adicionadas

| Propriedade | Valor final | Função |
| --- | --- | --- |
| `_InteriorColor` | `(0.035, 0.24, 0.13, 1)` | Verde profundo usado como destino cromático do interior |
| `_InteriorStrength` | `0.75` | Intensidade principal do fake density |
| `_InteriorRadius` | `0.026` | Escala do volume interno em unidades do objeto |
| `_AOStrength` | `0.35` | Peso artístico do SSAO disponível |
| `_HeightDarkening` | `0.15` | Intensidade do viés inferior |
| `_HeightGradientPosition` | `-0.05` | Posição normalizada da transição vertical |

## Validação

- Unity 6000.6.0f1, URP, cena `Assets/TreeShader/Test/TreeShader_TestScene.unity`.
- Objetos observados: `Tree_Test` e `Foliage_Test`; o material do tronco permaneceu separado.
- O mesh de folhagem mede aproximadamente `0.0465 × 0.0372 × 0.0396` em espaço local. `_CanopyCenterOffset` coincide com o centro do bounds, e o raio foi calibrado contra esses dados.
- `_InteriorStrength = 0` foi comparado diretamente com o valor final `0.75`. O valor final aprofunda miolo e sobreposições internas; os extremos do elipsoide permanecem próximos de máscara zero e preservam a paleta externa da Spec 006.
- O interior continua verde colorido e com detalhe legível; não vira preto nem forma um bloco circular sólido.
- Sun testado em frente `(45,325,0)`, lateral `(45,55,0)`, direção oposta `(45,235,0)`, alto `(80,325,0)` e baixo `(10,325,0)`; restaurado para `(45,325,0)`.
- O renderer `PC_Renderer` possui `ScreenSpaceAmbientOcclusion` ativo, portanto o caminho de AO foi exercitado na cena.
- `_AlphaClipThreshold = 0.95` confirmou recorte de folhas e sombras. O pass `ShadowCaster`, o pass `DepthOnly` e `Cull Off` não foram modificados.
- `_StylizedNormalStrength = 0/1` continua alterando claramente a leitura card-by-card versus volume radial; o valor final foi restaurado para `1`.
- `_ShadowStrength = 0/0.7` continua alterando a exposição de sombra sem quebrar a rampa; o valor final foi restaurado para `0.7`.
- A rampa contínua `Deep Shadow → Shadow → Midtone → Light`, a contribuição apenas luminosa da BaseMap e a iluminação do Main Light não foram reestruturadas.
- O shader foi reimportado, o Editor terminou a compilação e permaneceu `ready_for_tools`.
- Não há erro de shader, material ou TreeShader no Console. Permanecem somente mensagens preexistentes e não relacionadas: `NoSubscription` do Unity AI e um aviso transitório de WebSocket do MCP.

## Comparação com TargetArtDirection

O resultado se aproxima do princípio visual da referência: bordas e folhagem exposta continuam claras, o corpo médio mantém verdes saturados e regiões internas recebem um verde mais profundo sem perder cor. A mudança organiza melhor o volume, mas respeita a silhueta conífera e mais espaçada do mesh de teste em vez de tentar reproduzir a copa larga da referência por escurecimento global.

## Limitações

- O fake density usa um único centro e raio por material. Copas compostas por volumes muito separados podem exigir materiais/centros distintos.
- O viés vertical usa o eixo Z local, que é o eixo vertical do source asset atual. Assets com convenção de eixo diferente exigem adaptação da orientação do objeto ou do shader.
- As limitações já documentadas para escala não uniforme e centro radial continuam válidas.
- O SSAO só contribui quando a feature e a keyword do URP estão disponíveis; sem elas, o termo retorna neutro e o fake density continua funcional.
- A direção completamente oposta ainda produz a grande massa sombreada esperada da Spec 006; a Spec 007 não recalibra essa paleta nem a iluminação anterior.

## Próxima Spec

O próximo marco é `Spec 008 — Detail Variation`, mas não foi iniciado.
