using System.Collections.Generic;

namespace DataStructures
{
    public class PrioQueue<T>
    {

        private Dictionary<int, Queue<T>> _queues;
        int min;
        int max;

        public PrioQueue()
        {
            _queues = new Dictionary<int, Queue<T>>();
            min = 1;
            max = 0;
        }

        public void Enqueue(T item, int prio = 0)
        {
            /*
            adds item to the queue at the specified priority
            if not priority is given, adds it to priority 0
            */
//            Console.WriteLine(prio.ToString());
            if (!_queues.ContainsKey(prio))
            {
                _queues.Add(prio, new Queue<T>());
            }
            if (prio < min)
            {
                min = prio;
            }
            if (prio > max)
            {
                max = prio;
            }
            _queues[prio].Enqueue(item);
        }

        public T Dequeque()
        {
            /*
            returns the first item of the highest priority in the cue
            if the cue is empty, returns null
            */
            //adjust max if empty
            if (!_queues.ContainsKey(min) || _queues[min].Count == 0)
            {
                _queues.Remove(min);
                while (!_queues.ContainsKey(min))
                {
                    min++;
                    if (max < min)
                    {
                        return default(T);
                    }
                }
            }
            return _queues[min].Dequeue();
        }

    }

}