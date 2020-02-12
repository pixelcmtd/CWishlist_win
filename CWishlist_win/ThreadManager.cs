using System;
using System.Collections.Generic;
using System.Threading;
using static binutils.bin;
using static binutils.str;
using static binutils.io;

namespace CWishlist_win
{
    public delegate void function();

    public class ThreadManager
    {
        readonly int proc_count = Environment.ProcessorCount;
        Thread[] threads;
        Thread update;
        public List<task> tasks = new List<task>();
        public readonly object task_mutex = new object();
        public readonly object wtask_mutex = new object(); //write task(s) mutex

        public ThreadManager()
        {
            lock (task_mutex)
            {
                lock (wtask_mutex)
                {
                    threads = new Thread[proc_count];
                    for (int j = 0; j < proc_count; j++)
                        (threads[j] = new Thread(worker_thread)).Start();
                    (update = new Thread(update_thread)).Start();
                }
            }
        }

        void update_thread()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        finishall();
                        Thread.Sleep(1);
                    }
                    catch (Exception e)
                    {
                        if (e is ThreadAbortException) throw e;
                        else dbg("[ThreadManager-UpdateThread]Loop Exception: {0}", b64(utf8(e.ToString())));
                    }
                }
            }
            catch
            {
                dbg("[ThreadManager-UpdateThread]Thread shutdown.");
            }
        }

        void worker_thread()
        {
            try
            {
                while (true)
                {
                    task t = next;
                    if (t == null)
                        Thread.Sleep(1);
                    else
                    {
                        try
                        {
                            t.func();
                        }
                        catch (Exception e)
                        {
                            if (e is ThreadAbortException) throw e;
                            else dbg("[ThreadManager-WorkerThread]t.func Exception: {0}", b64(utf8(e.ToString())));
                        }
                        //in this order because otherwise the other threads could think:
                        //executed = false, running = false, ill execute this
                        //which would lead to double execution
                        t.executed = true;
                        t.running = false;
                    }
                }
            }
            catch
            {
                dbg("[ThreadManager-WorkerThread]Thread shutdown.");
            }
        }

        task next
        {
            get
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
        }

        public int start(function f)
        {
            lock (task_mutex)
            {
                lock (wtask_mutex)
                {
                    int i = tasks.Count;
                    tasks.Add(new task(f));
                    return i;
                }
            }
        }

        public void shutdown()
        {
            lock (wtask_mutex)
            {
                dbg("[ThreadManager]Shutting down.");
                foreach (task t in tasks)
                    if (t.func != finishall)
                        t.join();
                foreach (Thread t in threads)
                    t.Abort();
                update.Abort();
                //ensures we won't be able to use this manager anymore
                threads = null;
                tasks = null;
                update = null;
            }
        }

        public void join(int taskid) => tasks[taskid].join();

        public void finishall()
        {
            lock (wtask_mutex)
                foreach (task t in tasks)
                    if (t.func != finishall)
                        t.join();
            lock (task_mutex)
                if (tasks.where((t) => !t.executed) != default)
                    lock (wtask_mutex)
                        tasks.Clear();
                else
                    start(finishall);
        }
    }

    public class task
    {
        public readonly function func;
        public volatile bool running;
        public volatile bool executed;

        public task(function func)
        {
            this.func = func;
            running = false;
            executed = false;
        }

        public static bool operator ==(task t1, task t2)
        {
            return (!(t1 is null || t2 is null) && t1.Equals(t2))
                || (t1 is null && t2 is null);
        }

        public static bool operator !=(task t1, task t2)
        {
            return !(t1 == t2);
        }

        public bool Equals(task t)
        {
            return running == t.running && executed == t.executed && func == t.func;
        }

        public override bool Equals(object obj)
        {
            return obj is task ? Equals((task)obj) : false;
        }

        public override int GetHashCode()
        {
            return (int)func_ptr;
        }

        long func_ptr { get => func.Method.MethodHandle.GetFunctionPointer().ToInt64(); }

        public override string ToString()
        {
            return $"{func.Method.Name} @ {func_ptr.ToString("x16")}";
        }

        public string task_mgr_fmt()
        {
            return $"{this} ({(running ? "Running" : "Not running")}, {(executed ? "done executing" : "not executed yet")}";
        }

        public void join()
        {
            dbg($"[ThreadManager]Joining func {this}");
            while (!executed) ;
        }
    }
}
