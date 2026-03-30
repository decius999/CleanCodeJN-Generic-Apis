using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Automatically registers a GraphQL mutation field for creating an entity of type <typeparamref name="TEntity"/>.
/// Validation runs through the MediatR pipeline, so FluentValidation validators are applied automatically.
/// </summary>
/// <typeparam name="TDto">The DTO type returned by the mutation.</typeparam>
/// <typeparam name="TEntity">The entity type to create.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TInput">The input type accepted by the mutation.</typeparam>
public class AutoCreateMutationTypeExtensions<TDto, TEntity, TKey, TInput>(GraphQLOptions options, CleanCodeNamingConventions namingConventions = null) : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        var conventions = namingConventions ?? new CleanCodeNamingConventions();
        var field = descriptor
            .Field(conventions.GraphQLCreatePrefix + typeof(TEntity).Name)
            .Argument("input", a => a.Type<NonNullType<InputObjectType<TInput>>>())
            .Type<ObjectType<TDto>>();

        if (options?.AddAuthorizationWithPolicyName is not null)
        {
            field.Authorize(options.AddAuthorizationWithPolicyName);
        }

        field.Resolve(async ctx =>
        {
            var mediator = ctx.Service<IMediator>();
            var mapper = ctx.Service<ICleanCodeMapper>();
            var input = ctx.ArgumentValue<TInput>("input");

            var response = await mediator.Send(new PostRequest<TEntity, TInput> { Dto = input });

            return !response.Succeeded ? throw new GraphQLException(response.Message) : (object)mapper.Map<TDto>(response.Data);
        });
    }
}
