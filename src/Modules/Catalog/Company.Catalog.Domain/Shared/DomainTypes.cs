using Company.Webshop.Shared.Exceptions;

namespace Company.Catalog.Domain.Shared;

internal interface IEntityId
{
    Guid Value { get; }
}

internal static class EntityId
{
    public static TId New<TId>() where TId : struct, IEntityId
    {
        object? identifier = Activator.CreateInstance(typeof(TId), Guid.NewGuid());
        return identifier is TId typedIdentifier
            ? typedIdentifier
            : throw new InvalidOperationException($"{typeof(TId).Name} must have a Guid constructor.");
    }
}

internal abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : struct, IEntityId
{
    protected Entity() { }
    protected Entity(TId id) => Id = id;
    public TId Id { get; protected set; }
    protected abstract void ValidateState();
    protected void EnsureValidState() => ValidateState();
    public bool Equals(Entity<TId>? other) =>
        other is not null && other.GetType() == GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

internal abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    public bool Equals(ValueObject? other) => other is not null && other.GetType() == GetType() &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    public override bool Equals(object? obj) => Equals(obj as ValueObject);
    public override int GetHashCode() => GetEqualityComponents().Aggregate(17, HashCode.Combine);
}

internal interface IRepository<TEntity, in TId> where TEntity : Entity<TId> where TId : struct, IEntityId
{
    Task<TEntity?> Get(TId id, CancellationToken cancellationToken);
    Task Add(TEntity entity, CancellationToken cancellationToken);
}

internal static class Assertions
{
    public static void NotEmpty(Guid value, string message) { if (value == Guid.Empty) throw new BusinessRuleException(message); }
    public static void NotBlank(string? value, string message) { if (string.IsNullOrWhiteSpace(value)) throw new BusinessRuleException(message); }
    public static void Positive(decimal value, string message) { if (value <= 0) throw new BusinessRuleException(message); }
    public static void Positive(int value, string message) { if (value <= 0) throw new BusinessRuleException(message); }
}
