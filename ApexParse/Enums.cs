using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    [Flags]
    enum PSO2DamageTrackers
    {
        Basic       = (1 << 0),
        Zanverse    = (1 << 1),
        DarkBlast   = (1 << 2),
        AIS         = (1 << 3),
        HTF         = (1 << 4), //Hero Finish
        PWP         = (1 << 5), //Photon 
        Ride        = (1 << 6), //Rideroid
        LSW         = (1 << 7), //Laconium
        Elem        = (1 << 8),
        All = Basic | Zanverse | DarkBlast | AIS | HTF | PWP | Ride | LSW | Elem,
        None = 0
    }

    [Flags]
    enum EnabledLineSeries
    {
        TotalDPS            = (1 << 0),
        TotalDamageDealt    = (1 << 1),
        TotalDamageTaken    = (1 << 2),
        InstanceDPS         = (1 << 3),
        AverageMPADPS       = (1 << 4),
        All = TotalDPS | TotalDamageDealt | TotalDamageTaken | InstanceDPS | AverageMPADPS
    }

    [Flags]
    enum DetailedDamageVisibleColumns
    {
        Name            = (1 << 0),
        Count           = (1 << 1),
        TotalDamage     = (1 << 2),
        JAPercent       = (1 << 3),
        CritPercent     = (1 << 4),
        MinDamage       = (1 << 5),
        AverageDamage   = (1 << 6),
        MaxDamage       = (1 << 7),
        DPS             = (1 << 8),
        All = Name | Count | TotalDamage | JAPercent | CritPercent | MinDamage | AverageDamage | MaxDamage | DPS
    }
}
