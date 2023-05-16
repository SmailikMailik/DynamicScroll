[![CodeFactor](https://www.codefactor.io/repository/github/bugrahanakbulut/unity-infinite-scroll/badge/main)](https://www.codefactor.io/repository/github/bugrahanakbulut/unity-infinite-scroll/overview/main)

# Unity-Infinite-Scroll
Unity UI Optimized Scroll Rect to represent large number of entities with less rect transform. Currently only vertical scroll supported. 

DynamicScrollRect does not rely any layout group element to keep entities organized. You can use vertical, horizontal or grid orientation without using any built-in layout element.

![Alt Text](https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/scroll_infinite.gif)
![Alt Text](https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/scroll_jumpback.gif)
![Alt Text](https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/horizontal_scroll.gif)


## For Custom Usage

By inheriting from ScrollItemData and ScrollItem<T> you can create your custom scroll entities. You should be aware of CustomScrollItem inherited from monobehaviour so its' file name and class name must be identical to attach component to game object.

```cs

public class CustomScrollItemData : ScrollItemData
{
    // Some arbitrary fields and properties

    public CustomScrollItemData(int index) : base(index)
    {
        
    }
}

public class CustomScrollItem : ScrollItem<CustomScrollItemData> 
{
    protected override void InitItemData(CustomScrollItemData data)
    {
        base.InitItemData(data);
    }

    protected override void ActivatedCustomActions()
    {
        base.ActivatedCustomActions();
    }

    protected override void DeactivatedCustomActions()
    {
        base.DeactivatedCustomActions();
    }
}
```
    
## Focusing Items 
    
Scroll rect can make focus easily when entities outside of the viewport. You can add offset to focus or determine focus duration from focus settings on DynamicScrollRect component.
    
<p align="center">
    <img src="https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/scroll_focus.gif" alt="animated" />
</p>