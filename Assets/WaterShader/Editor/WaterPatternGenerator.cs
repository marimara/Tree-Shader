using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Meganeura.Water.Editor
{
    public enum WaterPatternRole { Primary, Secondary }

    [Serializable]
    public sealed class WaterPatternSettings
    {
        public WaterPatternRole Role = WaterPatternRole.Primary;
        public int Seed = 1107;
        public int Resolution = 256;
        [Range(0f, 360f)] public float Direction;
        [Range(3, 10)] public int MacroBandCount = 6;
        [Range(.025f, .16f)] public float BandWidth = .064f;
        [Range(0f, 1f)] public float WidthVariation = .62f;
        [Range(0f, 1f)] public float BaselineCurvature = .58f;
        [Range(0f, 1f)] public float PulseAmount = .62f;
        [Range(0f, 1f)] public float PulseStrength = .84f;
        [Range(.06f, .3f)] public float PulseWidth = .2f;
        [Range(0f, 1f)] public float PulseSharpness = .26f;
        [Range(0f, 1f)] public float CompressionStrength = .8f;
        [Range(0f, 1f)] public float BreakupAmount = .5f;
        [Range(1f, 12f)] public float BreakupScale = 5.4f;
        [Range(.03f, .35f)] public float BrushSmearLength = .2f;
        [Range(.025f, .18f)] public float BrushWidth = .08f;
        [Range(0f, 1f)] public float BrushSoftness = .82f;
        [Range(0f, 1f)] public float DryBrushVariation = .46f;
        [Range(0f, 1f)] public float SecondaryDetail = .24f;
        [Range(.5f, 2.5f)] public float Contrast = 1.05f;

        public WaterPatternSettings Clone() => (WaterPatternSettings)MemberwiseClone();

        public void ApplyPreset(WaterPatternRole role)
        {
            Role = role;
            if (role == WaterPatternRole.Primary)
            {
                MacroBandCount = 6; BandWidth = .064f; WidthVariation = .68f;
                BaselineCurvature = .72f; PulseAmount = .68f; PulseStrength = .84f;
                PulseWidth = .2f; PulseSharpness = .26f; CompressionStrength = .8f;
                BreakupAmount = .5f; BreakupScale = 5.4f; BrushSmearLength = .22f;
                BrushWidth = .08f; BrushSoftness = .82f; DryBrushVariation = .46f;
                SecondaryDetail = .2f; Contrast = 1.05f;
            }
            else
            {
                MacroBandCount = 8; BandWidth = .046f; WidthVariation = .72f;
                BaselineCurvature = .7f; PulseAmount = .48f; PulseStrength = .48f;
                PulseWidth = .12f; PulseSharpness = .55f; CompressionStrength = .62f;
                BreakupAmount = .58f; BreakupScale = 7.2f; BrushSmearLength = .14f;
                BrushWidth = .058f; BrushSoftness = .72f; DryBrushVariation = .62f;
                SecondaryDetail = .52f; Contrast = 1.28f;
            }
        }
    }

    public sealed class WaterPatternResult
    {
        public Texture2D Texture { get; internal set; }
        public Vector2 FlowDirection { get; internal set; }
        public float Coverage { get; internal set; }
        public float MidtoneCoverage { get; internal set; }
        public float NearBlackCoverage { get; internal set; }
        public double GenerationMilliseconds { get; internal set; }

        public string Summary(WaterPatternSettings settings)
        {
            int resolution = Texture != null ? Texture.width : settings.Resolution;
            return $"{settings.Role} | Seed {settings.Seed} | {resolution} px | " +
                $"Direction {settings.Direction:0.#} deg | {settings.MacroBandCount} macro bands | " +
                $"Highlights {Coverage:P1} | Midtones {MidtoneCoverage:P1} | " +
                $"Near black {NearBlackCoverage:P1} | {GenerationMilliseconds:0} ms";
        }
    }

    public static class WaterPatternGenerator
    {
        const string OutputFolder = "Assets/WaterShader/Generated/Patterns";
        const float Tau = Mathf.PI * 2f;

        struct PatternRandom
        {
            uint state;
            public PatternRandom(int seed)
            {
                state = unchecked((uint)seed) ^ 0xA511E9B3u;
                if (state == 0) state = 0x6D2B79F5u;
            }
            public float Next()
            {
                uint x = state; x ^= x << 13; x ^= x >> 17; x ^= x << 5; state = x;
                return (x & 0x00FFFFFFu) / 16777216f;
            }
            public float Signed() => Next() * 2f - 1f;
        }

        struct Pulse { public float center, width, amplitude, sign, skew; }

        struct Ribbon
        {
            public float baseV, width, brightness;
            public float curvePhaseA, curvePhaseB, widthPhase, breakupPhase, fiberPhase;
            public Pulse pulseA, pulseB;
        }

        sealed class RibbonField
        {
            public Vector2Int tangentLattice;
            public Vector2Int normalLattice;
            public Vector2 tangent;
            public Ribbon[] ribbons;
        }

        public static WaterPatternResult Generate(WaterPatternSettings source) =>
            GenerateAtResolution(source, Sanitize(source).Resolution);

        public static WaterPatternResult GeneratePreview(WaterPatternSettings source, int previewResolution) =>
            GenerateAtResolution(source, Mathf.Clamp(previewResolution, 64, 256));

        static WaterPatternResult GenerateAtResolution(WaterPatternSettings source, int resolution)
        {
            WaterPatternSettings settings = Sanitize(source);
            settings.Resolution = resolution;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            RibbonField field = CreateField(settings);
            int size = settings.Resolution;
            var pixels = new Color32[size * size];
            int highlights = 0, midtones = 0, nearBlack = 0;

            for (int y = 0; y < size; y++)
            {
                float py = (y + .5f) / size;
                for (int x = 0; x < size; x++)
                {
                    float value = Evaluate(new Vector2((x + .5f) / size, py), field, settings);
                    byte gray = (byte)Mathf.RoundToInt(value * 255f);
                    if (gray >= 128) highlights++;
                    if (gray >= 16 && gray < 240) midtones++;
                    if (gray < 16) nearBlack++;
                    pixels[y * size + x] = new Color32(gray, gray, gray, 255);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true, true)
            {
                name = $"T_WaterPattern_{settings.Role}_{settings.Seed}",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 1
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            watch.Stop();
            float count = size * size;
            return new WaterPatternResult
            {
                Texture = texture,
                FlowDirection = field.tangent,
                Coverage = highlights / count,
                MidtoneCoverage = midtones / count,
                NearBlackCoverage = nearBlack / count,
                GenerationMilliseconds = watch.Elapsed.TotalMilliseconds
            };
        }

        public static string Save(WaterPatternSettings settings, WaterPatternResult result = null)
        {
            int resolution = Sanitize(settings).Resolution;
            bool ownsResult = result == null || result.Texture == null || result.Texture.width != resolution;
            if (ownsResult) result = Generate(settings);
            EnsureFolder(OutputFolder);
            string path = $"{OutputFolder}/T_WaterPattern_{settings.Role}_{settings.Seed}.png";
            File.WriteAllBytes(path, result.Texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            ConfigureImporter(path, resolution);
            Debug.Log("[Water Pattern Generator] Saved " + path + "\n" + result.Summary(settings));
            if (ownsResult) UnityEngine.Object.DestroyImmediate(result.Texture);
            return path;
        }

        public static Texture2D CreateSilhouettePreview(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32(0);
            for (int i = 0; i < pixels.Length; i++)
            {
                byte value = pixels[i].r >= 128 ? (byte)255 : (byte)0;
                pixels[i] = new Color32(value, value, value, 255);
            }
            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };
            texture.SetPixels32(pixels); texture.Apply(false, false); return texture;
        }

        public static void ConfigureImporter(string path, int resolution)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Texture importer not found for " + path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = Mathf.Max(256, resolution);
            importer.SaveAndReimport();
        }

        public static Texture2D CreateTiledPreview(Texture2D source, int tiles, bool silhouette)
        {
            int tileSize = source.width, size = tileSize * tiles;
            Color32[] input = source.GetPixels32(0);
            var output = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Color32 pixel = input[(y % tileSize) * tileSize + x % tileSize];
                    if (silhouette)
                    {
                        byte value = pixel.r >= 128 ? (byte)255 : (byte)0;
                        pixel = new Color32(value, value, value, 255);
                    }
                    output[y * size + x] = pixel;
                }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat
            };
            texture.SetPixels32(output); texture.Apply(false, false); return texture;
        }

        public static float MeasureOppositeEdgeDifference(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32(0);
            int size = texture.width;
            double sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum += Mathf.Abs(pixels[i * size].r - pixels[i * size + size - 1].r);
                sum += Mathf.Abs(pixels[i].r - pixels[(size - 1) * size + i].r);
            }
            return (float)(sum / (size * 2.0 * 255.0));
        }

        [MenuItem("Tools/WaterShader/Generate Water Pattern Review Set")]
        public static void GenerateReviewSet()
        {
            const string folder = "Assets/WaterShader/Validation/PatternGenerator";
            EnsureFolder(folder);
            int[] seeds = { 1107, 2309, 4513, 7823 };
            foreach (WaterPatternRole role in Enum.GetValues(typeof(WaterPatternRole)))
                foreach (int seed in seeds)
                {
                    var settings = new WaterPatternSettings { Seed = seed, Resolution = 256, Direction = 0f };
                    settings.ApplyPreset(role);
                    WaterPatternResult result = Generate(settings);
                    string stem = $"{folder}/{role}_Seed{seed}";
                    File.WriteAllBytes(stem + "_Raw.png", result.Texture.EncodeToPNG());
                    Texture2D tiled = CreateTiledPreview(result.Texture, 2, false);
                    File.WriteAllBytes(stem + "_Tiled2x2.png", tiled.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(tiled);
                    UnityEngine.Object.DestroyImmediate(result.Texture);
                }

            var angled = new WaterPatternSettings { Seed = 1107, Resolution = 256, Direction = 32f };
            angled.ApplyPreset(WaterPatternRole.Primary);
            WaterPatternResult angledResult = Generate(angled);
            File.WriteAllBytes(folder + "/Primary_Seed1107_Direction32_Raw.png", angledResult.Texture.EncodeToPNG());
            Texture2D angledTiled = CreateTiledPreview(angledResult.Texture, 2, false);
            File.WriteAllBytes(folder + "/Primary_Seed1107_Direction32_Tiled2x2.png", angledTiled.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(angledTiled);
            UnityEngine.Object.DestroyImmediate(angledResult.Texture);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        static WaterPatternSettings Sanitize(WaterPatternSettings source)
        {
            if (source == null) source = new WaterPatternSettings();
            WaterPatternSettings s = source.Clone();
            s.Resolution = s.Resolution <= 256 ? 256 : s.Resolution <= 512 ? 512 : 1024;
            s.Direction = Mathf.Repeat(s.Direction, 360f);
            s.MacroBandCount = Mathf.Clamp(s.MacroBandCount, 3, 10);
            s.BandWidth = Mathf.Clamp(s.BandWidth, .025f, .16f);
            s.WidthVariation = Mathf.Clamp01(s.WidthVariation);
            s.BaselineCurvature = Mathf.Clamp01(s.BaselineCurvature);
            s.PulseAmount = Mathf.Clamp01(s.PulseAmount);
            s.PulseStrength = Mathf.Clamp01(s.PulseStrength);
            s.PulseWidth = Mathf.Clamp(s.PulseWidth, .06f, .3f);
            s.PulseSharpness = Mathf.Clamp01(s.PulseSharpness);
            s.CompressionStrength = Mathf.Clamp01(s.CompressionStrength);
            s.BreakupAmount = Mathf.Clamp01(s.BreakupAmount);
            s.BreakupScale = Mathf.Clamp(s.BreakupScale, 1f, 12f);
            s.BrushSmearLength = Mathf.Clamp(s.BrushSmearLength, .03f, .35f);
            s.BrushWidth = Mathf.Max(s.BandWidth * .72f, Mathf.Clamp(s.BrushWidth, .025f, .18f));
            s.BrushSoftness = Mathf.Clamp01(s.BrushSoftness);
            s.DryBrushVariation = Mathf.Clamp01(s.DryBrushVariation);
            s.SecondaryDetail = Mathf.Clamp01(s.SecondaryDetail);
            s.Contrast = Mathf.Clamp(s.Contrast, .5f, 2.5f);
            return s;
        }

        static RibbonField CreateField(WaterPatternSettings settings)
        {
            Vector2Int lattice = ChooseLatticeDirection(settings.Direction);
            Vector2Int normal = new Vector2Int(-lattice.y, lattice.x);
            var random = new PatternRandom(settings.Seed);
            var ribbons = new Ribbon[settings.MacroBandCount];
            float eventA = random.Next();
            float eventB = Mathf.Repeat(eventA + Mathf.Lerp(.32f, .58f, random.Next()), 1f);
            for (int i = 0; i < ribbons.Length; i++)
            {
                float spacing = 1f / ribbons.Length;
                ribbons[i] = new Ribbon
                {
                    baseV = Mathf.Repeat((i + .5f) * spacing + random.Signed() * spacing * .27f, 1f),
                    width = settings.BandWidth * Mathf.Lerp(.7f, 1.28f, random.Next()),
                    brightness = Mathf.Lerp(.72f, 1.08f, random.Next()),
                    curvePhaseA = random.Next() * Tau,
                    curvePhaseB = random.Next() * Tau,
                    widthPhase = random.Next() * Tau,
                    breakupPhase = random.Next() * Tau,
                    fiberPhase = random.Next() * Tau,
                    pulseA = CreatePulse(ref random, settings, i % 2 == 0 ? 1f : -1f, eventA),
                    pulseB = CreatePulse(ref random, settings, i % 3 == 0 ? -1f : 1f, eventB)
                };
            }
            return new RibbonField
            {
                tangentLattice = lattice,
                normalLattice = normal,
                tangent = new Vector2(lattice.x, lattice.y).normalized,
                ribbons = ribbons
            };
        }

        static Pulse CreatePulse(ref PatternRandom random, WaterPatternSettings settings,
            float preferredSign, float eventCenter)
        {
            float enabled = random.Next() < Mathf.Lerp(.28f, .94f, settings.PulseAmount) ? 1f : 0f;
            return new Pulse
            {
                center = Mathf.Repeat(eventCenter + random.Signed() * settings.PulseWidth * 1.35f, 1f),
                width = settings.PulseWidth * Mathf.Lerp(.72f, 1.42f, random.Next()),
                amplitude = enabled * settings.PulseStrength * Mathf.Lerp(.06f, .14f, random.Next()),
                sign = random.Next() < .72f ? preferredSign : -preferredSign,
                skew = random.Signed() * .42f
            };
        }

        static float Evaluate(Vector2 point, RibbonField field, WaterPatternSettings settings)
        {
            float s = Mathf.Repeat(point.x * field.tangentLattice.x + point.y * field.tangentLattice.y, 1f);
            float v = Mathf.Repeat(point.x * field.normalLattice.x + point.y * field.normalLattice.y, 1f);
            float accumulated = 0f, strongest = 0f, proximity = 0f;

            for (int i = 0; i < field.ribbons.Length; i++)
            {
                Ribbon ribbon = field.ribbons[i];
                float pulseA, compressionA, trailA, pulseB, compressionB, trailB;
                EvaluatePulse(s, ribbon.pulseA, settings.PulseSharpness, settings.BrushSmearLength,
                    out pulseA, out compressionA, out trailA);
                EvaluatePulse(s, ribbon.pulseB, settings.PulseSharpness, settings.BrushSmearLength,
                    out pulseB, out compressionB, out trailB);

                float baseline = Mathf.Sin(s * Tau + ribbon.curvePhaseA) *
                    (.012f + settings.BaselineCurvature * .042f);
                baseline += Mathf.Sin(s * Tau * 2f + ribbon.curvePhaseB) *
                    settings.BaselineCurvature * .016f;
                float center = ribbon.baseV + baseline + pulseA * ribbon.pulseA.amplitude * ribbon.pulseA.sign +
                    pulseB * ribbon.pulseB.amplitude * ribbon.pulseB.sign;

                float compression = Mathf.Clamp01(Mathf.Max(compressionA, compressionB));
                float widthNoise = .72f + .28f * Mathf.Sin(s * Tau * 2f + ribbon.widthPhase);
                widthNoise += .14f * Mathf.Sin(s * Tau * 5f + ribbon.widthPhase * 1.71f);
                float localWidth = ribbon.width * Mathf.Lerp(1f, widthNoise, settings.WidthVariation);
                localWidth *= 1f - compression * settings.CompressionStrength * .46f;

                float distance = Mathf.Abs(WrapSigned(v - center));
                float normalized = distance / Mathf.Max(.001f, localWidth);
                float body = Mathf.Exp(-normalized * normalized * Mathf.Lerp(1.5f, .78f, settings.BrushSoftness));
                float haloWidth = Mathf.Max(settings.BrushWidth, localWidth * 1.08f);
                float haloDistance = distance / haloWidth;
                float halo = Mathf.Exp(-haloDistance * haloDistance * Mathf.Lerp(2.4f, .82f, settings.BrushSoftness));

                float organicMask = Mathf.Lerp(1f,
                    SmoothStep(.25f, .76f, PeriodicBreakup(s, v, ribbon, settings.BreakupScale)),
                    settings.BreakupAmount);
                float dryFiber = .5f + .5f * Mathf.Sin(v * Tau * (18f + i * 2f) + s * Tau * 2f + ribbon.fiberPhase);
                dryFiber = Mathf.Lerp(1f, Mathf.Lerp(.55f, 1.06f, dryFiber), settings.DryBrushVariation);

                float trail = Mathf.Max(trailA, trailB);
                float localizedLight = compression * settings.CompressionStrength;
                float ribbonInk = (body * (.11f + localizedLight * 1.05f) +
                    halo * (.055f + trail * .32f + localizedLight * .45f)) *
                    organicMask * dryFiber * ribbon.brightness;
                strongest = Mathf.Max(strongest, ribbonInk);
                accumulated += ribbonInk * .58f;
                proximity += body * organicMask;

                if (settings.SecondaryDetail > .001f)
                {
                    float fineOffset = localWidth * (1.35f + .3f * Mathf.Sin(s * Tau * 3f + ribbon.fiberPhase));
                    float fineDistance = Mathf.Abs(WrapSigned(v - center - fineOffset));
                    float fine = Mathf.Exp(-Mathf.Pow(fineDistance / Mathf.Max(.002f, localWidth * .16f), 2f) * 1.7f);
                    float fineTail = .35f + .65f * Mathf.Max(trail,
                        .35f + .35f * Mathf.Sin(s * Tau * 4f + ribbon.breakupPhase));
                    strongest = Mathf.Max(strongest,
                        fine * fineTail * organicMask * settings.SecondaryDetail * .62f);
                }
            }

            float overlapLight = SmoothStep(1.05f, 2.05f, proximity) * settings.CompressionStrength * .38f;
            float combined = Mathf.Max(strongest, 1f - Mathf.Exp(-accumulated));
            combined = Mathf.Clamp01((combined + overlapLight - .035f) * 1.12f);
            return Mathf.Pow(combined, settings.Contrast);
        }

        static void EvaluatePulse(float s, Pulse pulse, float sharpness, float smearLength,
            out float displacement, out float compression, out float trail)
        {
            if (pulse.amplitude <= 0f)
            {
                displacement = compression = trail = 0f;
                return;
            }
            float x = WrapSigned(s - pulse.center);
            float normalized = x / Mathf.Max(.001f, pulse.width);
            float bell = Mathf.Exp(-normalized * normalized * Mathf.Lerp(2.1f, 4.2f, sharpness));
            float sCurve = (float)Math.Tanh(normalized * Mathf.Lerp(1.1f, 2.7f, sharpness)) * bell;
            float wedge = (float)Math.Tanh((normalized + pulse.skew) * 2.5f) *
                Mathf.Pow(bell, 1.5f) * sharpness * .18f;
            displacement = sCurve + wedge;
            compression = bell * (.65f + .35f * Mathf.Abs(sCurve));
            float trailingX = WrapSigned(x + smearLength * .5f);
            trail = Mathf.Exp(-trailingX * trailingX / Mathf.Max(.001f, smearLength * smearLength));
        }

        static float PeriodicBreakup(float s, float v, Ribbon ribbon, float scale)
        {
            int frequency = Mathf.Max(1, Mathf.RoundToInt(scale));
            float low = .5f + .5f * Mathf.Sin(s * Tau * frequency + ribbon.breakupPhase +
                Mathf.Sin(v * Tau * 2f + ribbon.curvePhaseB) * 1.2f);
            float medium = .5f + .5f * Mathf.Sin(s * Tau * (frequency + 3) +
                v * Tau * 3f + ribbon.breakupPhase * 1.61f);
            float fleck = .5f + .5f * Mathf.Sin(s * Tau * (frequency * 2 + 5) -
                v * Tau * 7f + ribbon.fiberPhase);
            return low * .5f + medium * .32f + fleck * .18f;
        }

        static float WrapSigned(float value) => Mathf.Repeat(value + .5f, 1f) - .5f;

        static Vector2Int ChooseLatticeDirection(float degrees)
        {
            float angle = degrees * Mathf.Deg2Rad;
            Vector2 target = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2Int best = Vector2Int.right;
            float bestScore = float.MinValue;
            for (int y = -3; y <= 3; y++)
                for (int x = -3; x <= 3; x++)
                {
                    if (x == 0 && y == 0 || GreatestCommonDivisor(Mathf.Abs(x), Mathf.Abs(y)) != 1) continue;
                    Vector2 candidate = new Vector2(x, y).normalized;
                    float score = Vector2.Dot(candidate, target) - new Vector2(x, y).magnitude * .012f;
                    if (score > bestScore) { bestScore = score; best = new Vector2Int(x, y); }
                }
            return best;
        }

        static int GreatestCommonDivisor(int a, int b)
        {
            if (a == 0) return Mathf.Max(1, b);
            if (b == 0) return Mathf.Max(1, a);
            while (b != 0) { int remainder = a % b; a = b; b = remainder; }
            return a;
        }

        static float SmoothStep(float from, float to, float value)
        {
            float t = Mathf.Clamp01((value - from) / Mathf.Max(.000001f, to - from));
            return t * t * (3f - 2f * t);
        }

        static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
