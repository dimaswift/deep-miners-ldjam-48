using System.Threading.Tasks;

namespace DeepMiners.Systems.Input
{
    public interface IWindow
    {
        void SetInteractable(bool interactable);
        Task Show();
        Task Hide();
    }
}