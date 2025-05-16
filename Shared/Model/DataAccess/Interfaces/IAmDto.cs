namespace BirdWatching.Shared.Model;

public interface IAmDto<TEntity>
{
    public TEntity ToEntity();
}
