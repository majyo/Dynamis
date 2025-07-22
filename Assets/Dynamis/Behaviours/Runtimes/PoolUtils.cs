using System.Collections.Generic;
using UnityEngine.Pool;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// Provides object pooling utilities for Queue and Stack of Node
    /// </summary>
    public static class PoolUtils
    {
        // Object pool for Queue<Node>
        private static readonly ObjectPool<Queue<Node>> QueuePool = new(
            createFunc: () => new Queue<Node>(),
            actionOnGet: queue => queue.Clear(),
            actionOnRelease: queue => queue.Clear(),
            actionOnDestroy: null,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        // Object pool for Stack&lt;Node&gt;
        private static readonly ObjectPool<Stack<Node>> StackPool = new(
            createFunc: () => new Stack<Node>(),
            actionOnGet: stack => stack.Clear(),
            actionOnRelease: stack => stack.Clear(),
            actionOnDestroy: null,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        // Object pool for List&lt;Node&gt;
        private static readonly ObjectPool<List<Node>> ListPool = new(
            createFunc: () => new List<Node>(),
            actionOnGet: list => list.Clear(),
            actionOnRelease: list => list.Clear(),
            actionOnDestroy: null,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        /// <summary>
        /// Gets a Queue&lt;Node&gt; from the pool
        /// </summary>
        /// <returns>A cleared Queue&lt;Node&gt; instance</returns>
        public static Queue<Node> GetQueue()
        {
            return QueuePool.Get();
        }

        /// <summary>
        /// Releases a Queue&lt;Node&gt; back to the pool
        /// </summary>
        /// <param name="queue">The queue to release</param>
        public static void ReleaseQueue(Queue<Node> queue)
        {
            if (queue != null)
            {
                QueuePool.Release(queue);
            }
        }

        /// <summary>
        /// Gets a Stack&lt;Node&gt; from the pool
        /// </summary>
        /// <returns>A cleared Stack&lt;Node&gt; instance</returns>
        public static Stack<Node> GetStack()
        {
            return StackPool.Get();
        }

        /// <summary>
        /// Releases a Stack&lt;Node&gt; back to the pool
        /// </summary>
        /// <param name="stack">The stack to release</param>
        public static void ReleaseStack(Stack<Node> stack)
        {
            if (stack != null)
            {
                StackPool.Release(stack);
            }
        }

        /// <summary>
        /// Gets a List&lt;Node&gt; from the pool
        /// </summary>
        /// <returns>A cleared List&lt;Node&gt; instance</returns>
        public static List<Node> GetList()
        {
            return ListPool.Get();
        }

        /// <summary>
        /// Releases a List&lt;Node&gt; back to the pool
        /// </summary>
        /// <param name="list">The list to release</param>
        public static void ReleaseList(List<Node> list)
        {
            if (list != null)
            {
                ListPool.Release(list);
            }
        }
    }
}
