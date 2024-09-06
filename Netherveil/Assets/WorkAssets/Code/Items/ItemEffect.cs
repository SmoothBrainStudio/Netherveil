[System.Serializable]
public abstract class ItemEffect
{
    public string Name { get => GetType().ToString(); }
    public bool HasBeenRetreived { get; set; } = false;
    public float CurrentEnergy = 0f;
}