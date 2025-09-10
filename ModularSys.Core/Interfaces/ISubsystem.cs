using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Core.Interfaces;

public interface ISubsystem
{
    string Name { get; }
    string Route { get; }
    string? Icon { get; }
    int Order { get; }

    void RegisterServices(IServiceCollection services);
}


