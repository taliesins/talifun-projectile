using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

namespace Talifun.Projectile.Rubbish.Helper
{
    internal class LazyAsyncResult : IAsyncResult
    {
        [ThreadStatic]
        private static LazyAsyncResult.ThreadContext t_ThreadContext;
        private object m_AsyncObject;
        private object m_AsyncState;
        private AsyncCallback m_AsyncCallback;
        private object m_Result;
        private int m_ErrorCode;
        private int m_IntCompleted;
        private bool m_EndCalled;
        private bool m_UserEvent;
        private object m_Event;
        private const int c_HighBit = -2147483648;
        private const int c_ForceAsyncCount = 50;

        private static LazyAsyncResult.ThreadContext CurrentThreadContext
        {
            get
            {
                LazyAsyncResult.ThreadContext threadContext = LazyAsyncResult.t_ThreadContext;
                if (threadContext == null)
                {
                    threadContext = new LazyAsyncResult.ThreadContext();
                    LazyAsyncResult.t_ThreadContext = threadContext;
                }
                return threadContext;
            }
        }

        internal object AsyncObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_AsyncObject;
            }
        }

        public object AsyncState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_AsyncState;
            }
        }

        protected AsyncCallback AsyncCallback
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_AsyncCallback;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_AsyncCallback = value;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                this.m_UserEvent = true;
                if (this.m_IntCompleted == 0)
                    Interlocked.CompareExchange(ref this.m_IntCompleted, int.MinValue, 0);
                ManualResetEvent waitHandle = (ManualResetEvent)this.m_Event;
                while (waitHandle == null)
                    this.LazilyCreateEvent(out waitHandle);
                return (WaitHandle)waitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                int num = this.m_IntCompleted;
                if (num == 0)
                    num = Interlocked.CompareExchange(ref this.m_IntCompleted, int.MinValue, 0);
                return num > 0;
            }
        }

        public bool IsCompleted
        {
            get
            {
                int num = this.m_IntCompleted;
                if (num == 0)
                    num = Interlocked.CompareExchange(ref this.m_IntCompleted, int.MinValue, 0);
                return (num & int.MaxValue) != 0;
            }
        }

        internal bool InternalPeekCompleted
        {
            get
            {
                return (this.m_IntCompleted & int.MaxValue) != 0;
            }
        }

        internal object Result
        {
            get
            {
                if (this.m_Result != DBNull.Value)
                    return this.m_Result;
                else
                    return (object)null;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_Result = value;
            }
        }

        internal bool EndCalled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_EndCalled;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_EndCalled = value;
            }
        }

        internal int ErrorCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_ErrorCode;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_ErrorCode = value;
            }
        }

        internal LazyAsyncResult(object myObject, object myState, AsyncCallback myCallBack)
        {
            this.m_AsyncObject = myObject;
            this.m_AsyncState = myState;
            this.m_AsyncCallback = myCallBack;
            this.m_Result = (object)DBNull.Value;
        }

        internal LazyAsyncResult(object myObject, object myState, AsyncCallback myCallBack, object result)
        {
            this.m_AsyncObject = myObject;
            this.m_AsyncState = myState;
            this.m_AsyncCallback = myCallBack;
            this.m_Result = result;
            this.m_IntCompleted = 1;
            if (this.m_AsyncCallback == null)
                return;
            this.m_AsyncCallback((IAsyncResult)this);
        }

        [Conditional("DEBUG")]
        protected void DebugProtectState(bool protect)
        {
        }

        protected void ProtectedInvokeCallback(object result, IntPtr userToken)
        {
            if (result == DBNull.Value)
                throw new ArgumentNullException("result");
            if ((this.m_IntCompleted & int.MaxValue) != 0 || (Interlocked.Increment(ref this.m_IntCompleted) & int.MaxValue) != 1)
                return;
            if (this.m_Result == DBNull.Value)
                this.m_Result = result;
            ManualResetEvent manualResetEvent = (ManualResetEvent)this.m_Event;
            if (manualResetEvent != null)
            {
                try
                {
                    manualResetEvent.Set();
                }
                catch (ObjectDisposedException ex)
                {
                }
            }
            this.Complete(userToken);
        }

        internal void InvokeCallback(object result)
        {
            this.ProtectedInvokeCallback(result, IntPtr.Zero);
        }

        internal void InvokeCallback()
        {
            this.ProtectedInvokeCallback((object)null, IntPtr.Zero);
        }

        protected virtual void Complete(IntPtr userToken)
        {
            bool flag = false;
            LazyAsyncResult.ThreadContext currentThreadContext = LazyAsyncResult.CurrentThreadContext;
            try
            {
                ++currentThreadContext.m_NestedIOCount;
                if (this.m_AsyncCallback == null)
                    return;
                if (currentThreadContext.m_NestedIOCount >= 50)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.WorkerThreadComplete));
                    flag = true;
                }
                else
                    this.m_AsyncCallback((IAsyncResult)this);
            }
            finally
            {
                --currentThreadContext.m_NestedIOCount;
                if (!flag)
                    this.Cleanup();
            }
        }

        protected virtual void Cleanup()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal object InternalWaitForCompletion()
        {
            return this.WaitForCompletion(true);
        }

        private object WaitForCompletion(bool snap)
        {
            ManualResetEvent waitHandle = (ManualResetEvent)null;
            bool flag = false;
            if (!(snap ? this.IsCompleted : this.InternalPeekCompleted))
            {
                waitHandle = (ManualResetEvent)this.m_Event;
                if (waitHandle == null)
                    flag = this.LazilyCreateEvent(out waitHandle);
            }
            if (waitHandle != null)
            {
                try
                {
                    waitHandle.WaitOne(-1, false);
                }
                catch (ObjectDisposedException ex)
                {
                }
                finally
                {
                    if (flag && !this.m_UserEvent)
                    {
                        ManualResetEvent manualResetEvent = (ManualResetEvent)this.m_Event;
                        this.m_Event = (object)null;
                        if (!this.m_UserEvent)
                            manualResetEvent.Close();
                    }
                }
            }
            while (this.m_Result == DBNull.Value)
                Thread.SpinWait(1);
            return this.m_Result;
        }

        internal void InternalCleanup()
        {
            if ((this.m_IntCompleted & int.MaxValue) != 0 || (Interlocked.Increment(ref this.m_IntCompleted) & int.MaxValue) != 1)
                return;
            this.m_Result = (object)null;
            this.Cleanup();
        }

        private bool LazilyCreateEvent(out ManualResetEvent waitHandle)
        {
            waitHandle = new ManualResetEvent(false);
            try
            {
                if (Interlocked.CompareExchange(ref this.m_Event, (object)waitHandle, (object)null) == null)
                {
                    if (this.InternalPeekCompleted)
                        waitHandle.Set();
                    return true;
                }
                else
                {
                    waitHandle.Close();
                    waitHandle = (ManualResetEvent)this.m_Event;
                    return false;
                }
            }
            catch
            {
                this.m_Event = (object)null;
                if (waitHandle != null)
                    waitHandle.Close();
                throw;
            }
        }

        private void WorkerThreadComplete(object state)
        {
            try
            {
                this.m_AsyncCallback((IAsyncResult)this);
            }
            finally
            {
                this.Cleanup();
            }
        }

        private class ThreadContext
        {
            internal int m_NestedIOCount;
        }
    }
}
