using DynamicScrollView;

namespace DynamicScrollViewDemo
{
    internal sealed class DynamicScrollItemDataDemo : IDynamicScrollItemData
    {
        internal int Index { get; }

        internal DynamicScrollItemDataDemo(int index)
        {
            Index = index;
        }
    }
}