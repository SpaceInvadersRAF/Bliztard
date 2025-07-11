using System.Diagnostics.CodeAnalysis;

namespace Bliztard.Application.Utilities;

public class CountCoordinator
{
    private int m_Count;

    private readonly object               m_Lock          = new();
    private readonly ManualResetEventSlim m_ZeroEventLock = new(true);

    public void Increase()
    {
        lock (m_Lock)
        {
            m_Count++;
            m_ZeroEventLock.Reset();
        }
    }

    public void Decrease()
    {
        lock (m_Lock)
        {
            m_Count--;

            if (m_Count == 0)
                m_ZeroEventLock.Set();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public void WaitForZero()
    {
        m_ZeroEventLock.Wait();
    }
}
