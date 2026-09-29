"""Offline mirror and visual investigation harness for the SPEC-011.7 field generator.

The production workflow remains WaterPatternGeneratorWindow. This script mirrors
the deterministic periodic-field equations so candidate families can be compared
quickly before they are integrated into the Unity Editor implementation.
"""
from dataclasses import dataclass, replace
from pathlib import Path
import hashlib
import json
import math
import time

import numpy as np
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[1]
OLD_SOURCES = ROOT / "GeneratedSources"
FIELD_SOURCES = ROOT / "FieldSources"
EXPERIMENTS = ROOT / "FieldExperiments"
SEEDS = [1107, 2309, 4513, 7823]
TAU = math.pi * 2.0


class PatternRandom:
    def __init__(self, seed):
        self.state = (seed & 0xFFFFFFFF) ^ 0xA511E9B3
        if not self.state:
            self.state = 0x6D2B79F5

    def next(self):
        x = self.state
        x ^= (x << 13) & 0xFFFFFFFF
        x ^= x >> 17
        x ^= (x << 5) & 0xFFFFFFFF
        self.state = x & 0xFFFFFFFF
        return (self.state & 0x00FFFFFF) / 16777216.0

    def signed(self):
        return self.next() * 2.0 - 1.0


@dataclass
class Settings:
    role: str
    seed: int
    resolution: int = 512
    direction: float = 0.0
    band_count: int = 3
    band_width: float = 0.34
    width_variation: float = 0.92
    warp_strength: float = 0.74
    warp_scale: float = 1.15
    curvature: float = 0.84
    continuity: float = 0.74
    breakup_amount: float = 0.78
    breakup_scale: float = 1.10
    branching: float = 0.58
    ridge_sharpness: float = 0.42
    edge_softness: float = 0.56

    @classmethod
    def preset(cls, role, seed):
        if role == "Primary":
            return cls(role, seed)
        return cls(
            role, seed, band_count=6, band_width=0.18,
            width_variation=0.88, warp_strength=0.58, warp_scale=1.65,
            curvature=0.62, continuity=0.55, breakup_amount=0.86,
            breakup_scale=1.9, branching=0.30, ridge_sharpness=0.60,
            edge_softness=0.48)


def smoothstep(lo, hi, value):
    t = np.clip((value - lo) / np.maximum(1e-6, np.asarray(hi) - np.asarray(lo)), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def choose_lattice_normal(direction_degrees, requested_count):
    angle = math.radians(direction_degrees)
    target = np.array([-math.sin(angle), math.cos(angle)], dtype=np.float32)
    best = None
    best_score = float("inf")
    limit = max(2, requested_count + 2)
    for ky in range(-limit, limit + 1):
        for kx in range(-limit, limit + 1):
            length = math.hypot(kx, ky)
            if length < 0.5:
                continue
            direction = np.array([kx, ky], dtype=np.float32) / length
            alignment = abs(float(np.dot(direction, target)))
            angular_error = 1.0 - alignment
            magnitude_error = abs(length - requested_count) / requested_count
            score = angular_error * 4.0 + magnitude_error
            if score < best_score:
                best_score = score
                best = (kx, ky)
    if np.dot(np.array(best), target) < 0:
        best = (-best[0], -best[1])
    return best


def make_terms(rng, count, max_frequency, directional_bias=None):
    terms = []
    for index in range(count):
        best = None
        best_score = -1e9
        for _ in range(8):
            kx = int(math.floor(rng.signed() * max_frequency + 0.5))
            ky = int(math.floor(rng.signed() * max_frequency + 0.5))
            if kx == 0 and ky == 0:
                kx = 1
            length = math.hypot(kx, ky)
            score = -length * 0.04 + rng.next() * 0.15
            if directional_bias is not None:
                tangent = np.array(directional_bias, dtype=np.float32)
                tangent /= max(1e-6, np.linalg.norm(tangent))
                wave = np.array([kx, ky], dtype=np.float32) / length
                score += abs(float(np.dot(wave, tangent))) * 0.24
            if score > best_score:
                best_score = score
                best = (kx, ky)
        amplitude = (0.58 ** index) * (0.72 + 0.28 * rng.next())
        terms.append((best[0], best[1], rng.next() * TAU, amplitude))
    total = sum(term[3] for term in terms)
    return [(kx, ky, phase, amp / total) for kx, ky, phase, amp in terms]


def periodic_field(px, py, terms):
    value = np.zeros_like(px)
    for kx, ky, phase, amplitude in terms:
        value += np.sin((px * kx + py * ky) * TAU + phase) * amplitude
    return value


def generate(settings, variant="chosen"):
    started = time.perf_counter()
    size = settings.resolution
    coords = (np.arange(size, dtype=np.float32) + 0.5) / size
    px, py = np.meshgrid(coords, coords)
    rng = PatternRandom(settings.seed + (0 if settings.role == "Primary" else 0x31A7))
    normal = choose_lattice_normal(settings.direction, settings.band_count)
    tangent = (normal[1], -normal[0])

    warp_frequency = max(1, int(round(1.0 + settings.warp_scale * 1.6)))
    warp_terms = make_terms(rng, 4, warp_frequency, tangent)
    width_terms = make_terms(rng, 4, max(2, int(round(1.0 + settings.warp_scale))), None)
    breakup_terms = make_terms(rng, 4, max(2, int(round(settings.breakup_scale * 1.8))), None)
    intensity_terms = make_terms(rng, 3, 3, None)
    branch_terms = make_terms(rng, 3, max(1, warp_frequency + 1), tangent)
    branch_mask_terms = make_terms(rng, 3, 2, tangent)

    warp = periodic_field(px, py, warp_terms)
    width_field = periodic_field(px, py, width_terms)
    breakup_field = periodic_field(px, py, breakup_terms)
    intensity_field = periodic_field(px, py, intensity_terms)
    branch_warp = periodic_field(px, py, branch_terms)
    branch_mask_field = periodic_field(px, py, branch_mask_terms)

    phase = normal[0] * px + normal[1] * py
    phase += warp * settings.warp_strength * settings.curvature * 0.72
    phase += width_field * settings.warp_strength * 0.10
    phase_distance = np.abs(np.mod(phase + 0.5, 1.0) - 0.5)

    width_modulation = 1.0 + settings.width_variation * (
        width_field * 0.72 + warp * 0.28)
    width_modulation = np.clip(width_modulation, 0.04, 1.96)
    half_width = settings.band_width * 0.5 * width_modulation
    breakup_threshold = -0.48 + (1.0 - settings.continuity) * 0.92
    breakup_gate = smoothstep(breakup_threshold - 0.24, breakup_threshold + 0.26,
                              breakup_field + width_field * 0.18)
    half_width *= (1.0 - settings.breakup_amount) + settings.breakup_amount * breakup_gate
    softness = 0.012 + settings.edge_softness * 0.080
    main = 1.0 - smoothstep(np.maximum(0.002, half_width - softness),
                            half_width + softness, phase_distance)

    if variant != "phase_only" and settings.branching > 0.0:
        branch_phase = phase + 0.22 + branch_warp * (0.12 + 0.34 * settings.curvature)
        branch_distance = np.abs(np.mod(branch_phase + 0.5, 1.0) - 0.5)
        branch_width = half_width * (0.42 + settings.branching * 0.52)
        branch = 1.0 - smoothstep(np.maximum(0.002, branch_width - softness),
                                  branch_width + softness, branch_distance)
        branch_regions = smoothstep(0.05, 0.55, branch_mask_field + width_field * 0.28)
        branch *= smoothstep(0.06, 0.42, branch_regions * settings.branching)
        main = np.maximum(main, branch)

    if variant == "phase_only":
        no_breakup_width = settings.band_width * 0.5 * width_modulation
        main = 1.0 - smoothstep(np.maximum(0.002, no_breakup_width - softness),
                                no_breakup_width + softness, phase_distance)

    if variant == "multiscale":
        detail_phase = phase * 1.92 + branch_warp * 0.24 + 0.31
        detail_distance = np.abs(np.mod(detail_phase + 0.5, 1.0) - 0.5)
        detail_width = half_width * 0.26
        detail = 1.0 - smoothstep(np.maximum(0.002, detail_width - softness * 0.45),
                                  detail_width + softness * 0.72, detail_distance)
        detail_mask = smoothstep(0.12, 0.70, breakup_field - width_field * 0.18)
        main = np.maximum(main, detail * detail_mask * (0.20 if settings.role == "Primary" else 0.34))

    profile_power = 0.78 + settings.ridge_sharpness * 1.70
    main = np.power(np.clip(main, 0.0, 1.0), profile_power)
    intensity = np.clip(0.88 + intensity_field * (0.10 if settings.role == "Primary" else 0.16), 0.58, 1.0)
    output = np.clip(main * intensity, 0.0, 1.0)
    if settings.role == "Secondary":
        output *= 0.82

    elapsed_ms = (time.perf_counter() - started) * 1000.0
    metadata = {
        "normal": normal,
        "generation_ms": elapsed_ms,
        "coverage_gt_127": float(np.mean(output >= 0.5)),
        "mean": float(np.mean(output)),
    }
    return np.uint8(output * 255.0 + 0.5), metadata


def tiled(image, count):
    return Image.fromarray(np.tile(np.asarray(image), (count, count)), "L")


def font(size):
    return ImageFont.truetype("C:/Windows/Fonts/arial.ttf", size)


def contact_sheet(entries, title, columns=2, panel=420):
    rows = int(math.ceil(len(entries) / columns))
    header = 46
    label = 30
    sheet = Image.new("RGB", (columns * panel + 32, header + rows * (panel + label) + 16), "#101921")
    draw = ImageDraw.Draw(sheet)
    draw.text((16, 10), title, font=font(20), fill="white")
    for index, (name, image) in enumerate(entries):
        x = 16 + (index % columns) * panel
        y = header + (index // columns) * (panel + label)
        preview = image.resize((panel - 12, panel - 12), Image.Resampling.LANCZOS).convert("RGB")
        sheet.paste(preview, (x, y))
        draw.text((x, y + panel - 10), name, font=font(17), fill="white")
    return sheet


def histogram_metrics(pixels, metadata):
    edge_x = np.mean(np.abs(pixels[:, 0].astype(float) - pixels[:, -1].astype(float))) / 255.0
    edge_y = np.mean(np.abs(pixels[0].astype(float) - pixels[-1].astype(float))) / 255.0
    return {
        **metadata,
        "edge_pixel_delta_x": float(edge_x),
        "edge_pixel_delta_y": float(edge_y),
        "coverage_gt_127": float(np.mean(pixels >= 128)),
        "mean": float(np.mean(pixels) / 255.0),
        "midtones_16_239": float(np.mean((pixels >= 16) & (pixels <= 239))),
        "sha256": hashlib.sha256(pixels.tobytes()).hexdigest(),
    }


def generate_experiments():
    EXPERIMENTS.mkdir(parents=True, exist_ok=True)
    settings = replace(Settings.preset("Primary", 1107), resolution=512)
    entries = []
    for variant, label in [
        ("phase_only", "A - single warped phase"),
        ("chosen", "B - localized branch union"),
        ("multiscale", "C - coherent multiscale"),
    ]:
        pixels, _ = generate(settings, variant)
        image = Image.fromarray(pixels, "L")
        image.save(EXPERIMENTS / f"{variant}.png")
        entries.append((label, image))
    contact_sheet(entries, "SPEC-011.7 field representation experiments", columns=3, panel=390).save(
        EXPERIMENTS / "ApproachComparison.png")


def generate_review_set():
    FIELD_SOURCES.mkdir(parents=True, exist_ok=True)
    metrics = {}
    for role in ["Primary", "Secondary"]:
        entries = []
        for seed in SEEDS:
            settings = Settings.preset(role, seed)
            pixels, metadata = generate(settings, "chosen")
            image = Image.fromarray(pixels, "L")
            image.save(FIELD_SOURCES / f"{role}_Seed{seed}_Raw.png")
            tiled(image, 2).save(FIELD_SOURCES / f"{role}_Seed{seed}_Tiled2x2.png")
            tiled(image, 4).save(FIELD_SOURCES / f"{role}_Seed{seed}_Tiled4x4.png")
            entries.append((f"Seed {seed} - 2x2", tiled(image, 2)))
            metrics[f"{role}_{seed}"] = histogram_metrics(pixels, metadata)
        contact_sheet(entries, f"New field {role}: four seeds tiled 2x2").save(
            ROOT / f"Field_{role}_Seeds_Contact.png")

    old = Image.open(OLD_SOURCES / "Primary_Seed1107_Raw.png").convert("L")
    new = Image.open(FIELD_SOURCES / "Primary_Seed1107_Raw.png").convert("L")
    comparison = [
        ("Previous stamp generator - raw", old),
        ("New periodic field - raw", new),
        ("Previous stamp generator - 2x2", tiled(old, 2)),
        ("New periodic field - 2x2", tiled(new, 2)),
    ]
    contact_sheet(comparison, "SPEC-011.7 previous stamps vs periodic ridge field").save(
        ROOT / "Old_vs_Field_Comparison.png")
    (ROOT / "FieldSourceMetrics.json").write_text(json.dumps(metrics, indent=2), encoding="utf-8")


def main():
    generate_experiments()
    generate_review_set()


if __name__ == "__main__":
    main()
