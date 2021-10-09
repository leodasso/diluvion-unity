// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System;
#if !UNITY_WSA
using System.Threading;
#endif

namespace FluffyUnderware.DevTools
{
    public class ThreadPoolWorker : IDisposable
    {
#if !UNITY_WSA
        private int _remainingWorkItems = 1;
        private ManualResetEvent _done = new ManualResetEvent(false);

        public void QueueWorkItem(WaitCallback callback)
        {
            QueueWorkItem(callback, null);
        }

        public void QueueWorkItem(Action act)
        {
            QueueWorkItem(act, null);
        }

        public void QueueWorkItem(WaitCallback callback, object state)
        {
            ThrowIfDisposed();
            QueuedCallback qc = new QueuedCallback();
            qc.Callback = callback;
            qc.State = state;
            lock (_done)
                _remainingWorkItems++;
            ThreadPool.QueueUserWorkItem(new WaitCallback(HandleWorkItem), qc);
        }

        public void QueueWorkItem(Action act, object state)
        {
            ThrowIfDisposed();
            QueuedCallback qc = new QueuedCallback();
            qc.Callback = (x => act());
            qc.State = state;
            lock (_done)
                _remainingWorkItems++;
            ThreadPool.QueueUserWorkItem(new WaitCallback(HandleWorkItem), qc);
        }

        public bool WaitAll()
        {
            return WaitAll(-1, false);
        }

        public bool WaitAll(TimeSpan timeout, bool exitContext)
        {
            return WaitAll((int)timeout.TotalMilliseconds, exitContext);
        }

        public bool WaitAll(int millisecondsTimeout, bool exitContext)
        {
            ThrowIfDisposed();
            DoneWorkItem();
            bool rv = _done.WaitOne(millisecondsTimeout, exitContext);
            lock (_done)
            {
                if (rv)
                {
                    _remainingWorkItems = 1;
                    _done.Reset();
                }
                else
                    _remainingWorkItems++;
            }
            return rv;
        }

        private void HandleWorkItem(object state)
        {
            QueuedCallback qc = (QueuedCallback)state;
            try
            {
                qc.Callback(qc.State);
            }
            finally
            {
                DoneWorkItem();
            }
        }

        private void DoneWorkItem()
        {
            lock (_done)
            {
                --_remainingWorkItems;
                if (_remainingWorkItems == 0)
                    _done.Set();
            }
        }

        private class QueuedCallback
        {
            public WaitCallback Callback;
            public object State;
        }

        private void ThrowIfDisposed()
        {
            if (_done == null)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            if (_done != null)
            {
                ((IDisposable)_done).Dispose();
                _done = null;
            }
        }
#else

        public void QueueWorkItem(Action act)
        {
        }

        public void Dispose()
        {
        }

        public bool WaitAll()
        {
            return true;
        }
#endif
    }
}
