# Spec 012 — Validação Final

## Status geral

**Aprovado.** O Stylized Foliage Shader final foi validado na cena `Assets/TreeShader/Test/TreeShader_TestScene.unity` em Unity 6000.6.0f1/URP. As funcionalidades das Specs 003–011 continuam funcionando em conjunto, sem regressão bloqueante observada. Nenhuma feature nova foi implementada e o shader/material principal não precisou de alteração nesta etapa.

## Assets finais

- Shader: `Assets/TreeShader/Shaders/StylizedFoliage.shader`
- Material principal: `Assets/TreeShader/Materials/MAT_StylizedFoliage_Test.mat`
- Cena: `Assets/TreeShader/Test/TreeShader_TestScene.unity`
- Relatório: `Assets/TreeShader/Test/Validation/Spec012_Report.md`
- Evidências novas: capturas `Assets/TreeShader/Test/Validation/Spec012_*.png`
- Evidências anteriores consultadas: controles de cor da Spec 006, AO da Spec 007, close-up de normal da Spec 009 e máscaras/distâncias da Spec 010.

O material `MAT_StylizedFoliage_Test 1.mat` já estava modificado antes da validação e foi preservado. Ele foi usado, sem gravação adicional, na árvore importada `Gledista_Triacanthos`.

## Objetos e assets testados

- `Tree_Test`: árvore principal, com `tree1` e filho `leaves1`.
- `Foliage_Test`: mesh `leaves1` sem tronco, exercitado isoladamente.
- `Sphere_Test`: referência Lit para confirmar a direção da Main Light.
- `Gledista_Triacanthos`: segunda árvore importada, 375.401 vértices, 4 submeshes e tangentes completas. Tronco e folhagem usam slots/materiais separados.

Os assets importados permaneceram inalterados. Os objetos adicionais foram ativados apenas temporariamente e restaurados ao estado inativo.

## Features validadas — Specs 003–011

### Fundação de foliage

- Alpha clipping funcional no Forward e nos passes auxiliares. O teste `_AlphaClipThreshold = 0.95` reduziu a silhueta e o recorte da sombra de forma correspondente.
- `Cull Off` mantém foliage two-sided. A captura traseira preserva a folhagem.
- O `ShadowCaster` usa a mesma `_AlphaMap`, transformação UV e threshold do Forward, produzindo sombras com forma de folhas.
- Tronco e folhagem continuam separados: `MAT_Bark_Test.mat` no renderer do tronco e `MAT_StylizedFoliage_Test.mat` em `leaves1`.

### Normais radiais e iluminação estilizada

- A copa mantém leitura arredondada e não expõe fortemente a orientação individual dos cards.
- `_StylizedNormalStrength = 0` versus o default `0.742` altera claramente a organização das massas de luz.
- `_CanopyCenterOffset` deslocado altera a distribuição radial e o volume interno.
- A Main Directional Light dirige a máscara. O `Sun` foi testado em frente `(45,325,0)`, lateral `(45,55,0)`, lado oposto `(45,235,0)`, alto `(80,325,0)` e baixo `(10,325,0)`.
- `_ShadowThreshold`, `_ShadowSoftness`, `_ShadowStrength` e `_LightDirectionBias` respondem e possuem funções distintas: extensão da região clara, largura da transição, contraste em stops e pequeno viés angular.
- O `Sun` foi restaurado ao default atual `(20,325,0)`; câmera restaurada a posição `(9.6,4.9,-7.2)`, rotação `(14.16132,322.045776,0)` e FOV 42.

### Sistema de cor

- A rampa contínua `Deep Shadow → Shadow → Mid → Light` responde em todas as regiões. As evidências diagnósticas da Spec 006 confirmam individualmente os quatro endpoints.
- As sombras permanecem verdes/saturadas, sem multiplicação direta para preto.
- A BaseMap contribui luminância limitada a `0.85–1.15`; sua textura continua legível sem assumir o hue final da copa.
- `_BaseColor` continua sendo um tint global compreensível.

### Profundidade e variação

- Fake density/interior darkening organiza miolo e sobreposições sem formar disco rígido.
- `_InteriorStrength`, `_InteriorRadius` e `_InteriorColor` respondem; regiões externas preservam a paleta.
- Height darkening responde a `_HeightDarkening` e `_HeightGradientPosition` e permanece subordinado ao volume.
- O renderer `PC_Renderer` tem `ScreenSpaceAmbientOcclusion` ativo. `_AOStrength` é funcional, deliberadamente sutil, e depende do pass `DepthNormalsOnly`.
- Normal noise e color variation respondem em força e escala. O default é sutil; força alta torna o efeito claramente visível sem alterar alpha.
- Não existem IDs per-card/per-island; a variação atual é um campo procedural contínuo em espaço do objeto.

### Normal map / leaf relief

- `_NormalMap`, `_NormalStrength` e `_NormalDetailContrast` produzem relevo local legível em regiões claras e escuras.
- O detalhe é aplicado como modulação estrutural pós-rampa no Forward e como normal detalhada no `DepthNormalsOnly`, preservando a forma radial de grande escala.
- `Tree_Test`, `Foliage_Test` e `Gledista_Triacanthos` possuem tangentes completas; a base tangente tem fallback finito para casos degenerados.

### Distance behaviour

- `_DistanceStart = 15` e `_DistanceEnd = 45` produzem fade por `smoothstep` calculado uma vez por posição do objeto, evitando gradiente entre cards.
- Pesos verificados: 1,0 em 15; 0,5 em 30; 0,0 em 45. Amostras imediatamente ao redor dos limites confirmaram continuidade.
- Capturas perto/médio/longe, inclusive com enquadramento compensado, mostram redução suave de normal noise, color variation e normal detail sem alteração da silhueta alpha.
- Um render estático repetido com vento zero produziu diferença exata de 0 pixels, sem shimmer temporal procedural.
- Em distância real a redução é sutil, como intencionado pela Spec 010. O enquadramento compensado torna a perda de microdetalhe mais evidente.

### Wind

- Testado com `_WindStrength = 0`, default `0.231` e valor alto `1`.
- Testadas as direções `(1,0,0)` e `(-0.6,0,0.8)`, com velocidades baixa `0.2` e alta `2.8`.
- A máscara procedural por volume mantém o miolo mais estável e move regiões externas fora de fase; a copa não se comporta como bloco rígido e não apresentou jitter.
- O tronco permanece imóvel porque usa renderer/material separado.
- `ForwardLit`, `ShadowCaster`, `DepthOnly` e `DepthNormalsOnly` chamam a mesma deformação de vértice. As capturas em frames distintos confirmam que a sombra alpha acompanha o movimento.
- Após o teste, o MaterialPropertyBlock foi removido e os defaults do material foram restaurados sem deixar a cena dirty.

## Passes e regressão

O material encontra os quatro passes esperados:

| Índice | Pass | Resultado |
| --- | --- | --- |
| 0 | `ForwardLit` | Válido; alpha, lighting, palette, depth/detail, distância e wind integrados |
| 1 | `ShadowCaster` | Válido; alpha clipping e wind compartilhados |
| 2 | `DepthOnly` | Válido; alpha clipping e wind compartilhados |
| 3 | `DepthNormalsOnly` | Válido; alpha, radial/noise/detail normals e wind compartilhados |

O shader está `isSupported = true`, possui exatamente 4 passes e zero mensagens de `ShaderUtil`. Os 13 materiais que usam o shader dentro de `Assets/TreeShader/Materials` possuem `_BaseMap`, `_AlphaMap` e `_NormalMap` atribuídas.

## Valores default finais — material principal

| Propriedade | Valor |
| --- | --- |
| `_BaseColor` | `(1,1,1,1)` |
| `_AlphaClipThreshold` | `0.5` |
| `_StylizedNormalStrength` | `0.742` |
| `_CanopyCenterOffset` | `(0.014050544,-0.002111507,-0.003863758,0)` |
| `_NormalStrength` | `1.0` |
| `_NormalDetailContrast` | `0.5` |
| `_NormalNoiseStrength` / `_NormalNoiseScale` | `0.18` / `0.75` |
| `_ShadowThreshold` / `_ShadowSoftness` / `_ShadowStrength` | `0.5` / `0.5` / `0.7` stops |
| `_LightDirectionBias` | `(0,0,0,0)` |
| `_LightColor` | `(0.36,0.68,0.18,1)` |
| `_MidColor` | `(0.18,0.55,0.20,1)` |
| `_ShadowColor` | `(0.08,0.42,0.20,1)` |
| `_DeepShadowColor` | `(0.055,0.34,0.19,1)` |
| `_InteriorColor` | `(0.035,0.24,0.13,1)` |
| `_InteriorStrength` / `_InteriorRadius` | `0.457` / `0.026` |
| `_AOStrength` | `0.35` |
| `_HeightDarkening` / `_HeightGradientPosition` | `0.15` / `-0.05` |
| `_ColorVariationStrength` / `_ColorVariationScale` | `0.18` / `0.9` |
| `_DistanceStart` / `_DistanceEnd` | `15` / `45` |
| `_WindDirection` | `(0.8,0,0.6,0)` |
| `_WindStrength` / `_WindSpeed` / `_WindScale` | `0.231` / `1.01` / `0.97` |

Todas as propriedades expostas têm uso no shader e efeito mensurável/compreensível. Nenhuma propriedade obsoleta ou redundante foi encontrada, e nenhuma foi renomeada.

## Resultado visual versus TargetArtDirection

O resultado atende à direção artística em caráter, não em correspondência literal: a copa apresenta massas de luz grandes e coerentes, transições suaves, verdes claros/médios saturados, sombras frias ainda coloridas, interior mais profundo e microdetalhe subordinado ao volume. Alpha e two-sided ocultam bem os cards na distância normal de uso.

A diferença principal vem do mesh `tree1`: sua silhueta é conífera, estratificada e mais espaçada que as copas largas e densas da referência. A segunda `Gledista_Triacanthos` confirma portabilidade funcional, mas o material específico atualmente produz foliage mais claro/raro e precisa de authoring próprio para convergir visualmente ao alvo. Isso não bloqueia o shader principal.

## Console

- Zero shader compilation errors.
- Zero material/property errors.
- Zero missing texture errors causados pelo TreeShader.
- Zero exceptions introduzidas pelo TreeShader.
- Durante a inspeção, dez entradas idênticas foram geradas pelo recurso `GameObjectResource` do próprio MCP: a conversão `UnityEngine.EntityId` lançou `NotImplementedException` dentro do pacote `com.coplaydev.unity-mcp`. A inspeção prosseguiu por `find_gameobjects`, hierarchy e código in-memory; o Editor/MCP permaneceu conectado e `ready_for_tools`. Também houve um warning do MCP informando que `batch_execute` serializou comandos solicitados em paralelo. Essas entradas são infraestrutura de validação, não erros do shader/projeto, e foram removidas do Console ao final depois de documentadas.
- Console final após a limpeza das entradas MCP geradas nesta sessão: zero errors e zero warnings.

## Performance / complexidade

- Forward usa três amostras lógicas: alpha, BaseMap e normal map.
- ShadowCaster e DepthOnly amostram apenas alpha; DepthNormalsOnly amostra alpha + normal.
- Não há loops por pixel.
- Os poucos branches/ternários protegem normalização, base tangente e direção de wind degeneradas; são justificados por estabilidade.
- Nenhum código morto ou propriedade sem uso foi identificado.
- Custo potencial para otimização futura: `ComputeDistanceDetailWeight` é repetido entre vertex/fragment, e o ValueNoise3D tem várias avaliações hash. Remover isso exigiria avaliar interpoladores/variants e não é uma correção segura de validação, portanto não foi alterado.

## Limitações conhecidas

- Um único canopy center/radius por material representa a copa inteira; árvores com vários clusters separados precisam de authoring/materialização adicional.
- Escala não uniforme produz resposta elipsoidal pela transformação inverse-transpose, não uma esfera perfeita em world space.
- SSAO artístico depende da renderer feature/keyword e do `DepthNormalsOnly`; sem essa configuração o fake density continua funcional, mas o termo SSAO fica neutro.
- Não há IDs per-card/per-island; normal noise e variação de cor usam campos procedurais contínuos.
- Distance behaviour é intencionalmente sutil em tamanho real de tela.
- Leaf relief depende de tangentes válidas; há fallback para degeneração local, mas assets sem tangentes devem ser corrigidos no import/authoring.
- Wind usa máscara procedural por posição/volume, sem vertex colors, UV2 ou pesos authored. Em árvores com tronco e folha combinados no mesmo submesh/material, essa máscara não garante isolamento do tronco.
- `_CanopyCenterOffset`, `_InteriorRadius`, eixo vertical local e controles de detalhe exigem calibração por modelo/material. A segunda árvore evidencia essa necessidade.
- Capturas do Game View têm 589×331; são adequadas para massas, silhueta e regressão, mas não substituem avaliação artística final em resolução de produção.

## Próximos passos recomendados — não implementados

- Sistema de LOD.
- Billboard/impostor para longa distância.
- Auditoria e redução de shader variants para plataformas-alvo.
- Suporte a múltiplos canopy clusters/centros.
- Authoring por árvore final: center/radius, eixo vertical, paleta, detail e wind mask.
- Integração e validação em cena real de produção, com iluminação, pós-processamento e densidade finais.

Não existe próxima Spec numerada: a sequência 003–012 está concluída. As sugestões acima são trabalho futuro separado.
