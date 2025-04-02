using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public abstract int hp { get; }
    public abstract int downTime { get; }

    public abstract void ListenSound();
}
