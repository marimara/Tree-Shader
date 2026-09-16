# \# SPEC-010 — Flow Map Baker Tool

# 

# \## Objective

# 

# Create an Editor-side tool that automatically generates Flow Map data for a water surface.

# 

# The user should not need to manually paint Flow Maps.

# 

# The generated data must use the format established in SPEC-008.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-008 and SPEC-009.

# 

# \---

# 

# \# Workflow Goal

# 

# Target authoring workflow:

# 

# 1\. Select a water surface.

# 2\. Add/configure a Flow Map Baker component.

# 3\. Define basic flow inputs.

# 4\. Bake.

# 5\. Shader automatically uses the generated Flow Map.

# 

# The tool should prioritize simplicity and repeatability.

# 

# \---

# 

# \# Tool Architecture

# 

# Create an Editor-friendly water-flow baking component.

# 

# Suggested runtime component name:

# 

# StylizedWaterFlowBaker

# 

# or an equivalent clear name.

# 

# Editor-specific logic should remain in an Editor folder if required.

# 

# \---

# 

# \# Initial Inputs

# 

# For this Spec, support simple authored flow guidance.

# 

# At minimum provide:

# 

# \- water surface bounds;

# \- primary flow direction;

# \- source/entry position or region;

# \- target/exit position or region;

# \- bake resolution.

# 

# Exact UX may vary.

# 

# \---

# 

# \# Bake Output

# 

# Generate Flow Map data matching:

# 

# R = Direction X  

# G = Direction Y  

# B = Flow Strength  

# A = reserved

# 

# The tool may generate:

# 

# \- Texture2D asset;

# \- RenderTexture converted to asset;

# \- another appropriate persistent texture format.

# 

# Prefer a persistent generated asset suitable for runtime use.

# 

# \---

# 

# \# Generated Asset Location

# 

# Generated assets should belong under:

# 

# Assets/WaterShader/Generated/

# 

# or another clearly isolated WaterShader-generated location.

# 

# Do not place generated data in References or source texture folders.

# 

# Create the folder only if needed.

# 

# \---

# 

# \# Re-baking

# 

# The user must be able to update the bake after changing inputs.

# 

# Avoid manual asset cleanup requirements where practical.

# 

# Do not silently overwrite unrelated assets.

# 

# \---

# 

# \# Initial Flow Generation

# 

# This Spec does not need advanced obstacle avoidance.

# 

# The initial baker may produce a smooth field from entry toward exit.

# 

# The purpose is validating:

# 

# \- automatic map creation;

# \- correct data encoding;

# \- shader integration;

# \- repeatable rebake workflow.

# 

# \---

# 

# \# Debugging

# 

# Provide useful debug visualization for:

# 

# \- generated direction;

# \- generated strength;

# \- bake bounds.

# 

# Scene gizmos are allowed.

# 

# Keep them lightweight.

# 

# \---

# 

# \# Validation

# 

# Confirm:

# 

# \- baker generates valid Flow Map data;

# \- shader can use generated output;

# \- rebaking updates the result;

# \- different source/target configurations alter direction;

# \- output uses the SPEC-008 encoding convention;

# \- uniform fallback remains functional.

# 

# \---

# 

# \# External Assets

# 

# No manually painted Flow Map is required.

# 

# The tool itself must generate the data.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-010 is complete when:

# 

# \- a water surface can generate its own Flow Map through an Editor workflow;

# \- generated data is persistent;

# \- generated direction is readable in shader debug mode;

# \- rebaking works;

# \- no manual texture painting is required.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement yet:

# 

# \- collider-aware avoidance;

# \- boundary distance fields;

# \- automatic width-based speed;

# \- automatic lake detection;

# \- runtime fluid simulation;

# \- foam;

# \- VFX.

