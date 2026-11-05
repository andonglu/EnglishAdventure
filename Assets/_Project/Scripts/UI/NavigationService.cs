using System.Collections.Generic;

namespace EA
{
    /// <summary>可进导航栈的 UI 页面。</summary>
    public interface IPage
    {
        bool IsOpen { get; }
        void ClosePage();   // 页面自己负责关闭动画/音效，导航栈只负责登记
    }

    /// <summary>
    /// 页面导航栈：管理 AreaPage/QuitConfirm 等页面的打开顺序，
    /// 返回键（InputRouter.OnBack）统一由 NavigationService.PopTop 处理。
    /// 页面 Show 时 Push，Hide 时 Remove；不代替页面自己的 Show/Hide 逻辑。
    /// </summary>
    public static class NavigationService
    {
        static readonly List<IPage> _stack = new List<IPage>();

        public static int Depth => _stack.Count;

        public static void Push(IPage page)
        {
            if (page == null || _stack.Contains(page)) return;
            _stack.Add(page);
        }

        public static void Remove(IPage page) => _stack.Remove(page);

        /// <summary>关闭栈顶页面（若处于打开状态）。返回是否有页面被关闭。</summary>
        public static bool PopTop()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i].IsOpen)
                {
                    var p = _stack[i];
                    _stack.RemoveAt(i);
                    p.ClosePage();
                    return true;
                }
                _stack.RemoveAt(i);   // 已不在打开状态的脏引用顺手清理
            }
            return false;
        }
    }
}
