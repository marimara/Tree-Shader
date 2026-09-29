# SPEC-011.6 — Stable Stylized Flow Pattern and Visual Fidelity
## Revision B — Silhouette, Composition and Stretch Refinement

## Objective

Improve the visual quality, silhouette design and temporal stability of the stylized surface flow pattern before continuing to automatic river-to-lake behavior.

The water-flow architecture is already functionally capable of:

- following a centerline;
- following curved channels;
- using generated channel coordinates;
- reacting to obstacles;
- supporting Transform-aware water meshes.

A first SPEC-011.6 visual pass also substantially improved:

- temporal continuity;
- previous blinking / popping;
- visible downstream translation;
- severe faceting;
- overly thin streaks.

However, artistic review shows that the base flow pattern is still not visually approved.

The remaining issue is now primarily:

shape language and composition.

Current visible problems include:

- excessive longitudinal stretching, especially in straight entry and exit sections;
- broad marks sometimes becoming smeared or pulled too far along the flow direction;
- primary shapes with feather-like / fibrous / stringy silhouettes;
- too many thin tips extending from larger masses;
- insufficient solid painterly body in the primary marks;
- repeated visual language between large clusters;
- weak distribution between large, medium and small marks;
- periods of excessive empty space followed by dense bright clusters;
- overlap between samples creating overly bright or visually explosive masses;
- obstacle flow sometimes revealing the contour mathematics too clearly, producing halo-like / wrapping shapes;
- curved sections occasionally compressing marks on the inside and smearing them on the outside;
- remaining distance from the visual language of the reference.

This revision focuses on refining those issues while preserving the temporal improvements already achieved.

Do not implement foam, reflections, refraction, waves or future water systems yet.

---

# Dependencies

Requires completed and validated:

- SPEC-006 — Stylized Flow Pattern Shaping
- SPEC-007 — Flow Strength
- SPEC-008 — Flow Map Runtime Support
- SPEC-009 — Curved Flow / Channel Coordinates
- SPEC-010 — Flow Map Baker
- SPEC-011 — Boundary / Obstacle Flow
- SPEC-011.5 — Automatic Mesh Setup
- SPEC-011.6 first visual pass

Preserve the validated flow architecture.

Do not redesign:

- centerline generation;
- Flow Map encoding;
- obstacle contour steering;
- generated channel coordinates;
- Transform-aware metrics;

unless diagnosis proves a real integration defect.

---

# First-pass Baseline

The first SPEC-011.6 implementation established a useful new baseline.

Preserve, unless a demonstrated problem requires otherwise:

- continuous downstream motion;
- greatly reduced blinking / popping;
- lack of obvious dual-phase crossfade in the production channel-coordinate path;
- improved loop continuity;
- better temporal tracking of individual marks;
- corrected redundant transverse deformation;
- improved broad-mark presence;
- existing curved-flow and obstacle-flow integration.

Do not regress temporal stability while refining appearance.

A visually better still frame is not acceptable if animation again reads as:

"texture frames changing"

instead of:

"water marks travelling downstream."

---

# Reference Goal

Use the existing reference video as the primary visual target.

Focus only on the base surface-flow language.

Prioritize:

- broad painterly directional strokes;
- strong but irregular silhouettes;
- clear body/mass in primary marks;
- varied width;
- varied length;
- organic broken edges;
- meaningful negative space;
- stable downstream translation;
- strong hierarchy between primary, secondary and accent shapes;
- composition that remains visually active without becoming noisy;
- readable stylization at gameplay distance.

Do not attempt to reproduce yet:

- foam banks;
- waterfall foam;
- impact foam;
- reflection;
- refraction;
- custom normals;
- splash VFX;
- mist;
- full turbulence.

The base pattern itself must become visually convincing before those later layers are added.

---

# Core Visual Problem — Excessive Longitudinal Stretch

A major remaining defect is excessive elongation along the channel direction.

This is especially visible in the straight sections at the beginning and end of the current validation scene.

These areas should be the cleanest representation of the authored pattern.

Instead, some marks become:

- excessively long;
- thin relative to their length;
- smeared;
- dragged;
- lacking painterly body.

Investigate this independently from curve deformation.

Do not assume the problem comes from the Flow Map.

Possible contributors may include:

- longitudinal pattern scaling;
- Flow Strength stretch mapping;
- Flow Coordinate scaling;
- scale relationships between the current pattern samples;
- shaping after coordinate transformation;
- overlap between multiple samples;
- another interaction.

These are diagnostic possibilities, not prescribed fixes.

---

# Straight-flow Silhouette Requirement

On a straight channel with constant Flow Strength:

Primary marks should read as:

broad stylized brush strokes moving downstream.

They should NOT read as:

- infinitely stretched streaks;
- hair-like lines;
- smeared texture;
- dragged smoke;
- long feather structures.

Straight flow is the primary silhouette benchmark.

If the pattern is not visually correct in a straight channel, do not attempt to compensate for it using curved-flow logic.

---

# Primary Mark Identity

Primary marks should carry most of the visual identity.

Target qualities:

- broad body;
- clear silhouette;
- meaningful width;
- irregular but controlled edges;
- varied length;
- limited thin protrusions;
- recognizable painterly mass;
- readable negative space around them.

Avoid primary marks composed mainly of:

- many thin forks;
- dense fibrous tails;
- numerous small spikes;
- feather-like structures;
- repeated narrow tendrils.

Broken edges are desirable.

Excessively shredded silhouettes are not.

---

# Secondary Detail

Secondary details should support the primary pattern.

They may be:

- thinner;
- shorter;
- less opaque / less visually dominant;
- more numerous.

However, secondary detail must not become the dominant visual language.

The viewer should first perceive:

Primary painterly flow masses

and only then:

smaller supporting streaks.

---

# Shape Hierarchy

The final pattern should contain deliberate visual hierarchy.

Conceptually:

Primary Marks
- broad;
- dominant;
- painterly;
- visually stable;
- strong silhouette.

Secondary Marks
- medium scale;
- supporting directional rhythm;
- lower visual weight.

Accent Detail
- small;
- sparse;
- subtle;
- used only where beneficial.

The implementation does not need three literal shader layers.

The hierarchy is a visual requirement, not an implementation requirement.

---

# Existing Multi-sample Solution

The current implementation uses multiple samples of the existing Noise to improve hierarchy and temporal continuity.

Do not assume this architecture must be removed.

However, investigate whether the current sample combination contributes to:

- repetitive silhouettes;
- excessive longitudinal stretching;
- overly bright cluster overlap;
- dense clumps followed by large empty areas;
- insufficient distinction between primary and secondary roles.

If multiple samples remain:

they should have deliberate visual responsibilities.

Avoid simply layering several equally important versions of the same pattern.

---

# Sample Combination

If multiple samples contribute to the final result, their combination should preserve readable visual hierarchy.

Potential concerns to investigate include:

- additive-looking bright overlap;
- simultaneous alignment of large structures;
- similar thresholds producing identical silhouette language;
- scale ratios that reinforce excessive stretch;
- all samples contributing equally to the final mask.

Possible solutions may involve differentiated:

- scale;
- coverage;
- intensity;
- shaping;
- role;

but no specific implementation is required.

Choose the most visually effective approach.

---

# Density and Distribution

The current pattern may occasionally produce:

large empty interval
→ very dense bright cluster
→ large empty interval.

Negative space is required, but distribution should feel intentional.

Target a more balanced rhythm:

large shape
+
medium marks
+
small accents
+
negative space

rather than isolated "islands" of flow pattern.

Do not solve this by simply increasing global density.

Preserve negative space.

Improve composition.

---

# Repetition

Investigate recognizable repetition of similar clusters.

The current Noise may produce recurring shapes with similar:

- body;
- tail;
- branching;
- tip structure.

Use the existing Noise first.

Reduce obvious repetition through the shaping/composition pipeline where reasonably possible.

Do not introduce random temporal flickering as a method of hiding repetition.

---

# Temporal Stability

The temporal improvements of the first pass remain mandatory.

Individual visible marks should generally travel downstream for a perceptible distance.

Preserve:

- continuous translation;
- visually unobtrusive loop transitions;
- stable density;
- stable brightness;
- no obvious global reset.

Do not reintroduce crossfading between unrelated fully-shaped patterns unless visual testing demonstrates that it no longer causes popping.

---

# Temporal Continuity vs Shape Quality

Do not optimize silhouette only in static screenshots.

A shape that looks excellent in one frame but:

- dissolves;
- morphs;
- blinks;
- snaps;
- changes topology rapidly;

is not acceptable.

Likewise, perfectly stable motion with poor silhouettes is not sufficient.

SPEC-011.6 requires both.

---

# Authored Noise Preservation

Continue using the existing Noise texture for this revision.

The current Noise has demonstrated that it can produce a useful moving pattern.

Avoid excessive processing that turns its broad information into:

- narrow filaments;
- procedural-looking fibers;
- repeated sharp tendrils.

Reassess:

- distortion;
- threshold;
- contrast;
- masking;
- scale;
- combination between samples.

Preserve the authored texture character where it is visually useful.

---

# Distortion

Distortion is optional.

Keep it only where it improves organic painterly character.

Avoid distortion that:

- creates feather-like shredding;
- introduces excessive thin tips;
- bends marks independently of the flow path;
- causes blocky/faceted silhouettes;
- creates visible swimming;
- makes large marks look stretched rather than flowing.

Less distortion may be preferable if the source texture already contains useful shape.

---

# Threshold and Masking

Thresholding and masking must not destroy the primary mark body.

Review whether current shaping creates:

broad input shape
→ thin output skeleton.

Avoid overly aggressive erosion of the authored pattern.

Small coordinate differences should not cause major topology changes.

Preserve enough softness/body for broad painterly masses.

---

# Curved Flow

The S-shaped validation must preserve approximately the same visual language as the straight test.

Marks should not become a different kind of pattern merely because the channel curves.

Specifically inspect:

- inside of bends;
- outside of bends.

Avoid:

Inside
→ excessive compression / thinning

Outside
→ excessive stretching / smearing.

The result should look like the same painterly marks bending with the channel.

---

# Obstacle Flow

Preserve the validated obstacle steering.

However, artistic review shows that the pattern can expose the underlying contour field too explicitly.

Around a cylindrical obstacle, avoid a result that resembles:

a visible circular halo or ring wrapping around the collider.

The intended read is:

water marks divert around the solid and recombine downstream.

Do not modify the obstacle solver first.

Investigate whether:

- pattern width;
- shaping;
- mark scale;
- threshold sensitivity;
- channel-coordinate consumption;

is exaggerating the contour curvature.

Change obstacle steering only if visual/debug evidence demonstrates that the vector field itself is responsible.

---

# Faceted / Pixel-like Artifacts

Continue monitoring faceted or pixel-like deformation.

The first pass reduced these artifacts substantially.

Do not regress.

If they remain visible, diagnose whether they originate from:

- UV2 interpolation;
- UV3 frame;
- Flow Map sampling;
- generated mesh density;
- obstacle refinement;
- nonlinear shaping;
- another stage.

Do not automatically increase mesh or Flow Map resolution.

Fix the stage actually responsible.

---

# Flow Strength

Preserve the concept of Flow Strength.

Higher strength should support:

- faster apparent movement;
- stronger directional read;
- more energetic pattern.

Lower strength should support:

- calmer motion;
- lower pattern dominance;
- broader / gentler visual character where appropriate.

However, Flow Strength should not linearly exaggerate longitudinal stretch until marks become smeared.

Review the current relationship between:

Flow Strength
and
mark aspect ratio.

Fast water may have longer directional shapes.

It should still retain readable width and body.

---

# Calm / River / Fast

Validate manually using representative states.

Calm:

- sparse;
- broad;
- subtle;
- slow;
- not merely frozen Fast water.

River:

- clear painterly directional marks;
- balanced hierarchy;
- stable translation.

Fast:

- more energetic;
- stronger directional read;
- increased movement;
- may be somewhat more elongated;
- must not collapse into thin stretched lines.

Transitions should remain smooth.

---

# Diagnostic Comparison

For this revision create clear comparison material for:

1. pre-SPEC-011.6 implementation;
2. first SPEC-011.6 pass;
3. revised SPEC-011.6 result;
4. reference footage.

Include both:

- still-frame silhouette comparison;
- motion comparison.

Evaluate:

- mark aspect ratio;
- primary mark body;
- thin-tip frequency;
- cluster repetition;
- distribution;
- negative space;
- temporal continuity;
- curved-flow deformation;
- obstacle behavior.

---

# Straight-channel Validation

This is the first mandatory checkpoint.

Use:

- straight channel;
- constant Flow Strength;
- no obstacles;
- stable camera.

Inspect separately:

- entry;
- middle;
- exit.

Requirements:

- no excessive longitudinal stretching;
- primary marks retain visible body;
- movement remains continuous;
- no blinking;
- no loop pop;
- balanced density;
- no major faceting;
- marks remain painterly.

Do not continue until this passes.

---

# Curved-channel Validation

After straight flow passes:

validate the existing S-shaped channel.

Requirements:

- same painterly identity;
- similar mark thickness;
- broad shapes remain broad;
- no severe inner-curve compression;
- no severe outer-curve smearing;
- no arbitrary extra curves;
- temporal continuity preserved.

---

# Obstacle Validation

Then validate the current cylinder obstacle.

Requirements:

- marks divert around the object;
- pattern does not form an exaggerated circular halo;
- no blocky fragmentation;
- no flashing near collider edges;
- primary mark body remains readable;
- downstream recombination looks natural.

---

# Transform Validation

Preserve SPEC-011.5 Transform-aware behavior.

Compare:

- Scale 1;
- strong non-uniform X/Z scale.

Pattern:

- width;
- aspect ratio;
- density;
- apparent movement;

should remain broadly comparable in world space.

---

# Long-duration Validation

Run multiple complete animation cycles.

Inspect:

- temporal continuity;
- loop boundary;
- density stability;
- brightness stability;
- cluster repetition;
- disappearing/reappearing structures;
- accumulated drift.

The first-pass temporal improvements must remain intact.

---

# Performance

Maintain practical stylized-water shader cost.

The first pass currently uses multiple samples of the same Noise.

Those samples may remain if justified visually.

If the revised solution changes sample count:

report:

- number of texture samples;
- approximate difference from previous implementation;
- reason for the cost.

Do not optimize away visible quality prematurely.

Avoid excessive per-fragment integration or simulation.

---

# Artist Controls

Keep the material understandable.

Prefer controls that map directly to visible artistic concepts.

Potential useful concepts include:

- Primary Mark Scale;
- Primary Width / Body;
- Primary Coverage;
- Secondary Strength;
- Secondary Scale;
- Detail Strength;
- Stretch / Aspect;
- Pattern Density.

These are suggestions.

Do not add all of them automatically.

Only expose controls that materially improve art direction.

Avoid implementation-specific tuning clutter.

---

# Texture Resource Decision

Do not create a new Noise texture during this revision.

First determine how far the existing texture can go with improved:

- shaping;
- composition;
- scale hierarchy;
- aspect control.

If the revised pipeline is temporally stable and well-composed but the authored source clearly prevents further improvement:

stop and report.

Document specifically what a future generated/authored texture should improve, such as:

- broader base shapes;
- different stroke-length distribution;
- fewer fibrous tips;
- stronger painterly breakup;
- reduced repetition.

A future procedural Water Pattern Generator may then address those requirements in a separate Spec.

---

# External Assets

No new external visual asset is required for this revision.

Use:

- existing Noise texture;
- existing validation scenes;
- existing reference video.

---

# Acceptance Criteria

SPEC-011.6 Revision B is complete when:

- first-pass temporal continuity is preserved;
- obvious blinking / popping remains eliminated;
- loop transitions remain unobtrusive;
- visible marks can be tracked moving downstream;
- straight entry and exit sections no longer show excessive longitudinal smearing;
- primary marks contain clear painterly body;
- large marks are not dominated by fibrous thin tips;
- width-to-length ratio is visually controlled;
- broad / medium / small hierarchy is clearly readable;
- sample overlap does not produce distracting over-bright clusters;
- pattern distribution feels balanced while retaining negative space;
- repetition is reduced to an acceptable level;
- straight flow is visually approved first;
- curved flow preserves the same mark identity;
- curve interiors do not severely compress marks;
- curve exteriors do not severely smear marks;
- obstacle flow does not produce distracting halo-like shapes;
- faceted artifacts remain below gameplay visibility;
- Transform-aware behavior remains coherent;
- Calm / River / Fast remain distinct;
- Fast flow remains energetic without becoming thin smeared streaks;
- baker / Flow Map / obstacle systems are not regressed;
- the base pattern is materially closer to the reference.

---

# Visual Approval Requirement

Do not consider this revision complete because:

- shader compiles;
- movement is continuous;
- previous blinking is gone;
- tests technically pass.

Completion requires visual approval.

The desired read is:

"broad painterly water marks travelling naturally with the current"

not:

"noise stretched along a flow field."

---

# Out of Scope

Do not implement:

- procedural Water Pattern Generator;
- new authored Noise asset;
- SPEC-012;
- automatic river-to-lake strength;
- strength override zones;
- foam;
- shoreline foam;
- waterfall foam;
- waves;
- custom normals;
- reflection;
- refraction;
- splash VFX;
- mist;
- interaction;
- runtime fluid simulation.