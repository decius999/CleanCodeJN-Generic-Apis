using System.Linq.Expressions;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetByIdsRequest<TEntity> : IRequest<BaseListResponse<TEntity>>
      where TEntity : class
{
    public List<int> Ids { get; init; }

    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];
}
