namespace Reziox.Model
{
    [Flags]
    public enum MYDays
    {
        monday =    0b_0000_0001,
        tuesday =   0b_0000_0010,
        wednesday = 0b_0000_0100,
        thursday =  0b_0000_1000,
        friday =    0b_0001_0000,
        saturday =  0b_0010_0000,
        sunday =    0b_0100_0000
    } 
    [Flags]
    public enum MyShifts
    {
        morning =    0b_01,
        night  =     0b_10,
        full  =  morning | night 
    }
    public enum MyStatus
    {
        confirmation=2,
        approve = 1,
        pending = 0,
        reject = -1,
        cancel= -2,
        deleted=-3,
    }
    public enum MyCitys
    {
        amman = 1,
        zarqa = 2,
        irbid = 3,
        aqaba = 4,
        madaba = 5,
        mafraq = 6,
        jerash = 7,
        ajloun = 8,
        karak = 9,
        tafila = 10,
        maan = 11,
        salt = 12
    }
}
