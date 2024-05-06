using System.Linq.Expressions;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetRequest<TEntity> : BasePaginationRequest, IRequest<BaseListResponse<TEntity>>
{
    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];

    public Expression<Func<TEntity, bool>> Where { get; set; } = x => true;
}
