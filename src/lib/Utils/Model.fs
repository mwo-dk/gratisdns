module GratisDns.Model

type UserName = string
type Password = string
type Host = string
type Domain = string
type IpAddress = string

type Credentials = {
    UserName: UserName
    Password: Password
}

type HostSetting = {
    HostCredentials: Credentials
    Host: Host
    Domain: Domain
    IpAddress: IpAddress
}

type Settings = {
    User: Credentials
    Hosts: HostSetting array
}

