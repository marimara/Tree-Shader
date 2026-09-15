# Spec 006 — Refinamento do sistema artístico de cor

## Implementado

Somente sistemas já pertencentes à Spec 006 foram refinados. A rampa contínua `Deep Shadow → Shadow → Midtone → Light` continua sendo dirigida pela máscara de iluminação da Spec 005, mas agora aplica um viés suave (`lightingMask * 0.85 + 0.15`) antes dos três segmentos. Isso comprime a presença visual de Deep Shadow e faz MidColor/LightColor entrarem mais cedo, sem criar bandas discretas e sem remover a editabilidade de nenhuma das quatro cores.

Não foram adicionados canopy density, interior darkening, AO, normal noise, normal map, distance behaviour ou wind.

## Paleta final

| Propriedade | Valor linear RGBA |
| --- | --- |
| `_LightColor` | `(0.36, 0.68, 0.18, 1.00)` |
| `_MidColor` | `(0.18, 0.55, 0.20, 1.00)` |
| `_ShadowColor` | `(0.08, 0.42, 0.20, 1.00)` |
| `_DeepShadowColor` | `(0.055, 0.34, 0.19, 1.00)` |

O Light perdeu o caráter amarelo/lime excessivo. Shadow e Deep Shadow permanecem verdes saturados; Deep Shadow tem apenas uma tendência fria leve. A distância cromática e de valor entre os quatro pontos foi reduzida.

## Tratamento da BaseMap

`leaf0_diff` continua fornecendo somente luminância/detalhe, sem multiplicação do RGB original pela paleta. A modulação foi reduzida de `0.65–1.35` para `0.85–1.15`. O hue oliva continua sem controlar a identidade final, mas diferenças locais entre folhas e textura fina permanecem visíveis.

## Iluminação

`_ShadowStrength` final: `0.7` stop. `_ShadowThreshold = 0.5`, `_ShadowSoftness = 0.5`, `_LightDirectionBias = (0,0,0,0)` e `_StylizedNormalStrength = 1` foram preservados. A contribuição de Main Light e ambiente não foi reestruturada.

## Assets modificados

- `Assets/TreeShader/Shaders/StylizedFoliage.shader`
- `Assets/TreeShader/Materials/MAT_StylizedFoliage_Test.mat`
- `Assets/TreeShader/Test/Validation/Spec006_Report.md`
- Capturas `Spec006_Refine_*.png` desta pasta

`TreeShader_TestScene.unity` foi usada e salva após a restauração do Sun; o arquivo permanece sem diferença de conteúdo. Nenhum source asset, mesh ou material de tronco foi alterado.

## Validação

- Unity 6000.6.0f1, URP, cena `Assets/TreeShader/Test/TreeShader_TestScene.unity`.
- Objetos observados: `Tree_Test`, `Foliage_Test`, Ground e Sun.
- Sun testado em frente `(45,325,0)`, lateral `(45,55,0)`, direção oposta `(45,235,0)`, alto `(80,325,0)` e baixo `(10,325,0)`; restaurado para `(45,325,0)`.
- `_LightColor`, `_MidColor`, `_ShadowColor` e `_DeepShadowColor` foram substituídas individualmente por cores diagnósticas e cada uma afetou a região esperada. Deep Shadow ficou concentrada principalmente nas áreas direcionais mais profundas.
- `_AlphaClipThreshold = 0.95` confirmou o recorte das folhas e das sombras. `Cull Off`, ShadowCaster e DepthOnly não foram modificados.
- `_StylizedNormalStrength = 0/1`, `_ShadowStrength = 0/0.7` e `_ShadowSoftness = 0/0.5` continuaram produzindo diferenças claras; os valores finais foram restaurados.
- O shader foi reimportado, o Editor terminou o ciclo de compilação e permaneceu `ready_for_tools`, sem mensagens de shader no Console.
- O Console não contém erro causado por TreeShader. Permanecem mensagens não relacionadas: `NoSubscription` do pacote Unity AI e um aviso transitório do transporte WebSocket do MCP.

## Comparação com TargetArtDirection

O resultado final está visualmente mais próximo de `TargetArtDirection.png`: predominam verdes médios e claros, os highlights não estouram em lime, as sombras são verdes em vez de teal dominante, a transição é mais suave e as regiões muito escuras ocupam menos área. A BaseMap ainda separa folhas localmente, mas sua variação de luminância deixou de fragmentar as massas principais.

A correspondência não é literal: o mesh de teste é conífero e composto por foliage cards mais espaçados, enquanto a referência mostra copas largas e densas. Dentro da Spec 006, a paleta, contraste, saturação e balanço de luz/sombra estão razoavelmente alinhados ao alvo.

## Limitações

- A geometria do asset de teste limita a semelhança de silhueta e densidade com a referência.
- A rampa continua baseada em um único centro radial por objeto, com as limitações já documentadas na Spec 004 para copas compostas e escala não uniforme.
- Sob iluminação completamente oposta, uma área direcional sombreada grande ainda é esperada; não foi mascarada com densidade/AO porque isso pertence à Spec 007.
- A BaseMap contribui somente luminância por design; seu hue original não é preservado.

## Próxima Spec

`Spec 007 — Canopy Depth and Interior Shading` permanece o próximo marco, mas não foi implementada nem iniciada nesta tarefa.
