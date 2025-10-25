# Design: Single-Level 2D Escape Room (Hidden Object + Ghosts)

Goal: Create one polished escape-room level in 2D where the player finds 5 hidden objects. Some objects are blocked by ghosts. The player must interact with ghosts and select the correct dialogue choice to dispel them. Once all 5 objects are collected into inventory, the level completes.

Quick setup steps:
- Use Unity 2D template.
- Create folders: `Assets/Art`, `Assets/Scenes`, `Assets/Prefabs`, `Assets/UI`, `Assets/Scripts`, `Assets/Data`, `Assets/Docs`.
- Add the scripts in `Assets/Scripts` (Interactable, ItemSO, ItemPickup, Inventory, PlayerInteraction, Ghost, DialogueSystem, LevelManager).
- Create an empty GameObject `LevelManager` in the scene and attach `LevelManager` script; set `totalItems = 5` and wire `onLevelComplete` to a simple popup.
- Create an empty GameObject `DialogueSystem` and attach `DialogueSystem` script.
- Create an empty GameObject `Inventory` and attach `Inventory` script.
- Create a Player object with `PlayerInteraction` and a simple sprite; position at scene.
- Make 5 item prefabs: sprite + collider + `ItemPickup` component. Set `ItemSO` assets for each item (create via right-click Create->Game/Item).
- For items to be blocked, set `blockedByGhostId` to the ghost's id.
- Create Ghost GameObjects with `Ghost` script assigned, set `ghostId`, dialogues and correct choice index.

Next dev tasks:
- Implement UI for inventory and dialogue (replace the auto-choice in `DialogueSystem` with real UI interactions).
- Add visual feedback (hover highlights) and audio.
- Polish ghost animations.
