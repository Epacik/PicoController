namespace PicoController.Plugin.DisplayInfos

open System
open System.Runtime.InteropServices

type public ProgressBar private (indeterminate: bool, value: float32, min: float32, max: float32) =
    class
        member this.Indeterminate: bool = indeterminate;
        member this.Min: float32 = min;
        member this.Max: float32 = max;
        member this.Value: float32 = value;
        new(
            value: float32,
            [<OptionalArgument; DefaultParameterValue(0)>]min: float32, 
            [<OptionalArgument; DefaultParameterValue(100)>]max: float32) =
                ProgressBar(false, value, min, max)

        new() = ProgressBar(true, 0f, 0f, 0f)

         override this.GetHashCode()  = 
             HashCode.Combine(this.Indeterminate, this.Min, this.Max, this.Value)

         override this.Equals(other: Object) =
             match other with
             | :? ProgressBar as bar -> this = bar
             | _ -> false

         static member op_Equality (one: ProgressBar, other: ProgressBar) = 
             (one.Indeterminate, one.Min, one.Max, one.Value) = (other.Indeterminate, other.Min, other.Max, other.Value)

         static member op_Inequality (one: ProgressBar, other: ProgressBar) = 
             not (one = other)
    end

type public Text (
    content: string, 
    [<OptionalArgument; DefaultParameterValue(14)>] fontSize: double, 
    [<OptionalArgument; DefaultParameterValue(400)>] fontWeight: int) =
        class
            member this.Content: string  = content;
            member this.FontSize: double = fontSize;
            member this.FontWeight: int  = fontWeight;

            override this.GetHashCode()  = 
                HashCode.Combine(this.Content, this.FontSize, this.FontWeight)

            override this.Equals(other: Object) =
                match other with
                | :? Text as text -> this = text
                | _ -> false

            static member op_Equality (one: Text, other: Text) = 
                (one.Content, one.FontSize, one.FontWeight) = (other.Content, other.FontSize, other.FontWeight)

            static member op_Inequality (one: Text, other: Text) = 
                not (one = other)
        end


type public DisplayInformations = 
    | Text of Text
    | ProgressBar of ProgressBar