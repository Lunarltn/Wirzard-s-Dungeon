using UnityEngine;

public interface IDamageable
{
    public Damage TakeDamage(Damage damage);
}

public struct Damage
{
    int value;
    public Transform Caster { get; set; }
    public bool IsIgnored { get; set; }
    public int Value
    {
        get { return value; }
        set { this.value = Mathf.Max(0, value); }
    }
    public bool IsCritical { get; set; }
    public bool IsDie { get; set; }
}