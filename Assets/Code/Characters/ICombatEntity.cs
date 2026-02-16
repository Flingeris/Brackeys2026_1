public interface ICombatEntity
{
    int MaxHP { get; }
    int CurrHP { get; }
    bool IsDead { get; }

    void TakeDamage(int amount);
    void Heal(int amount);
    void AddShield(int amount);
}