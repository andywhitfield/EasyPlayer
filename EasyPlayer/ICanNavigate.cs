namespace EasyPlayer
{
    public interface ICanNavigate
    {
        bool CanNavigateTo(string searchValue);
        void NavigateTo(string searchValue);
    }
}
