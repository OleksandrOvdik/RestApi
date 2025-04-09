﻿namespace Electronics;
// SmartWatches -> SW
public class SmartWatches : Device, IPowerNotifier
{
    public int _batteryPercentage;
    public int batteryPercent
    {
        get
        {
            return _batteryPercentage;
        }
        set
        {
            if (value < 20)
            {
                NotifyPower();
            }

            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException("Batery percent must be between 0 and 100");
            }
            
            _batteryPercentage = value;
        }
    }


    public SmartWatches(string id, string name, int BatteryPercent) : base(id, name)
    {
        batteryPercent = BatteryPercent;    
    }

    public override void TurnedOn()
    {
        if (batteryPercent < 11)
        {
            
            throw new EmptyBatteryException();
        }
        
        base.TurnedOn();
        batteryPercent -= 10;
    }

    public void NotifyPower()
    {
        Console.WriteLine("You put low battery percent");
    }

    public override string ToString()
    {
        return $"Smartwatch: ID -> {ID}, {Name}, Mode: {(IsDeviceTurned ? "TurnedOn" : "TurnedOff")}, Battery: {batteryPercent}%";
    }
}