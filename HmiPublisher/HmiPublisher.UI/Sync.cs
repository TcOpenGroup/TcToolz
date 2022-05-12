using System;
using System.Collections.Generic;
using System.Threading;

namespace HmiPublisher.UI
{
    public class Sync
    {
        public static Mutex SpinLock = new Mutex();
        public static List<ThreadWrapper> MainThreads = new List<ThreadWrapper>();
        public static List<ThreadWrapper> CopyThread = new List<ThreadWrapper>();
        public static List<ThreadWrapper> MeasureThread = new List<ThreadWrapper>();
        public static List<ThreadWrapper> PrepareThread = new List<ThreadWrapper>();
        public static List<ThreadWrapper> CommandThread = new List<ThreadWrapper>();
        private static readonly Object obj = new Object();


        public static void ClearThreads()
        {
            foreach (var item in MainThreads)
            {
                try
                {
                    item.Thread.Abort();
                }
                catch (Exception ex)
                {

                }
            }
            foreach (var item in CopyThread)
            {
                try
                {
                    item.Thread.Abort();
                }
                catch (Exception ex)
                {

                }
            }
            foreach (var item in MeasureThread)
            {
                try
                {
                    item.Thread.Abort();
                }
                catch (Exception ex)
                {

                }
            }
            foreach (var item in PrepareThread)
            {
                try
                {
                    item.Thread.Abort();
                }
                catch (Exception ex)
                {

                }
            }

            foreach (var item in CommandThread)
            {
                try
                {
                    item.Thread.Abort();
                }
                catch (Exception ex)
                {

                }
            }

            MainThreads.Clear();
            CopyThread.Clear();
            MeasureThread.Clear();
            PrepareThread.Clear();
            try
            {
                SpinLock.ReleaseMutex();
            }
            catch (Exception)
            {

            }
        }
    }

    public class ThreadWrapper
    {
        public string Id { get; set; }
        public Thread Thread { get; set; }
    }
}
