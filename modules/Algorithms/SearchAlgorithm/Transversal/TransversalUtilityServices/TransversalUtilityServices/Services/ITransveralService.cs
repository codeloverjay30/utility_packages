namespace TransversalUtilityServices
{
    public interface ITransversalService
    {
        void Transverse(object root, Action<object> onVisited);
    }
}