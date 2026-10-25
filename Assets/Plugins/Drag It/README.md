# Drag It

I made this to move the d-pad in my game, but you can use it for other UI too, like dialog boxes and windows.

## Setup

Add `DraggableUIElement` to the UI object you want to move. It needs a `RectTransform` parent so it can keep the object inside the parent bounds. Make sure your Canvas has a `GraphicRaycaster` and your scene has an `EventSystem`.

The object needs a UI Graphic to receive pointer input. If it doesn't have one, the component adds a transparent Image. `TextMeshProUGUI` works too.

## Use

Turn on **Edit Mode** while the game is running, then drag the object. Turn it off when you're done. Its position is saved automatically after you finish dragging.

The component has **Enable Edit Mode** and **Disable Edit Mode** buttons in the Inspector during Play Mode. You can also hook these methods up to your own buttons:

```csharp
draggableUIElement.EnableEditMode();
draggableUIElement.DisableEditMode();
```

## API

- `EditMode` — gets or sets whether dragging is enabled.
- `EnableEditMode()` / `DisableEditMode()` — toggle dragging and invoke the matching event.
- `OnEditModeEnter` / `OnEditModeExit` — UnityEvents for when Edit Mode is turned on or off.

Position is saved in PlayerPrefs using the `saveKey` value with `_x` and `_y` added. Give each draggable object a different key if you want them to save separately. The default key is `controls_group_pos`.

The demo scene is in the `Demo` folder.
