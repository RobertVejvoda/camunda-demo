using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camunda.Command
{
    public record ResolveIncident([Required] long IncidentKey);
}
