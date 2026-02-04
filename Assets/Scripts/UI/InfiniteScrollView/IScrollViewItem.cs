namespace EngSFXCheckList.UI.InfiniteScrollView
{
    public interface IScrollViewItem
    {
        void UpdateContent(int index);
        void SetActive(bool active);
    }
}
