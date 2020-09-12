module GratisDns.Utils

open System.Net
open System.Net.Http
open Newtonsoft.Json

type Payload = {ip: string}

// https://aaronluna.dev/blog/csharp-retrieve-local-public-ip-address/
let getPublicIp() = 
    try
        use client = new HttpClient()
        use response = client.GetAsync("https://api.ipify.org?format=json") |> Async.AwaitTask |> Async.RunSynchronously
        if response.IsSuccessStatusCode then
            let data = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            let payload = JsonConvert.DeserializeObject<Payload>(data)
            let ok, result = IPAddress.TryParse(payload.ip)
            if ok then result |> Some
            else None
        else None
    with
    | error -> None

let private getUpdateUrl userName password domain host (ip: IPAddress) =
    sprintf "https://admin.gratisdns.com/ddns.php?u=%s&p=%s&d=%s&h=%s&i=%A" userName password domain host ip
let updateDns userName password domain host ip = 
    let url = getUpdateUrl userName password domain host ip 
    use client = new HttpClient()
    use response = client.GetAsync(url) |> Async.AwaitTask |> Async.RunSynchronously
    let message = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
    if response.IsSuccessStatusCode && message.Substring(0, 2).ToLower() = "ok" then true, HttpStatusCode.OK, ""
    else false, response.StatusCode, message