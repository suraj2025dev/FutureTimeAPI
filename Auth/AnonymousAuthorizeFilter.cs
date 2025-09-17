using Microsoft.AspNetCore.Authorization;


namespace Auth
{
    public class AnonymousAuthorizeFilter : Attribute, IAllowAnonymous
    {
    }
}
