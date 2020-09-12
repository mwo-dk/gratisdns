module GratisDns.Parsers

open System
open System.IO
open System.Globalization
open System.Security.Cryptography.X509Certificates
open Microsoft.FSharp.Reflection
open McMaster.Extensions.CommandLineUtils.Abstractions

type IntegerValueParser(minimum, maximum) =
    let validateValue value =
        match minimum, maximum with
        | Some min, _ when value < min ->
            false, sprintf "Invalid value. Value (%d) cannot be lower than provided minimum (%d)" value min |> Some
        | _, Some max when max < value ->
            false, sprintf "Invalid value. Value (%d) cannot be higher than provided maximum (%d)" value max |> Some
        | _ -> true, None

    let validate argName (value: string) =
        let mutable result = 0
        let ok = System.Int32.TryParse(value, &result)
        if ok |> not then
            raise (ArgumentException("Provided value must be a proper integer"))
        else 
        match result |> validateValue with
            | true, _ -> result
            | false, Some error ->
                raise (ArgumentOutOfRangeException(sprintf "Invalid argument %s. %s" argName error))
            | false, None ->
                raise (ArgumentOutOfRangeException("Unknown range validation error occurred"))

    do
        match minimum, maximum with
        | Some minimum, Some maximum ->
            if maximum < minimum then
                raise (ArgumentOutOfRangeException("Maximum value must be greater than or equal to minimum value"))
        | _ -> ()

    interface IValueParser<int> with
        member this.Parse(argName: string, value: string, cultureInfo: CultureInfo) =
            validate argName value :> obj
        member this.Parse(argName: string, value: string, cultureInfo: CultureInfo) =
            validate argName value
        member this.TargetType = typeof<int>

let private tryParseEnum<'a when 'a: (new: unit -> 'a) and 'a: struct and 'a :> System.ValueType> (value: string) = Enum.TryParse<'a>(value, true)
type EnumerationParser<'a when 'a: (new: unit -> 'a) and 'a: struct and 'a :> System.ValueType>() =
    let getUnionCaseFromString (s: string) =
        let toLower (x: string) = x.ToLower()
        let value, ok = Enum.TryParse(s, true)
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name |> toLower = (s |> toLower)) with
        | [|case|] -> FSharpValue.MakeUnion(case, Array.empty) :?> 'a |> Some
        | _ -> None

    let validate argName value =
        match value |> tryParseEnum with
        | true, value -> value
        | _ -> raise (ArgumentOutOfRangeException(sprintf "Invalid argument %s (%s)" argName value))

    interface IValueParser<'a> with
        member this.Parse(argName: string, value: string, cultureInfo: CultureInfo) =
            validate argName value :> obj
        member this.Parse(argName: string, value: string, cultureInfo: CultureInfo) : 'a =
            validate argName value
        member this.TargetType = typeof<'a>

type UnionCaseParser<'a>() =
    //http://www.fssnip.net/9l/title/toString-and-fromString-for-discriminated-unions
    let getUnionCaseFromString (s: string) =
        let toLower (x: string) = x.ToLower()
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name |> toLower = (s |> toLower)) with
        | [|case|] -> FSharpValue.MakeUnion(case, Array.empty) :?> 'a |> Some
        | _ -> None

    let validate argName value =
        match value |> getUnionCaseFromString with
        | Some value -> value
        | _ -> raise (ArgumentOutOfRangeException(sprintf "Invalid argument %s (%s)" argName value))

    interface IValueParser<'a> with
        member this.Parse(argName: string, value: string, cultureInfo: CultureInfo) =
            validate argName value :> obj
        member this.Parse(argName: string, value: string, cultureInfo: CultureInfo) =
            validate argName value
        member this.TargetType = typeof<'a>

type EnvironmentParser() =
    inherit UnionCaseParser<Environment>()

type StoreLocationParser() =
    inherit EnumerationParser<StoreLocation>()

type StoreNameParser() =
    inherit EnumerationParser<StoreName>()
