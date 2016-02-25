using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.BL.Domain
{
  public class HardwareTicket : Ticket
  {
    [RegularExpression("^([A-Z]+-)[0-9]+")]
    public string DeviceName { get; set; }
  }
}
