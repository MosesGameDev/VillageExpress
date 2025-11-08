using EPOOutline;
using System;
using UnityEngine;


/// <summary>
/// [required!] for game objects that the player will be interacting with, gameobject subscribes to Oninteraction start and end
/// </summary>
public class Interactible : MonoBehaviour, IInteractible
{
    public string id = "Interaction Object";
    public string instruction = "Press 'E'";

    [Space]
    public bool displayUIInstruction = true;
    public Color instructionsTextColor;
    public Sprite instructionSprite;
    public KeyCode interactionInput;

    [Space]
    public Color outlineColor;
    [SerializeField] private Outlinable[] outlinables;

    bool isHighlighted;

    public Action<Interactible> OnStartInteraction;
    public Action<Interactible> OnEndInteraction;

    public Action onHighlight;
    public Action onDisableHighlight;


    private void Start()
    {
        ToggleOutline(false);
    }

    void IInteractible.Highlight()
    {
        if (isHighlighted)
        {
            return;
        }


        isHighlighted = true;
        ToggleOutline(isHighlighted);
        onHighlight?.Invoke();

    }

    void IInteractible.DisableHighlight()
    {
        if (!isHighlighted)
        {
            return;
        }

        isHighlighted = false;
        ToggleOutline(isHighlighted);
        onDisableHighlight?.Invoke();

    }


    public void ToggleOutline(bool visible)
    {
        foreach (var outlinable in outlinables)
        {
            outlinable.enabled = visible;
        }
    }



    void IInteractible.StartInteraction()
    {
        OnStartInteraction?.Invoke(this);
    }


    public void EndInteraction()
    {
        OnEndInteraction?.Invoke(this);
    }
}
