using ElShrine.Common.DataStructure;

namespace ElShrine.Common
{
    public static class TreeHelper
    {
        #region State
        /// <summary>
        /// 获取当前节点的根节点（即没有父节点的祖先节点）。
        /// 如果当前节点已经是根节点（Parent 为 null），则返回它自身。
        /// </summary>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前节点。</param>
        /// <returns>树的根节点。</returns>
        public static TNode GetRoot<TNode>(this TNode node) 
            where TNode : class, ITreeNode<TNode>
        {
            var parent = node.Parent;
            while (parent?.Parent is not null) parent = parent.Parent;
            return parent ?? node;
        }
        /// <summary>
        /// 检查当前节点是否是给定子节点的父节点（或祖先节点）。
        /// </summary>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前（潜在的父/祖先）节点。</param>
        /// <param name="child">要检查的子节点。</param>
        /// <param name="directlyOnly">
        /// 如果为 true，则仅检查直接父子关系。
        /// 如果为 false，则检查是否是子节点的任意祖先。
        /// </param>
        /// <returns>如果当前节点是子节点的父节点或祖先节点，则返回 true；否则返回 false。</returns>
        public static bool IsParentOf<TNode>(this TNode node, TNode child, bool directlyOnly = false) 
            where TNode : class, ITreeNode<TNode>
        {
            if (directlyOnly) return child.Parent == node;
            else
            {
                while (child.Parent != null)
                {
                    if (child.Parent == node) return true;
                    child = child.Parent;
                }
                return false;
            }
        }
        /// <summary>
        /// 检查当前节点是否是给定父节点的子节点（或后代节点）。
        /// </summary>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前（潜在的子/后代）节点。</param>
        /// <param name="parent">要检查的父节点。</param>
        /// <param name="directlyOnly">
        /// 如果为 true，则仅检查直接父子关系。
        /// 如果为 false，则检查是否是父节点的任意后代。
        /// </param>
        /// <returns>如果当前节点是父节点的子节点或后代节点，则返回 true；否则返回 false。</returns>
        public static bool IsChildOf<TNode>(this TNode node, TNode parent, bool directlyOnly = false) 
            where TNode : class, ITreeNode<TNode> 
            => parent.IsParentOf(node, directlyOnly);
        #endregion

        #region Tree Modify
        /// <summary>
        /// 从当前节点（父/祖先）中移除指定的子节点（后代）。
        /// </summary>
        /// <remarks>
        /// 移除操作通过将子节点的 Parent 属性设置为 null 来实现。
        /// 实际从子节点集合中移除的逻辑必须由 TNode 的具体实现来保证（在 Parent 的设置器中）。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前父节点或祖先节点。</param>
        /// <param name="child">要移除的子节点或后代节点。</param>
        /// <param name="directlyOnly">
        /// 如果为 true，则只检查是否是直接子节点，如果不是则不会执行移除。
        /// 如果为 false（默认），则检查是否是任意后代节点，只要是后代节点，即可通过设置 Parent = null 将其从旧父级移除。
        /// 注意：如果移除的是非直接子节点，该方法只是断开了该子节点与其当前父节点的关系链。
        /// </param>
        /// <returns>如果子节点是当前节点的后代且成功断开链接，则返回 true；否则返回 false。</returns>
        public static bool Remove<TNode>(this TNode node, TNode child, bool directlyOnly = true)
            where TNode : class, ITreeNode<TNode>
        {
            if (child.IsChildOf(node, directlyOnly))
            {
                child.Parent = null;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 从当前节点（父/祖先）中移除指定的多个子节点（后代）。
        /// </summary>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前父节点或祖先节点。</param>
        /// <param name="children">要移除的子节点或后代节点集合。</param>
        /// <param name="directlyOnly">该参数被用于传递给单个 Remove 方法，控制是只移除直接子节点还是任意后代节点。</param>
        public static void Remove<TNode>(this TNode node, IEnumerable<TNode> children, bool directlyOnly = true)
            where TNode : class, ITreeNode<TNode>
        {
            foreach (var child in children) node.Remove(child, directlyOnly);
        }

        /// <summary>
        /// 移除当前节点下的所有直接子节点。
        /// </summary>
        /// <remarks>
        /// 遍历 GetChildren() 获得的子节点列表，并对每个子节点调用 Remove 方法。
        /// 移除操作的实际执行依赖于 Remove 扩展方法和 TNode 内部 Parent 设置器的实现约定。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前父节点。</param>
        public static void RemoveAllChildren<TNode>(this TNode node)
            where TNode : class, ITreeNode<TNode>
        {
            foreach (var child in node.Children) node.Remove(child);
        }
        /// <summary>
        /// 将指定的子节点添加到当前节点下，作为其新的直接子节点。
        /// </summary>
        /// <remarks>
        /// 此操作是原子的：如果子节点已经有父节点，会首先将其从旧父节点下移除。
        /// 随后通过设置子节点的 Parent 属性来建立新的父子关系。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">新的父节点。</param>
        /// <param name="child">要添加的子节点。</param>
        public static void Append<TNode>(this TNode node, TNode child)
            where TNode : class, ITreeNode<TNode>
        {
            child.Parent?.Remove(child);
            child.Parent = node;
        }
        /// <summary>
        /// 将一个子节点集合添加到当前节点下，作为其新的直接子节点。
        /// </summary>
        /// <remarks>
        /// 依次对集合中的每个节点调用 Append(TNode node, TNode child) 方法。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">新的父节点。</param>
        /// <param name="children">要添加的子节点集合。</param>
        public static void Append<TNode>(this TNode node, IEnumerable<TNode> children)
            where TNode : class, ITreeNode<TNode>
        {
            foreach (var child in children) node.Append(child);
        }
        /// <summary>
        /// 使用 <c>newNode</c> 替换 <c>node</c> 在其父节点下的位置。
        /// </summary>
        /// <remarks>
        /// 1. 将当前节点 (<c>node</c>) 从其旧父节点中移除。
        /// 2. 将新节点 (<c>newNode</c>) 插入到旧父节点的位置。
        /// 3. 如果 <c>maintainOldSubTree</c> 为 <c>true</c>，则会将 <c>node</c> 的所有子节点（其子树）转移到 <c>newNode</c> 下。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">当前节点，即将被替换的节点。</param>
        /// <param name="newNode">替换当前节点的新节点。</param>
        /// <param name="maintainOldSubTree">
        /// 如果为 <c>true</c>，则 <c>newNode</c> 将首先清空其自身的所有子节点，然后继承 <c>node</c> 原有的所有子节点。
        /// 如果为 <c>false</c>（默认），则 <c>newNode</c> 的子节点保持不变，<c>node</c> 的子树保持断开状态（<c>node</c> 仍为父节点，但无父级）。
        /// </param>
        public static void ReplaceWith<TNode>(this TNode node, TNode newNode, bool maintainOldSubTree = false)
            where TNode : class, ITreeNode<TNode>
        {
            var oldParent = node.Parent;
            node.Parent = null;
            if (oldParent is null) newNode.Parent = null;
            else newNode.Append(oldParent);
            if (maintainOldSubTree)
            {
                newNode.RemoveAllChildren();
                newNode.Append(node.Children);
            }
        }
        #endregion

        #region Traversal
        /// <summary>
        /// 定义树形结构遍历时，访问者函数 (visitor) 返回的指令状态。
        /// 用于控制遍历的流程。
        /// </summary>
        public enum TraversalState
        {
            Continue,
            SkipSubtree,
            Stop
        }
        private static TraversalState TraverseDLRInner<TNode>(TNode node, Func<TNode, TraversalState> visitor)
            where TNode : class, ITreeNode<TNode>
        {
            var action = visitor(node);
            switch (action)
            {
                case TraversalState.Stop: return TraversalState.Stop;
                case TraversalState.SkipSubtree: return TraversalState.Continue;
                case TraversalState.Continue:
                default:
                    foreach (var child in node.Children)
                    {
                        if (TraverseDLRInner(child, visitor) != TraversalState.Continue) return TraversalState.Stop;
                    }
                    return TraversalState.Continue;
            }
        }
        /// <summary>
        /// 执行深度优先遍历（DLR：先序遍历），并允许通过 <see cref="TraversalState"/> 精确控制流程。
        /// </summary>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">开始遍历的根节点。</param>
        /// <param name="visitor">访问者函数，接收当前节点，并返回一个 <see cref="TraversalState"/>。</param>
        /// <returns>如果遍历完成（没有被 Stop 状态中断），则返回 true；否则返回 false。</returns>
        public static bool TraverseDLR<TNode>(this TNode node, Func<TNode, TraversalState> visitor)
            where TNode : class, ITreeNode<TNode>
            => TraverseDLRInner(node, visitor) == TraversalState.Continue;
        /// <summary>
        /// 执行深度优先遍历（DLR：先序遍历），使用 Predicate 访问者，用于查找或简单中止。
        /// </summary>
        /// <remarks>
        /// 如果 Predicate 返回 false，则遍历会立即停止（等同于 <see cref="TraversalState.Stop"/>）。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">开始遍历的根节点。</param>
        /// <param name="visitor">访问者 Predicate 函数。返回 true 则继续，返回 false 则停止。</param>
        /// <returns>如果遍历完成（没有被 Predicate 返回 false 中断），则返回 true；否则返回 false。</returns>
        public static bool TraverseDLR<TNode>(this TNode node, Predicate<TNode> visitor)
            where TNode : class, ITreeNode<TNode>
            => TraverseDLRInner(node, n => visitor(n) ? TraversalState.Continue: TraversalState.Stop) == TraversalState.Continue;
        /// <summary>
        /// 执行深度优先遍历（DLR：先序遍历），使用 Action 访问者，只用于简单的节点访问。
        /// </summary>
        /// <remarks>
        /// 遍历将访问所有节点，无法通过返回值控制流程（等同于始终返回 <see cref="TraversalState.Continue"/>）。
        /// </remarks>
        /// <typeparam name="TNode">实现 ITreeNode 的节点类型。</typeparam>
        /// <param name="node">开始遍历的根节点。</param>
        /// <param name="visitor">访问者 Action 函数，对每个节点执行操作。</param>
        /// <returns>始终返回 true，因为 Action 访问者不会中断遍历。</returns>
        public static bool TraverseDLR<TNode>(this TNode node, Action<TNode> visitor)
            where TNode : class, ITreeNode<TNode>
            => TraverseDLRInner(node, n =>
            {
                visitor(n);
                return TraversalState.Continue;
            }) == TraversalState.Continue;
        #endregion
    }
}
