namespace Bliztard.Application.Extension;

public static class Lock
{
    public delegate void    RefAction<T>(ref T @object);
    public delegate TResult RefFunc<T, out TResult>(ref T arg);

    #region By Value Locking

    public static void ReadBlock(this ReaderWriterLockSlim @lock, Action action)
    {
        @lock.EnterReadLock();

        try
        {
            action();
        }
        finally
        {
            @lock.ExitReadLock();
        }
    }

    public static TResult ReadBlock<TResult>(this ReaderWriterLockSlim @lock, Func<TResult> function)
    {
        @lock.EnterReadLock();

        try
        {
            return function();
        }
        finally
        {
            @lock.ExitReadLock();
        }
    }
    
    public static void WriteBlock(this ReaderWriterLockSlim @lock, Action action)
    {
        @lock.EnterWriteLock();

        try
        {
            action();
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }
    
    public static TResult WriteBlock<TResult>(this ReaderWriterLockSlim @lock, Func<TResult> function)
    {
        @lock.EnterWriteLock();

        try
        {
            return function();
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }
    
    #endregion

    #region By Reference Locking
    
    public static void ReadBlockRef<TInstance>(this ReaderWriterLockSlim @lock, TInstance value, RefAction<TInstance> action)
    {
        @lock.EnterReadLock();

        try
        {
            action(ref value);
        }
        finally
        {
            @lock.ExitReadLock();
        }
    }
    
    public static TResult ReadBlockRef<TInstance, TResult>(this ReaderWriterLockSlim @lock, ref TInstance value, RefFunc<TInstance, TResult> function)
    {
        @lock.EnterReadLock();

        try
        {
            return function(ref value);
        }
        finally
        {
            @lock.ExitReadLock();
        }
    }
    
    public static void WriteBlockRef<TInstance>(this ReaderWriterLockSlim @lock, ref TInstance value, RefAction<TInstance> action)
    {
        @lock.EnterWriteLock();

        try
        {
            action(ref value);
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }
    
    public static TResult WriteBlockRef<TInstance, TResult>(this ReaderWriterLockSlim @lock, ref TInstance value, RefFunc<TInstance, TResult> function)
    {
        @lock.EnterWriteLock();

        try
        {
            return function(ref value);
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }
    
    #endregion
}
