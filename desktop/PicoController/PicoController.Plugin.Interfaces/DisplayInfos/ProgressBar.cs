using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin.DisplayInfos;

public class ProgressBar
{
	public ProgressBar()
    {
        Indeterminate = true;
        Min = 0f;
        Max = 0f;
        Value = 0f;
    }

    public ProgressBar(float min, float max, float value)
    {
        Indeterminate = false;
        Min = min;
        Max = max;
        Value = value;
    }

    public bool Indeterminate { get; }
    public float Min { get; }
    public float Max { get; }
    public float Value { get; }

    public void Deconstruct(out float min, out float max, out float value, out bool indeterminate) 
    {
        min = Min;
        max = Max;
        value = Value;
        indeterminate = Indeterminate;
    }

    public static bool operator ==(ProgressBar one, ProgressBar other)
        => one.Indeterminate == other.Indeterminate
        && one.Min           == other.Min
        && one.Max           == other.Max
        && one.Value         == other.Value;

    public static bool operator !=(ProgressBar one, ProgressBar other)
        => one.Indeterminate != other.Indeterminate
        || one.Min != other.Min
        || one.Max != other.Max
        || one.Value != other.Value;

    public override bool Equals(object? obj)
        => obj is ProgressBar bar
        && Min.Equals(bar.Min)
        && Max.Equals(bar.Max)
        && Value.Equals(bar.Value)
        && Indeterminate.Equals(bar.Indeterminate);


    public override int GetHashCode() 
        => HashCode.Combine(Indeterminate, Min, Max, Value);
}
