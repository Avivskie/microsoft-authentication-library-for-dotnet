﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Microsoft.Identity.Client.Utils
{
    /// <summary>
    /// An object that either wraps a SemaphoreSlim for syncronization or ignores syncronization completely and just keeps track of Wait / Release operations.
    /// </summary>
    internal class OptionalSemaphoreSlim
    {
        private readonly bool _useRealSemaphore;
        private int _noLockCurrentCount;
        private SemaphoreSlim _semaphoreSlim;

        public int CurrentCount
        {
            get
            {
                return _useRealSemaphore ? _semaphoreSlim.CurrentCount : _noLockCurrentCount;
            }
        }

        public OptionalSemaphoreSlim(bool useRealSemaphore)
        {
            _useRealSemaphore = useRealSemaphore;
            if (_useRealSemaphore)
            {
                _semaphoreSlim = new SemaphoreSlim(1, 1);
            }
            _noLockCurrentCount = 1;
        }

        public void Release()
        {
            if (_useRealSemaphore)
            {
                _semaphoreSlim.Release();
            }
            else
            {
                Interlocked.Increment(ref _noLockCurrentCount);
            }
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            if (_useRealSemaphore)
            {
                return _semaphoreSlim.WaitAsync(cancellationToken);
            }
            else
            {
                Interlocked.Decrement(ref _noLockCurrentCount);
                return Task.FromResult(true);
            }
        }
        
        public void Wait()
        {
            if (_useRealSemaphore)
            {
                _semaphoreSlim.Wait();
            }
            else
            {
                Interlocked.Decrement(ref _noLockCurrentCount);
            }
        }
    }
}
