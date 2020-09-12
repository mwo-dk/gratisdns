module GratisDns.Options

open System.Net
open McMaster.Extensions.CommandLineUtils
open McMaster.Extensions.CommandLineUtils.Abstractions
open Parsers
open GratisDns

let setOptionDescription description (x: CommandOption) = x.Description <- description
let isRequired (x: CommandOption) = x.IsRequired() |> ignore
let hasValue (x: CommandOption) = x.HasValue()
let doesNotHaveValue x = x |> hasValue |> not
let allHaveValue xs = xs |> Seq.fold (fun acc x -> acc && x |> hasValue) true
let anyHasValue xs = xs |> Seq.fold (fun acc x -> acc || x |> hasValue) false
let noneHasValue xs = xs |> Seq.fold (fun acc x -> acc && x |> doesNotHaveValue) true
let getValue (option: CommandOption) = option.Value()
let getParsedValue (option: CommandOption<'a>) = option.ParsedValue
let getStringOrDefault defaultValue (option: CommandOption) = if option |> hasValue then option.Value() else defaultValue
let setCmdName name (cmd: CommandLineApplication) = cmd.Name <- name
let setCmdDescription description (cmd: CommandLineApplication) = cmd.Description <- description
let withOption option (cmd: CommandLineApplication) = cmd.Options.Add(option)
let withArguments name description (cmd: CommandLineApplication) = cmd.Argument(name, description, true)
let withCommand position subCommand (cmd: CommandLineApplication) = 
    cmd.Commands.Insert(position, subCommand)

type HelpOption() as this =
    inherit CommandOption("-h|--help", CommandOptionType.NoValue)

    do
        this |> setOptionDescription "Shows invokation options"

type UserNameOption() as this = 
    inherit CommandOption<string>(ValueParser.Create(fun _ value _ -> value), "-u|--user-name", CommandOptionType.SingleValue)

    do 
        this |> setOptionDescription "The user name of the account on whose behalf you call"

type PasswordOption() as this =
    inherit CommandOption("-p|--password", CommandOptionType.SingleValue)

    do
        this |> setOptionDescription "The password of the account on whose behalf you call"

type DomainOption() as this =
    inherit CommandOption("-d|--domain", CommandOptionType.SingleValue)

    do
        this |> setOptionDescription "The domain you want to update"

type IPAddressOption() as this =
    inherit CommandOption<IPAddress>(ValueParser.Create(fun _ value _ -> IPAddress.Parse(value)), "-i|--ip-address", CommandOptionType.SingleValue)

    do
        this |> setOptionDescription "The IP address"