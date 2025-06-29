using Towel;

namespace Bliztard.Persistence.Extension;

public static class Generic
{
    // note MEDIC: pomaze da zaleci onu budzevinu
    public static CompareResult Medic<TMedic>(this TMedic instance, TMedic compareTo) where TMedic : IComparable<TMedic>
    {
        return instance.CompareTo(compareTo) switch
               {
                   < 0 => CompareResult.Less,
                   > 0 => CompareResult.Greater,
                   0   => CompareResult.Equal
               };
    }
}