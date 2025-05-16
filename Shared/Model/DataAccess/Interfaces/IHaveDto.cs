namespace BirdWatching.Shared.Model;

public interface IHaveDto<TDto>
{
    public TDto ToDto();
    public TDto ToFullDto();
}
