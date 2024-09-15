using UnityEngine;

public abstract class ColoredMoveableObject : MoveableObject
{
    public Color Color { get => GetColor(); set => SetColor(value); }

    protected abstract Color GetColor();
        
    protected abstract void SetColor(Color color);
}