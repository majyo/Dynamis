using System.Collections.Generic;
using UnityEngine.Pool;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 提供Queue和Stack的对象池化工具类
    /// </summary>
    public static class PoolUtils
    {
        // Queue&lt;Node&gt;的对象池
        private static readonly ObjectPool<Queue<Node>> QueuePool = new(
            createFunc: () => new Queue<Node>(),
            actionOnGet: queue => queue.Clear(),
            actionOnRelease: queue => queue.Clear(),
            actionOnDestroy: null,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        // Stack&lt;Node&gt;的对象池
        private static readonly ObjectPool<Stack<Node>> StackPool = new(
            createFunc: () => new Stack<Node>(),
            actionOnGet: stack => stack.Clear(),
            actionOnRelease: stack => stack.Clear(),
            actionOnDestroy: null,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        // List&lt;Node&gt;的对象池
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
        /// 从池中获取一个Queue&lt;Node&gt;
        /// </summary>
        /// <returns>清空的Queue&lt;Node&gt;实例</returns>
        public static Queue<Node> GetQueue()
        {
            return QueuePool.Get();
        }

        /// <summary>
        /// 将Queue&lt;Node&gt;归还到池中
        /// </summary>
        /// <param name="queue">要归还的队列</param>
        public static void ReleaseQueue(Queue<Node> queue)
        {
            if (queue != null)
            {
                QueuePool.Release(queue);
            }
        }

        /// <summary>
        /// 从池中获取一个Stack&lt;Node&gt;
        /// </summary>
        /// <returns>清空的Stack&lt;Node&gt;实例</returns>
        public static Stack<Node> GetStack()
        {
            return StackPool.Get();
        }

        /// <summary>
        /// 将Stack&lt;Node&gt;归还到池中
        /// </summary>
        /// <param name="stack">要归还的栈</param>
        public static void ReleaseStack(Stack<Node> stack)
        {
            if (stack != null)
            {
                StackPool.Release(stack);
            }
        }

        /// <summary>
        /// 从池中获取一个List&lt;Node&gt;
        /// </summary>
        /// <returns>清空的List&lt;Node&gt;实例</returns>
        public static List<Node> GetList()
        {
            return ListPool.Get();
        }

        /// <summary>
        /// 将List&lt;Node&gt;归还到池中
        /// </summary>
        /// <param name="list">要归还的列表</param>
        public static void ReleaseList(List<Node> list)
        {
            if (list != null)
            {
                ListPool.Release(list);
            }
        }
    }
}
