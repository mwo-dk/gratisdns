module GratisDns.Commands

open System.Net
open McMaster.Extensions.CommandLineUtils
open Utils
open Console
open Options

let private getIp() =
    printfn "Fetching publich IP address..."
    match getPublicIp() with
    | Some ipAddress ->
        (fun () -> printf "Your public IP address is: ") |> useForegroundColor System.ConsoleColor.Green
        (fun () -> printfn "%A" ipAddress) |> useForegroundColor System.ConsoleColor.White
        ipAddress |> Some
    | _ -> 
        (fun () -> printfn "Unable to retreive your public up") |> useForegroundColor System.ConsoleColor.Red
        None

type PublicIpCommand() as this =
    inherit CommandLineApplication()

    let helpOption = HelpOption()

    do
        this |> setCmdName "public-ip"
        this |> setCmdDescription "Gets the public ip"

        this |> withOption helpOption

        this.OnExecute(fun () ->
            if helpOption |> hasValue then
                this.ShowHelp()
            else getIp() |> ignore)

type UpdateCommand() as this =
    inherit CommandLineApplication()

    let helpOption = HelpOption()
    let userNameOption = UserNameOption()
    let passwordOption = PasswordOption()
    let domainOption = DomainOption()
    let ipAddressOption = IPAddressOption()

    do
        this |> setCmdName "update"
        this |> setCmdDescription "Updates the host(s) (A-record(s)). Multiple can be updated for the same account by providing more hosts"

        this |> withOption helpOption
        this |> withOption userNameOption
        this |> withOption passwordOption
        this |> withOption domainOption
        this |> withOption ipAddressOption
        let hostArguments = this |> withArguments "Hosts" "The hosts to update"

        this.OnExecute(fun () ->
            if helpOption |> hasValue then
                this.ShowHelp()
            else
                let hosts = 
                    hostArguments.Values |>
                    Seq.toList
                if hosts |> List.isEmpty then
                    (fun () -> printf "No hosts denoted as arguments") |> useForegroundColor System.ConsoleColor.Red
                else
                    let userName = userNameOption |> getOptionValue "User name> "
                    let password = passwordOption |> getOptionValueHidden "Password> "
                    let domain = domainOption |> getOptionValue "Domain> "
                    let ipAddress = 
                        if ipAddressOption |> hasValue then ipAddressOption.ParsedValue |> Some
                        else getIp()
                    //let ipAddress = getPublicIp()
                    match ipAddress with
                    | Some ipAddress ->
                        hosts |>
                        List.iter (fun host ->
                            (fun () -> printf "Updating public IP for Domain: ") |> useForegroundColor System.ConsoleColor.Green
                            printf "%s" domain
                            (fun () -> printf " IP address: ") |> useForegroundColor System.ConsoleColor.Green
                            printf "%A" ipAddress
                            (fun () -> printf " Host: ") |> useForegroundColor System.ConsoleColor.Green
                            printfn "%s" host
                            let ok, statusCode, reason = updateDns userName password domain host ipAddress
                            if ok then printfn "%s updated successfully" host
                            else
                                (fun () -> printfn "Error updating host %s" host) |> useForegroundColor System.ConsoleColor.Red
                                printfn "Http status code: %A" statusCode
                                printfn "Error reason: %s" reason)
                    | _ -> ())

type PingCommand() as this =
    inherit CommandLineApplication()

    let helpOption = HelpOption()

    do
        this |> setCmdName "ping"
        this |> setCmdDescription "Pings the GratisDns service"

        this |> withOption helpOption

        this.OnExecute(fun () ->
            if helpOption |> hasValue then
                this.ShowHelp())

type ServiceCommand() as this =
    inherit CommandLineApplication()

    let helpOption = HelpOption()

    do
        this |> setCmdName "service"
        this |> setCmdDescription "Talks to the GratisDns service"

        this |> withOption helpOption

        this |> withCommand 0 (new PingCommand())

        this.OnExecute(fun () ->
            if helpOption |> hasValue then
                this.ShowHelp())

let logo = [|
    "   ______           __  _      ____  _   _______"
    "  / ____/________ _/ /_(_)____/ __ \/ | / / ___/"
    " / / __/ ___/ __ `/ __/ / ___/ / / /  |/ /\__ \ "
    "/ /_/ / /  / /_/ / /_/ (__  ) /_/ / /|  /___/ / "
    "\____/_/   \__,_/\__/_/____/_____/_/ |_//____/  "
|]
let private printLogo() = 
    logo |> 
    Array.iter (fun line -> printfn "%s" line)
    
let private printInfo() =
    printLogo()
    let version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
    let versionInfo = sprintf "gratisdns v%A" version
    printLineForegroundColored System.ConsoleColor.Green versionInfo

type RootCommand() as this =
    inherit CommandLineApplication()

    do
        this |> setCmdName "gratisdns"
        this |> setCmdDescription "The gratisdns root command"
        this |> withCommand 0 (new PublicIpCommand())
        this |> withCommand 1 (new UpdateCommand())
        this |> withCommand 2 (new ServiceCommand())

        this.OnExecute(fun () ->
            printInfo()
            this.ShowHelp())