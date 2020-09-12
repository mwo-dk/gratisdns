module GratisDns.Console

open System
open McMaster.Extensions.CommandLineUtils
open Options

let useForegroundColor color f =
    let oldColor = Console.ForegroundColor
    try
        Console.ForegroundColor <- color
        f()
    finally
        Console.ForegroundColor <- oldColor

let printLineForegroundColored color line = useForegroundColor color (fun () -> printfn "%s" line)

let useBackgroundColor color f =
    let oldColor = Console.BackgroundColor
    try
        Console.BackgroundColor <- color
        f()
    finally
        Console.BackgroundColor <- oldColor

let printLineBackgroundColored color line = useBackgroundColor color (fun () -> printfn "%s" line)

let useColor foregroundColor backgroundColor f =
    let oldForegroundColor = Console.ForegroundColor
    let oldBackgroundColor = Console.BackgroundColor
    try
        Console.ForegroundColor <- foregroundColor
        Console.BackgroundColor <- backgroundColor
        f()
    finally
        Console.ForegroundColor <- oldForegroundColor
        Console.BackgroundColor <- oldBackgroundColor

let printLineColored foregroundColor backgroundColor line = 
    useColor foregroundColor backgroundColor (fun () -> printfn "%s" line)

let getIndex n N M = 
    let ratio = (float n)/(float N)
    int(round(ratio*(float M)))

let printWithForegroundColorScheme scheme lines =
    let N = lines |> List.length
    let M = scheme |> List.length
    lines |>
    List.iteri (fun n line ->
        let index = getIndex n N M
        let color = scheme |> Seq.item index
        printLineForegroundColored color line)

let printWithBackgroundColorScheme scheme lines =
    let N = lines |> List.length
    let M = scheme |> List.length
    lines |>
    List.iteri (fun n line ->
        let index = getIndex n N M
        let color = scheme |> Seq.item index
        printLineBackgroundColored color line)

let printWithColorScheme foregroundScheme backgroundScheme lines =
    let N = lines |> List.length
    let M = foregroundScheme |> List.length
    let M' = backgroundScheme |> List.length
    lines |>
    List.iteri (fun n line ->
        let index = getIndex n N M
        let foregroundColor = foregroundScheme |> List.item index
        let index' = getIndex n N M'
        let backgroundColor = backgroundScheme |> List.item index'
        printLineColored foregroundColor backgroundColor line)

let private rainbow = [
    ConsoleColor.Red
    ConsoleColor.DarkRed
    ConsoleColor.Red
    ConsoleColor.Yellow
    ConsoleColor.DarkGreen
    ConsoleColor.Cyan
    ConsoleColor.DarkBlue
    ConsoleColor.Magenta
]

let private rainbowForeground = [
    ConsoleColor.Black
    ConsoleColor.White
    ConsoleColor.Black
    ConsoleColor.Black
    ConsoleColor.White
    ConsoleColor.Black
    ConsoleColor.White
    ConsoleColor.White
]

let printForegroundRainbow lines = printWithForegroundColorScheme rainbow lines
let printBackgroundRainbow lines = printWithBackgroundColorScheme rainbow lines
let printRainbow lines = printWithColorScheme rainbowForeground rainbow lines

let private promptValue title =
    (fun () -> printf "%s" title) |> useForegroundColor System.ConsoleColor.Green
    System.Console.ReadLine()

let private readHiddenString() =
    let rec getString acc =
        let keyInfo = Console.ReadKey(true)
        match keyInfo.Key with
        | ConsoleKey.Enter -> 
            printfn ""
            acc |> List.rev
        | ConsoleKey.Backspace -> getString (acc |> List.tail)
        | x -> 
            printf "*"
            getString ((keyInfo.KeyChar)::acc)
    System.String(getString [] |> List.toArray)

let private promptValueHidden title =
    (fun () -> printf "%s" title) |> useForegroundColor System.ConsoleColor.Green
    readHiddenString()

let getOptionValue title (option: CommandOption) =
    if option |> hasValue then option |> getValue
    else
        promptValue title

let getOptionValueHidden title (option: CommandOption) =
    if option |> hasValue then option |> getValue
    else
        promptValueHidden title

let getOptionValue' title parser (option: CommandOption<'a>) =
    if option |> hasValue then option |> getParsedValue |> Some
    else
        let rec getSafe() =
            let value = promptValue title
            match value |> parser with
            | Some value -> value |> Some
            | _ ->
                (fun () -> printf "Invalid value:") |> useForegroundColor System.ConsoleColor.Red
                printfn " %s" value
                getSafe()
        getSafe()