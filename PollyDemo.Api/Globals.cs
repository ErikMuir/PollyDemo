global using System;
global using System.Net;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using MuirDev.ConsoleTools;

public static class Globals
{
    public static LogOptions NoEOL => new LogOptions(false);
}
