using System;
using System.Collections.Generic;
using System.Threading;

namespace CWishlist_win
{
    public delegate void function();

    public class ThreadManager
    {
        readonly int proc_count = Environment.ProcessorCount;
        readonly Thread[] threads;
        readonly List<task> tasks = new List<task>();
        public readonly object task_mutex = new object();

        public ThreadManager()
        {
            threads = new Thread[proc_count];
            for (int j = 0; j < proc_count; j++)
                (threads[j] = new Thread(worker_thread)).Start();
        }

        void worker_thread()
        {
            try
            {
                while (true)
                {
                    task t = next();
                    if (t == null)
                        Thread.Sleep(1);
                    else
                    {
                        t.func();
                        //in this order because otherwise the other threads could think:
                        //executed = false, running = false, ill execute this
                        //which would lead to double execution
                        t.executed = true;
                        t.running = false;
                    }
                }
            }
            catch { }
        }

        task next()
        {
            lock (task_mutex)
            {
                foreach (task t in tasks)
                {
                    if (!t.running && !t.executed)
                    {
                        //we must set this here because race condition
                        t.running = true;
                        return t;
                    }
                }
            }
            return null;
        }

        public int start(function f)
        {
            lock (task_mutex)
            {
                int i = tasks.Count;
                tasks.Add(new task(f));
                return i;
            }
        }

        public void shutdown()
        {
            foreach (task t in tasks)
                t.join();
            foreach (Thread t in threads)
                t.Abort();
        }

        public void join(int taskid)
        {
            tasks[taskid].join();
        }

        public void finishall()
        {
            foreach (task t in tasks)
                t.join();
            lock (task_mutex)
            {
                tasks.Clear();
            }
        }
    }

    class task
    {
        public readonly function func;
        public bool running;
        public bool executed;

        public task(function func)
        {
            this.func = func;
            running = false;
            executed = false;
        }

        public void join()
        {
            while (!executed) ;
        }
    }
}
