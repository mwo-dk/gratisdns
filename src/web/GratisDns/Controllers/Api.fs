namespace GratisDns.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open GratisDns.Model
open Microsoft.AspNetCore.Authorization

[<ApiController>]
[<AllowAnonymous>]
[<Route("[controller]")>]
type Api (logger : ILogger<Api>) =
    inherit ControllerBase()

    [<HttpGet>]
    [<Route("ping")>]
    member _.PingAsync([<FromQuery>] msg: string) =
        let response : PingResponse = {
            Timestamp = DateTimeOffset.UtcNow
            Message = 
                if String.IsNullOrWhiteSpace(msg) then "Ok"
                else $"Ok. Message received: {msg}"
        }
        Task.FromResult(response)
