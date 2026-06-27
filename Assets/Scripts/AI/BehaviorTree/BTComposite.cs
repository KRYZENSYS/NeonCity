// =============================================================================
// NeonCity — AI / BehaviorTree / BTComposite.cs
// -----------------------------------------------------------------------------
// Composite nodes: Selector (OR), Sequence (AND), Parallel.
// =============================================================================

using System.Collections.Generic;

namespace NeonCity.AI.BehaviorTree
{
    /// <summary>Tries children in order until one succeeds/runs.</summary>
    public class BTSelector : BTNode
    {
        private readonly List<BTNode> _children;
        public BTSelector(string name, params BTNode[] children)
        {
            Name = name;
            _children = new List<BTNode>(children);
        }

        public override BTState Tick(BTContext ctx)
        {
            foreach (var c in _children)
            {
                var s = c.Tick(ctx);
                if (s == BTState.Success) return BTState.Success;
                if (s == BTState.Running) return BTState.Running;
            }
            return BTState.Failure;
        }
    }

    /// <summary>Runs children in order; fails if any child fails.</summary>
    public class BTSequence : BTNode
    {
        private readonly List<BTNode> _children;
        public BTSequence(string name, params BTNode[] children)
        {
            Name = name;
            _children = new List<BTNode>(children);
        }

        public override BTState Tick(BTContext ctx)
        {
            foreach (var c in _children)
            {
                var s = c.Tick(ctx);
                if (s == BTState.Failure) return BTState.Failure;
                if (s == BTState.Running) return BTState.Running;
            }
            return BTState.Success;
        }
    }

    /// <summary>Ticks all children each frame; returns Success when N succeed.</summary>
    public class BTParallel : BTNode
    {
        private readonly List<BTNode> _children;
        private readonly int _requiredSuccess;
        public BTParallel(string name, int requiredSuccess, params BTNode[] children)
        {
            Name = name; _requiredSuccess = requiredSuccess;
            _children = new List<BTNode>(children);
        }

        public override BTState Tick(BTContext ctx)
        {
            int success = 0;
            foreach (var c in _children)
            {
                var s = c.Tick(ctx);
                if (s == BTState.Success) success++;
            }
            return success >= _requiredSuccess ? BTState.Success : BTState.Running;
        }
    }
}