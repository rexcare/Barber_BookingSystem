using Base.Contracts.Domain;
using Microsoft.AspNetCore.Identity;

namespace Base.Domain.Identity;

public class BaseRole : BaseRole<Guid>, IDomainEntityId
{
    public BaseRole() : base()
    {
    }

    public BaseRole(string roleName) : base(roleName)
    {
    }    
}

public class BaseRole<TKey> : IdentityRole<TKey> , IDomainEntityId<TKey>
    where TKey : IEquatable<TKey>
{
    public BaseRole(): base()
    {
    }

    public BaseRole(string roleName): base(roleName)
    {
    }
    
}