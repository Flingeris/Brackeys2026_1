public interface ICombatEntity
{
    int MaxHP { get; }
    // int Speed { get; }
    int CurrShield { get; }
    int CurrHP { get; }
    bool IsDead { get; }

    public void SetTarget(bool b);

    public bool IsPossibleTarget { get; }

    void TakeDamage(int amount);
    void Heal(int amount);
    void AddShield(int amount);

    void Kill();
}