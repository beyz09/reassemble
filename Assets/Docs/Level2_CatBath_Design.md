# Level 2 design — "Cat Bath" sequence puzzle

Goal: A linear, ordered mini-puzzle where the player must perform 6 actions in order to successfully bathe the cat. Triggering an out-of-order action resets the sequence to the start. This enforces a careful, patient playstyle (taming the character's aggressive side).

Steps (zero-based indices)
- Step 0: Catch the cat ("Kediyi yakaladık")
- Step 1: Bring to the bathroom ("Banyoya götürdük")
- Step 2: Keep the cat from fleeing ("Kaçmamasını sağladık")
- Step 3: Apply shampoo/foam ("Köpükledik")
- Step 4: Rinse with water ("Su ile duruladık")
- Step 5: Dry and feed ("Kuruladık ve mama koyduk - level complete")

High level wiring
- Put a `SequenceManager` in the scene and fill `stepNames` with the strings above.
- For each step object (e.g. a clickable cat, door, towel, shampoo bottle, faucet, bowl) add `StepInteractable` and set `stepIndex` accordingly.
- Optionally add `DialogueUI` + `DialogueSystem` to provide feedback after each interact.
- Assign a UI Text to `SequenceManager.progressText` (or use `StepUI` helper) to show the current step in the HUD.
- Hook `SequenceManager.onSequenceComplete` to show a level-complete popup or advance the story.
- Hook `SequenceManager.onSequenceReset` to show a short failure message or play an animation/sfx.

Design notes
- When a step is correctly completed the `SequenceManager` increments currentStep and updates UI.
- When the player tries a wrong action, the sequence resets to step 0 (and you can optionally spawn a short dialog: "You lost your temper — start over").
- You can make specific steps more complex (e.g. Step 2: keep cat from fleeing could be a small quick-time or hold interaction), but start simple.

Implementation tips
- Use `StepInteractable` for simple click-to-advance objects. For multi-stage interactions (hold, minigame), call `SequenceManager.TryCompleteStep(index)` when that sub-minigame completes.
- Keep stepIndex values consistent and document them in the inspector comments.
