namespace NeuroViva.Domain.Exceptions;

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.") { }
}
