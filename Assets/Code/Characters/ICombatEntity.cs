public interface ICombatEntity
{
    int MaxHP { get; }
    int CurrHP { get; }
    bool IsDead { get; }
    int CurrShield { get; }
    
    void TakeDamage(int amount);
    void Heal(int amount);
    void AddShield(int amount);

    void Kill();
}